﻿using UnityEngine;

public class ForceTorque : IReactionStr
{

    private Transform handleTransform;
    private float stiffnessForce;
    private float stiffnessTorque;
    private Collision currentCollision;

    public ForceTorque(RaquetteController rc) : base(rc)
    {
        handleTransform = GameObject.Find("handlePosition").GetComponent<Transform>(); ;
        stiffnessForce = 30 * 100;//10000f;
        stiffnessTorque = 0.1f;
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuosePose();
        (Vector3 position, Quaternion rotation) = Utils.V2UPosRot(READposition, READrotation);

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
            rc.vm.Virtuose.Pose = rc.vm.Virtuose.Pose;
            Debug.Log($"torques total = {torques}");

            rc.vm.Virtuose.virtAddForce = (Utils.U2VVector3(forces), Utils.U2VVector3(torques));
        }
        else
        {
            rc.vm.Virtuose.Pose = rc.vm.Virtuose.Pose;
            rc.vm.Virtuose.virtAddForce = (Vector3.zero, Vector3.zero);
        }

        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        (Vector3 position, Quaternion rotation) = rc.vm.Virtuose.Pose;
        (position, rotation) = Utils.V2UPosRot(position, rotation);

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        if (!rc.IsColliding()) return (Vector3.zero, Vector3.zero);

        //IsColliding
        Vector3 normal = Vector3.Normalize(rc.target.transform.position - rc.targetRigidbody.position);

        float interpenetrationDistance = Vector3.Distance(rc.target.transform.position, rc.targetRigidbody.position);

        Vector3 forces = normal * interpenetrationDistance * stiffnessForce;

        Vector3 torques = Vector3.zero;
        {// compute the torque

            foreach (ContactPoint contactPoint in currentCollision.contacts)
            {
                Vector3 vectorToHandle = contactPoint.point - handleTransform.position;
                Vector3 normalToContact = contactPoint.normal;

                Vector3 localTorque = Vector3.Cross(normalToContact, vectorToHandle);

                torques += localTorque;
            }
            torques *= forces.magnitude * stiffnessTorque;
            torques /= currentCollision.contactCount;
        }

        //VectorManager.DrawVectorS(handlePosition.pos)

        return (forces, torques);
    }

    private Quaternion U2VRotation(Quaternion URotation)
    {
        Vector3 eulerRotation = URotation.eulerAngles;
        return Quaternion.Euler(-eulerRotation.z, eulerRotation.x, eulerRotation.y);

    }

    public override void HandleCollisionEnter(Collision collision)
    {
        currentCollision = collision;
    }

    public override void HandleCollisionExit(Collision collision)
    {
        currentCollision = null;
    }

    public override void HandleCollisionStay(Collision collision)
    {
        currentCollision = collision;
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 position, Quaternion rotation) = rc.vm.Virtuose.Pose;
        (position, rotation) = Utils.V2UPosRot(position, rotation);

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        Vector3 normal = rc.target.transform.position - rc.targetRigidbody.position;
        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene 
        Vector3 newPosition = rc.GetPosition() + (rc.infoCollision.IsCollided ? rc.stiffness * normal : Vector3.zero);
        Quaternion newRotation = rc.GetRotation();

        return (newPosition, newRotation);
    }
}