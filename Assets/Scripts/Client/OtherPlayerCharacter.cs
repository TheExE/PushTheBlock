using UnityEngine;
using System.Collections;

public class OtherPlayerCharacter : Character
{
    private Vect3LerpManager posInterpol;

    public OtherPlayerCharacter(GameObject charObject, int playerId) : base(charObject, playerId)
    {
        posInterpol = new Vect3LerpManager(GameConsts.MOVE_SPEED, GameConsts.MAX_OP_POS_IN_QUEUE, false);
    }
    public void AddInterpolationPos(Vector3 pos)
    {
        /* Dont need to interpolate to position that we already are at */
        if(!IsVector3sEqual(pos, CharacterTransform.position))
        {
            posInterpol.AddLerp(pos);
        }
    }
    public void IterpolatePositions()
    {
        Vector3 characterPosition = CharacterObj.transform.position;
        posInterpol.UpdateInterpolationTarget(characterPosition);
        if(posInterpol.IsReadyToInterpol)
        {
            Vector3 newPos = posInterpol.Interpolate();
            CharacterObj.transform.position = newPos;
        }
    }

    private bool IsVector3sEqual(Vector3 v1, Vector3 v2)
    {
        return Mathf.Round(v1.x) == Mathf.Round(v2.x) && Mathf.Round(v1.y) == Mathf.Round(v2.y) &&
            Mathf.Round(v1.z) == Mathf.Round(v2.z);
    }

}
