using UnityEngine;
using System.Collections;

public class Player
{
    private GameObject playerCharacterObj;
    private int connectionId;

    public Player(GameObject playerCharacterObj, int connectionId)
    {
        this.playerCharacterObj = playerCharacterObj;
        this.connectionId = connectionId;
    }


    public GameObject PlayerCharacterObj
    {
        get { return playerCharacterObj; }
    }

    public int ConnectionId
    {
        get { return connectionId; }
    }
}
