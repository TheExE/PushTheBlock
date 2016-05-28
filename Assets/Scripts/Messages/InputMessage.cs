using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class InputMessage : ConnectionId, Message
{
    public InputMessage(int connectionId)
    {
        connectionID = connectionId;
    }

    public int GetConnectionId()
    {
        return connectionID;
    }

    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Input;
    }
}
