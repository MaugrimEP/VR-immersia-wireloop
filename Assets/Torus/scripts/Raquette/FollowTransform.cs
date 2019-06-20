using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Color color;
    public GameObject Follow;

    void Start()
    {
        GetComponent<Renderer>().material.color = color;
    }

    void Update()
    {
        transform.position = Follow.transform.position;
        transform.rotation = Follow.transform.rotation;
    }
}
