using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    VirtuoseArm arm;
    VirtuoseAPIHelper helper;
    public bool virtuose;

    public Transform Camera;

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

    void Start () {
        if (VRTools.IsClient())
            enabled = false;

        if (virtuose)
        {
            {
                arm = new VirtuoseArm();
                helper = new VirtuoseAPIHelper(arm);
                helper.Open("127.0.0.1");

                //helper.Open("131.254.18.52#5126");
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
            helper.Force = new float[6] { Force.x, Force.y, Force.z, Torque.x, Torque.y, Torque.z };
        }
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH) 
        {
            (Vector3 position, Quaternion rotation) pose = helper.Pose;
            helper.Pose = pose;
        }
    }

    void Update () {

        if (virtuose)
        {
            (Vector3 position, Quaternion rotation) pose = helper.Pose;
            virtuose_Position = pose.position;
            virtuose_Rotation = pose.rotation;

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
        Vector3 transformedPosition = VirtuoseToUnityPos(virtuose_Position);
        Quaternion transformedQuaternion = VirtuoseToUnityRot(virtuose_Rotation);

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
}
