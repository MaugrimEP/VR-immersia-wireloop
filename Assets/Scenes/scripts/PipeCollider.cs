using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeCollider : MonoBehaviour
{

    public Interaction interaction;

    // Start is called before the first frame update
    void Start()
    {
        interaction = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Interaction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pipe collision enter");
        //if (ReferenceEquals(other.gameObject,interaction.FixedJoint.connectedBody))
        if (other.gameObject.CompareTag("Interaction"))
        {
            if (interaction.IsGrabbing())
            {
                interaction.FixedJoint.connectedBody.gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Pipe collision exit");
        if (other.gameObject.CompareTag("Interaction"))
        {
            if (interaction.IsGrabbing())
            {
                interaction.FixedJoint.connectedBody.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
    }

}
