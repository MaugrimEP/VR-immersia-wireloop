using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Move carrier base on the handle position.
/// </summary>
public class VirtuoseBubbleNavigation : MonoBehaviour
{
    public VirtuoseManager vm;

    [Range(0, 2)]
    public float DistanceMultiplier = 1;

    public float SpeedMultiplier = 1;
    public float BubbleCenter = 0.6f;
    public float ThresholdDistanceInf = 0.15f;
    public float ThresholdDistanceSup = 0.18f;
    public float MaxDistance = 0.25f;
    public bool NeedButton = false;

    public AnimationCurve DampingDistance;
    public bool HasDamping = true;

    /// <summary>
    /// To fix a bug where physical handle pose is not at the good position.
    /// </summary>
    public Vector3 PhysicalPoseOffset;

    Vector2 wantedPosition;
    (Vector3, Quaternion) currentPose;
    bool isMoving = false;

    Vector2 lastPosePosition;
    Vector2 lastCarrierPosition;

    Queue<Vector2> lastPoseSpeeds;
    Queue<Vector2> lastCarrierSpeeds;
    public int filteredValue = 10;

    void Start()
    {
        lastPoseSpeeds = new Queue<Vector2>();
        lastCarrierSpeeds = new Queue<Vector2>();

        if (!vm)
            vm = GetComponent<VirtuoseManager>();
        vm.LogErrorIfNull();
    }

    void Update()
    {
        if (vm.Arm.IsConnected)
        {
            Vector3 posePosition = vm.Virtuose.Pose.position;
            Vector2 carrierPosition = vm.Virtuose.Scale1CarrierPosition;

            float[] articularsSpeed = vm.Virtuose.ArticularsSpeed;

            //Vector2 poseSpeed = new Vector2(
            //    (posePosition.x - lastPosePosition.x) / VRTools.GetDeltaTime(),
            //    (posePosition.z - lastPosePosition.y) / VRTools.GetDeltaTime());

            //Vector2 carrierSpeed = new Vector2(
            //    (carrierPosition.x - lastCarrierPosition.x) / VRTools.GetDeltaTime(),
            //    (carrierPosition.y - lastCarrierPosition.y) / VRTools.GetDeltaTime());

            Vector2 carrierSpeed = new Vector2(-articularsSpeed[1], articularsSpeed[0]);
            Vector2 poseSpeed = new Vector2(vm.Virtuose.Speed[1], -vm.Virtuose.Speed[0]);

            if (lastPoseSpeeds.Count > filteredValue)
            {
                lastPoseSpeeds.Dequeue();
                lastCarrierSpeeds.Dequeue();
            }
            lastPoseSpeeds.Enqueue(poseSpeed);
            lastCarrierSpeeds.Enqueue(carrierSpeed);

            if (lastCarrierSpeeds.Count != 0)
                carrierSpeed = AverageSpeed(lastCarrierSpeeds);

            if (lastPoseSpeeds.Count != 0)
                poseSpeed = AverageSpeed(lastPoseSpeeds);

            if (!NeedButton || vm.IsButtonPressed())
            {
                Vector3 bubble = vm.Virtuose.BubblePosition(BubbleCenter);
                Vector3 physical = vm.Virtuose.PhysicalPose.position;

                Vector3 difference = physical + PhysicalPoseOffset - bubble;
                difference.y = 0;
                float differenceMagnitude = difference.magnitude;
                if (differenceMagnitude > ThresholdDistanceSup)
                {
                    Vector2 carrierOffset = new Vector2(difference.x, difference.z);
                    carrierOffset = carrierOffset.normalized * Mathf.Min(MaxDistance, differenceMagnitude);

                    //SpeedMultiplier = DampingDistance.Evaluate((poseSpeed - carrierSpeed).magnitude);
                    bool damp = false;

                    if (HasDamping)
                    {
                        if ((Mathf.Abs(poseSpeed.x) < Mathf.Abs(carrierSpeed.x) && (Mathf.Sign(poseSpeed.x) == Mathf.Sign(carrierSpeed.x) || Mathf.Abs(poseSpeed.x) < 0.1f))
                        // ||  (Mathf.Abs(carrierSpeed.x) > 0.2f))
                        )
                        {
                            carrierOffset.x *= DampingDistance.Evaluate(Mathf.Abs(poseSpeed.x - carrierSpeed.x));
                            damp = true;
                        }
                        if (Mathf.Abs(poseSpeed.y) < Mathf.Abs(carrierSpeed.y) && Mathf.Sign(poseSpeed.y) == Mathf.Sign(carrierSpeed.y))
                            carrierOffset.y *= DampingDistance.Evaluate(Mathf.Abs(poseSpeed.y - carrierSpeed.y));
                    }

                    wantedPosition = carrierPosition + carrierOffset * DistanceMultiplier;

                    vm.Virtuose.Scale1CarrierPosition = wantedPosition;
                    currentPose = vm.Virtuose.Pose;
                    isMoving = true;
                }
                //  else if (diff.magnitude > ThresholdDistanceInf) { }
                else
                {
                    // vm.Virtuose.Pose = vm.Virtuose.Pose;
                    vm.Virtuose.Scale1CarrierPosition = carrierPosition;
                    currentPose = (Vector3.zero, Quaternion.identity);
                    isMoving = false;
                }

                float damping = DampingDistance.Evaluate(difference.magnitude);
                for (int a = 0; a < articularsSpeed.Length; a++)
                    articularsSpeed[a] *= SpeedMultiplier * (HasDamping ? damping : 1);
                vm.Virtuose.ArticularsSpeed = articularsSpeed;
                //vm.Virtuose.Speed = vm.Virtuose.Speed;
            }
            else
            {
                vm.Virtuose.Scale1CarrierPosition = carrierPosition;
                vm.Virtuose.ArticularsSpeed = vm.Virtuose.ArticularsSpeed;
            }
            lastPosePosition = new Vector2(posePosition.x, posePosition.z);
            lastCarrierPosition = carrierPosition;
        }
    }

    Vector2 AverageSpeed(Queue<Vector2> queues)
    {
        Vector2 averageSpeed = Vector2.zero;
        foreach (Vector2 speed in queues)
            averageSpeed += speed;
        return averageSpeed / queues.Count;
    }
}
