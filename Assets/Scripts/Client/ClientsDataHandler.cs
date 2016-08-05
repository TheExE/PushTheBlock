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
                if (!client.IsClientsCharacterCreated)
                {
                    client.CreateClientsCharacter(messageTransform);
                }
                else
                {
                    client.UpdateCharactersPosition(messageTransform);
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
}
