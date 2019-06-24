using System.Collections.Generic;
using UnityEngine;

public abstract class IReactionStr
{
    protected RaquetteController rc;
    protected InputController ic;
    protected List<Collision> currentCollisions;

    protected IReactionStr(RaquetteController rc)
    {
        this.rc = rc;
        this.ic = rc.ic;
    }

    public virtual void ComputeSimulationStep()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();

        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        ic.SetVirtuosePoseIdentity();

        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }

    protected virtual bool CheckTreshold(Vector3 oldPosition, Vector3 nextPosition, Quaternion oldRotation, Quaternion nextRotation)
    {
        float distance = Vector3.Distance(oldPosition, nextPosition);
        float dot = Quaternion.Dot(oldRotation, nextRotation);
        //Add extra protection to avoid high velocity movement.
        if (distance > VirtuoseAPIHelper.MAX_DISTANCE_PER_FRAME)
        {
            VRTools.LogError("[Warning][RaquetteController] Haption arm new position is aboved the authorized threshold distance (" + distance + ">" + VirtuoseAPIHelper.MAX_DISTANCE_PER_FRAME + "). Power off.");
        }
        if (dot < 1 - VirtuoseAPIHelper.MAX_DOT_DIFFERENCE)
        {
            VRTools.LogError("[Warning][RaquetteController] Haption arm new rotation is aboved authorized the threshold dot (" + (1 - dot) + " : " + VirtuoseAPIHelper.MAX_DOT_DIFFERENCE + "). Power off.");
        }
        return (dot < 1 - VirtuoseAPIHelper.MAX_DOT_DIFFERENCE) || (distance > VirtuoseAPIHelper.MAX_DISTANCE_PER_FRAME);
    }
    protected abstract (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation();
    protected abstract (Vector3 forces, Vector3 torques) SolveForceAndTorque();

    public virtual void HandleCollisionEnter(List<Collision> collisions)
    {
        currentCollisions = collisions;
    }
    public virtual void HandleCollisionStay(List<Collision> collisions)
    {
        currentCollisions = collisions;
    }
    public virtual void HandleCollisionExit(List<Collision> collisions)
    {
        currentCollisions = collisions;
    }
}