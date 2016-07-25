using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public static GameObject charPrefab;
    public Text text;

    private int socketId;
    private int reliableChannel;
    private int unReliableChannel;
    private int connectionId;

    private InputHandler inputaHandler;
    private ClientsDataHandler clientsDataManager;
    private Character player;
    private float sendPositionTimer = 0;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private Rigidbody playerBody;
    private bool isClientsCharacterCreated = false;
    private int clientId;

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
        connectionId = NetworkTransport.Connect(socketId, "127.0.0.1", Server.PORT, 0, out error);
        text.text = "This is Client";

        clientsDataManager = new ClientsDataHandler(this);
    }


    void Update()
    {
        ReceiveData();
        if (isClientsCharacterCreated)
        {
            List<InputType> inputs = inputaHandler.Update();
            if(inputs.Count > 0)
            {
                //TODO: SEND INPUTS TO SERVER
            }
            SendData();

            /* CAP VELOCITY OF CLIENTS CHARACTER */
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
            m.Position.Vect3 = player.CharacterObj.GetComponent<Transform>().position;
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
        player = new Character(Instantiate(charPrefab) as GameObject, mT.ReceiverId);
        player.CharacterObj.transform.parent = transform;
        playerBody = GetComponentInChildren<Rigidbody>();
        inputaHandler = new InputHandler(playerBody);
        GetComponentInChildren<Renderer>().material.color = Color.red;
        isClientsCharacterCreated = true;

        UpdateCharactersPosition(mT);
    }
    public void UpdateCharactersPosition(TransformMessage transformMsg)
    {
        player.CharacterObj.transform.position = transformMsg.Position.Vect3;
        player.CharacterObj.transform.localScale = transformMsg.Scale.Vect3;
        player.CharacterObj.transform.rotation = transformMsg.Rotation.Quaternion;
    }
    public GameObject SpawnCharacter()
    {
        return Instantiate(charPrefab);
    }

}
