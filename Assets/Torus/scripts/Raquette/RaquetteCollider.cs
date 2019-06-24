using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteCollider : MonoBehaviour
{
    public RaquetteController raquetteController;
    public bool IsCollided;

    private Collision currentCollision;

    private void OnCollisionEnter(Collision collision)
    {
        IsCollided = true;
        currentCollision = collision;
        raquetteController.HandleCollisionEnter(CollidingList());
    }

    private void OnCollisionStay(Collision collision)
    {
        IsCollided = true;
        currentCollision = collision;
        raquetteController.HandleCollisionStay(CollidingList());
    }

    private void OnCollisionExit(Collision collision)
    {
        IsCollided = false;
        currentCollision = collision;
        raquetteController.HandleCollisionExit(CollidingList());
    }

    public List<Collision> CollidingList()
    {
        return new List<Collision>() { currentCollision};
    }
}
