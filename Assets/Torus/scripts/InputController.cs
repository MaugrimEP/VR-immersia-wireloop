using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    private VirtuoseManager virtuoseManager;
    public ArmSelection armSelection;

    /// <summary>
    /// Object that will be controlled by the virtuose
    /// </summary>
    public Transform objectToMove;

    public VirtuoseAPI.VirtCommandType modeVirtuose;

    #region value (to add if position and rotation or raw for the force and torque) to the virtuose input for update, they should be in unity coordinate system
    [HideInInspector]
    public Vector3 ForceOutput;
    [HideInInspector]
    public Vector3 TorqueOutput;
    [HideInInspector]
    public Vector3 PositionOffset;
    [HideInInspector]
    public Quaternion RotationOffset;
    #endregion

    #region value read from the virtuose, they should be in unity coordinate system
    [HideInInspector]
    public Vector3 virtuose_Position;
    [HideInInspector]
    public Quaternion virtuose_Rotation;
    [HideInInspector]
    public Vector3 virtuose_Force;
    [HideInInspector]
    public Vector3 virtuose_Torque;
    #endregion

    [Range(VirtuoseAPIHelper.MIN_MASS, VirtuoseAPIHelper.MAX_MASS)]
    public float mass    = 0.2f;
    [Range(VirtuoseAPIHelper.MIN_INERTIE, VirtuoseAPIHelper.MAX_INERTIE)]
    public float inertie = 0f;

    private string GetIP()
    {
        switch (armSelection)
        {
            case ArmSelection.Unity:
                return "";
            case ArmSelection.Simulator:
                return "127.0.0.1";
            case ArmSelection.SingleArm125:
                return "131.254.18.52#5125";
            case ArmSelection.SingleArm126:
                return "131.254.18.52#5126";
            default:
                return "";
        }
    }

    public bool UseVirtuose()
    {
        return armSelection != ArmSelection.Unity;
    }

    void Start () {
        {//init the virtuoseManager component
            virtuoseManager = gameObject.GetComponent<VirtuoseManager>();
            virtuoseManager.mass = mass;
            virtuoseManager.inertie = inertie;
            virtuoseManager.BaseFramePosition = Vector3.zero;
            virtuoseManager.powerOnKey = KeyCode.P;
            virtuoseManager.CommandType = modeVirtuose;
            virtuoseManager.Arm.Ip = GetIP();

        }
        {//init the offset
            PositionOffset = Vector3.zero;
            RotationOffset = Quaternion.identity;
        }
    }

    #region input/output with virtuose
    /// <summary>
    /// Get the force and torque value from the virtuose
    /// </summary>
    /// <returns></returns>
    private (Vector3 force, Vector3 torque) GetForceAndTorque()
    {
        float[] forcesAndTorque = virtuoseManager.Virtuose.Force;
        return V2UForceTorque(new Vector3(forcesAndTorque[0], forcesAndTorque[1], forcesAndTorque[2]), new Vector3(forcesAndTorque[3], forcesAndTorque[4], forcesAndTorque[5]));
    }

    /// <summary>
    /// Get the Position and Rotation value from the virtuose
    /// </summary>
    /// <returns></returns>
    private (Vector3 position, Quaternion rotation) GetPositionAndRotation()
    {
        return V2UPosRot(virtuoseManager.Virtuose.Pose.position, virtuoseManager.Virtuose.Pose.rotation);
    }

    /// <summary>
    /// Set the Force and Torque value for the virtuose
    /// </summary>
    /// <param name="force"></param>
    /// <param name="torque"></param>
    private void SetForceAndTorque(Vector3 force, Vector3 torque)
    {
        (force, torque) = U2VForceTorque(force, torque);
        float[] forces = { force.x, force.y, force.z, torque.x, torque.y, torque.z };
        virtuoseManager.Virtuose.Force = forces;
    }

    /// <summary>
    /// Set the Position and Rotation for the virtuose
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    private void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        (position, rotation) = U2VPosRot(position, rotation);
        virtuoseManager.Virtuose.Pose = (position, rotation);
    }
    #endregion

    void Update () {

        if (UseVirtuose() && virtuoseManager.Initialized && virtuoseManager.Arm.IsConnected)
        {
            FetchVirtuoseValue();
            HandleVirtuoseInput();
            OutputToVirtuose();
            //ResetOffsetToVirtuose();
        }
    }

    /// <summary>
    /// Reset manually the offset to add to the virtuose datas
    /// </summary>
    public void ResetOffsetToVirtuose()
    {
        ForceOutput    = Vector3.zero;
        TorqueOutput   = Vector3.zero;
        PositionOffset = Vector3.zero;
        RotationOffset = Quaternion.identity;
    }

    /// <summary>
    /// Read the virtuose value and put them in the class attribut
    /// </summary>
    private void FetchVirtuoseValue()
    {
        (virtuose_Position, virtuose_Rotation) = GetPositionAndRotation();

        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
        {
            (virtuose_Force, virtuose_Torque) = GetForceAndTorque();
        }
    }

    private void HandleVirtuoseInput()
    {
        Vector3 transformedPosition = virtuose_Position;
        Quaternion transformedQuaternion = virtuose_Rotation;

        Transform objectMoved = GetTransformToMove();
        objectMoved.transform.position = transformedPosition;
        objectMoved.rotation = transformedQuaternion;
    }

    private void OutputToVirtuose()
    {
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE)
        {
            SetForceAndTorque(ForceOutput, TorqueOutput);
            Debug.Log($"ForceOutput : {ForceOutput}, TorqueOutput : {TorqueOutput}");
        }
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
        {
            Debug.Log($"Offset position : {PositionOffset.ToString("F3")}, offset rotation {RotationOffset}");
            SetPositionAndRotation(virtuose_Position + PositionOffset, virtuose_Rotation * RotationOffset);
        }
        ResetOffsetToVirtuose();
    }

    private Transform GetTransformToMove()
    {
        return transform;
    }

    /// <summary>
    /// Enum to choose what mode we are in, Unity to dont interact, Simulator to connect with the virtuose simulator, SingleArm to connect with the virtuose arm
    /// </summary>
    public enum ArmSelection
    {
        Unity, Simulator, SingleArm125, SingleArm126
    }

    #region Wrapper on virtuose input/output
    private (Vector3 Position, Quaternion Rotation) V2UPosRot(Vector3 Position, Quaternion Rotation)
    {
        return (new Vector3(Position.x, Position.y, Position.z), new Quaternion(Rotation.x, Rotation.y, Rotation.z, Rotation.w));
    }

    private (Vector3 Force, Vector3 Torque) V2UForceTorque(Vector3 Force, Vector3 Torque)
    {
        return (Force, Torque);
    }

    private (Vector3 Position, Quaternion Rotation) U2VPosRot(Vector3 Position, Quaternion Rotation)
    {
        return (new Vector3(Position.x, Position.y, Position.z), new Quaternion(Rotation.x, Rotation.y, Rotation.z, Rotation.w));
    }

    private (Vector3 Force, Vector3 Torque) U2VForceTorque(Vector3 Force, Vector3 Torque)
    {
        return (Force, Torque);
    }
    #endregion
}
