using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public class Server : MonoBehaviour
{
    public static int PORT = 9991;
    public GameObject player;

    private List<Player> allPlayers = new List<Player>();
    private int reliableChannel;
    private int unReliableChannel;
    private int hostId;
    private float sendPositionTimer = 0f;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private StayOnPlain sticktoPlain;
    
	void Start ()
    {
        Application.runInBackground = true;
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);
        hostId = NetworkTransport.AddHost(topology, PORT);
        // NetworkTransport.AddWebsocketHost(topology, PORT, null);

        sticktoPlain = new StayOnPlain(allPlayers);
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
            case NetworkEventType.Nothing:         //1
                break;

            case NetworkEventType.ConnectEvent:    //2

                if(!allPlayers.Exists(it => it.ConnectionId == connectionId))
                {
                    var a = Instantiate(player) as GameObject;
                    a.transform.parent = transform;
                    allPlayers.Add(new Player(a, connectionId));
                }
              
                break;

            case NetworkEventType.DataEvent:       //3

                Stream stream = new MemoryStream(recBuffer);
                Message message = (Message)binFormater.Deserialize(stream);

                switch (message.GetNetworkMessageType())
                {
                    case NetworkMessageType.Input:
                        InputMessage m = message as InputMessage;
                        Rigidbody playerBody = allPlayers.Find(it => it.ConnectionId == m.ConnectionID)
                            .PlayerCharacterObj.GetComponent<Rigidbody>();
                        foreach(InputType type in m.InputTypeMsg)
                        {
                            switch (type)
                            {
                                case InputType.MoveBack:
                                    playerBody.AddForce(Vector3.back * GameConsts.MOVE_SPEED);
                                    break;

                                case InputType.MoveForward:
                                    playerBody.AddForce(Vector3.forward * GameConsts.MOVE_SPEED);
                                    break;

                                case InputType.MoveLeft:
                                    playerBody.AddForce(Vector3.left * GameConsts.MOVE_SPEED);
                                    break;

                                case InputType.MoveRight:
                                    playerBody.AddForce(Vector3.right * GameConsts.MOVE_SPEED);
                                    break;
                            }

                        }

                        break;

                    case NetworkMessageType.Position:

                        PositionMessage mP = message as PositionMessage;
                        Player keyPlayer = allPlayers.Find(it => it.ConnectionId == mP.ConnectionID);
                        Vector3 playerPosition = keyPlayer.PlayerCharacterObj.transform.position;
                        if((playerPosition - mP.Position.Vect3).sqrMagnitude > 4)
                        {
                            SendPosition(keyPlayer);
                        }

                        break;
                }

                
                break;

            case NetworkEventType.DisconnectEvent: //4

                Player p = allPlayers.Find(it => it.ConnectionId == connectionId);
                Destroy(p.PlayerCharacterObj);
                allPlayers.Remove(p);

                break;
        }

        sendPositionTimer += Time.deltaTime;
        if(sendPositionTimer > 0.5f)
        {
            foreach(Player p in allPlayers)
            {
                /* Send all player positions that are with in radius */
                var allOtherPlayer = allPlayers.FindAll(it => it.ConnectionId != p.ConnectionId);
                foreach(Player otherP in allOtherPlayer)
                {
                    PositionMessage mm = new PositionMessage(otherP.ConnectionId);
                    mm.Position.Vect3 = otherP.PlayerCharacterObj.transform.position;
                    SendNetworkMessage(mm, otherP.ConnectionId);
                }
            }
            sendPositionTimer = 0;
        }

        sticktoPlain.Update();
        foreach(Player p in allPlayers)
        {
            Rigidbody rg = p.PlayerCharacterObj.GetComponent<Rigidbody>();
            if (rg.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
            {
                rg.velocity = rg.velocity.normalized * GameConsts.MAX_MOVE_SPEED;
            }
        }
      
    }

    private void SendNetworkMessage(Message m, int connectionID)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(hostId, connectionID, unReliableChannel, buffer, buffer.Length, out error);
    }
    private byte[] SerializeMessage(Message m)
    {
        MemoryStream stream = new MemoryStream();
        binFormater.Serialize(stream, m);

        return stream.GetBuffer();
    }
    private void SendPosition(Player p)
    {
        PositionMessage m = new PositionMessage(p.ConnectionId);
        m.Position.Vect3 = p.PlayerCharacterObj.transform.position;
        SendNetworkMessage(m, p.ConnectionId);
    }
}
