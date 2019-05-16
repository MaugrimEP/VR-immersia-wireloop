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

    /*
    public void TouchPipe(Collider collisionCollider)
    {
        UpdateChildOnTouch();
    }

    public void LeavePipe(Collider collisonCollider)
    {
        UpdateChildOnLeave();
    }
    */

    internal void TouchPipe(Collision collision)
    {
        UpdateChildOnTouch();

        //show the vector for the collision
        Debug.Log("collison.contactCount " + collision.contactCount);
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contactPoint = collision.GetContact(i);
            vectorManager.DrawVector(contactPoint.point, contactPoint.normal, Color.black, "contactPoint / contactNormal");
        }
    }

    internal void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
        vectorManager.ClearVector();
    }
}
