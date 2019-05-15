using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    VirtuoseArm arm;
    VirtuoseAPIHelper helper;

    public bool virtuose;

    void Start () {
        if(virtuose)
        {
            arm = new VirtuoseArm();
            helper = new VirtuoseAPIHelper(arm);
            helper.Open("127.0.0.1");
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
        transform.position = position;
        transform.rotation = rotation;
    }
}
