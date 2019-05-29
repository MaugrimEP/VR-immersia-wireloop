using UnityEngine;

public class PhysicSimulate : IReactionStr
{
    public float travelDistanceStep = 0.01f;

    public PhysicSimulate(RaquetteController rc):base(rc)
    {
    }

    protected override (Vector3 Position, Quaternion Rotation) Solve()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuosePose();
        (Vector3 objectTargetedPosition, Quaternion objectTargetedRotation) = Utils.V2UPosRot(READposition, READrotation);

        Physics.autoSimulation = false;

        Vector3 oldPosition = rc.infoCollision.IsCollided ? rc.target.transform.position : rc.targetRigidbody.position;
        Quaternion oldRotation = rc.infoCollision.IsCollided ? rc.target.transform.rotation : rc.targetRigidbody.rotation;


        float travelDistance = Vector3.Distance(oldPosition, READposition);
        float stepNumber = travelDistance / travelDistance;

        Vector3 directionStep = Vector3.Normalize(objectTargetedPosition - oldPosition) * travelDistanceStep;
        Quaternion rotationStep = Quaternion.Lerp(oldRotation, objectTargetedRotation, 1 / stepNumber);

        //var for the loop
        Vector3 currentPositionStep = oldPosition;
        Quaternion currentRotationStep = oldRotation;
        for (float i = 0; i < stepNumber; ++i)
        {
            Vector3 nextPositionStep = currentPositionStep + directionStep;
            Quaternion nextRotationStep = currentRotationStep * rotationStep;

            rc.targetRigidbody.MovePosition(nextPositionStep);
            rc.targetRigidbody.MoveRotation(nextRotationStep);

            Physics.Simulate(Time.fixedDeltaTime);

            currentPositionStep = rc.targetRigidbody.position;
            currentRotationStep = rc.targetRigidbody.rotation;
        }
        Physics.autoSimulation = true;
        return rc.GetVirtuosePose();
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
