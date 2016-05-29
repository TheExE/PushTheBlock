using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class InputMessage : ConnectionId, Message
{
    private InputType[] inputType;

    public InputMessage(int connectionId, InputType[] inputType)
    {
        this.inputType = inputType;
        connectionID = connectionId;
    }

    public int GetConnectionId()
    {
        return connectionID;
    }

    public InputType[] InputTypeMsg
    {
        get { return inputType; }
    }

    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Input;
    }
}
