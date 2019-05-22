﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeCollider : MonoBehaviour
{

    RaquetteController raquette;

    private List<GameObject> CollidingList;

    void Start()
    {
        raquette = GameObject.Find("Avatar").GetComponent<RaquetteController>();
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

    public void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag(RaquetteController.tagname))
        {
            raquette.StayPipe(collision);
        }
    }

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
