using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TransformMessage : ConnectionId, Message
{
    private int acknowledgmnentId = -1;
    private SerializableVector3 position;
    private SerializableVector3 scale;
    private SerializableQuaternion rotation;

    public TransformMessage(int receiverId)
    { 
        position = new SerializableVector3();
        scale = new SerializableVector3();
        rotation = new SerializableQuaternion();
        this.receiverId = receiverId;
    }

    public SerializableVector3 Position
    {
        get { return position; }
    }
    public SerializableVector3 Scale
    {
        get { return scale; }
    }
    public SerializableQuaternion Rotation
    {
        get { return rotation; }
    }
    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Transform;
    }
    public int GetReceiverId()
    {
        return receiverId;
    }
    public int AcknowledgmentId
    {
        get { return acknowledgmnentId; }
        set { acknowledgmnentId = value; }
    }
}
