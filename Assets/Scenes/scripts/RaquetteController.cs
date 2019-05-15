using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{

    public static string tagname = "Raquette";

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            child.tag = RaquetteController.tagname;
        }
    }

    public void TouchPipe(Collider collisionCollider)
    {
        Debug.Log("RaquetteController.TouchPipe");
        foreach(Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void LeavePipe(Collider collisonCollider)
    {
        Debug.Log("RaquetteController.LeavePipe");
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = Color.green;
        }
    }
}
