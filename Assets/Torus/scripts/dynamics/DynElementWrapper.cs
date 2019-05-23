using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynElementWrapper : MonoBehaviour
{
    public DynElement Element;

    private Vector3 lastPosition;

    [HideInInspector]
    public RaquetteController RaquetteController;

    private void Awake()
    {
        Element.Position = transform.position;
        Element.Velocity = Vector3.zero;
        Element.Force    = Vector3.zero;

        RaquetteController = GetComponent<RaquetteController>();
    }

    private void Update()
    {
        Element.Velocity = (transform.position - lastPosition) / VRTools.GetDeltaTime();
        Element.Position = transform.position;
        lastPosition     = transform.position;
    }

    public void AfterCollision()
    {
        RaquetteController.AfterCollision();
    }

    public void NoCollision()
    {
        RaquetteController.NoCollision();
    }
}
