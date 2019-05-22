using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static int ShowMode = 0;
    public static string tagname = "Raquette";

    public Transform Handle;
    public float stiffness;
    public List<Transform> raquettesChild;//child that compose the raquette

    private VectorManager vectorManager;
    private InputController avatarInputController;

    private Vector3 velocity;
    private Vector3 lastPosition;

    private float interpenetrationDelta;
    private float lastInterpenetration;

    private List<Transform> getChilds()
    {
        return raquettesChild;
    }

    private void Awake()
    {
        foreach(Transform child in getChilds()) //apply the tagname on child to filter the collisions
        {
            child.tag = tagname;
        }

        vectorManager = GameObject.Find("VectorCreator").GetComponent<VectorManager>();
        avatarInputController = GameObject.Find("Avatar").GetComponent<InputController>();
        velocity = Vector3.zero;
        lastPosition = transform.position;
    }

    private void Update()
    {
        velocity = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    public void StayPipe(Collision collision)
    {
        {
            float interpenetration = Utils.MeanCollisonSeparation(collision);
            interpenetrationDelta = interpenetration - lastInterpenetration;
            lastInterpenetration = interpenetration;
        }
        HandleCollision(collision);
    }

    public void UpdateChildOnTouch()
    {
        foreach (Transform child in getChilds())
        {
            child.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void UpdateChildOnLeave()
    {
        foreach (Transform child in getChilds())
        {
            child.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    public void TouchPipe(Collision collision)
    {
        lastInterpenetration = Utils.MeanCollisonSeparation(collision);
        UpdateChildOnTouch();
        HandleCollision(collision);
    }

    public void HandleCollision(Collision collision)
    {
        Vector3 forceTotal   = Vector3.zero;
        Vector3 torqueTotal  = Vector3.zero;
        Vector3 normalTotal  = Vector3.zero;
        float intersectionDistance   = 0;

        //show the vector for the collision
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contactPoint = collision.GetContact(i);

            float distanceToHandle = Vector3.Distance(Handle.position, contactPoint.point);
            float force = Mathf.Abs(contactPoint.separation) * stiffness;
            Vector3 forceVector = force * contactPoint.normal;

            float angle = Vector3.Angle(Handle.position, forceVector);
            Vector3 torques = forceVector * distanceToHandle * Mathf.Sin(angle); // torque = force * distance from axis * sin(angle between axis and force)

            forceVector *= -1;
            //torques *= -1;

            {//update of the function value 
                forceTotal   += forceVector;
                torqueTotal  += torques;
                normalTotal  += contactPoint.normal;
                intersectionDistance += contactPoint.separation;
            }
        }
        {//compute average of the function value
            forceTotal   /= collision.contactCount;
            torqueTotal  /= collision.contactCount;
            normalTotal  /= - collision.contactCount;
            intersectionDistance /= collision.contactCount;
        }
        {//draw vector
            if (ShowMode == 0) vectorManager.DrawVector(Handle.position, forceTotal, Color.green, "forceTotal");
            if (ShowMode == 1) vectorManager.DrawVector(Handle.position, torqueTotal, Color.cyan, "torqueTotal");
            if (ShowMode == 2) vectorManager.DrawVector(Handle.position, normalTotal, Color.magenta, "normalTotal");
        }

        {//update value to output for the virtuose

            //used for impedance mode
            avatarInputController.Force = forceTotal;
            //avatarInputController.Torque = torqueTotal;

            if(false){//used for admitance mode, using the force to compute the next position
                avatarInputController.Position = Handle.position + forceTotal;
                avatarInputController.Rotation = Handle.rotation * Quaternion.Euler(torqueTotal);
            }
            {//using the normal to compute the next position
                avatarInputController.Position = normalTotal * stiffness * intersectionDistance;
                //avatarInputController.Rotation = Quaternion.Euler(torqueTotal);
            }

            /* //physic simulation
            (Vector3 returnedPosition, Quaternion returnedRotation) = Handle.GetComponent<SimulatePhysic>().simulateCollisonEffect(forceTotal, torqueTotal, 10);
            
            Debug.Log($"Computed offset : {returnedPosition}, {returnedRotation}");

            avatarInputController.Position = returnedPosition;
            avatarInputController.Rotation = returnedRotation;
            */
        }
    } 

    public void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
        vectorManager.ClearVector();

        avatarInputController.Force = Vector3.zero;
        avatarInputController.Torque = Vector3.zero;
    }
}
