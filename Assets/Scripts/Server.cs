using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class Server : MonoBehaviour
{
    public const int PORT = 9991;
    public GameObject playerPrefab;

    private static float serverTime = 0.02f; 
    private List<Character> allPlayers = new List<Character>();
    private int reliableChannel;
    private int unReliableChannel;
    private int hostId;
    private float sendPositionTimer = 0f;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private List<Vector3> allPlayersLastSentPos = new List<Vector3>();
    
	void Start ()
    {
        Application.runInBackground = true;
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);
        hostId = NetworkTransport.AddHost(topology, PORT);
    }
	
	void Update ()
    {
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId,
            recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:

                if(!allPlayers.Exists(it => it.ClientId == connectionId))
                {
                    /* Create player */
                    var a = Instantiate(playerPrefab) as GameObject;
                    a.transform.parent = transform;
                    a.GetComponent<CollisionQueue>().ClientId = connectionId;
                    allPlayers.Add(new Character(a, connectionId));

                    /* Identify the player */
                    AuthenticateMessage mA = new AuthenticateMessage(connectionId, connectionId);
                    SendNetworkReliableMessage(mA, connectionId);

                    /* Send player his position */
                    TransformMessage mP = new TransformMessage(connectionId);
                    Ray r = new Ray();
                    r.direction = Vector3.down;
                    r.origin = transform.position;
                    RaycastHit hitInfo = new RaycastHit();
                    if (Physics.Raycast(transform.position + (Vector3.down*4), Vector3.down, out hitInfo))
                    {
                        if(hitInfo.collider.gameObject.tag == "Player")
                        {
                            a.transform.position = new Vector3(5f, 5f);
                        }
                    }
                    else
                    {
                        a.transform.position = new Vector3(0f, 5f);
                    }

                    mP.Position.Vect3 = a.transform.position;
                    mP.Scale.Vect3 = a.transform.localScale;
                    mP.Rotation.Quaternion = a.transform.rotation;
                    SendNetworkReliableMessage(mP, connectionId);

                    /* Send Info about other player positions */
                    foreach(Character player in allPlayers)
                    {
                        if(player.ClientId != connectionId)
                        {
                            TransformMessage msg = new TransformMessage(player.ClientId);
                            msg.Position.Vect3 = player.CharacterObj.transform.position;
                            msg.Rotation.Quaternion = player.CharacterObj.transform.rotation;
                            msg.Scale.Vect3 = player.CharacterObj.transform.localScale;
                            SendNetworkReliableMessage(msg, connectionId);
                        }

                    }
                }
              
                break;

            case NetworkEventType.DataEvent:

                Stream stream = new MemoryStream(recBuffer);
                Message message = (Message)binFormater.Deserialize(stream);

                switch (message.GetNetworkMessageType())
                {
                    case NetworkMessageType.Input:
                        InputMessage mI = message as InputMessage;
                        Rigidbody playerBody = allPlayers.Find(it => it.ClientId == mI.ReceiverId)
                            .CharacterObj.GetComponent<Rigidbody>();
                        foreach(InputType type in mI.InputTypeMsg)
                        {
                            switch (type)
                            {
                                case InputType.MoveBack:
                                    playerBody.AddForce(Vector3.back * GameConsts.MOVE_SPEED * serverTime);
                                    break;

                                case InputType.MoveForward:
                                    playerBody.AddForce(Vector3.forward * GameConsts.MOVE_SPEED * serverTime);
                                    break;

                                case InputType.MoveLeft:
                                    playerBody.AddForce(Vector3.left * GameConsts.MOVE_SPEED * serverTime);
                                    break;

                                case InputType.MoveRight:
                                    playerBody.AddForce(Vector3.right * GameConsts.MOVE_SPEED * serverTime);
                                    break;
                            }

                        }

                        break;

                    case NetworkMessageType.Transform:

                        TransformMessage mP = message as TransformMessage;
                        Character keyPlayer = allPlayers.Find(it => it.ClientId == mP.ReceiverId);
                        Vector3 playerPosition = keyPlayer.CharacterObj.transform.position;
                        if((playerPosition - mP.Position.Vect3).sqrMagnitude > 2)
                        {
                            SendPosition(keyPlayer);
                        }

                        break;
                }

                
                break;

            case NetworkEventType.DisconnectEvent: 

                Character p = allPlayers.Find(it => it.ClientId == connectionId);
                Destroy(p.CharacterObj);
                allPlayers.Remove(p);

                DisconnectMessage m = new DisconnectMessage(connectionId);
                foreach(Character player in allPlayers)
                {
                    SendNetworkReliableMessage(m, player.ClientId);
                }
                

                break;
        }

        sendPositionTimer += Time.deltaTime;
        if(sendPositionTimer > 0.5f)
        {
            foreach(Character p in allPlayers)
            {
                var playerPos = p.CharacterObj.transform.position;
                allPlayersLastSentPos.Add(new Vector3(playerPos.x, playerPos.y, playerPos.z));
                /* Send all player positions that are with in radius */
                var allOtherPlayer = allPlayers.FindAll(it => it.ClientId != p.ClientId);
                foreach(Character otherP in allOtherPlayer)
                {
                    var otherPPos = otherP.CharacterObj.transform.position;
                    if(otherPPos != otherP.LastSentPosition)
                    {
                        TransformMessage mm = new TransformMessage(otherP.ClientId);
                        mm.Position.Vect3 = otherP.CharacterObj.transform.position;
                        mm.Scale.Vect3 = otherP.CharacterObj.transform.localScale;
                        mm.Rotation.Quaternion = otherP.CharacterObj.transform.rotation;
                        SendNetworkUnreliableMessage(mm, p.ClientId);
                        otherP.LastSentPosition = new Vector3(mm.Position.X, mm.Position.Y, mm.Position.Z);
                    }
                }
            }
            sendPositionTimer = 0;
        }


        foreach(Character p in allPlayers)
        {
            /* Cap player move speed */
            Rigidbody rg = p.CharacterObj.GetComponent<Rigidbody>();
            if (rg.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
            {
                rg.velocity = rg.velocity.normalized * GameConsts.MAX_MOVE_SPEED;
            }

            /* Check for players pushing other off the ledge */
            int winnerId = p.Update();
            if (winnerId != -1)
            {
                Character winner = allPlayers.Find(it => it.ClientId == winnerId);
                winner.CharacterObj.GetComponent<Transform>().localScale += new Vector3(0.5f, 0.5f, 0.5f);
                winner.CharacterObj.GetComponent<Rigidbody>().mass += 0.5f;
            }
        }
      
    }

    private void SendNetworkUnreliableMessage(Message m, int connectionID)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(hostId, connectionID, unReliableChannel, buffer, buffer.Length, out error);
    }
    private void SendNetworkReliableMessage(Message m, int connectionID)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(hostId, connectionID, reliableChannel, buffer, buffer.Length, out error);
    }
    private byte[] SerializeMessage(Message m)
    {
        MemoryStream stream = new MemoryStream();
        binFormater.Serialize(stream, m);

        return stream.GetBuffer();
    }
    private void SendPosition(Character p)
    {
        TransformMessage m = new TransformMessage(p.ClientId);
        m.Position.Vect3 = p.CharacterObj.transform.position;
        m.Scale.Vect3 = p.CharacterObj.transform.localScale;
        m.Rotation.Quaternion = p.CharacterObj.transform.rotation;
        SendNetworkUnreliableMessage(m, p.ClientId);
    }
    public static float ServerTime
    {
        get { return serverTime; }
        set { serverTime = value; }
    }

}
