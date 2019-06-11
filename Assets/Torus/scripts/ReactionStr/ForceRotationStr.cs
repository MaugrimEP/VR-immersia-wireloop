using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Resolve the collision by using the force for the position, and using the last not colliding rotation to solve
/// the rotation
/// </summary>
public class ForceRotationStr : IReactionStr
{
    private Transform handleTransform;
    private float stiffnessForce = 30f * 100f;
    public ForceRotationStr(RaquetteController rc) : base(rc)
    {
        handleTransform = GameObject.Find("handlePosition").GetComponent<Transform>();
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuoseRawPose();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - READposition, rc.MAX_DISPLACEMENT);
        solvedNextPosition = READposition + displacementClamped;

        rc.vm.Virtuose.RawPose = (solvedNextPosition, solvedNextRotation);

        if(!rc.infoCollision.IsCollided)
            (rc.lastFramePosition, rc.lastFrameRotation) = (READposition, READrotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        rc.targetRigidbody.MovePosition(rc.GetVirtuosePose().Position);
        rc.targetRigidbody.MoveRotation(rc.GetVirtuosePose().Rotation);
        
        if (rc.infoCollision.IsCollided)
        {
            Vector3 normal = Vector3.Normalize(rc.target.transform.position - rc.targetRigidbody.position);

            float interpenetrationDistance = Vector3.Distance(rc.target.transform.position, rc.targetRigidbody.position);
            Vector3 forces = normal * interpenetrationDistance * stiffnessForce;

            VectorManager.Clear();//TODO to remove : mode verbose
            VectorManager.DrawVectorS(handleTransform.position, forces, Color.red, "force"); //TODO to remove : verbose

            rc.vm.Virtuose.virtAddForce= (Utils.U2VVector3(forces), Vector3.zero);

            return (rc.GetVirtuoseRawPose().Position, rc.lastFrameRotation);
        }
        else
        {
            rc.vm.Virtuose.virtAddForce = (Vector3.zero, Vector3.zero);
            return rc.GetVirtuoseRawPose();
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
