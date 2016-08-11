using UnityEngine;
using System.Collections;

public class Character
{
    private GameObject playerCharacterObj;
    private Transform characterTransf;
    private int connectionId;
    private Vector2 homePosition;
    private Vector3 lastSentPosition;

    public Character(GameObject characterObject, int connectionId)
    {
        this.connectionId = connectionId;
        playerCharacterObj = characterObject;
        characterTransf = characterObject.GetComponent<Transform>();
        var pos = characterObject.transform.position;
        homePosition = new Vector2(pos.x, pos.y);
        lastSentPosition = new Vector3();
    }

    public int Update()
    {
        int lastCollisionId = -1;
        if(playerCharacterObj.transform.position.y < 0)
        {
            lastCollisionId = playerCharacterObj.GetComponent<CollisionQueue>().LastPlayerCollisionId;
            Reset();
        }

        return lastCollisionId;
    }

    public void Reset()
    {
        playerCharacterObj.transform.position = new Vector2(homePosition.x, homePosition.y);
    }

    public GameObject CharacterObj
    {
        get { return playerCharacterObj; }
    }
    public Transform CharacterTransform
    {
        get { return characterTransf; }
    }


    public int ClientId
    {
        get { return connectionId; }
    }

    public Vector3 LastSentPosition
    {
        get { return lastSentPosition; }
        set { lastSentPosition = value; }
    }
}
