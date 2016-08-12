using UnityEngine;
using System.Collections.Generic;

public class Vect3LerpManager
{
    private Queue<Vector3> lerpTargets;
    private bool isReadyToInterPos = false;
    private Vector3 interpolateTo;
    private Vector3 interpolateFrom;
    private float curLearpProg = 0f;
    private float startTime;
    private float distToTarget;
    private float speed;
    private bool initTargetPos = true;

    public Vect3LerpManager(float speed)
    {
        lerpTargets = new Queue<Vector3>();
        this.speed = speed;
    }

    public void CancelAllInterpolations()
    {
        lerpTargets.Clear();
        isReadyToInterPos = false;
    }
    public Vector3 Interpolate()
    {
        float distCovered = (Time.time - startTime) * speed;
        curLearpProg = distCovered / distToTarget;
        Vector3 result = Vector3.Lerp(interpolateFrom, interpolateTo, curLearpProg);
        if(curLearpProg > 1)
        {
            initTargetPos = true;
        }
        return result;
    }
    public void UpdateInterpolationTarget(Vector3 pos)
    { 
       if (lerpTargets.Count != 0 && initTargetPos)
        {
            interpolateFrom = new Vector3(pos.x, pos.y, pos.z);
            interpolateTo = lerpTargets.Dequeue();
            startTime = Time.time;
            distToTarget = Vector3.Distance(interpolateFrom, interpolateTo);
            curLearpProg = 0f;
            if(distToTarget > 0)
            {
                isReadyToInterPos = true;
                initTargetPos = false;
            }
        }

        if((lerpTargets.Count == 0 && curLearpProg >= 1) || distToTarget == 0)
        {
            isReadyToInterPos = false;
        }
    }

    public void AddLerp(Vector3 position)
    {
        if(!lerpTargets.Contains(position))
        {
            lerpTargets.Enqueue(position);
            if (lerpTargets.Count > 0)
            {
                isReadyToInterPos = true;
            }
        }
        
    }

    public bool IsReadyToInterpol
    {
        get { return isReadyToInterPos; }
    }
}
