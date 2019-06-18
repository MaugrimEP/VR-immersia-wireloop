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
        stiffnessForce = 600f;// 30f * 100f / 4f;
        stiffnessTorque = 0.2f;
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

        if (Debug.isDebugBuild) VectorManager.Clear();

        foreach (ContactPoint contactPoint in currentCollision.contacts)
        {
            Vector3 vectorHandleContactPoint = contactPoint.point - handleTransform.position;
            Vector3 normalToContact = contactPoint.normal;
            float interpenetrationDistance = - contactPoint.separation;

            Vector3 localForce = normalToContact * interpenetrationDistance * stiffnessForce;
            Vector3 localTorque = Vector3.Cross(vectorHandleContactPoint, -localForce.normalized) * localForce.magnitude * stiffnessTorque;

            totalForce += localForce;
            totalTorque += localTorque;

            Debug.DrawLine(handleTransform.position, contactPoint.point, Utils.RandomColor());
            VectorManager.DrawSphereS(contactPoint.point, Vector3.one * 0.05f, Color.black);
        }
        if (Debug.isDebugBuild) Debug.Log($"totalForce = {totalForce}   ,totalTorque = {totalTorque}");//TODO to remove : verbose
        if (Debug.isDebugBuild) Debug.Log($"contactPointCount = {currentCollision.contactCount}");
        if (Debug.isDebugBuild) Debug.DrawLine(handleTransform.position, handleTransform.position + totalForce, Color.red);
        if (Debug.isDebugBuild) Debug.DrawLine(handleTransform.position, handleTransform.position + totalTorque, Color.green);

        return (totalForce, totalTorque);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        throw new System.NotImplementedException();
    }

    public override void HandleCollisionEnter(Collision collision)
    {
        base.HandleCollisionEnter(collision);
    }

    public override void HandleCollisionStay(Collision collision)
    {
        base.HandleCollisionStay(collision);
    }
}