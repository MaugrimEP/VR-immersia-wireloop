using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    VirtuoseArm arm;
    VirtuoseAPIHelper helper;
    private bool virtuose;

    public ArmSelection armSelection;

    public VirtuoseAPI.VirtCommandType modeVirtuose;
    //value (to add if position and rotation or raw for the force and torque) to the virtuose input for update
    [HideInInspector]
    public Vector3 Force;
    [HideInInspector]
    public Vector3 Torque;
    [HideInInspector]
    public Vector3 Position;
    [HideInInspector]
    public Quaternion Rotation;

    //value read from the virtuose 
    [HideInInspector]
    public Vector3 virtuose_Position;
    [HideInInspector]
    public Quaternion virtuose_Rotation;
    [HideInInspector]
    public Vector3 virtuose_Force;
    [HideInInspector]
    public Vector3 virtuose_Torque;

    private string GetIP()
    {
        switch (armSelection)
        {
            case ArmSelection.Unity:
                return "";
            case ArmSelection.Simulator:
                return "127.0.0.1";
            case ArmSelection.SingleArm:
                return "131.254.18.52#5126";
            default:
                return "";
        }
    }

    void Start () {

        virtuose = armSelection != ArmSelection.Unity;

        Position = Vector3.zero;
        Rotation = Quaternion.identity;

        if (VRTools.IsClient())
            enabled = false;

        if (virtuose)
        {
            {
                arm = new VirtuoseArm();
                helper = new VirtuoseAPIHelper(arm);
                helper.Open(GetIP());

                helper.InitDefault();
                helper.CommandType = modeVirtuose;
                
            }
        }
    }

    void OnApplicationQuit()
    {
        if (virtuose)
        {
            helper.Close();
        }
    }

    private void OutputToVirtuose()
    {
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE)
        {
            float[] forces = UnityToVirtuoseForce(Force, Torque);
            for (int i = 0; i < forces.Length; ++i)
                forces[i] = Mathf.Clamp(forces[i], -VirtuoseAPIHelper.MAX_FORCE, VirtuoseAPIHelper.MAX_FORCE);
               
            helper.Force = forces;
            Debug.Log($"Forces : {Force}, Torques : {Torque}");
        }
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH) 
        {
            Debug.Log($"Offset position : {Position}, offset rotation {Rotation}");
            helper.Pose = (UnityToVirtuosePos(virtuose_Position + Position), UnityToVirtuoseRot(virtuose_Rotation * Rotation));
        }
    }

    void Update () {

        if (virtuose)
        {
            (Vector3 position, Quaternion rotation) pose = helper.Pose;
            virtuose_Position = VirtuoseToUnityPos(pose.position);
            virtuose_Rotation = VirtuoseToUnityRot(pose.rotation);

            if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
            {
                virtuose_Force = new Vector3(helper.Force[0], helper.Force[1], helper.Force[2]);
                virtuose_Torque = new Vector3(helper.Force[3], helper.Force[4], helper.Force[5]);
            }

            HandleVirtuoseInput();
            OutputToVirtuose();
        }
    }

    private void HandleVirtuoseInput()
    {
        Vector3 transformedPosition = virtuose_Position;
        Quaternion transformedQuaternion = virtuose_Rotation;

        Transform objectToMove = GetTransformToMove();
        objectToMove.transform.position = transformedPosition;
        objectToMove.rotation = transformedQuaternion;
    }

    private Transform GetTransformToMove()
    {
        return transform;
    }

    private Vector3 VirtuoseToUnityPos(Vector3 posVirtu)
    {
        return new Vector3(posVirtu.x, posVirtu.y, posVirtu.z);
    }

    private Quaternion VirtuoseToUnityRot(Quaternion rotVirtu)
    {
        return new Quaternion(-rotVirtu.y, -rotVirtu.z, rotVirtu.x, rotVirtu.w);
    }

    private Vector3 UnityToVirtuosePos(Vector3 posUnity)
    {
        return posUnity;
    }

    private Quaternion UnityToVirtuoseRot(Quaternion rotUnity)
    {
        return new Quaternion(-rotUnity.y, -rotUnity.z, rotUnity.x, rotUnity.w);
    }

    private Vector3 UnityToVirtuoseForce(Vector3 vec3)
    {
        return new Vector3(-vec3.z, vec3.x, vec3.y);
    }

    private Vector3 UnityToVirtuoseTorque(Vector3 vec3)
    {
        return new Vector3(vec3.z, vec3.x, vec3.y);
    }

    private float[] UnityToVirtuoseForce(Vector3 force, Vector3 torque)
    {
        force = UnityToVirtuoseForce(force);
        torque = UnityToVirtuoseTorque(torque);

        float[] virtuoseForce = new float[6] {force.x,force.y,force.z,torque.x,torque.y,torque.z};

        return virtuoseForce;
    }

    public enum ArmSelection
    {
        Unity, Simulator, SingleArm
    }
}
