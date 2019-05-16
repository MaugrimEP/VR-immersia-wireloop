using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    VirtuoseArm arm;
    VirtuoseAPIHelper helper;

    public bool virtuose;

    public Transform Camera;

    void Start () {
        if(virtuose)
        {
            arm = new VirtuoseArm();
            helper = new VirtuoseAPIHelper(arm);
            helper.Open("127.0.0.1");
        }
    }

    void OnApplicationQuit()
    {
        if (virtuose)
        {
            helper.Close();
        }
    }


    void Update () {

        if (virtuose && !helper.IsInShiftPosition) //if we arent in offset mode
        {
            (Vector3 position, Quaternion rotation) pose = helper.Pose;
            HandleVirtuoseInput(pose.position, pose.rotation);
        }
    }

    private void HandleVirtuoseInput(Vector3 position, Quaternion rotation)
    {
        Vector3 transformedPosition = new Vector3(position.x, position.y, position.z);
        Quaternion transformedQuaternion = new Quaternion(- rotation.y,-  rotation.z, rotation.x,rotation.w); //new Quaternion(rotation.y, rotation.z, rotation.x, rotation.w);

        Transform objectToMove = getTransformToMove();
        objectToMove.transform.position = transformedPosition;
        objectToMove.rotation = transformedQuaternion;
    }

    private Transform getTransformToMove()
    {
        if (false && helper.Button(1))//if the button 0 is pressed, we will move the camera
        {
            return Camera;
        }
        else                //else we will move the raquette
        {
            return transform;
        }

    }
}
