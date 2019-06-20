using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideCollider : MonoBehaviour
{
    public List<GameObject> ToEnable;

    private void Start()
    {
        foreach (GameObject go in ToEnable)
            go.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (GameObject go in ToEnable)
            go.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (GameObject go in ToEnable)
            go.SetActive(false);
    }
}
