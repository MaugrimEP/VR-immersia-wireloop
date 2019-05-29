using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicSimulate : IReactionStr
{
    public (Vector3 Position, Quaternion Rotation) Solve(RaquetteController rc)
    {
        (Vector3 READposition, Quaternion READrotation) = rc.GetVirtuosePose();

        (Vector3 position, Quaternion rotation) = Utils.V2UPosRot(READposition, READrotation);


        return rc.GetVirtuosePose();
    }

    /*
     private void SetRigidbodyPositionsSAVE()
     {
         if (target != null)
         {
             (Vector3 position, Quaternion rotation) = vm.Virtuose.Pose;
             (position, rotation) = V2UPosRot(position, rotation);

             Vector3 rigidbodyOldPosition = targetRigidbody.position;
             Quaternion rigidbodyOldRotation = targetRigidbody.rotation;
             float travelDistance = Vector3.Distance(rigidbodyOldPosition, position);

             if (travelDistance < maximalDisplacementUnity)
             {
                 targetRigidbody.MovePosition(position);
                 targetRigidbody.MoveRotation(rotation);
             }
             else
             {

                 Debug.Log($"travelDistance < maximalDisplacementUnity : {travelDistance} < {maximalDisplacementUnity}");

                 Physics.autoSimulation = false;

                 //var declaration for the loop
                 bool previousIsCollided = infoCollision.IsCollided;
                 int stepNumber = Mathf.RoundToInt(travelDistance / distanceStep);
                 Vector3 previousPosition = rigidbodyOldPosition;
                 Quaternion previousRotation = rigidbodyOldRotation;
                 for (float i = 0; i < stepNumber; ++i)
                 {
                     Vector3 stepEndPosition = Vector3.Lerp(rigidbodyOldPosition, position, i / stepNumber);
                     Quaternion stepEndRotation = Quaternion.Lerp(rigidbodyOldRotation, rotation, i / stepNumber);
                     targetRigidbody.MovePosition(stepEndPosition);
                     targetRigidbody.MoveRotation(stepEndRotation);

                     Physics.Simulate(Time.fixedDeltaTime);

                     if (previousIsCollided && !infoCollision.IsCollided) // we just get out from a collision
                     {

                     }

                     if (!previousIsCollided && infoCollision.IsCollided)// we just get into collision
                     {// then we put the position as the previous position before the colliding
                         vm.Virtuose.Pose = (previousPosition, previousRotation);

                         lastFramePosition = position;
                         lastFrameRotation = rotation;

                         Debug.Log($"Displacement sized down from {travelDistance} to {Vector3.Distance(rigidbodyOldPosition, previousPosition)}");

                         return;
                     }

                     previousPosition = stepEndPosition;
                     previousRotation = stepEndRotation;
                     previousIsCollided = infoCollision.IsCollided;
                 }

                 Physics.autoSimulation = true;
             }


             float distance = 0;
             float dot = 0;

             Vector3 normal = target.transform.position - targetRigidbody.position;
             //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene physic.
             Vector3 oldPosition = infoCollision.IsCollided ? target.transform.position : targetRigidbody.position;
             Vector3 newPosition = infoCollision.IsCollided ? target.transform.position + stiffness * normal : targetRigidbody.position;
             Quaternion newRotation = infoCollision.IsCollided ? target.transform.rotation : targetRigidbody.rotation;

             distance = Vector3.Distance(oldPosition, newPosition);

             Vector3 displacementClamped = ClampDisplacement(newPosition - position);
             newPosition = oldPosition + displacementClamped;

             dot = Quaternion.Dot(rotation, newRotation);

             //Add extra protection to avoid high velocity movement.
             if (distance > VirtuoseAPIHelper.MAX_DISTANCE_PER_FRAME)
             {
                 VRTools.LogWarning("[Warning][VirtuoseTargetCollision] Haption arm new position is aboved the authorized threshold distance (" + distance + ">" + VirtuoseAPIHelper.MAX_DISTANCE_PER_FRAME + "). Power off.");
                 vm.Virtuose.Power = false;
             }

             if (dot < 1 - VirtuoseAPIHelper.MAX_DOT_DIFFERENCE)
             {
                 VRTools.LogWarning("[Warning][VirtuoseManager] Haption arm new rotation is aboved authorized the threshold dot (" + (1 - dot) + " : " + VirtuoseAPIHelper.MAX_DOT_DIFFERENCE + "). Power off.");
                 vm.Virtuose.Power = false;
             }

             vm.Virtuose.Pose = (newPosition, vm.Virtuose.Pose.rotation);

             lastFramePosition = position;
             lastFrameRotation = rotation;
         }
     }
     */

}
