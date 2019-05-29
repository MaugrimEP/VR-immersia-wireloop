using UnityEngine;

public interface IReactionStr
{
    (Vector3 Position, Quaternion Rotation) Solve(RaquetteController rc);
}