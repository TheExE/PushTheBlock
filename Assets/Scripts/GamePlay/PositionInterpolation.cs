using UnityEngine;
using System.Collections.Generic;

public class PositionInterpolation
{
    private Queue<Vector3> interpolationPositions;
    private bool isReadyToInterPos = false;
    private Vector3 lastLerpPosition;
    private float curLearpProg = 0f;

    public PositionInterpolation()
    {
        interpolationPositions = new Queue<Vector3>();
    }

    public void Reset()
    {
        isReadyToInterPos = false;
    }

    public Vector3 Interpolate(Vector3 pos)
    {
        if (curLearpProg >= 1f)
        {
            if (interpolationPositions.Count == 0)
            {
                Reset();
            }
            else
            {
                lastLerpPosition = interpolationPositions.Dequeue();
            }
            curLearpProg = 0f;
        }
        Vector3 result =  Vector3.Lerp(pos, lastLerpPosition, curLearpProg);
        curLearpProg += 0.05f;
        return result;
    }

    public void AddPosition(Vector3 v)
    {
        interpolationPositions.Enqueue(v);
        if (interpolationPositions.Count > 0)
        {
            isReadyToInterPos = true;
        }
    }

    public bool IsReadyToInterpol
    {
        get { return isReadyToInterPos; }
    }
}
