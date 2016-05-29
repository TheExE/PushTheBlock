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
    private Vector2 touchStartPosition;

    void Start()
    {
        /* Set up connection stuff */
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);

        socketId = NetworkTransport.AddHost(topology, Server.PORT + id);
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "192.168.56.1", Server.PORT, 0, out error);
        id++;

        /* Create player */
        player = Instantiate(playerPrefab) as GameObject;
        player.transform.parent = transform;
        playerBody = GetComponentInChildren<Rigidbody>();
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
            PositionMessage m = new PositionMessage(connectionId);
            m.Position.Vect3 = transform.position;
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
                switch (message.GetNetworkMessageType())
                {
                    case NetworkMessageType.Input:
                        break;

                    case NetworkMessageType.Position:
                        PositionMessage m = message as PositionMessage;
                        if (m.ConnectionID == connectionId)
                        {
                            player.transform.position = m.Position.Vect3;
                        }
                        else
                        {
                            int existIndex = otherPlayers.FindIndex(it => it.ConnectionId == m.ConnectionID);
                            if (existIndex > -1)
                            {
                                otherPlayers[existIndex].PlayerCharacterObj.transform.position = m.Position.Vect3;
                            }
                            else
                            {
                                GameObject other = Instantiate(playerPrefab) as GameObject;
                                other.transform.position = m.Position.Vect3;
                                otherPlayers.Add(new Player(other, m.ConnectionID));
                            }
                        }
                        break;
                }

                break;

            case NetworkEventType.DisconnectEvent: //4
                Application.Quit();
                break;
        }
    }
    private void HandleInput()
    {
        List<InputType> inputs = new List<InputType>();
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveFoward();
            inputs.Add(InputType.MoveForward);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveBack();
            inputs.Add(InputType.MoveBack);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
            inputs.Add(InputType.MoveLeft);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
            inputs.Add(InputType.MoveRight);
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                    float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, touchStartPosition.y, 0)).magnitude;
                    float swipeDistHorizontal = (new Vector3(touch.position.x, 0, 0) - new Vector3(touchStartPosition.x, 0, 0)).magnitude;
                    Debug.Log("vertical:" + swipeDistVertical);
                    Debug.Log("horizontal:" + swipeDistHorizontal);

                    if (swipeDistVertical > swipeDistHorizontal && swipeDistVertical > 0.1f)
                    {
                        float swipeValue = Mathf.Sign(touch.position.y - touchStartPosition.y);
                        if (swipeValue > 0)//up swipe
                        {
                            MoveFoward();
                            inputs.Add(InputType.MoveForward);
                        }
                        else if (swipeValue < 0)//down swipe
                        {
                            MoveBack();
                            inputs.Add(InputType.MoveBack);
                        }
                    }
                    else if (swipeDistHorizontal > 0.1f)
                    {
                        float swipeValue = Mathf.Sign(touch.position.x - touchStartPosition.x);
                        if (swipeValue > 0)//right swipe
                        {
                            MoveRight();
                            inputs.Add(InputType.MoveRight);
                        }
                        else if (swipeValue < 0)//left swipe
                        {
                            MoveLeft();
                            inputs.Add(InputType.MoveLeft);
                        }
                    }
                    break;
            }
        }


        if (inputs.Count > 0)
        {
            InputMessage m = new InputMessage(connectionId, inputs.ToArray());
            SendNetworkMessage(m, connectionId);
        }

        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
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
    private void MoveLeft()
    {
        playerBody.AddForce(Vector3.left * GameConsts.MOVE_SPEED * Time.deltaTime);
    }
    private void MoveRight()
    {
        playerBody.AddForce(Vector3.right * GameConsts.MOVE_SPEED * Time.deltaTime);
    }
    public void MoveBack()
    {
        playerBody.AddForce(Vector3.back * GameConsts.MOVE_SPEED * Time.deltaTime);
    }
    public void MoveFoward()
    {
        playerBody.AddForce(Vector3.forward * GameConsts.MOVE_SPEED * Time.deltaTime);
    }
}
