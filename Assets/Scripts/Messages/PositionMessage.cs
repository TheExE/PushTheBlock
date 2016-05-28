using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class PositionMessage : ConnectionId, Message
{
    private SerializableVector3 position;

    public PositionMessage(int connectionId)
    { 
        position = new SerializableVector3();
        connectionID = connectionId;
    }

    public SerializableVector3 Position
    {
        get { return position; }
    }


    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Position;
    }

    public int GetConnectionId()
    {
        return connectionID;
    }
}
