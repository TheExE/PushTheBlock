using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public GameObject playerPrefab;
    public Text text; 

    private int socketId;
    private int reliableChannel;
    private int unReliableChannel;
    private int connectionId;
    private GameObject player;
    private List<Player> allPlayers = new List<Player>();
    private float sendPositionTimer = 0;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private Rigidbody playerBody;
    private Vector2 touchStartPosition;
    private bool isPlayerCreated = false;
    private int clientId;
    private StayOnPlain stickToPlain;

    void Start()
    {
        Application.runInBackground = true;

        /* Set up connection stuff */
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);

        socketId = NetworkTransport.AddHost(topology);
        byte error;
        connectionId = NetworkTransport.Connect(socketId, "192.168.56.1", Server.PORT, 0, out error);
        text.text = "This is Client";
    }


    void Update()
    {
        ReceiveData();
        if(isPlayerCreated)
        {
            HandleInput();
            SendData();

            if (playerBody.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
            {
                playerBody.velocity = playerBody.velocity.normalized * GameConsts.MAX_MOVE_SPEED;
            }

            stickToPlain.Update();
        }

    }
    private void SendData()
    {
        sendPositionTimer += Time.deltaTime;
        if (sendPositionTimer > 0.5f)
        {
            PositionMessage m = new PositionMessage(clientId);
            m.Position.Vect3 = player.transform.position;
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
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                break;

            case NetworkEventType.DataEvent:

                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                Message message = (Message)formatter.Deserialize(stream);
                switch (message.GetNetworkMessageType())
                {

                    case NetworkMessageType.Authenticate:
                        AuthenticateMessage mA = message as AuthenticateMessage;
                        clientId = mA.ClientId;
                        text.text = "This is Client \n Id:" + clientId;
                        break;

                    case NetworkMessageType.Position:
                        PositionMessage m = message as PositionMessage;
                        if (m.ReceiverId == clientId)
                        {
                            if(!isPlayerCreated)
                            {
                                /* Create player */
                                player = Instantiate(playerPrefab) as GameObject;
                                player.transform.parent = transform;
                                playerBody = GetComponentInChildren<Rigidbody>();
                                GetComponentInChildren<Renderer>().material.color = Color.red;
                                isPlayerCreated = true;
                                Player p = new Player(player, clientId);
                                allPlayers.Add(p);
                                stickToPlain = new StayOnPlain(allPlayers);
                            }
                           
                            player.transform.position = new Vector3(m.Position.X, m.Position.Y, m.Position.Z);
                            player.transform.localScale = new Vector3(m.Scale.X, m.Scale.Y, m.Scale.Z);
                            player.transform.rotation = m.Rotation.Quaternion;
                        else
                        {
                            int existIndex = allPlayers.FindIndex(it => it.ConnectionId == m.ReceiverId);
                            if (existIndex > -1)
                            {
                                allPlayers[existIndex].PlayerCharacterObj.transform.position = m.Position.Vect3;
                                allPlayers[existIndex].PlayerCharacterObj.transform.localScale =
                                    new Vector3(m.Scale.X, m.Scale.Y, m.Scale.Z);
                                allPlayers[existIndex].PlayerCharacterObj.
                                    transform.rotation = m.Rotation.Quaternion;
                            }
                            else
                            {
                                GameObject other = Instantiate(playerPrefab) as GameObject;
                                other.transform.position = m.Position.Vect3;
                                other.transform.localScale = new Vector3(m.Scale.X, m.Scale.Y, m.Scale.Z);
                                other.transform.rotation = m.Rotation.Quaternion;
                                allPlayers.Add(new Player(other, m.ReceiverId));
                            }
                        }
                        break;

                    case NetworkMessageType.Disconnect:
                        Player pThatExitedGame = GetPlayerWithId(message.GetReceiverId());
                        if(pThatExitedGame != null)
                        {
                            Destroy(pThatExitedGame.PlayerCharacterObj);
                            allPlayers.Remove(pThatExitedGame);
                        }
                        break;
                }

                break;

            case NetworkEventType.DisconnectEvent:
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
            InputMessage m = new InputMessage(clientId, inputs.ToArray());
            SendNetworkMessage(m, connectionId);
        }

        if (Input.GetKey(KeyCode.Escape))
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
        playerBody.AddForce(Vector3.left * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    private void MoveRight()
    {
        playerBody.AddForce(Vector3.right * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    public void MoveBack()
    {
        playerBody.AddForce(Vector3.back * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    public void MoveFoward()
    {
        playerBody.AddForce(Vector3.forward * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    private Player GetPlayerWithId(int clientId)
    {
        Player keyPlayer = null;
        foreach(Player p in allPlayers)
        {
            if (p.ConnectionId == clientId)
            {
                keyPlayer = p;
                break;
            }
        }
        return keyPlayer;
    }
}
