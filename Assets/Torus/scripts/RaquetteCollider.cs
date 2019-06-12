﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteCollider : MonoBehaviour
{
    new Rigidbody rigidbody;
    private RaquetteController raquetteController;
    public bool IsCollided;

    public List<Collider> collidingRaquetteElements;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        raquetteController = GetComponent<RaquetteController>();
        collidingRaquetteElements = new List<Collider>();
    }

    private void SetIsCollided()
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
        collidingRaquetteElements.Remove(collision.collider);
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
