using System.Collections.Generic;
using UnityEngine;

public class OpenGLSolver : IReactionStr
{
    private class CollisionData
    {
        public Collision collision;
        public float restitution;
        public Transform transform;

        public CollisionData(Collision collision, float restitution, Transform transform)
        {
            this.collision = collision;
            this.restitution = restitution;
            this.transform = transform;
        }
    }

    private LinkedList<CollisionData> collidingList;

    public OpenGLSolver(RaquetteController rc) : base(rc)
    {
        collidingList = new LinkedList<CollisionData>();
    }

    public override void HandleCollisionEnter(Collision collision)
    {
        base.HandleCollisionEnter(collision);
        collidingList.AddLast(new CollisionData(collision, 1.0f, rc.transform));
    }

    public override void HandleCollisionExit(Collision collision)
    {
        base.HandleCollisionExit(collision);
    }

    public override void HandleCollisionStay(Collision collision)
    {
        base.HandleCollisionStay(collision);
        collidingList.AddLast(new CollisionData(collision, 1.0f, rc.transform));
    }

    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        Vector3 solvedPosition = rc.GetPosition();
        Quaternion solvedRotation = rc.GetRotation();

        while (collidingList.Count != 0)
        {
            CollisionData collision = collidingList.Last.Value;
            (Vector3 displacement, Quaternion rotation) = Solve(collision);

            solvedPosition += displacement;
            solvedRotation *= rotation; 

            collidingList.RemoveLast();
        }

        rc.targetRigidbody.MovePosition(solvedPosition);
        rc.targetRigidbody.MoveRotation(solvedRotation);

        return (solvedPosition, solvedRotation);
    }

    private (Vector3 displacement,Quaternion rotation) Solve(CollisionData collisionData)
    {
        // Compute interpenetration distance
        float interpenetrationDist = 0.0f;
        {
            foreach (ContactPoint cp in collisionData.collision.contacts)
                interpenetrationDist += cp.separation;
            interpenetrationDist /= collisionData.collision.contactCount;
        }

        // Compute the collision normal
        Vector3 collisionNormal = Vector3.zero;
        if (true)
        {
            foreach (ContactPoint cp in collisionData.collision.contacts)
                collisionNormal += cp.normal;
            collisionNormal /= -collisionData.collision.contactCount;
        }
        else
        {
            collisionNormal = -rc.targetRigidbody.velocity.normalized;
        }

        // Project element along the normal vector
        Vector3 previousPosition = collisionData.transform.position;
        Vector3 displacement = interpenetrationDist * collisionNormal;

        return (displacement, Quaternion.identity);
    }

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        throw new System.NotImplementedException();
    }
}
