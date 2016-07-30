using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

public class ServerNetworkManager
{
    private int reliableChannel;
    private int unReliableChannel;
    private int hostId;
    private BinaryFormatter binFormater = new BinaryFormatter();
    private ServerClientDataManager clientDataManager;
    private float sendPositionTimer = 0f;

    public ServerNetworkManager(ServerClientDataManager clientDataManager)
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);
        hostId = NetworkTransport.AddHost(topology, GameConsts.SERVER_PORT);
        this.clientDataManager = clientDataManager;
    }

    public void Update(Server server)
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
            case NetworkEventType.ConnectEvent:

                if (!clientDataManager.IsPlayerAlreadySpawned(connectionId))
                {
                    /* Create player */
                    GameObject characterObj = server.SpawnClientsCharacter(connectionId);
                    clientDataManager.CreateCharacter(characterObj, connectionId);

                    /* Identify the player */
                    AuthenticateMessage mA = new AuthenticateMessage(connectionId, connectionId);
                    SendNetworkReliableMessage(mA, connectionId);

                    /* Send player his position */
                    SendPositionToNewelyCreatedCharacter(server, connectionId);

                    /* Send Info about other player positions */
                    SendClientAllOtherCharacterPositions(connectionId);
                }

                break;

            case NetworkEventType.DataEvent:

                Stream stream = new MemoryStream(recBuffer);
                Message message = (Message)binFormater.Deserialize(stream);
                clientDataManager.HandlePlayerMessagesData(this, message);

                break;

            case NetworkEventType.DisconnectEvent:

                GameObject characterToRemove = clientDataManager.RemoveCharacterFromWorld(connectionId);
                server.DeleteGameObject(characterToRemove);
                clientDataManager.InformAllClientAboutCharacterDisconnect(this, connectionId);

                break;
        }

        /* Inform all clients about other client character positions */
        sendPositionTimer += Time.deltaTime;
        if (sendPositionTimer > GameConsts.TTW_FOR_OTHER_CHAR_POS_UPDATE)
        {
            clientDataManager.SendAllPlayersAllOtherPlayerPostions(this);
            sendPositionTimer = 0f;
        }
    }
    public void SendPosition(Character p)
    {
        TransformMessage m = new TransformMessage(p.ClientId);
        m.Position.Vect3 = p.CharacterObj.transform.position;
        m.Scale.Vect3 = p.CharacterObj.transform.localScale;
        m.Rotation.Quaternion = p.CharacterObj.transform.rotation;
        SendNetworkUnreliableMessage(m, p.ClientId);
    }
    public void SendNetworkReliableMessage(Message m, int connectionID)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(hostId, connectionID, reliableChannel, buffer, buffer.Length, out error);
    }
    public void SendNetworkUnreliableMessage(Message m, int connectionID)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(hostId, connectionID, unReliableChannel, buffer, buffer.Length, out error);
    }

    private void SendPositionToNewelyCreatedCharacter(Server server, int connectionId)
    {
        TransformMessage mP = new TransformMessage(connectionId);
        Ray r = new Ray();
        r.direction = Vector3.down;
        r.origin = server.transform.position;
        RaycastHit hitInfo = new RaycastHit();
        Vector3 result;
        if (Physics.Raycast(server.transform.position + (Vector3.down * 4), Vector3.down, out hitInfo) && 
            hitInfo.collider.gameObject.tag == "Player")
        {
           result = new Vector3(5f, 5f);
        }
        else
        {
            result = new Vector3(0f, 5f);
        }

        mP.Position.Vect3 = result;
        mP.Scale.Vect3 = new Vector3(1f, 1f, 1f);
        mP.Rotation.Quaternion = server.transform.rotation;
        SendNetworkReliableMessage(mP, connectionId);
    }
    private void SendClientAllOtherCharacterPositions(int connectionId)
    {
        clientDataManager.SendClientPosInfoOfAllOtherPlayers(this, connectionId);
    }
    private byte[] SerializeMessage(Message m)
    {
        MemoryStream stream = new MemoryStream();
        binFormater.Serialize(stream, m);

        return stream.GetBuffer();
    }

}
