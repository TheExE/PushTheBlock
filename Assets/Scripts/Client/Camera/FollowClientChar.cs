using UnityEngine;
using System.Collections;

public class FollowClientChar : MonoBehaviour
{
    public Vector3 offset;

    private Transform clientsCharTransf;
    private bool isInited = false;
    private Vector3 offsetFromChar;
    private Vect3LerpManager cameraLerp;
    private Vector3 lastCharPosition;
    private float lastCharRotation;

	void Update ()
    {
        if(isInited)
        {
            transform.position = clientsCharTransf.transform.position + offset;
        }
	}

    public void InitCharacterToFollow(Transform charTransform)
    {
        clientsCharTransf = charTransform;
        isInited = true;
    }
}
