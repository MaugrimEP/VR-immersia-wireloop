using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEController : MonoBehaviour
{
    public VirtuoseManager vm;
    public float toggleDisplayCD = 1f;
    private float nextToggleTime;

    private void Start()
    {
        nextToggleTime = Time.time;
    }

    private void Update()
    {
        if (VRTools.GetKeyDown(KeyCode.A) || VRTools.IsButtonToggled(0) // manage keyboard and wand
            || vm.IsButtonToggled(1)                                    // manage virtuose
            )
        {
            if (Time.time > nextToggleTime)
            {
                ToggleChildDisplay();
                nextToggleTime = Time.time + toggleDisplayCD;
            }
        }

    }

    private void ToggleChildDisplay()
    {
        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = !child.GetComponent<Renderer>().enabled; ;
    }
}
