using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEController : MonoBehaviour
{
    private void Update()
    {
        if (VRTools.GetKeyDown(KeyCode.A) || VRTools.IsButtonToggled(0))
            ToggleChildDisplay();
    }

    private void ToggleChildDisplay()
    {
        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = !child.GetComponent<Renderer>().enabled; ;
    }
}
