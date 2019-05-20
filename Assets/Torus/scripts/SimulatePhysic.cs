using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatePhysic : MonoBehaviour
{
    public (Vector3 position, Quaternion rotation) simulateCollisonEffect(Vector3 force, Vector3 torque, float simulationStep)
    {
        Vector3 oldPosition = transform.position;
        Quaternion oldRotation = transform.rotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(force);
            rb.AddTorque(torque);
        }

        Physics.Simulate(simulationStep);

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Vector3 returnedPosition = transform.position - oldPosition;
        Quaternion returnedRotation = transform.rotation * Quaternion.Inverse(oldRotation);

        transform.position = oldPosition;
        transform.rotation = oldRotation;

        return (returnedPosition, returnedRotation);
    }
}
