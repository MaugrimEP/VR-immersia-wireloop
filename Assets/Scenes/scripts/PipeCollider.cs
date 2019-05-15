using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeCollider : MonoBehaviour
{

    RaquetteController raquette;

    void Start()
    {
        raquette = GameObject.FindGameObjectsWithTag(RaquetteController.tagname)[0].GetComponent<RaquetteController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pipe collision enter");

        if (other.gameObject.CompareTag(RaquetteController.tagname))
        {
            raquette.TouchPipe(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Pipe collision exit");

        if (other.gameObject.CompareTag(RaquetteController.tagname))
        {
            raquette.LeavePipe(other);
        }
    }

}
