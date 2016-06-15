using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class DisconnectMessage : ConnectionId, Message
{
    public DisconnectMessage(int receiverId)
    {
        this.receiverid = receiverId;
    }

    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Disconnect;
    }

    public int GetReceiverId()
    {
        return receiverid;
    }
}
