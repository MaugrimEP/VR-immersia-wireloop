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
    //value to give to the virtuose for update
    [HideInInspector]
    public Vector3 Force;
    [HideInInspector]
    public Vector3 Torque;
    [HideInInspector]
    public Vector3 Position;
    [HideInInspector]
    public Quaternion Rotation;



    private Vector3 virtuoseOffset; //vector to add from the virtuose position to have the transform position in unity
    private bool isOffseting;
    private void OnOffsetEnter()
    {

    }

    private void OnOffsetExit()
    {

    }

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
            isOffseting = helper.IsInShiftPosition;
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
        if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_ADMITTANCE || modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH) 
        {
            
            (Vector3 position, Quaternion rotation) pose = helper.Pose;
        }
    }

    void Update () {

        if (virtuose)
        {
            (Vector3 position, Quaternion rotation) pose = helper.Pose;
            Debug.Log($"virtuose_im: {pose.position}, {pose.rotation}");

            if (modeVirtuose == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_ADMITTANCE)
            {
                Vector3 torque = new Vector3(helper.Force[3], helper.Force[4], helper.Force[5]);
                Vector3 force = new Vector3(helper.Force[0], helper.Force[1], helper.Force[2]);
                Debug.Log($"virtuose_ad: {force}, {torque}");
            }

            HandleVirtuoseInput(pose.position, pose.rotation);
            OutputToVirtuose();
        }
    }

    private void HandleVirtuoseInput(Vector3 position, Quaternion rotation)
    {
        Vector3 transformedPosition = virtuoseToUnityPos(position);
        Quaternion transformedQuaternion = virtuoseToUnityRot(rotation); //new Quaternion(rotation.y, rotation.z, rotation.x, rotation.w);

        Transform objectToMove = GetTransformToMove();
        objectToMove.transform.position = transformedPosition;
        objectToMove.rotation = transformedQuaternion;
    }

    private Transform GetTransformToMove()
    {
        return transform;
    }

    private Vector3 virtuoseToUnityPos(Vector3 posVirtu)
    {
        return new Vector3(posVirtu.x, posVirtu.y, posVirtu.z);
    }

    private Quaternion virtuoseToUnityRot(Quaternion rotVirtu)
    {
        return new Quaternion(-rotVirtu.y, -rotVirtu.z, rotVirtu.x, rotVirtu.w);
    }

    private Vector3 unityToVirtuosePos(Vector3 posUnity)
    {
        return posUnity;
    }

    private Quaternion unityToVirtuoseRot(Quaternion rotUnity)
    {
        return new Quaternion(-rotUnity.y, -rotUnity.z, rotUnity.x, rotUnity.w);
    }
}
