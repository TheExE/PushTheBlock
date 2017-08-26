using UnityEngine;
using System.Collections;

public class Character
{
    private GameObject _playerCharacterObj;
    private Transform _characterTransf;
    private int _connectionId;
    private Vector2 _homePosition;
    private Vector3 _lastSentPosition;

    public Character(GameObject characterObject, int connectionId)
    {
        _connectionId = connectionId;
        _playerCharacterObj = characterObject;
        _characterTransf = characterObject.GetComponent<Transform>();
        var pos = characterObject.transform.position;
        _homePosition = new Vector2(pos.x, pos.y);
        _lastSentPosition = new Vector3();
    }

    public GameObject CharacterObj
    {
        get { return _playerCharacterObj; }
    }

    public Transform CharacterTransform
    {
        get { return _characterTransf; }
    }

    public int ClientId
    {
        get { return _connectionId; }
    }

    public Vector3 LastSentPosition
    {
        get { return _lastSentPosition; }
        set { _lastSentPosition = value; }
    }
}
