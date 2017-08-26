using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public FollowClientChar cameraFollow;
    public GameObject otherCharPrefab;
    public GameObject clientsCharPrefab;
    public Text text;

    private InputHandler inputHandler;
    private ClientsDataHandler clientsDataManager;
    private ClientsNetworkManager clientsNetworkManager;
    private Character playerChar;
    private bool isClientsCharacterCreated = false;
    private int clientId;
    private bool isClientInited = false;

    public int ClientId
    {
        get { return clientId; }
    }

    public string ClientTitle
    {
        get { return text.text; }
    }

    public void InitClient(int clientId, string clientTitle)
    {
        if (!isClientInited)
        {
            isClientInited = true;
        }
        this.clientId = clientId;
        text.text = clientTitle;
    }

    public bool IsClientsCharacterCreated
    {
        get { return isClientsCharacterCreated; }
    }

    public void CreateClientsCharacter(TransformMessage mT)
    {
        GameObject characterObject = Instantiate(clientsCharPrefab) as GameObject;
        playerChar = new Character(characterObject, mT.ReceiverId);
        playerChar.CharacterObj.transform.parent = transform;
        inputHandler.InitInputHandler(characterObject.GetComponent<Transform>());
        characterObject.GetComponent<Renderer>().material.color = Color.red;
        isClientsCharacterCreated = true;
        UpdateCharactersTransform(mT);

        cameraFollow.InitCharacterToFollow(characterObject.transform);
    }

    public void UpdateCharactersTransform(TransformMessage transformMsg)
    {
        playerChar.CharacterObj.transform.position = transformMsg.Position.Vect3;
        playerChar.CharacterObj.transform.localScale = transformMsg.Scale.Vect3;
        playerChar.CharacterObj.transform.rotation = transformMsg.Rotation.Quaternion;
    }

	/// <summary>
	/// Update character position using interpolation.
	/// </summary>
	/// <param name="position"> New position to move to. </param>
    public void SlowlyUpdateCharactersPosition(Vector3 position)
    {
        Vector3 curPosition = playerChar.CharacterTransform.position;
        if((curPosition - position).magnitude > 0.5f)
        {
            playerChar.CharacterTransform.position = Vector3.Lerp(curPosition, position, 0.5f);
        }
        else
        {
            playerChar.CharacterTransform.position = position;
        }
    }

    public GameObject SpawnCharacter()
    {
        return Instantiate(otherCharPrefab);
    }

    public void DestroyGameObject(GameObject gameObjToDestroy)
    {
        Destroy(gameObjToDestroy);
    }

    public Character PlayerChar
    {
        get { return playerChar; }
    }

    public Vector3 GetPositionChangeBasedOnInput(InputMessage inputMsg)
    {
        return inputHandler.GetPositionChangeBasedOnInput(inputMsg);
    }

	private void Start()
	{
		Application.targetFrameRate = 60;
		Application.runInBackground = true;
		text.text = "This is Client";
		inputHandler = new InputHandler();
		clientsNetworkManager = new ClientsNetworkManager();
		clientsDataManager = new ClientsDataHandler(this);
	}


	private void Update()
	{
		List<InputType> inputs = inputHandler.Update(isClientsCharacterCreated);
		clientsNetworkManager.Update(clientsDataManager, playerChar, inputs);
		clientsDataManager.Update();
	}
}
