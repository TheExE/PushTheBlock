using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CollisionQueue : MonoBehaviour
{
    private int clientId = -1;
    private int lastContactedPlayerId = -1;
    private bool isServerSide = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("Server"))
        {
            isServerSide = true;
        }
    }


    void Update()
    {
    }

    public void OnCollisionExit(Collision collision)
    {
        if (isServerSide)
        {
            if (collision.gameObject.tag == "Player")
            {
                lastContactedPlayerId = collision.gameObject.GetComponent<CollisionQueue>().ClientId;
            }
        }
    }

    public int ClientId
    {
        get { return clientId; }
        set { clientId = value; }
    }
    public int LastPlayerCollisionId
    {
        get { return lastContactedPlayerId; }
    }

}
