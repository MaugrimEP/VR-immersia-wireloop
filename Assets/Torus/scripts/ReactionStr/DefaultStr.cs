using UnityEngine;

public class DefaultStr : IReactionStr
{
    public static float MAX_CLAMP = 0.05f;

    public (Vector3 Position, Quaternion Rotation) Solve(RaquetteController rc)
    {
        (Vector3 position, Quaternion rotation) = rc.vm.Virtuose.Pose;
        (position, rotation) = Utils.V2UPosRot(position, rotation);

        rc.targetRigidbody.MovePosition(position);
        rc.targetRigidbody.MoveRotation(rotation);

        Vector3 normal = rc.target.transform.position - rc.targetRigidbody.position;
        //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene physic.
        Vector3 newPosition = rc.infoCollision.IsCollided ? rc.target.transform.position + rc.stiffness * normal : rc.targetRigidbody.position;
        Quaternion newRotation = rc.infoCollision.IsCollided ? rc.target.transform.rotation : rc.targetRigidbody.rotation;

        return (newPosition, rotation);
    }
}
