using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceTorque : IReactionStr
{

    private Transform handleTransform;
    private float stiffnessForce;
    private float stiffnessTorque;
    private Collision currentCollision;

    private float minContactPointDistance = 0.05f;

    public ForceTorque(RaquetteController rc) : base(rc)
    {
        handleTransform = GameObject.Find("handlePosition").GetComponent<Transform>(); ;
        stiffnessForce = 30 * 100;//10000f;
        stiffnessTorque = 0.1f;
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 position, Quaternion rotation) = rc.GetVirtuosePose();

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
            rc.vm.Virtuose.virtAddForce = (Utils.U2VVector3(forces), Utils.U2VVector3(torques));
        }
        else
        {
            rc.vm.Virtuose.RawPose = rc.vm.Virtuose.RawPose;
            rc.vm.Virtuose.virtAddForce = (Vector3.zero, Vector3.zero);
        }

        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        VectorManager.Clear();//TODO to remove : verbose
        (Vector3 position, Quaternion rotation) = rc.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        if (!rc.IsColliding()) return (Vector3.zero, Vector3.zero);

        //IsColliding
        Vector3 normal = Vector3.Normalize(rc.target.transform.position - rc.targetRigidbody.position);

        float interpenetrationDistance = Vector3.Distance(rc.target.transform.position, rc.targetRigidbody.position);
        Vector3 forces = normal * interpenetrationDistance * stiffnessForce;

        Vector3 torques = Vector3.zero;
        {// compute the torque

            //we first filter the contactPoint where the distance is less than minContactPointDistance
            List<ContactPoint> filteredContactPoint = new List<ContactPoint>();
            foreach (ContactPoint cp in currentCollision.contacts)
            {
                if (filteredContactPoint.FindAll(cp2 => Vector3.Distance(cp.point, cp2.point) <= minContactPointDistance).Count == 0)
                    filteredContactPoint.Add(cp);
            }


            foreach (ContactPoint contactPoint in filteredContactPoint)
            {
                Vector3 vectorToHandle = contactPoint.point - handleTransform.position;
                Vector3 normalToContact = -contactPoint.normal; // minus because you need the normal to point to the contact point

                VectorManager.DrawVectorS(contactPoint.point, normalToContact, Color.red, "normalToContact"); //TODO to remove : verbose
                VectorManager.DrawVectorS(handleTransform.position, vectorToHandle, Color.magenta, "vectorToHandle");//TODO to remove : verbose

                Vector3 localTorque = Vector3.Cross(vectorToHandle, normalToContact);

                torques += localTorque;
            }
            torques *= forces.magnitude * stiffnessTorque;
            torques /= currentCollision.contactCount;
        }

        Debug.Log($"torques = {torques}");//TODO to remove : verbose

        return (forces, torques);
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
        (Vector3 position, Quaternion rotation) = rc.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        Vector3 normal = rc.target.transform.position - rc.targetRigidbody.position;
        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene 
        Vector3 newPosition = rc.GetPosition() + (rc.infoCollision.IsCollided ? rc.stiffness * normal : Vector3.zero);
        Quaternion newRotation = rc.GetRotation();

        return (newPosition, newRotation);
    }
}