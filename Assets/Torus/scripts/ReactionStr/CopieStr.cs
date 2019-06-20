using System.Collections.Generic;
using UnityEngine;

public class CopieStr : IReactionStr
{

    public CopieStr(RaquetteController rc) : base(rc)
    {
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();
        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - position, rc.MAX_DISPLACEMENT);
        solvedNextPosition = oldPosition + displacementClamped;

        if (Debug.isDebugBuild)
        {
            VectorManager.Clear();
            VectorManager.DrawSphereS(rc.target.transform.position, Vector3.one * 0.015f, Color.yellow);
            VectorManager.DrawSphereS(rc.targetRigidbody.position, Vector3.one * 0.015f, Color.black);
        }

        if (rc.infoCollision.IsCollided)
        {
            ic.SetVirtuosePose(solvedNextPosition, solvedNextRotation);
        }
        else
        {
            ic.SetVirtuosePoseIdentity();
        }
        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        Vector3 newPosition = rc.target.transform.position;
        Quaternion newRotation = rc.target.transform.rotation;

        return (newPosition, newRotation);
    }

    private Quaternion U2VRotation(Quaternion URotation)
    {
        Vector3 eulerRotation = URotation.eulerAngles;
        return Quaternion.Euler(-eulerRotation.z, eulerRotation.x, eulerRotation.y);

    }

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        throw new System.NotImplementedException();
    }
}
