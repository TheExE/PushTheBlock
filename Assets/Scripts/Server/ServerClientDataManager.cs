using UnityEngine;
using System.Collections.Generic;

public class ServerClientDataManager
{
	private List<Character> allCharacters = new List<Character>();


	public void HandlePlayerMessagesData(ServerNetworkManager servNetworkManager, Message message)
	{
		switch (message.GetNetworkMessageType())
		{
			case NetworkMessageType.Input:

				HandlePlayerInput(servNetworkManager, message as InputMessage);
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

	public void HandlePlayerInput(ServerNetworkManager servNetworkManager, InputMessage msgInput)
	{
		Transform characterTransf = allCharacters.Find(it => it.ClientId == msgInput.ReceiverId)
				 .CharacterObj.GetComponent<Transform>();
		float deltaTime = Time.deltaTime + (System.DateTime.Now.Subtract(msgInput.TimeStamp).Milliseconds / 1000f);
		foreach (InputType type in msgInput.InputTypeMsg)
		{

			switch (type)
			{
				case InputType.MoveBack:

					characterTransf.position += GetVelocityToMoveBack(deltaTime);
					break;

				case InputType.MoveForward:

					characterTransf.position += GetVelocityToMoveForward(deltaTime);
					break;

				case InputType.MoveLeft:

					characterTransf.position += GetVelocityToMoveLeft(deltaTime);
					break;

				case InputType.MoveRight:

					characterTransf.position += GetVelocityToMoveRight(deltaTime);
					break;


				case InputType.MoveUp:

					characterTransf.position += GetVelocityToMoveUp(deltaTime);
					break;

				case InputType.MoveDown:

					characterTransf.position += GetVelocityToMoveDown(deltaTime);
					break;
			}
		}


		TransformMessage tranformMsg = new TransformMessage(msgInput.ReceiverId);
		tranformMsg.AcknowledgmentId = msgInput.RequestId;
		tranformMsg.Position.Vect3 = characterTransf.position;
		servNetworkManager.SendNetworkUnreliableMessage(tranformMsg, msgInput.ReceiverId);
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

	public void InformAllClientAboutCharacterDisconnect(ServerNetworkManager servNetworkManager,
		int connectionId)
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
				TransformMessage transfMsg = new TransformMessage(otherCharacter.ClientId);
				transfMsg.Position.Vect3 = otherCharacter.CharacterObj.transform.position;
				transfMsg.Scale.Vect3 = otherCharacter.CharacterObj.transform.localScale;
				transfMsg.Rotation.Quaternion = otherCharacter.CharacterObj.transform.rotation;
				allTranformMsg.Add(transfMsg);
				otherCharacter.LastSentPosition = new Vector3(transfMsg.Position.X, transfMsg.Position.Y, transfMsg.Position.Z);
			}
			MultipleTranformMessage multipleTransfMsg = new MultipleTranformMessage(allTranformMsg.ToArray());
			servNetworkManger.SendNetworkUnreliableMessage(multipleTransfMsg, character.ClientId);
		}
	}

	private Vector3 GetVelocityToMoveLeft(float deltaTime)
	{
		return Vector3.left * GameConsts.MOVE_SPEED * deltaTime;
	}

	private Vector3 GetVelocityToMoveRight(float deltaTime)
	{
		return Vector3.right * GameConsts.MOVE_SPEED * deltaTime;
	}

	private Vector3 GetVelocityToMoveBack(float deltaTime)
	{
		return Vector3.back * GameConsts.MOVE_SPEED * deltaTime;
	}

	private Vector3 GetVelocityToMoveForward(float deltaTime)
	{
		return Vector3.forward * GameConsts.MOVE_SPEED * deltaTime;
	}
	private Vector3 GetVelocityToMoveUp(float deltaTime)
	{
		return Vector3.up * GameConsts.MOVE_SPEED * deltaTime;
	}

	private Vector3 GetVelocityToMoveDown(float deltaTime)
	{
		return Vector3.down * GameConsts.MOVE_SPEED * deltaTime;
	}
}
