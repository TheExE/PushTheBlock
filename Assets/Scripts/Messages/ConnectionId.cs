using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ConnectionId
{
    protected int receiverId;

    public int ReceiverId
    {
        get { return receiverId; }
    }
}
