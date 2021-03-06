﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class InputMessage : ConnectionId, Message
{
    private static int uniqueId = 0;
    private int msgId;
    private InputType[] inputType;
    private DateTime timeStamp;

    public InputMessage(int receiverId, InputType[] inputType, DateTime curTime)
    {
        this.timeStamp = curTime;
        this.inputType = inputType;
        this.receiverId = receiverId;
        if (msgId > GameConsts.MAX_QUEUED_MSGES)
        {
            uniqueId = 0;
        }
        uniqueId++;
        msgId = uniqueId;
    }

    public int GetReceiverId()
    {
        return receiverId;
    }

    public InputType[] InputTypeMsg
    {
        get { return inputType; }
    }

    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.Input;
    }

    public int RequestId
    {
        get { return msgId; }
    }
    public DateTime TimeStamp
    {
        get { return timeStamp; }
    }
}
