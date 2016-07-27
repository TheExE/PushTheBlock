using UnityEngine;
using System.Collections.Generic;

public class OtherPlayerManager
{
    private Client client;
    private List<OtherPlayerCharacter> allOtherPlayers = new List<OtherPlayerCharacter>();

    public OtherPlayerManager(Client client)
    {
        this.client = client;
    }

    public void Update()
    {
        HandleOtherPlayerInterpolation();
    }
    public void UpdateOtherCharPosition(TransformMessage transformMsg)
    {
        int keyIndex = allOtherPlayers.FindIndex(it => it.ClientId == transformMsg.ReceiverId);
        if (keyIndex > -1)
        {
            UpdateOtherCharacterPosition(transformMsg);
        }
        else
        {
            CreateOtherPlayerCharacter(transformMsg);
        }
    }
    public void DespawnDisconnectedPlayer(DisconnectMessage dcMsg)
    {
        OtherPlayerCharacter player = allOtherPlayers.Find(it => it.ClientId == dcMsg.ReceiverId);
        if (player != null)
        {
            client.DestroyGameObject(player.CharacterObj);
            allOtherPlayers.Remove(player);
        }
    }

    private void HandleOtherPlayerInterpolation()
    {
        foreach (OtherPlayerCharacter playerChar in allOtherPlayers)
        {
            playerChar.IterpolatePositions();
        }
    }
    private void CreateOtherPlayerCharacter(TransformMessage transformMsg)
    {
        GameObject other = client.SpawnCharacter();
        other.transform.parent = client.transform;
        other.transform.position = transformMsg.Position.Vect3;
        other.transform.localScale = transformMsg.Scale.Vect3;
        other.transform.rotation = transformMsg.Rotation.Quaternion;
        OtherPlayerCharacter otherPlayerChar = new OtherPlayerCharacter(other, transformMsg.ReceiverId);
        allOtherPlayers.Add(otherPlayerChar);
    }
    private void UpdateOtherCharacterPosition(TransformMessage transformMsg)
    {
        foreach (OtherPlayerCharacter other in allOtherPlayers)
        {
            if (transformMsg.ReceiverId == other.ClientId)
            {
                other.AddInterpolationPos(transformMsg.Position.Vect3);
                other.CharacterObj.transform.localScale = transformMsg.Scale.Vect3;
                other.CharacterObj.transform.rotation = transformMsg.Rotation.Quaternion;
            }
        }
    }
}
