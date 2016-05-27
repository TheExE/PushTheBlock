using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
    public GameObject playerPrefab;

    private static int id = 1;
    private int socketId;
    private int reliableChannel;
    private int unReliableChannel;
    private int connectionId;
    private GameObject player;
    private List<Player> otherPlayers = new List<Player>();
    private float sendPositionTimer = 0;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private Rigidbody playerBody;

    void Start()
    {
        playerBody = GetComponent<Rigidbody>();
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);

        socketId = NetworkTransport.AddHost(topology, Server.PORT + id);
        Debug.Log("client Socket Open. SocketId is: " + socketId);
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", Server.PORT, 0, out error);
        id++;
        player = Instantiate(playerPrefab) as GameObject;
    }


    void Update()
    {
        HandleInput();
        ReceiveData();
        SendData();
    }

    private void SendData()
    {
        sendPositionTimer += Time.deltaTime;
        if (sendPositionTimer > 1)
        {
            Message m = new Message(connectionId);
            m.Position = transform.position;
            SendNetworkMessage(m, connectionId);
            sendPositionTimer = 0f;
        }
    }
    private void ReceiveData()
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
                break;

            case NetworkEventType.DataEvent:       //3

                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                Message message = (Message)formatter.Deserialize(stream);

                if (message.ConnectionId == connectionId)
                {
                    player.transform.position = message.Position;
                }
                else
                {
                    int existIndex = otherPlayers.FindIndex(it => it.ConnectionId == message.ConnectionId);
                    if (existIndex > -1)
                    {
                        otherPlayers[existIndex].PlayerCharacterObj.transform.position = message.Position;
                    }
                    else
                    {
                        GameObject other = Instantiate(playerPrefab) as GameObject;
                        other.transform.position = message.Position;
                        otherPlayers.Add(new Player(other, message.ConnectionId));
                    }
                }

                break;

            case NetworkEventType.DisconnectEvent: //4
                break;
        }
    }
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerBody.AddForce(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            playerBody.AddForce(Vector3.down);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            playerBody.AddForce(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            playerBody.AddForce(Vector3.right);
        }
    }
    private void SendNetworkMessage(Message m, int connectionID)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(socketId, connectionID, unReliableChannel, buffer, buffer.Length, out error);
    }
    private byte[] SerializeMessage(Message m)
    {
        MemoryStream stream = new MemoryStream();
        binFormater.Serialize(stream, m);

        return stream.GetBuffer();
    }
}
