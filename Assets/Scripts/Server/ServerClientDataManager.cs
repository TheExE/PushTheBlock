using UnityEngine;
using System.Collections.Generic;

public class ServerClientDataManager
{
    private List<Character> allCharacters = new List<Character>();


    public void Update()
    {
        UpdatePlayers();
    }
    public void HandlePlayerMessagesData(ServerNetworkManager servNetworkManager, Message message)
    {
        switch (message.GetNetworkMessageType())
        {
            case NetworkMessageType.Input:
                HandlePlayerInput(message as InputMessage);
                break;

            case NetworkMessageType.Transform:

                TransformMessage mP = message as TransformMessage;
                Character keyPlayer = allCharacters.Find(it => it.ClientId == mP.ReceiverId);
                Vector3 playerPosition = keyPlayer.CharacterObj.transform.position;
                if ((playerPosition - mP.Position.Vect3).sqrMagnitude > 2)
                {
                    servNetworkManager.SendPosition(keyPlayer);
                }

                break;
        }
    }
    public void SendClientPosInfoOfAllOtherPlayers(ServerNetworkManager servNetworkManager, int connectionId)
    {
        /* Send client all TransformMessages about all other player characters */

        List<TransformMessage> allCharcterPositions = new List<TransformMessage>();
        foreach (Character playerCharacter in allCharacters)
        {
            if (playerCharacter.ClientId != connectionId)
            {
                TransformMessage msg = new TransformMessage(playerCharacter.ClientId);
                msg.Position.Vect3 = playerCharacter.CharacterObj.transform.position;
                msg.Rotation.Quaternion = playerCharacter.CharacterObj.transform.rotation;
                msg.Scale.Vect3 = playerCharacter.CharacterObj.transform.localScale;
                allCharcterPositions.Add(msg);
            }

        }
        MultipleTranformMessage multipleTransfMsg = new MultipleTranformMessage(allCharcterPositions.ToArray());
        servNetworkManager.SendNetworkReliableMessage(multipleTransfMsg, connectionId);
    }
    public void HandlePlayerInput(InputMessage msgInput)
    {
        Rigidbody playerBody = allCharacters.Find(it => it.ClientId == msgInput.ReceiverId)
                 .CharacterObj.GetComponent<Rigidbody>();

        foreach (InputType type in msgInput.InputTypeMsg)
        {
            switch (type)
            {
                case InputType.MoveBack:
                    playerBody.AddForce(Vector3.back * GameConsts.MOVE_SPEED * GameConsts.SERVER_DELTA_TIME);
                    break;

                case InputType.MoveForward:
                    playerBody.AddForce(Vector3.forward * GameConsts.MOVE_SPEED * GameConsts.SERVER_DELTA_TIME);
                    break;

                case InputType.MoveLeft:
                    playerBody.AddForce(Vector3.left * GameConsts.MOVE_SPEED * GameConsts.SERVER_DELTA_TIME);
                    break;

                case InputType.MoveRight:
                    playerBody.AddForce(Vector3.right * GameConsts.MOVE_SPEED * GameConsts.SERVER_DELTA_TIME);
                    break;
            }
        }
    }
    public bool IsPlayerAlreadySpawned(int connectionId)
    {
        return allCharacters.Exists(it => it.ClientId == connectionId);
    }
    public void CreateCharacter(GameObject charcterObj, int connectionId)
    {
        allCharacters.Add(new Character(charcterObj, connectionId));
    }
    public GameObject RemoveCharacterFromWorld(int connectionId)
    {
        Character character = allCharacters.Find(it => it.ClientId == connectionId);
        allCharacters.Remove(character);
        return character.CharacterObj;
    }
    public void InformAllClientAboutCharacterDisconnect(ServerNetworkManager servNetworkManager, int connectionId)
    {
        DisconnectMessage m = new DisconnectMessage(connectionId);
        foreach (Character player in allCharacters)
        {
            servNetworkManager.SendNetworkReliableMessage(m, player.ClientId);
        }
    }
    public void SendAllPlayersAllOtherPlayerPostions(ServerNetworkManager servNetworkManger)
    {
        foreach (Character character in allCharacters)
        {
            Vector3 playerPos = character.CharacterObj.transform.position;
            character.LastSentPosition = new Vector3(playerPos.x, playerPos.y, playerPos.z);

            /* Send inform this characters client about all other characters positions */
            List<Character> allOtherCharacters = allCharacters.FindAll(it => it.ClientId != character.ClientId);
            List<TransformMessage> allTranformMsg = new List<TransformMessage>();
            foreach (Character otherCharacter in allOtherCharacters)
            {
                var otherPPos = otherCharacter.CharacterObj.transform.position;
                if (otherPPos != otherCharacter.LastSentPosition)
                {
                    TransformMessage transfMsg = new TransformMessage(otherCharacter.ClientId);
                    transfMsg.Position.Vect3 = otherCharacter.CharacterObj.transform.position;
                    transfMsg.Scale.Vect3 = otherCharacter.CharacterObj.transform.localScale;
                    transfMsg.Rotation.Quaternion = otherCharacter.CharacterObj.transform.rotation;
                    allTranformMsg.Add(transfMsg);
                    otherCharacter.LastSentPosition = new Vector3(transfMsg.Position.X, transfMsg.Position.Y, transfMsg.Position.Z);
                }
            }
            MultipleTranformMessage multipleTransfMsg = new MultipleTranformMessage(allTranformMsg.ToArray());
            servNetworkManger.SendNetworkUnreliableMessage(multipleTransfMsg, character.ClientId);
        }
    }

    private void UpdatePlayers()
    {
        foreach (Character p in allCharacters)
        {
            /* Cap player move speed */
            Rigidbody rg = p.CharacterObj.GetComponent<Rigidbody>();
            if (rg.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
            {
                rg.velocity = rg.velocity.normalized * GameConsts.MAX_MOVE_SPEED;
            }

            /* Check for players pushing other off the ledge */
            int winnerId = p.Update();
            if (winnerId != -1)
            {
                Character winner = allCharacters.Find(it => it.ClientId == winnerId);
                winner.CharacterObj.GetComponent<Transform>().localScale += new Vector3(0.5f, 0.5f, 0.5f);
                winner.CharacterObj.GetComponent<Rigidbody>().mass += 0.5f;
            }
        }
    }

}
