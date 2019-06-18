using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteCollider : MonoBehaviour
{
    public RaquetteController raquetteController;
    public bool IsCollided;

    public List<Collider> collidingRaquetteElements;

    private void Start()
    {
        collidingRaquetteElements = new List<Collider>();
    }

    private void Update()
    {
        ClearCollisions();
    }

    public void ClearCollisions()
    {
        collidingRaquetteElements.Clear();
    }

    public void SetIsCollided()
    {
        IsCollided = collidingRaquetteElements.Count != 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collidingRaquetteElements.Add(collision.collider);
        SetIsCollided();
        raquetteController.HandleCollisionEnter(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        SetIsCollided();
        raquetteController.HandleCollisionExit(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        collidingRaquetteElements.Add(collision.collider);
        SetIsCollided();
        raquetteController.HandleCollisionStay(collision);
    }
}
