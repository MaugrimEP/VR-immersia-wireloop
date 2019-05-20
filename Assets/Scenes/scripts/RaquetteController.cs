using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static string tagname = "Raquette";
    public Transform ApplicationForcePoint; // where we will apply the force with the virtuose
    public Transform Handle;
    private VectorManager vectorManager;

    public float K;//stiffness coeff

    private InputController avatarInputController;

    public List<Transform> raquettesChild;//child that compose the raquette

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
        UpdateChildOnTouch();
        HandleCollision(collision);
    }

    public void HandleCollision(Collision collision)
    {
        Vector3 forceTotal = Vector3.zero;
        Vector3 torqueTotal = Vector3.zero;
        Vector3 normalTotal = Vector3.zero;

        //show the vector for the collision
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contactPoint = collision.GetContact(i);

            float distanceToHandle = Vector3.Distance(ApplicationForcePoint.position, contactPoint.point);
            float force = Mathf.Abs(contactPoint.separation) * K;
            Vector3 forceVector = force * contactPoint.normal;

            float angle = Vector3.Angle(ApplicationForcePoint.position, forceVector);
            Vector3 torques = forceVector * distanceToHandle * Mathf.Sin(angle); // torque = force * distance from axis * sin(angle between axis and force)

            forceVector *= -1;
            //torques *= -1;

            {//update of the function value 
                forceTotal += forceVector;
                torqueTotal += torques;
                normalTotal += contactPoint.normal;
            }
        }
        {//compute average of the function value
            forceTotal /= collision.contactCount;
            torqueTotal /= collision.contactCount;
            normalTotal /= - collision.contactCount;
        }
        {//draw vector
            vectorManager.DrawVector(ApplicationForcePoint.position, forceTotal, Color.magenta, "forceTotal");
            vectorManager.DrawVector(ApplicationForcePoint.position, torqueTotal, Color.cyan, "torqueTotal");
            vectorManager.DrawVector(ApplicationForcePoint.position, normalTotal, Color.black, "normalTotal");
        }

        {//update value to output for the virtuose

            //used for impedance mode
            avatarInputController.Force = forceTotal;
            avatarInputController.Torque = torqueTotal;

            //used for admitance mode
            avatarInputController.Position = ApplicationForcePoint.position + forceTotal;
            avatarInputController.Rotation = ApplicationForcePoint.rotation * Quaternion.Euler(torqueTotal);

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
