using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class Server : MonoBehaviour
{
    public const int PORT = 9991;
    public GameObject player;

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

                if(!allPlayers.Exists(it => it.ConnectionId == connectionId))
                {
                    var a = Instantiate(player) as GameObject;
                    a.transform.parent = transform;
                    a.GetComponent<CollisionQueue>().ClientId = connectionId;
                    allPlayers.Add(new Character(a, connectionId));

                    AuthenticateMessage mA = new AuthenticateMessage(connectionId, connectionId);
                    SendNetworkReliableMessage(mA, connectionId);
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
                    SendNetworkUnreliableMessage(mP, connectionId);
                }
              
                break;

            case NetworkEventType.DataEvent:

                Stream stream = new MemoryStream(recBuffer);
                Message message = (Message)binFormater.Deserialize(stream);

                switch (message.GetNetworkMessageType())
                {
                    case NetworkMessageType.Input:
                        InputMessage mI = message as InputMessage;
                        Rigidbody playerBody = allPlayers.Find(it => it.ConnectionId == mI.ReceiverId)
                            .PlayerCharacterObj.GetComponent<Rigidbody>();
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
                        Character keyPlayer = allPlayers.Find(it => it.ConnectionId == mP.ReceiverId);
                        Vector3 playerPosition = keyPlayer.PlayerCharacterObj.transform.position;
                        if((playerPosition - mP.Position.Vect3).sqrMagnitude > 2)
                        {
                            SendPosition(keyPlayer);
                        }

                        break;
                }

                
                break;

            case NetworkEventType.DisconnectEvent: 

                Character p = allPlayers.Find(it => it.ConnectionId == connectionId);
                Destroy(p.PlayerCharacterObj);
                allPlayers.Remove(p);

                DisconnectMessage m = new DisconnectMessage(connectionId);
                foreach(Character player in allPlayers)
                {
                    SendNetworkReliableMessage(m, player.ConnectionId);
                }
                

                break;
        }

        sendPositionTimer += Time.deltaTime;
        if(sendPositionTimer > 0.5f)
        {
            foreach(Character p in allPlayers)
            {
                var playerPos = p.PlayerCharacterObj.transform.position;
                allPlayersLastSentPos.Add(new Vector3(playerPos.x, playerPos.y, playerPos.z));
                /* Send all player positions that are with in radius */
                var allOtherPlayer = allPlayers.FindAll(it => it.ConnectionId != p.ConnectionId);
                foreach(Character otherP in allOtherPlayer)
                {
                    var otherPPos = otherP.PlayerCharacterObj.transform.position;
                    if(otherPPos != otherP.LastSentPosition)
                    {
                        TransformMessage mm = new TransformMessage(otherP.ConnectionId);
                        mm.Position.Vect3 = otherP.PlayerCharacterObj.transform.position;
                        mm.Scale.Vect3 = otherP.PlayerCharacterObj.transform.localScale;
                        mm.Rotation.Quaternion = otherP.PlayerCharacterObj.transform.rotation;
                        SendNetworkUnreliableMessage(mm, p.ConnectionId);
                        otherP.LastSentPosition = new Vector3(mm.Position.X, mm.Position.Y, mm.Position.Z);
                    }
                }
            }
            sendPositionTimer = 0;
        }


        foreach(Character p in allPlayers)
        {
            /* Cap player move speed */
            Rigidbody rg = p.PlayerCharacterObj.GetComponent<Rigidbody>();
            if (rg.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
            {
                rg.velocity = rg.velocity.normalized * GameConsts.MAX_MOVE_SPEED;
            }

            /* Check for players pushing other off the ledge */
            int winnerId = p.Update();
            if (winnerId != -1)
            {
                Character winner = allPlayers.Find(it => it.ConnectionId == winnerId);
                winner.PlayerCharacterObj.GetComponent<Transform>().localScale += new Vector3(0.5f, 0.5f, 0.5f);
                winner.PlayerCharacterObj.GetComponent<Rigidbody>().mass += 0.5f;
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
        TransformMessage m = new TransformMessage(p.ConnectionId);
        m.Position.Vect3 = p.PlayerCharacterObj.transform.position;
        m.Scale.Vect3 = p.PlayerCharacterObj.transform.localScale;
        m.Rotation.Quaternion = p.PlayerCharacterObj.transform.rotation;
        SendNetworkUnreliableMessage(m, p.ConnectionId);
    }
    public static float ServerTime
    {
        get { return serverTime; }
        set { serverTime = value; }
    }

}
