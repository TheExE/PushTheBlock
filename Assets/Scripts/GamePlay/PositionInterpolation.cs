using UnityEngine;
using System.Collections.Generic;

public class PositionInterpolation
{
    private Queue<Vector3> interpolationPositions;
    private bool isReadyToInterPos = false;
    private Vector3 posToInterpolateTo;
    private Vector3 posToInterpolateFrom;
    private float curLearpProg = 0f;
    private float startTime;
    private float distToTarget;
    private float speed = 2.5f;
    private bool initTargetPos = true;

    public PositionInterpolation()
    {
        interpolationPositions = new Queue<Vector3>();
    }

    public Vector3 Interpolate()
    {
        float distCovered = (Time.time - startTime) * speed;
        curLearpProg = distCovered / distToTarget;
        Vector3 result = Vector3.Lerp(posToInterpolateFrom, posToInterpolateTo, curLearpProg);
        if(curLearpProg > 1)
        {
            initTargetPos = true;
        }
        return result;
    }
    public void UpdateInterpolationTarget(Vector3 pos)
    { 
       if (interpolationPositions.Count != 0 && initTargetPos)
        {
            posToInterpolateFrom = new Vector3(pos.x, pos.y, pos.z);
            posToInterpolateTo = interpolationPositions.Dequeue();
            startTime = Time.time;
            distToTarget = Vector3.Distance(posToInterpolateFrom, posToInterpolateTo);
            curLearpProg = 0f;
            if(distToTarget > 0)
            {
                isReadyToInterPos = true;
                initTargetPos = false;
            }
        }

        if((interpolationPositions.Count == 0 && curLearpProg >= 1) || distToTarget == 0)
        {
            isReadyToInterPos = false;
        }
    }

    public void AddPosition(Vector3 position)
    {
        if(!interpolationPositions.Contains(position))
        {
            interpolationPositions.Enqueue(position);
            if (interpolationPositions.Count > 0)
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
