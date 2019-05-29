using UnityEngine;

public class DefaultStr : IReactionStr
{

    public DefaultStr(RaquetteController rc) : base(rc)
    {
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuosePose();
        (Vector3 position, Quaternion rotation) = Utils.V2UPosRot(READposition, READrotation);

        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = Solve();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - position, rc.MAX_CLAMP);
        solvedNextPosition = oldPosition + displacementClamped;

        #region check threshold distance and rotation
        if (CheckTreshold(oldPosition, solvedNextPosition, rotation, solvedNextRotation))
            rc.vm.Virtuose.Power = false;
        #endregion

        if (rc.infoCollision.IsCollided)
        {
            rc.vm.Virtuose.Pose = (solvedNextPosition, U2VRotation(solvedNextRotation).normalized);
        }
        else
        {
            rc.vm.Virtuose.Pose = rc.vm.Virtuose.Pose;

            //Vector3 torques = Utils.ComputeTorqueFromRotation(Quaternion.Euler(10f, 0f, 0f), VRTools.GetDeltaTime());
            //Vector3 torques = Utils.ComputeTorqueFromRotation(rotationR * Quaternion.Inverse(solvedNextRotation), VRTools.GetDeltaTime());


            //Debug.Log($"torques = {torques}");

            //rc.vm.Virtuose.Testforce = new float []{ 0,0,0 ,5,0,0 };
        }

        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected override (Vector3 Position, Quaternion Rotation) Solve()
    {
        (Vector3 position, Quaternion rotation) = rc.vm.Virtuose.Pose;
        (position, rotation) = Utils.V2UPosRot(position, rotation);

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        Vector3 normal = rc.target.transform.position - rc.targetRigidbody.position;
        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene 
        //Vector3 newPosition = rc.infoCollision.IsCollided ? rc.target.transform.position + rc.stiffness * normal : rc.targetRigidbody.position;
        Vector3 newPosition = rc.GetPosition() + (rc.infoCollision.IsCollided ? rc.stiffness * normal : Vector3.zero);
        Quaternion newRotation = rc.GetRotation();

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
}
