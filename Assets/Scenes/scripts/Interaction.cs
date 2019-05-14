using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interaction : MonoBehaviour
{

    public FixedJoint FixedJoint = null;

    private List<Rigidbody> _contactRigidBody = new List<Rigidbody>();

    // Use this for initialization
    void Start()
    {
        FixedJoint = GetComponent<FixedJoint>();
    }

    public bool IsGrabbing()
    {
        return FixedJoint.connectedBody != null;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.gameObject.CompareTag("Interaction")) return;
        Debug.Log("OnTriggerEnter");

        _contactRigidBody.Add(collider.gameObject.GetComponent<Rigidbody>());
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!collider.gameObject.CompareTag("Interaction")) return;
        Debug.Log("OnTriggerExit");

        _contactRigidBody.Remove(collider.gameObject.GetComponent<Rigidbody>());
    }

    public void Grab()
    {
        Rigidbody nearestRigidBody = GetNearestRigidBody();
        if (!nearestRigidBody || FixedJoint.connectedBody) return;

        Debug.Log("Grab");

        FixedJoint.connectedBody = nearestRigidBody;
    }

    public void Drop()
    {

        if (!FixedJoint.connectedBody) return;

        Debug.Log("Drop");

        FixedJoint.connectedBody = null;
    }

    private Rigidbody GetNearestRigidBody()
    {
        Rigidbody nearestRigidbody = null;

        float minDistance = float.MaxValue;

        float distance = 0.0f;

        foreach (Rigidbody rb in _contactRigidBody)
        {
            distance = (rb.gameObject.transform.position - transform.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestRigidbody = rb;
            }
        }

        return nearestRigidbody;
    }

}
