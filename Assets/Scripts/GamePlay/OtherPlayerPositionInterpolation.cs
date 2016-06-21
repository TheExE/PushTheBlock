using UnityEngine;
using System.Collections.Generic;

public class OtherPlayerPositionInterpolation
{
    private List<Vector3> interpolationPositions;
    private int clientId;
    private bool isReadyToInterPos = false;
    private int playerIdx = -1;

    public OtherPlayerPositionInterpolation(int clientId, int playerIndex)
    {
        this.clientId = clientId;
        this.playerIdx = playerIndex;
        interpolationPositions = new List<Vector3>();
    }

    public void Reset()
    {
        interpolationPositions.Clear();
        isReadyToInterPos = false;
    }

    public Vector3 Interpolate(Vector3 pos)
    {
        Vector3 result = Vector3.Lerp(interpolationPositions[0], interpolationPositions[1], 1f);
        if (pos == interpolationPositions[1])
        {
            Reset();
        }
        return result;
    }

    public void AddPosition(Vector3 v)
    {
        if(!isReadyToInterPos)
        {
            interpolationPositions.Add(v);
            if (interpolationPositions.Count >= 2)
            {
                isReadyToInterPos = true;
            }
        }

    }

    public bool IsReadyToInterPol
    {
        get { return isReadyToInterPos; }
    }

    public int PlayerIndex
    {
        get { return playerIdx; }
    }


    public int ClientId
    {
        get { return clientId; }
    }
}
