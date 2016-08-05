using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public FollowClientChar cameraFollow;
    public GameObject otherCharPrefab;
    public GameObject clientsCharPrefab;
    public Text text;

    private InputHandler inputaHandler;
    private ClientsDataHandler clientsDataManager;
    private ClientsNetworkManager clientsNetworkManager;
    private Character playerChar;
    private bool isClientsCharacterCreated = false;
    private int clientId;
    private bool isClientInited = false;

    void Start()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        text.text = "This is Client";
        inputaHandler = new InputHandler();
        clientsNetworkManager = new ClientsNetworkManager();
        clientsDataManager = new ClientsDataHandler(this);
    }


    void Update()
    {
        List<InputType> inputs = inputaHandler.Update(isClientsCharacterCreated);
        clientsNetworkManager.Update(clientsDataManager, playerChar, inputs);
        clientsDataManager.Update();
    }
    
   
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
        if(!isClientInited)
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
        inputaHandler.InitInputHandler(characterObject.GetComponent<Rigidbody>());
        characterObject.GetComponent<Renderer>().material.color = Color.red;
        isClientsCharacterCreated = true;
        UpdateCharactersPosition(mT);

        cameraFollow.InitCharacterToFollow(characterObject.transform);
    }
    public void UpdateCharactersPosition(TransformMessage transformMsg)
    {
        playerChar.CharacterObj.transform.position = transformMsg.Position.Vect3;
        playerChar.CharacterObj.transform.localScale = transformMsg.Scale.Vect3;
        playerChar.CharacterObj.transform.rotation = transformMsg.Rotation.Quaternion;
    }
    public GameObject SpawnCharacter()
    {
        return Instantiate(otherCharPrefab);
    }
    public void DestroyGameObject(GameObject gameObjToDestroy)
    {
        Destroy(gameObjToDestroy);
    }
}
