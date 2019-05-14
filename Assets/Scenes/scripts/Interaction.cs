using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Interaction : MonoBehaviour {

	private FixedJoint _fixedJoint = null;

	private List<Rigidbody> _contactRigidBody = new List<Rigidbody>();

	// Use this for initialization
	void Start () {
		_fixedJoint = GetComponent<FixedJoint>();
	}

	private void OnTriggerEnter(Collider collider){
		if(!collider.gameObject.CompareTag("Interaction")) return;
        Debug.Log("OnTriggerEnter");

        _contactRigidBody.Add(collider.gameObject.GetComponent<Rigidbody>());
	}

    private void OnTriggerExit(Collider collider){
		if(!collider.gameObject.CompareTag("Interaction")) return;
        Debug.Log("OnTriggerExit");

        _contactRigidBody.Remove(collider.gameObject.GetComponent<Rigidbody>());
	}

	public void Grab(){
        Rigidbody nearestRigidBody = GetNearestRigidBody();
        if (!nearestRigidBody) return;

        Debug.Log("Grab");

        _fixedJoint.connectedBody = nearestRigidBody;
	}

    public void Drop(){

        if (!_fixedJoint.connectedBody) return;

        Debug.Log("Drop");

        _fixedJoint.connectedBody = null;
	}

    private Rigidbody GetNearestRigidBody()
    {
        Rigidbody nearestRigidbody = null;

        float minDistance = float.MaxValue;

        float distance = 0.0f;

        foreach(Rigidbody rb in _contactRigidBody)
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
