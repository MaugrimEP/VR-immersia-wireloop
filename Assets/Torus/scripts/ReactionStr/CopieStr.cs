using System.Collections.Generic;
using UnityEngine;

public class CopieStr : IReactionStr
{

    public CopieStr(RaquetteController rc) : base(rc)
    {
    }

    public override void ComputeSimulationStep()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();
        Vector3 oldPosition = rc.GetPosition();
        Quaternion oldRotation = rc.GetRotation();

        (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = SolvePositiondAndRotation();

        Vector3 displacementClamped = Utils.ClampDisplacement(solvedNextPosition - position, rc.MAX_DISPLACEMENT);
        solvedNextPosition = oldPosition + displacementClamped;

        if (Debug.isDebugBuild)
        {
            VectorManager.Clear();
            VectorManager.DrawSphereS(rc.targetRigidbody.position, Vector3.one * 0.03f, Color.black);
            VectorManager.DrawSphereS(rc.target.transform.position, Vector3.one * 0.03f, Color.yellow);
            VRTools.Log($"distence RB / target: {Vector3.Distance(rc.target.transform.position, rc.targetRigidbody.position)}"); //TODO remove Verbose
            Debug.DrawLine(position, position + (solvedNextPosition - position).normalized, Color.black);
        }


        if (rc.infoCollision.IsCollided)
        {
            ic.SetVirtuosePose(solvedNextPosition, solvedNextRotation);
        }
        else
        {
            //rc.target.transform.position = rc.targetRigidbody.position;//TODO check the utility, or the one below
            //rc.target.transform.rotation = rc.targetRigidbody.rotation;

            //rc.targetRigidbody.MovePosition(rc.target.transform.position);
            //rc.targetRigidbody.MoveRotation(rc.target.transform.rotation);

            ic.SetVirtuosePoseIdentity();
        }
        (rc.lastFramePosition, rc.lastFrameRotation) = (position, rotation);
    }



    protected override (Vector3 Position, Quaternion Rotation) SolvePositiondAndRotation()
    {
        (Vector3 position, Quaternion rotation) = ic.GetVirtuosePose();

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene 
        Vector3 newPosition = rc.target.transform.position;
        Quaternion newRotation = rc.target.transform.rotation;

        return (newPosition, newRotation);
    }

    private Quaternion U2VRotation(Quaternion URotation)
    {
        Vector3 eulerRotation = URotation.eulerAngles;
        return Quaternion.Euler(-eulerRotation.z, eulerRotation.x, eulerRotation.y);

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

    protected override (Vector3 forces, Vector3 torques) SolveForceAndTorque()
    {
        throw new System.NotImplementedException();
    }
}
