using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class MultipleTranformMessage : Message
{
    private TransformMessage[] allTransformMessages;

    public MultipleTranformMessage(TransformMessage[] allTransformMessages)
    {
        this.allTransformMessages = allTransformMessages;
    } 


    public NetworkMessageType GetNetworkMessageType()
    {
        return NetworkMessageType.MultipleTransforms;
    }

    public int GetReceiverId()
    {
        return -1;
    }

    public TransformMessage[] AllTransformMessages
    {
        get { return allTransformMessages; }
    }
}
