using UnityEngine;

public class CopieStr : IReactionStr
{

    public CopieStr(RaquetteController rc) : base(rc)
    {
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuosePose();
        (Vector3 position, Quaternion rotation) = Utils.V2UPosRot(READposition, READrotation);

        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - position, rc.MAX_DISPLACEMENT);
        solvedNextPosition = oldPosition + displacementClamped;

        #region check threshold distance and rotation
        if (CheckTreshold(oldPosition, solvedNextPosition, rotation, rotation))//TODO : CHECK LA ROTATION
            rc.vm.Virtuose.Power = false;
        #endregion

        if (rc.infoCollision.IsCollided)
        {
            rc.vm.Virtuose.Pose = (solvedNextPosition, U2VRotation(solvedNextRotation).normalized);
        }
        else
        {
            rc.vm.Virtuose.Pose = rc.vm.Virtuose.Pose;
        }

        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 position, Quaternion rotation) = rc.vm.Virtuose.Pose;
        (position, rotation) = Utils.V2UPosRot(position, rotation);

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene 
        Vector3 newPosition = rc.target.transform.position;
        Quaternion newRotation = rc.target.transform.rotation;

        {
            //Need to apply the possible shifting position and rotation
            (Vector3 PosWithshiftPosition, Quaternion RotWithshiftRotation) = rc.vm.Virtuose.AvatarPose;

            Vector3 offsetPosition = PosWithshiftPosition - position;
            Quaternion offsetRotatino = RotWithshiftRotation * Quaternion.Inverse(rotation);
        }

        return (newPosition, newRotation);
    }

    private Quaternion U2VRotation(Quaternion URotation)
    {
        Vector3 eulerRotation = URotation.eulerAngles;
        return Quaternion.Euler(-eulerRotation.z, eulerRotation.x, eulerRotation.y);

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
