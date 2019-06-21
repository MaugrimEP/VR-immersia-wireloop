using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceRotationStr : IReactionStr
{

    private Transform handleTransform;
    private float stiffnessForce;

    public ForceRotationStr(RaquetteController rc) : base(rc)
    {
        handleTransform = GameObject.Find("handlePosition").GetComponent<Transform>();
        stiffnessForce = 1000f;// 30f * 100f / 4f;
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 position, Quaternion rotation) = rc.ic.GetVirtuosePose();

        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        //compute forces and torques
        (Vector3 forces, Vector3 torques) = SolveForceAndTorque();
        (Vector3 positionSolved, Quaternion rotationSolved) = SolvePositiondAndRotation();
        forces = Utils.ClampVector3(forces, rc.MAX_FORCE);
        torques = Utils.ClampVector3(torques, rc.MAX_TORQUE);

        if (rc.infoCollision.IsCollided)
        {
            ic.SetVirtuosePose(positionSolved, rotationSolved);
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

        if (Debug.isDebugBuild) VectorManager.Clear();

        foreach (ContactPoint contactPoint in currentCollision.contacts)
        {
            Vector3 vectorHandleContactPoint = contactPoint.point - handleTransform.position;
            Vector3 normalToContact = contactPoint.normal;
            float interpenetrationDistance = -contactPoint.separation;

            Vector3 localForce = normalToContact * interpenetrationDistance * stiffnessForce;

            totalForce += localForce;

            if (Debug.isDebugBuild) Debug.DrawLine(handleTransform.position, contactPoint.point, Utils.RandomColor());
            if (Debug.isDebugBuild) VectorManager.DrawSphereS(contactPoint.point, Vector3.one * 0.05f, Color.black);
        }
        if (Debug.isDebugBuild) Debug.Log($"contactPointCount = {currentCollision.contactCount}");
        if (Debug.isDebugBuild) Debug.DrawLine(handleTransform.position, handleTransform.position + totalForce, Color.red);

        return (totalForce, Vector3.zero);
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 virtPos, Quaternion virtRot) = ic.GetVirtuosePose();

        if (!rc.IsColliding()) return (virtPos, virtRot);
        else return (virtPos, rc.target.transform.rotation);
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