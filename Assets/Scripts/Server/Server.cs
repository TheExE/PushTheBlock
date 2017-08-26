using UnityEngine;
using System.Collections.Generic;

public class Server : MonoBehaviour
{	
	[SerializeField] private GameObject _playerPrefab;
    private ServerNetworkManager _networkManager;
    private ServerClientDataManager _clientDataManager;


	/// <summary>
	/// Destroy the games object.
	/// </summary>	
	public void DeleteGameObject(GameObject gameObjToDestroy)
	{
		Destroy(gameObjToDestroy);
	}

	/// <summary>
	/// Spawn the player in the world.
	/// </summary>
	/// <param name="connectionId"> The id for the player that connected. </param>
	/// <returns></returns>
	public GameObject SpawnClientsCharacter(int connectionId)
	{
		GameObject a = Instantiate(_playerPrefab) as GameObject;
		a.transform.parent = transform;
		a.transform.position = new Vector3(0, 0);		

		return a;
	}

	private void Awake ()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;

        _clientDataManager = new ServerClientDataManager();
        _networkManager = new ServerNetworkManager(_clientDataManager);
    }
	
	private void Update ()
    {
        _networkManager.Update(this);
    }
}
