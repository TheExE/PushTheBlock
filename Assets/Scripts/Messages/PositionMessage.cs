using UnityEngine;
using System.Collections;
using System;

public class PositionMessage : Message
{
    private SerializableVector3 position;
    private int connectionId;

    public PositionMessage(int connectionId)
    {
        position = new SerializableVector3();
        this.connectionId = connectionId;
    }

    public Vector3 Position
    {
        get { return position.Vect3; }
        set { position.Vect3 = value; }
    }

    public int ConnectionId
    {
        get { return connectionId; }
    }

    MessageType Message.GetType()
    {
        return MessageType.Position;
    }

    public int GetConnectionId()
    {
        return connectionId;
    }
}
