using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static string tagname = "Raquette";
    public Transform ApplicationForcePoint;

    private VectorManager vectorManager;

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            child.tag = RaquetteController.tagname;
        }

        vectorManager = GameObject.Find("VectorCreator").GetComponent<VectorManager>();
    }

    public void UpdateChildOnTouch()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void UpdateChildOnLeave()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    public void TouchPipe(Collider collisionCollider)
    {
        UpdateChildOnTouch();
    }

    public void LeavePipe(Collider collisonCollider)
    {
        UpdateChildOnLeave();
    }

    internal void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
    }

    internal void TouchPipe(Collision collision)
    {
        UpdateChildOnTouch();
    }
}
