using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ConnectionId
{
    protected int receiverid;

    public int ReceiverId
    {
        get { return receiverid; }
    }
}
