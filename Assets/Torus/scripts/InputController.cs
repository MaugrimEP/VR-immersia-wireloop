using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    public VirtuoseManager virtuoseManager;
    public ArmSelection armSelection;
    /// <summary>
    /// If HapticEnable is True, then the output to the virtuose will use the class value, if it's False, the input will be the output
    /// </summary>
    public bool HapticEnable;

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

    private void Awake () {
        if(UseVirtuose())
        {//init the virtuoseManager component
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
    
    private void Update () {

        if (UseVirtuose() && virtuoseManager.Initialized && virtuoseManager.Arm.IsConnected)
        {
            FetchVirtuoseValue();
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
    
    /// <summary>
    /// Write value to the virtuose
    /// </summary>
    private void OutputToVirtuose()
    {
        Vector3 forceApplied  = Vector3.zero;
        Vector3 torqueApplied = Vector3.zero;
        Vector3 positionApplied = virtuose_Position;
        Quaternion rotationApplied = virtuose_Rotation;

        if (HapticEnable)
        {
            forceApplied = ForceOutput;
            torqueApplied = TorqueOutput;
            positionApplied += PositionOffset;

            rotationApplied = virtuoseManager.Virtuose.Pose.rotation;//rotationApplied *= RotationOffset; //TODO : change the rotation
        }
        else
        {//we set the output as the input
            (positionApplied , rotationApplied) = virtuoseManager.Virtuose.Pose;
            float [] forces = virtuoseManager.Virtuose.Force;
            (forceApplied, torqueApplied) = (new Vector3(forces[0], forces[1], forces[2]), new Vector3(forces[3], forces[4], forces[5]) );
        }

        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE)
        {
            SetForceAndTorque(forceApplied, torqueApplied);
        }
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
        {
            SetPositionAndRotation(positionApplied, rotationApplied);
        }

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
        Vector3 newPosition = new Vector3(Position.x, Position.y, Position.z);
        Quaternion newRotation = new Quaternion( - Rotation.y, - Rotation.z, Rotation.x, Rotation.w);

        return (newPosition, newRotation);
    }

    private (Vector3 Force, Vector3 Torque) V2UForceTorque(Vector3 Force, Vector3 Torque)
    {
        return (Force, Torque);
    }

    private (Vector3 Position, Quaternion Rotation) U2VPosRot(Vector3 Position, Quaternion Rotation)
    {
        Vector3 newPosition = new Vector3(Position.x, Position.y, Position.z);
        Quaternion newRotation = new Quaternion(Rotation.x, Rotation.y, Rotation.x, Rotation.w);//TODO : change the rotation

        return (newPosition, Rotation);
    }

    private (Vector3 Force, Vector3 Torque) U2VForceTorque(Vector3 Force, Vector3 Torque)
    {
        return (new Vector3(-Force.z, Force.x, Force.y), new Vector3(Torque.z, Torque.x, Torque.y));
    }
    #endregion
}
