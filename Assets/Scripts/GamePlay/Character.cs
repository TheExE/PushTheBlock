using UnityEngine;
using System.Collections;

public class Character
{
    private GameObject playerCharacterObj;
    private Rigidbody characterPhysicsBody;
    private int connectionId;
    private Vector2 homePosition;
    private Vector3 lastSentPosition;

    public Character(GameObject characterObject, int connectionId)
    {
        this.connectionId = connectionId;
        playerCharacterObj = characterObject;
        characterPhysicsBody = characterObject.GetComponent<Rigidbody>();
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

        /* CAP VELOCITY OF CLIENTS CHARACTER */
        if (characterPhysicsBody.velocity.magnitude > GameConsts.MAX_MOVE_SPEED)
        {
            characterPhysicsBody.velocity = characterPhysicsBody.
                velocity.normalized * GameConsts.MAX_MOVE_SPEED;
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
