using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockStr : IReactionStr
{
    public BlockStr(RaquetteController rc) : base(rc)
    {
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuosePose();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - READposition, rc.MAX_DISPLACEMENT);
        solvedNextPosition = READposition + displacementClamped;

        #region check threshold distance and rotation
        if (CheckTreshold(READposition, solvedNextPosition, READrotation, solvedNextRotation))
            rc.vm.Virtuose.Power = false;
        #endregion

        rc.vm.Virtuose.Pose = (solvedNextPosition, solvedNextRotation.normalized);

        if(!rc.infoCollision.IsCollided)
            (rc.lastFramePosition, rc.lastFrameRotation) = (READposition, READrotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.vm.Virtuose.Pose;
        (Vector3 position, Quaternion rotation) = Utils.V2UPosRot(READposition, READrotation);

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        if (rc.infoCollision.IsCollided)
        {
            return (rc.lastFramePosition, rc.lastFrameRotation);
        }
        else
        {
            return rc.vm.Virtuose.Pose;
        }
    }

    public override void HandleCollisionEnter(Collision collision)
    {
    }

    public override void HandleCollisionExit(Collision collision)
    {
    }

    public override void HandleCollisionStay(Collision collision)
    {
    }

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        throw new System.NotImplementedException();
    }
}
