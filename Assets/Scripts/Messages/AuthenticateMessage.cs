﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class AuthenticateMessage : ConnectionId, Message
{
    private int clientId;

    public AuthenticateMessage(int receiverId, int clientId)
    {
        this.receiverId = receiverId;
        this.clientId = clientId;
    }

    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Authenticate;
    }

    public int GetReceiverId()
    {
       return receiverId;
    }

    public int ClientId
    {
        get { return clientId; }
    }
}
