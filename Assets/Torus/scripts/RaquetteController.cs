using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public InputController avatarInputController;
    public List<Renderer> renderers;

    public enum SolverStr
    {
        Default, Block, PhysicSimulate
    }
    [Header("Solver str")]
    public SolverStr SolverStrategy;
    private IReactionStr str;
    public float MAX_CLAMP = 0.05f;
    [Space(10)]

    #region oldVar
    public VirtuoseManager vm;
    public GameObject target;
    [HideInInspector]
    public Rigidbody targetRigidbody;
    [HideInInspector]
    public float stiffness = 1;
    [HideInInspector]
    public RaquetteCollider infoCollision;
    [HideInInspector]
    public Vector3 lastFramePosition;
    [HideInInspector]
    public Quaternion lastFrameRotation;

    public float[] forces = { 0, 0, 0, 0, 0, 0 };
    #endregion

    #region handle collision behaviour
    public void HandleCollisionEnter(Collision collision)
    {
        UpdateChildOnTouch();
    }


    public void HandleCollisionExit(Collision collision)
    {
        UpdateChildOnLeave();
    }

    public void HandleCollisionStay(Collision collision)
    {
    }
    #endregion

    #region change the apparence of the raquette when interacting with the pipe
    private void UpdateChildOnTouch()
    {
        foreach (Renderer r in renderers)
            r.material.color = Color.red;
    }

    private void UpdateChildOnLeave()
    {
        foreach (Renderer r in renderers)
            r.material.color = Color.green;
    }
    #endregion

    public (Vector3 Position, Quaternion Rotation) GetVirtuosePose()
    {
        return vm.Virtuose.Pose;
    }

    private void Reset()
    {
        vm = GetComponent<VirtuoseManager>();
    }

    private IReactionStr GetStr()
    {
        switch (SolverStrategy)
        {
            case SolverStr.Default:
                return new DefaultStr();
            case SolverStr.Block:
                return new BlockStr();
            default:
                return new DefaultStr();
        }
    }

    private void Start()
    {
        str = GetStr();
        if (target)
        {
            targetRigidbody = target.GetComponentInChildren<Rigidbody>();
            targetRigidbody.LogErrorIfNull();

            infoCollision = target.GetComponentInChildren<RaquetteCollider>();
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

    private void FixedUpdate()
    {
        if (VRTools.IsMaster() && vm.Initialized && target)
        {
            if (vm.Arm.IsConnected
                && vm.CommandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
            {
                SetRigidbodyPositions();
            }
        }
    }

    private void SetForce()
    {
        vm.Virtuose.Force = forces;
    }

    private void SetTargetPositions()
    {
        var pose = vm.Virtuose.Pose;

        if (target != null)
        {
            target.transform.SetPose(pose);
        }
    }
 
    private void SetRigidbodyPositions()
    {
        if (target != null)
        {
            (Vector3 READposition, Quaternion READrotation) = vm.Virtuose.Pose;

            (Vector3 solvedNextPosition, Quaternion solvedNextRotation) = str.Solve(this);

            Vector3 clampedDisplacementVector = Utils.ClampDisplacement(solvedNextPosition - READposition, MAX_CLAMP);

            float distance = Vector3.Distance(READposition, clampedDisplacementVector);
            float dot = Quaternion.Dot(READrotation, solvedNextRotation);
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

            vm.Virtuose.Pose = (clampedDisplacementVector, solvedNextRotation);

            lastFramePosition = READposition;
            lastFrameRotation = READrotation;
        }
    }
}
