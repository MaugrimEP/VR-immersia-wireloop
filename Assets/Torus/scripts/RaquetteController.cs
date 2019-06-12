using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public List<Renderer> renderers;

    public enum SolverStr
    {
        Default, Block, PhysicSimulate, OpenGLSolver, CopieTransform, ForceTorque, ForceRotationStr
    }
    [Header("Solver str")]
    public SolverStr SolverStrategy;
    private IReactionStr str;
    public float MAX_DISPLACEMENT = 0.05f;
    public float MAX_ROTATION = 0.05f;
    public float MAX_FORCE = 30;
    public float MAX_TORQUE = 2f;
    [Space(10)]

    #region oldVar
    private VirtuoseManager vm;
    public GameObject target;
    [HideInInspector]
    public Rigidbody targetRigidbody;
    public float stiffness = 10;
    [HideInInspector]
    public RaquetteCollider infoCollision;
    [HideInInspector]
    public Vector3 lastFramePosition;
    [HideInInspector]
    public Quaternion lastFrameRotation;

    public float[] forces = { 0, 0, 0, 0, 0, 0 };
    #endregion
    [HideInInspector]
    public InputController ic;

    #region handle collision behaviour
    public void HandleCollisionEnter(Collision collision)
    {
        UpdateChildOnTouch();
        str.HandleCollisionEnter(collision);
    }

    public void HandleCollisionExit(Collision collision)
    {
        UpdateChildOnLeave();
        str.HandleCollisionExit(collision);
    }

    public void HandleCollisionStay(Collision collision)
    {
        UpdateChildOnTouch();
        str.HandleCollisionStay(collision);
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

    /// <summary>
    /// return True if we should send force/torque to the virtuose, return False if
    /// we should send position and rotations
    /// </summary>
    /// <returns></returns>
    public bool SendForce()
    {
        return SolverStrategy == SolverStr.ForceTorque;
    }

    public bool IsColliding()
    {
        return infoCollision.IsCollided;
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
                return new DefaultStr(this);
            case SolverStr.Block:
                return new BlockStr(this);
            case SolverStr.PhysicSimulate:
                return new PhysicSimulate(this);
            case SolverStr.OpenGLSolver:
                return new OpenGLSolver(this);
            case SolverStr.CopieTransform:
                return new CopieStr(this);
            case SolverStr.ForceTorque:
                return new ForceTorque(this);
            case SolverStr.ForceRotationStr:
                return new ForceRotationStr(this);
            default:
                return new DefaultStr(this);
        }
    }

    private void Start()
    {
        ic = GetComponent<InputController>();
        vm = GetComponent<VirtuoseManager>();
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

    public Vector3 GetPosition()
    {
        return infoCollision.IsCollided ? target.transform.position : targetRigidbody.position;
    }

    public Quaternion GetRotation()
    {
        return infoCollision.IsCollided ? target.transform.rotation : targetRigidbody.rotation;
    }

    /// <summary>
    /// Handle collision process and solve the collision
    /// </summary>
    private void SetRigidbodyPositions()
    {
        if (target == null) return;

        str.ComputeSimulationStep();
    }
}
