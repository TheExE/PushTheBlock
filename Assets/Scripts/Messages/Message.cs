using UnityEngine;
using System;
using System.Collections;


public interface Message
{
    NetworkMessageType GetNetworkMessageType();
    int GetConnectionId();
}
