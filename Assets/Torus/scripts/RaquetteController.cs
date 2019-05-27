using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static int ShowMode = 4;
    public static string tagname = "Raquette";

    /// <summary>
    /// Childs that compose the raquette
    /// </summary>
    public List<Transform> RaquettesChilds;
    [HideInInspector]
    public List<Rigidbody> RigidBodyChilds;

    private VectorManager vectorManager;
    [HideInInspector]
    public InputController avatarInputController;


    private List<Transform> getChilds()
    {
        return RaquettesChilds;
    }

    private void Awake()
    {
        RigidBodyChilds = new List<Rigidbody>();
        foreach(Transform child in getChilds()) //apply the tagname on child to filter the collisions
        {
            child.tag = tagname;
            RigidBodyChilds.Add(child.gameObject.GetComponent<Rigidbody>());
        }

        vectorManager = GameObject.Find("VectorCreator").GetComponent<VectorManager>();
        avatarInputController = GameObject.Find("Avatar").GetComponent<InputController>();
    }

    private Vector3 ClampDisplacement(Vector3 previousPosition, Vector3 nextPosition)
    {
        float travelDistance = Vector3.Distance(previousPosition, nextPosition);

        float maxDisplacement = 0.02f;

        if (travelDistance > maxDisplacement)
        {// need to size down the distance
            Vector3 newDisplacementVector = nextPosition - previousPosition;
            newDisplacementVector /= travelDistance / maxDisplacement;

            nextPosition = previousPosition + newDisplacementVector;
        }

        return nextPosition;
    }

    private void HandleCollision(Collision collision)
    {
        float interpenetrationDistance = 0.0f;
        Vector3 normal = Vector3.zero;
        {
            foreach(Rigidbody rb in RigidBodyChilds)
            {
                //distance entre le game object et le rigidbody
                //Vector3 normal = target.transform.position - targetRigidbody.position;
                normal += rb.gameObject.transform.position - rb.position;
            }
            normal /= RigidBodyChilds.Count;
            interpenetrationDistance = normal.magnitude;
            normal = normal.normalized;
        }


    }

    #region Handle pipe collision interaction
    public void TouchPipe(Collision collision)
    {
        UpdateChildOnTouch();
        HandleCollision(collision);
    }

    public void StayPipe(Collision collision)
    {
        HandleCollision(collision);
    }

    public void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
    }

    #region change the apparence of the raquette when interacting with the pipe
    private void UpdateChildOnTouch()
    {
        foreach (Transform child in getChilds())
        {
            child.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void UpdateChildOnLeave()
    {
        foreach (Transform child in getChilds())
        {
            child.GetComponent<Renderer>().material.color = Color.green;
        }
    }
    #endregion

    #endregion
}
