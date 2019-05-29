using UnityEngine;

public abstract class IReactionStr
{
    protected RaquetteController rc;

    protected IReactionStr(RaquetteController rc)
    {
        this.rc = rc;
    }

    public virtual void ComputeSimulationStep()
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

        rc.vm.Virtuose.Pose = (solvedNextPosition, solvedNextRotation.normalized);

        #region verbose mode
        if (false)
        {
            Vector3 displacement = solvedNextPosition - position;
            if (rc.infoCollision.IsCollided)
            {
                Debug.Log($"déplacement : {displacement}");
                VectorManager.VECTOR_MANAGER.DrawVector(position, displacement, Color.magenta);
            }
        }
        #endregion

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
    protected abstract (Vector3 Position, Quaternion Rotation) Solve();

    public abstract void HandleCollisionEnter(Collision collision);
    public abstract void HandleCollisionStay(Collision collision);
    public abstract void HandleCollisionExit(Collision collision);
}