using UnityEngine;
using System;
using System.Collections;

public interface Message
{
    MessageType GetType();
    int GetConnectionId();  
}
