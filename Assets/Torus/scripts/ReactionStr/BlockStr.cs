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
        (Vector3 READposition, Quaternion READrotation) = ic.GetVirtuosePoseRaw();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - READposition, rc.MAX_DISPLACEMENT);
        solvedNextPosition = READposition + displacementClamped;

        ic.SetVirtuosePoseRaw(solvedNextPosition, solvedNextRotation);

        if(!rc.infoCollision.IsCollided)
            (rc.lastFramePosition, rc.lastFrameRotation) = (READposition, READrotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        rc.targetRigidbody.MovePosition(ic.GetVirtuosePose().Position);
        rc.targetRigidbody.MoveRotation(ic.GetVirtuosePose().Rotation);

        if (rc.infoCollision.IsCollided)
        {
            return (rc.lastFramePosition, rc.lastFrameRotation);
        }
        else
        {
            return ic.GetVirtuosePoseRaw();
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
