using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteCollider : MonoBehaviour
{
    new Rigidbody rigidbody;
    private RaquetteController raquetteController;
    public bool IsCollided;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        raquetteController = GetComponent<RaquetteController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        IsCollided = true;
        raquetteController.HandleCollisionEnter(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        IsCollided = false;
        raquetteController.HandleCollisionExit(collision);

    }

    void OnCollisionStay(Collision collision)
    {
        IsCollided = true;
        raquetteController.HandleCollisionStay(collision);
    }
}
