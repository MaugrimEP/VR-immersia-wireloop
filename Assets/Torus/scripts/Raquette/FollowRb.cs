using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRb : MonoBehaviour
{
    public Color color;
    public GameObject Follow;

    private Rigidbody rb;

    void Start()
    {
        GetComponent<Renderer>().material.color = color;
        rb = Follow.GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.position = rb.position;
        transform.rotation = rb.rotation;
    }
}
