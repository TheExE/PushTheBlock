using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class PositionMessage : ConnectionId, Message
{
    private SerializableVector3 position;

    public PositionMessage(int receiverId)
    { 
        position = new SerializableVector3();
        this.receiverid = receiverId;
    }

    public SerializableVector3 Position
    {
        get { return position; }
    }


    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Position;
    }

    public int GetReceiverId()
    {
        return receiverid;
    }
}
