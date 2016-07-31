using UnityEngine;
using System.Collections.Generic;

public class Server : MonoBehaviour
{ 
    public GameObject playerPrefab;

    private ServerNetworkManager networkManager;
    private ServerClientDataManager clientDataManager;
    
	void Start ()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;

        clientDataManager = new ServerClientDataManager();
        networkManager = new ServerNetworkManager(clientDataManager);
    }
	
	void Update ()
    {
        networkManager.Update(this);
        clientDataManager.Update();
    }

    public void DeleteGameObject(GameObject gameObjToDestroy)
    {
        Destroy(gameObjToDestroy);
    }
    public GameObject SpawnClientsCharacter(int connectionId)
    {
        GameObject a = Instantiate(playerPrefab) as GameObject;
        a.transform.parent = transform;
        a.transform.position = new Vector3(0, 30f);
        a.GetComponent<CollisionQueue>().ClientId = connectionId;

        return a;
    }
}
