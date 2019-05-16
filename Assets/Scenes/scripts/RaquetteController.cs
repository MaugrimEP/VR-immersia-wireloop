using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static string tagname = "Raquette";
    public Transform ApplicationForcePoint;
    private VectorManager vectorManager;

    private float raquetteMass;
    private Vector3 vitesse;
    private Vector3 lastPosition;

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            child.tag = RaquetteController.tagname;
        }

        vectorManager = GameObject.Find("VectorCreator").GetComponent<VectorManager>();

        foreach(Transform child in transform)
        {
            raquetteMass += child.gameObject.GetComponent<Rigidbody>().mass;
        }
    }

    private void Update()
    {
        vitesse = (transform.position - lastPosition) / VRTools.GetDeltaTime();
        lastPosition = transform.position;
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
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contactPoint = collision.GetContact(i);

            float distanceToHandle = Vector3.Distance(ApplicationForcePoint.position, contactPoint.point);

            //(0.5 * m * v^2) ÷ d
            Vector3 forces = 0.5f * raquetteMass * Utils.Pow(vitesse, 2.0f) / distanceToHandle; //collision.relativeVelocity
            forces = Utils.Mul(forces, contactPoint.normal);
            Vector3 torques = forces * distanceToHandle;

            vectorManager.DrawVector(contactPoint.point, forces, Color.blue, "forces");
            vectorManager.DrawVector(contactPoint.point, torques, Color.magenta, "torques");
        }
    }

    internal void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
        vectorManager.ClearVector();
    }
}
