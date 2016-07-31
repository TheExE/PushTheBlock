using UnityEngine;
using System.Collections.Generic;

public class OtherPlayerManager
{
    private Client client;
    private List<OtherPlayerCharacter> allOtherCharacters = new List<OtherPlayerCharacter>();

    public OtherPlayerManager(Client client)
    {
        this.client = client;
    }

    public void Update()
    {
        HandleOtherPlayerInterpolation();
    }
    public void UpdateOtherCharPosition(MultipleTranformMessage multipleTransfMsg)
    {
        foreach (TransformMessage transformMsg in multipleTransfMsg.AllTransformMessages)
        {
            int keyIndex = allOtherCharacters.FindIndex(it => it.ClientId == transformMsg.ReceiverId);
            if (keyIndex > -1)
            {
                UpdateOtherCharacterPosition(transformMsg);
            }
            else
            {
                CreateOtherPlayerCharacter(transformMsg);
            }
        }
    }
    public void DespawnDisconnectedPlayer(DisconnectMessage dcMsg)
    {
        OtherPlayerCharacter otherCharaceter = allOtherCharacters.Find(it => it.ClientId == dcMsg.ReceiverId);
        if (otherCharaceter != null)
        {
            client.DestroyGameObject(otherCharaceter.CharacterObj);
            allOtherCharacters.Remove(otherCharaceter);
        }
    }

    private void HandleOtherPlayerInterpolation()
    {
        foreach (OtherPlayerCharacter otherPlayerChar in allOtherCharacters)
        {
            otherPlayerChar.IterpolatePositions();
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
        allOtherCharacters.Add(otherPlayerChar);
    }
    private void UpdateOtherCharacterPosition(TransformMessage transformMsg)
    {
        foreach (OtherPlayerCharacter other in allOtherCharacters)
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
