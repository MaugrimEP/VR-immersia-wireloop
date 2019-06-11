using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEController : MonoBehaviour
{
    public VirtuoseManager vm;
    private void Update()
    {
        if (VRTools.GetKeyDown(KeyCode.A) || VRTools.IsButtonToggled(0) // manage the wand
            || vm.IsButtonPressed(1))
            ToggleChildDisplay();
    }

    private void ToggleChildDisplay()
    {
        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = !child.GetComponent<Renderer>().enabled; ;
    }
}
