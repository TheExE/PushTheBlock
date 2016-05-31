using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class InputMessage : ConnectionId, Message
{
    private InputType[] inputType;

    public InputMessage(int receiverId, InputType[] inputType)
    {
        this.inputType = inputType;
        this.receiverid = receiverId;
    }

    public int GetReceiverId()
    {
        return receiverid;
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
