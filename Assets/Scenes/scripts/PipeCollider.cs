using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeCollider : MonoBehaviour
{

    RaquetteController raquette;

    private List<GameObject> CollidingList;

    void Start()
    {
        raquette = GameObject.FindGameObjectsWithTag(RaquetteController.tagname)[0].GetComponent<RaquetteController>();
        CollidingList = new List<GameObject>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        CollidingList.Add(collision.gameObject);

        if (collision.collider.gameObject.CompareTag(RaquetteController.tagname))
        {
            raquette.TouchPipe(collision);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        CollidingList.Remove(collision.gameObject);
        if (collision.collider.gameObject.CompareTag(RaquetteController.tagname) && !IsColladingWithTag(RaquetteController.tagname))  //check if we are still colliding with the tag
        {
            raquette.LeavePipe(collision);
        }
    }
    /*
    public void OnTriggerEnter(Collider other)
    {
        CollidingList.Add(other.gameObject);

        if (other.gameObject.CompareTag(RaquetteController.tagname))
        {
            raquette.TouchPipe(other);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        CollidingList.Remove(other.gameObject);
        if (other.gameObject.CompareTag(RaquetteController.tagname) && !IsColladingWithTag(RaquetteController.tagname))  //check if we are still colliding with the tag
        {
            raquette.LeavePipe(other);
        }
    }
    */

    private bool IsColladingWithTag(string tag)
    {
        foreach (GameObject item in CollidingList)
        {
            if (item!=null && item.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

}
