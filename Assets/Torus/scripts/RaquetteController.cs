using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public List<Renderer> renderers;

    public enum SolverStr
    {
        Default, Block, PhysicSimulate, OpenGLSolver, CopieTransform, ForceTorque
    }
    [Header("Solver str")]
    public SolverStr SolverStrategy;
    private IReactionStr str;
    public float MAX_DISPLACEMENT = 0.05f;
    public float MAX_ROTATION     = 0.05f;
    public float MAX_FORCE        = 30;
    public float MAX_TORQUE       = 2f;
    [Space(10)]

    #region oldVar
    public VirtuoseManager vm;
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

    public (Vector3 Position, Quaternion Rotation) GetVirtuosePose()
    {
        return vm.Virtuose.Pose;
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
            default:
                return new DefaultStr(this);
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

    /// <summary>
    /// the old SetRigidbodyPositions as a save
    /// </summary>
    private void SetRigidbodyPositionsSAVE()
    {
        if (target != null)
        {
            (Vector3 position, Quaternion rotation) = vm.Virtuose.Pose;
            (position, rotation) = Utils.V2UPosRot(position, rotation);

            targetRigidbody.MovePosition(position);
            targetRigidbody.MoveRotation(rotation);

            float distance = 0;
            float dot = 0;

            Vector3 normal = target.transform.position - targetRigidbody.position;
            //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene physic.
            Vector3 oldPosition = infoCollision.IsCollided ? target.transform.position : targetRigidbody.position;
            Vector3 newPosition = infoCollision.IsCollided ? target.transform.position + stiffness * normal : targetRigidbody.position;
            Quaternion newRotation = infoCollision.IsCollided ? target.transform.rotation : targetRigidbody.rotation;

            distance = Vector3.Distance(oldPosition, newPosition);

            Vector3 displacementClamped = Utils.ClampDisplacement(newPosition - position, MAX_DISPLACEMENT);
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
}
