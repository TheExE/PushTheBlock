using UnityEngine;
using System.Collections.Generic;

public class ClientsDataHandler
{
    private Client client;
    private OtherPlayerManager otherPlayerCharManager;
    private List<InputMessage> unAcknowledgedInputRequests = new List<InputMessage>();
    private Vector3 lastAcknowledgedPosition = Vector3.zero;

    public ClientsDataHandler(Client client)
    {
        this.client = client;
        otherPlayerCharManager = new OtherPlayerManager(client);
    }


    public void Update()
    {
        otherPlayerCharManager.Update();
    }
    public void HandleIncomingMessage(Message msg)
    {
        switch (msg.GetNetworkMessageType())
        {
            case NetworkMessageType.Authenticate:
                AuthenticateMessage mA = msg as AuthenticateMessage;
                client.InitClient(mA.ClientId, "This is Client \n Id:" + mA.ClientId);
                break;

            case NetworkMessageType.Transform:

                TransformMessage transformMsg = msg as TransformMessage;
                if (!client.IsClientsCharacterCreated)
                {
                    client.CreateClientsCharacter(transformMsg);
                }
                else
                {
                    if(transformMsg.AcknowledgmentId != -1)
                    {
                        int acknoledgmentIdx = unAcknowledgedInputRequests.
                        FindIndex(it => it.RequestId == transformMsg.AcknowledgmentId);
                        if (acknoledgmentIdx != -1)
                        {
                            /* Removes this unacknowledged message and all that came before */
                            unAcknowledgedInputRequests.RemoveRange(0, acknoledgmentIdx + 1);

                            /* Peform position predition based on last acknowledged position */
                            lastAcknowledgedPosition = transformMsg.Position.Vect3;
                            Vector3 newPosBasedOnAck = new Vector3(lastAcknowledgedPosition.x,
                                lastAcknowledgedPosition.y, lastAcknowledgedPosition.z);
                            foreach (InputMessage inputMsg in unAcknowledgedInputRequests)
                            {
                                newPosBasedOnAck += client.GetPositionChangeBasedOnInput(inputMsg);
                            }
                            client.UpdateCharactersPosition(newPosBasedOnAck);
                        }
                    }
                    else
                    {
                        client.UpdateCharactersTransform(transformMsg);
                    }
                }
                
                break;

            case NetworkMessageType.MultipleTransforms:

                MultipleTranformMessage multipleTransfMsg = msg as MultipleTranformMessage;
                /* Update other player characters */
                otherPlayerCharManager.UpdateOtherCharPosition(multipleTransfMsg);

                break;

            case NetworkMessageType.Disconnect:
                otherPlayerCharManager.DespawnDisconnectedPlayer(msg as DisconnectMessage);
                break;
        }
    }
    public void AddUnAcknowledgedMsg(InputMessage msg)
    {
        unAcknowledgedInputRequests.Add(msg);
    }
}
