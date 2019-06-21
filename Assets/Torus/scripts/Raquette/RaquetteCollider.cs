using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteCollider : MonoBehaviour
{
    public RaquetteController raquetteController;
    public bool IsCollided;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        IsCollided = true;
        raquetteController.HandleCollisionEnter(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        IsCollided = true;
        raquetteController.HandleCollisionStay(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        IsCollided = false;
        raquetteController.HandleCollisionExit(collision);
    }
}
