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
                    Debug.Log("I AM HERE !");
                    var a = Instantiate(player) as GameObject;
                    a.transform.parent = transform;
                    allPlayers.Add(new Player(a, connectionId));
                }
              
                break;

            case NetworkEventType.DataEvent:       //3

               /** Stream stream = new MemoryStream(recBuffer);
                Message message = (Message)binFormater.Deserialize(stream);*/
                
                break;

            case NetworkEventType.DisconnectEvent: //4

                Player p = allPlayers.Find(it => it.ConnectionId == connectionId);
                Destroy(p.PlayerCharacterObj);
                allPlayers.Remove(p);

                break;
        }

        sendPositionTimer += Time.deltaTime;
        if(sendPositionTimer > 2f)
        {
            foreach(Player p in allPlayers)
            {
                PositionMessage m = new PositionMessage(p.ConnectionId);
                m.Position.Vect3 = p.PlayerCharacterObj.transform.position;
                SendNetworkMessage(m, p.ConnectionId);

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
}
