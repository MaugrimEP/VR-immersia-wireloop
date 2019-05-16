using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static string tagname = "Raquette";
    public Transform ApplicationForcePoint; // where we will apply the force with the virtuose aka the handle
    private VectorManager vectorManager;

    private float raquetteMass;
    private Vector3 vitesse;
    private Vector3 lastPosition;

    public float K;//stiffness coeff

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
        HandleCollision(collision);
    }

    public void HandleCollision(Collision collision)
    {
        Vector3 forceTotal = Vector3.zero;
        Vector3 torqueTotal = Vector3.zero;

        //show the vector for the collision
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contactPoint = collision.GetContact(i);

            float distanceToHandle = Vector3.Distance(ApplicationForcePoint.position, contactPoint.point);
            float force = Mathf.Abs(contactPoint.separation) * K; //force = K * penetration distance
            Vector3 forceVector = force * contactPoint.normal;//vitesse.normalized;//
            float angle = Vector3.Angle(ApplicationForcePoint.position, forceVector);
            Vector3 torques = forceVector * distanceToHandle * Mathf.Sin(angle); // torque = force * distance from axis * sin(angle between axis and force)

            forceVector *= -1;

            forceTotal += forceVector;
            torqueTotal += torques;
        }
        forceTotal /= collision.contactCount;
        torqueTotal /= collision.contactCount;

        vectorManager.DrawVector(ApplicationForcePoint.position, forceTotal, Color.magenta, "forceTotal");
        vectorManager.DrawVector(ApplicationForcePoint.position, torqueTotal, Color.cyan, "torqueTotal");
    }

    
    public (Vector3 targetedPosition, Quaternion targetedRotation) GetTargetedVirtuosePosition(Vector3 force, Vector3 torque)
    {

        throw new NotImplementedException();
    }

    internal void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
        vectorManager.ClearVector();
    }
}
