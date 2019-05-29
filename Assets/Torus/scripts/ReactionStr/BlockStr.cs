using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockStr : IReactionStr
{
    public (Vector3 Position, Quaternion Rotation) Solve(RaquetteController rc)
    {
        if (rc.infoCollision.IsCollided)
        {
            return (rc.lastFramePosition, rc.lastFrameRotation);
        }
        else
        {
            return rc.vm.Virtuose.Pose;
        }
    }
}
