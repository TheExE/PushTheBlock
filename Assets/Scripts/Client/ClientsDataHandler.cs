using UnityEngine;
using System.Collections;

public class ClientsDataHandler
{
    private Client client;

    public ClientsDataHandler(Client client)
    {
        this.client = client;
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
                TransformMessage m = msg as TransformMessage;
                /* Is this message about me or some other people */
                if (m.ReceiverId == m.ReceiverId)
                {
                    if (!client.IsClientsCharacterCreated)
                    {
                        client.CreateCharacter(m);
                    }
                    else
                    {
                        client.UpdateCharactersPosition(m);
                    }
                }
                else
                {
                    int existIndex = allPlayers.FindIndex(it => it.ConnectionId == m.ReceiverId);
                    if (existIndex > -1)
                    {
                        otherPlayerPosInterpolation.Find(it => it.ClientId == m.ReceiverId)
                             .AddPosition(m.Position.Vect3);
                        allPlayers[existIndex].PlayerCharacterObj.transform.localScale =
                            new Vector3(m.Scale.X, m.Scale.Y, m.Scale.Z);
                        allPlayers[existIndex].PlayerCharacterObj.
                            transform.rotation = m.Rotation.Quaternion;
                    }
                    else
                    {
                        var interPos = new OtherPlayerPositionInterpolation(m.ReceiverId, allPlayers.Count);
                        interPos.AddPosition(m.Position.Vect3);
                        otherPlayerPosInterpolation.Add(interPos);
                        GameObject other = Instantiate(playerPrefab) as GameObject;
                        other.transform.localScale = new Vector3(m.Scale.X, m.Scale.Y, m.Scale.Z);
                        other.transform.rotation = m.Rotation.Quaternion;
                        allPlayers.Add(new Character(other, m.ReceiverId));
                    }
                }
                break;

            case NetworkMessageType.Disconnect:
                Character pThatExitedGame = GetPlayerWithId(message.GetReceiverId());
                if (pThatExitedGame != null)
                {
                    Destroy(pThatExitedGame.PlayerCharacterObj);
                    allPlayers.Remove(pThatExitedGame);
                }
                break;
        }
    }
}
