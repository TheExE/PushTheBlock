﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientsNetworkManager
{
    private BinaryFormatter binFormater = new BinaryFormatter();
    private int socketId;
    private int reliableChannel;
    private int unReliableChannel;
    private int connectionId;

    public ClientsNetworkManager()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannel = config.AddChannel(QosType.Reliable);
        unReliableChannel = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, 100);

        socketId = NetworkTransport.AddHost(topology);
        byte error;
        connectionId = NetworkTransport.Connect(socketId, GameConsts.SERVER_IP,
            GameConsts.SERVER_PORT, 0, out error);
    }


    public void Update(ClientsDataHandler clientsDataManager, Character playerChar, List<InputType> inputs)
    {
        /* Receives data from server */
        ReceiveData(clientsDataManager);

        /* Informs server about player inputs */
        if (playerChar != null && inputs.Count > 0)
        {
            InputMessage inputMsg = new InputMessage(playerChar.ClientId, inputs.ToArray(), System.DateTime.Now);
            SendRelibleMessage(inputMsg, connectionId);
            clientsDataManager.AddUnAcknowledgedMsg(inputMsg);
        }
    }

    private void SendCharPosition(Vector3 playerCharPos, int clientId)
    {
        TransformMessage m = new TransformMessage(clientId);
        m.Position.Vect3 = playerCharPos;
        SendUnreliableMessage(m, connectionId);
    }
    private void ReceiveData(ClientsDataHandler clientsDataManager)
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
                clientsDataManager.HandleIncomingMessage(message);
                break;

            case NetworkEventType.DisconnectEvent:

                Application.Quit();
                break;
        }


    }
    private void SendUnreliableMessage(Message m, int connectionId)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(socketId, connectionId, unReliableChannel, buffer, buffer.Length, out error);
        if (error != 0)
        {
            Debug.Log("Error: " + error);
        }
    }
    private void SendRelibleMessage(Message m, int connectionId)
    {
        byte[] buffer = SerializeMessage(m);
        byte error;
        NetworkTransport.Send(socketId, connectionId, reliableChannel, buffer, buffer.Length, out error);
        if (error != 0)
        {
            Debug.Log("Error: " + error);
        }
    }
    private byte[] SerializeMessage(Message m)
    {
        MemoryStream stream = new MemoryStream();
        binFormater.Serialize(stream, m);

        return stream.GetBuffer();
    }
}
