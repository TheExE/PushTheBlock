using UnityEngine;
using System.Collections;

public class ClientsDataHandler
{
    private Client client;
    private OtherPlayerManager otherPlayerCharManager;

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
                TransformMessage messageTransform = msg as TransformMessage;
                /* Is this message about me or some other people */
                if (client.ClientId == messageTransform.ReceiverId)
                {
                    if (!client.IsClientsCharacterCreated)
                    {
                        client.CreateCharacter(messageTransform);
                    }
                    else
                    {
                        client.UpdateCharactersPosition(messageTransform);
                    }
                }
                else
                {
                    /* Update other player characters */
                    otherPlayerCharManager.UpdateOtherCharPosition(messageTransform);
                }
                break;

            case NetworkMessageType.Disconnect:
                otherPlayerCharManager.DespawnDisconnectedPlayer(msg as DisconnectMessage);
                break;
        }
    }
}
