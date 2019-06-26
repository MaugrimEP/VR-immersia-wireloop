using System.Collections.Generic;
using UnityEngine;

public class CopieStr : IReactionStr
{
    public CopieStr(RaquetteController rc) : base(rc)
    {
    }

    /// <summary>
    /// called for each fixed update frame
    /// </summary>
    public override void ComputeSimulationStep()
    {
        Vector3 oldPosition = rc.targetRigidbody.position;
        Quaternion oldRotation = rc.targetRigidbody.rotation;

        if (rc.IsColliding())
        {
            ic.SetVirtuosePose(oldPosition, oldRotation);
        }
        else
        {
            ic.SetVirtuosePoseIdentity();
        }

        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        throw new System.NotImplementedException();
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
