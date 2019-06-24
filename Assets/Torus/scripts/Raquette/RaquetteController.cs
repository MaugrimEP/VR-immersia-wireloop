using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    private Transform handleTransform;
    public RaquetteApparence raquetteApparence;
    public List<Transform> ListToRotateOnScaleOne;

    public enum SolverStr
    {
        CopieTransform, ForceTorque
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
    public void HandleCollisionEnter(List<Collision> collisions)
    {
        raquetteApparence.UpdateChildOnEnter(collisions);
        str.HandleCollisionEnter(collisions);
    }

    public void HandleCollisionExit(List<Collision> collisions)
    {
        raquetteApparence.UpdateChildOnLeave(collisions);
        str.HandleCollisionExit(collisions);
    }

    public void HandleCollisionStay(List<Collision> collisions)
    {
        raquetteApparence.UpdateChildOnStay(collisions);
        str.HandleCollisionStay(collisions);
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
            case SolverStr.CopieTransform:
                return new CopieStr(this);
            case SolverStr.ForceTorque:
                return new ForceTorque(this);
            default:
                return new CopieStr(this);
        }
    }

    private void Start()
    {
        ic = GetComponent<InputController>();
        vm = GetComponent<VirtuoseManager>();
        handleTransform = GameObject.Find("handlePosition").GetComponent<Transform>();
        str = GetStr();

        if (target)
        {
            targetRigidbody = target.GetComponentInChildren<Rigidbody>();

            if (!ic.UseVirtuose())
            {
                targetRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            targetRigidbody.LogErrorIfNull();

            infoCollision = target.GetComponentInChildren<RaquetteCollider>();
            infoCollision.LogErrorIfNull();

            if (ic.UseVirtuose())
                StartCoroutine(vm.WaitVirtuoseConnexion(Init));
        }
    }

    private void Update()
    {
        if (vm.CommandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE && vm.Arm.IsConnected)
            SetForce();
    }

    private void SetForce()
    {
        vm.Virtuose.Force = forces;
    }


    private void Init()
    {
        var pose = ic.GetVirtuosePose();
        targetRigidbody.position = pose.Position;
        targetRigidbody.rotation = pose.Rotation;

        lastFramePosition = pose.Position;
        lastFrameRotation = pose.Rotation;
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
        str.ComputeSimulationStep();
        ic.SetSpeedIdentity();
    }
}
