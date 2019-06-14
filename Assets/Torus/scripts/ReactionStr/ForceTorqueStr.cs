using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceTorque : IReactionStr
{

    private Transform handleTransform;
    private float stiffnessForce;
    private float stiffnessTorque;

    public ForceTorque(RaquetteController rc) : base(rc)
    {
        handleTransform = GameObject.Find("handlePosition").GetComponent<Transform>();
        stiffnessForce = 30f * 100f / 4f;
        stiffnessTorque = 0.1f;
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 position, Quaternion rotation) = rc.ic.GetVirtuosePose();

        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        //compute forces and torques
        (Vector3 forces, Vector3 torques) = SolveForceAndTorque();
        forces = Utils.ClampVector3(forces, rc.MAX_FORCE);
        torques = Utils.ClampVector3(torques, rc.MAX_TORQUE);

        //compute positions and rotations
        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();
        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - position, rc.MAX_DISPLACEMENT);
        solvedNextPosition = oldPosition + displacementClamped;

        if (rc.infoCollision.IsCollided)
        {
            ic.SetVirtuosePoseIdentity();
            ic.virtAddForce(forces, torques);
        }
        else
        {
            ic.SetVirtuosePoseIdentity();
            ic.virtAddForce(Vector3.zero, Vector3.zero);
        }

        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        if (!rc.IsColliding()) return (Vector3.zero, Vector3.zero);

        Vector3 totalForce = Vector3.zero;
        Vector3 totalTorque = Vector3.zero;

        foreach (ContactPoint contactPoint in currentCollision.contacts)
        {
            Vector3 vectorHandleContractPoint = contactPoint.point - handleTransform.position;
            Vector3 normalToContact = contactPoint.normal;
            float interpenetrationDistance = Mathf.Abs(contactPoint.separation);

            Vector3 localForce = normalToContact * interpenetrationDistance * stiffnessForce;
            Vector3 localTorque = Vector3.Cross(vectorHandleContractPoint, -localForce.normalized) * localForce.magnitude * stiffnessTorque;

            Debug.DrawLine(handleTransform.position, contactPoint.point, Color.magenta);
            Debug.DrawLine(handleTransform.position, handleTransform.position + localTorque, Color.black);
            Debug.Log($"localTorque {localTorque}");

            totalForce += localForce;
            totalTorque += localTorque;
        }
        Debug.Log($"contactPoint.count = {currentCollision.contactCount}");
        Debug.DrawLine(handleTransform.position, handleTransform.position + totalForce, Color.red);
        Debug.DrawLine(handleTransform.position, handleTransform.position + totalTorque, Color.green);

        Debug.Log($"impulsion = {currentCollision.impulse}  totalForce = {totalForce}   ,totalTorque = {totalTorque}");//TODO to remove : verbose

        return (totalForce, totalTorque);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        Vector3 normal = rc.target.transform.position - rc.targetRigidbody.position;
        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene 
        Vector3 newPosition = rc.GetPosition() + (rc.infoCollision.IsCollided ? rc.stiffness * normal : Vector3.zero);
        Quaternion newRotation = rc.GetRotation();

        return (newPosition, newRotation);
    }
}