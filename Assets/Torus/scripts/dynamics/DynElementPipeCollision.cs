using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynElementPipeCollision : DynCollision
{
    private Collision  collision;
    private DynElement element;

    private float interpenetrationDist;
    private Vector3 collisionNormal;

    public DynElementPipeCollision(float _restitution, Collision _collision, DynElement _element) : base(_restitution)
    {
        collision = _collision;
        element   = _element;

        postProcessingCollision();
    }

    private void postProcessingCollision()
    {
        // Compute interpenetration distance
        interpenetrationDist = 0.0f;
        {
            foreach (ContactPoint cp in collision.contacts)
                interpenetrationDist += cp.separation;
            interpenetrationDist /= collision.contactCount;
        }

        // Compute the collision normal
        collisionNormal = Vector3.zero;
        if(true){
            foreach (ContactPoint cp in collision.contacts)
                collisionNormal += cp.normal;
            collisionNormal /= -collision.contactCount;
        }else
        {
            collisionNormal = - element.Velocity.normalized;
        }

    }

    public override void SolveCollision()
    {
        Debug.Log("DynElementPipeCollision.SolveCollision");

        // Don't process fixed element, assuming that the pipe is fixed
        if (element.IsFixed) return;

        // Project element along the normal vector
        Vector3 previousPosition = element.Position;
        Vector3 nextPosition = previousPosition + interpenetrationDist * collisionNormal;
        element.Position = nextPosition;

        // Compute post-collision velocity
        element.Velocity -= (1 + restitution) * Vector3.Dot(element.Velocity, collisionNormal) * collisionNormal;
    }
}
