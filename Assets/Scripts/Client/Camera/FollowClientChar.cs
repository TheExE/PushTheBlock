using UnityEngine;
using System.Collections;

public class FollowClientChar : MonoBehaviour
{
    public float offsetY;
    public float offsetZ;

    private Transform clientsCharTransf;
    private bool isInited = false;
    private Vector3 offsetFromChar;

    void Start()
    {
        offsetFromChar = new Vector3(0, offsetY, offsetZ);
    }

	void Update ()
    {
        if(isInited)
        {
            transform.position = clientsCharTransf.transform.position + offsetFromChar;
        }
	}

    public void InitCharacterToFollow(Transform charTransform)
    {
        clientsCharTransf = charTransform;
        isInited = true;
    }
}
