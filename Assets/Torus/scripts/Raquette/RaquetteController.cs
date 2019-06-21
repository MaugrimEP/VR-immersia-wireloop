using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public List<Renderer> renderers;
    private Transform handleTransform;

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
    public void HandleCollisionEnter(Collision collision)
    {
        UpdateChildOnEnter(collision);
        str.HandleCollisionEnter(collision);
    }

    public void HandleCollisionExit(Collision collision)
    {
        UpdateChildOnLeave(collision);
        str.HandleCollisionExit(collision);
    }

    public void HandleCollisionStay(Collision collision)
    {
        UpdateChildOnStay(collision);
        str.HandleCollisionStay(collision);
    }
    #endregion

    #region change the apparence of the raquette when interacting with the pipe
    private void UpdateChildOnStay(Collision collision)
    {
        foreach (Renderer r in renderers)
            r.material.color = Color.red;
    }

    private void UpdateChildOnEnter(Collision collision)
    {
        foreach (Renderer r in renderers)
            r.material.color = Color.red;

        foreach (ContactPoint cp in collision.contacts)
        {
            ElectricityManager.DrawElectricityS(handleTransform, cp.point);
        }
    }

    private void UpdateChildOnLeave(Collision collision)
    {
        if (!infoCollision.IsCollided)
        {
            foreach (Renderer r in renderers)
                r.material.color = Color.green;
        }

        ElectricityManager.ClearS();
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

        if (ic.IsScaleOne())//for the scale one the raquette need to be rotated 90° on X
        {
            foreach (Transform transform in ListToRotateOnScaleOne)
                transform.rotation *= Quaternion.Euler(Vector3.right * 90);
        }

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
