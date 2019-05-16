using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtuoseTargetCollision : MonoBehaviour
{
    public VirtuoseManager vm;

    public GameObject target;

    public float stiffness = 10;

    Rigidbody targetRigidbody;
    InfoCollision infoCollision;

    Vector3 lastFramePosition;
    Quaternion lastFrameRotation;

    public float[] forces = { 0, 0, 0, 0, 0, 0 };

    void Reset()
    {
        vm = GetComponent<VirtuoseManager>();
    }

    private void Start()
    {
        if (target)
        {
            targetRigidbody = target.GetComponentInChildren<Rigidbody>();
            targetRigidbody.LogErrorIfNull();

            infoCollision = target.GetComponentInChildren<InfoCollision>();
            infoCollision.LogErrorIfNull();

            StartCoroutine(vm.WaitVirtuoseConnexion(Init));    
        }
    }

    private void Init()
    {
        var pose = vm.Virtuose.Pose;
        targetRigidbody.position = pose.position;
        targetRigidbody.rotation = pose.rotation;

        lastFramePosition = pose.position;
        lastFrameRotation = pose.rotation;
    }

    private void Update()
    {
        if (vm.CommandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE && vm.Arm.IsConnected)
            SetForce();
    }

    void FixedUpdate()
    {
        if (VRTools.IsMaster() && vm.Initialized && target)
        {
            //VirtuoseAPI.VirtCommandType.COMMAND_TYPE_ADMITTANCE is deprecated
            if (vm.Arm.IsConnected
                && vm.CommandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
            {
                // GetPositions();
                //SetPositions();
                SetRigidbodyPositions();
            }
        }
    }

    void SetForce()
    {
        vm.Virtuose.Force = forces;
    }
    

    void SetTargetPositions()
    {
        var pose = vm.Virtuose.Pose;

        if (target != null)
        {
            target.transform.SetPose(pose);
        }
    }

    void SetRigidbodyPositions()
    {
        if (target != null)
        {
            (Vector3 position, Quaternion rotation) = vm.Virtuose.Pose;

            targetRigidbody.MovePosition(position);
            targetRigidbody.MoveRotation(rotation);

            float distance = 0;
            float dot = 0;

            Vector3 normal = target.transform.position - targetRigidbody.position;
            //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene physic.
            Vector3 newPosition = infoCollision.IsCollided ? target.transform.position + stiffness * normal : targetRigidbody.position;
            Quaternion newRotation = infoCollision.IsCollided ? target.transform.rotation : targetRigidbody.rotation;

            distance = Vector3.Distance(position, newPosition);
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

            vm.Virtuose.Pose = (newPosition, newRotation);

            lastFramePosition = position;
            lastFrameRotation = rotation;
        }
    }
}
