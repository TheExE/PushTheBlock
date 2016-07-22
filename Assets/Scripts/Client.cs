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
    private List<Character> allPlayers = new List<Character>();
    private float sendPositionTimer = 0;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private Rigidbody playerBody;
    private Vector2 touchStartPosition;
    private bool isClientsCharacterCreated = false;
    private int clientId;
    private List<OtherPlayerPositionInterpolation> otherPlayerPosInterpolation
        = new List<OtherPlayerPositionInterpolation>();

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
        if (isClientsCharacterCreated)
        {
            HandleInput();
            SendData();

            if (playerBody.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
            {
                playerBody.velocity = playerBody.velocity.normalized * GameConsts.MAX_MOVE_SPEED;
            }
        }

    }
    private void SendData()
    {
        sendPositionTimer += Time.deltaTime;
        if (sendPositionTimer > 0.5f)
        {
            TransformMessage m = new TransformMessage(clientId);
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


                break;

            case NetworkEventType.DisconnectEvent:
                Application.Quit();
                break;
        }

        HandleOtherPlayerInterpolation();
    }
    private void HandleOtherPlayerInterpolation()
    {
        foreach (OtherPlayerPositionInterpolation oIntrPos in otherPlayerPosInterpolation)
        {
            if (oIntrPos.IsReadyToInterPol)
            {
                var otherPlayerPos = allPlayers[oIntrPos.PlayerIndex].PlayerCharacterObj.transform.position;
                allPlayers[oIntrPos.PlayerIndex].PlayerCharacterObj.transform.position = oIntrPos.
                   Interpolate(otherPlayerPos);
            }
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
            if (Input.GetKey(KeyCode.Escape))
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
    private Character GetPlayerWithId(int clientId)
    {
        Character keyPlayer = null;
        foreach (Character p in allPlayers)
        {
            if (p.ConnectionId == clientId)
            {
                keyPlayer = p;
                break;
            }
        }
        return keyPlayer;
    }
    public int ClientId
    {
        get { return clientId; }
    }
    public string ClientTitle
    {
        get { return text.text; }
    }
    public void InitClient(int clientId, string clientTitle)
    {
        this.clientId = clientId;
        text.text = clientTitle;
    }
    public bool IsClientsCharacterCreated
    {
        get { return isClientsCharacterCreated; }
    }
    public void CreateCharacter(TransformMessage mT)
    {
        player = Instantiate(playerPrefab) as GameObject;
        player.transform.parent = transform;
        playerBody = GetComponentInChildren<Rigidbody>();
        GetComponentInChildren<Renderer>().material.color = Color.red;
        Character p = new Character(player, clientId);
        allPlayers.Add(p);
        isClientsCharacterCreated = true;

        UpdateCharactersPosition(mT);
    }
    public void UpdateCharactersPosition(TransformMessage mT)
    {
        player.transform.position = mT.Position.Vect3;
        player.transform.localScale = mT.Scale.Vect3;
        player.transform.rotation = mT.Rotation.Quaternion;
    }


}
