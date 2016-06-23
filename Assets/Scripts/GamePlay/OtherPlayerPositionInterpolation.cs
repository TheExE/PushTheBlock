﻿using UnityEngine;
using System.Collections.Generic;

public class OtherPlayerPositionInterpolation
{
    private Queue<Vector3> interpolationPositions;
    private int clientId;
    private bool isReadyToInterPos = false;
    private int playerIdx = -1;
    private Vector3 lastLerpPosition;
    private float curLearpProg = 0f;

    public OtherPlayerPositionInterpolation(int clientId, int playerIndex)
    {
        this.clientId = clientId;
        this.playerIdx = playerIndex;
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