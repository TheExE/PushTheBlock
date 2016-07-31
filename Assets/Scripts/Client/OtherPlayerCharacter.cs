using UnityEngine;
using System.Collections;

public class OtherPlayerCharacter : Character
{
    private PositionInterpolation posInterPol;
    public OtherPlayerCharacter(GameObject charObject, int playerId) : base(charObject, playerId)
    {
        posInterPol = new PositionInterpolation();
    }

    public void AddInterpolationPos(Vector3 pos)
    {
        posInterPol.AddPosition(pos);
    }

    public void IterpolatePositions()
    {
        if(posInterPol.IsReadyToInterpol)
        {
            CharacterObj.transform.position = posInterPol.Interpolate(CharacterObj.transform.position);
        }
    }

}
