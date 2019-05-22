using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (VRTools.GetKeyDown(KeyCode.A) || VRTools.IsButtonToggled(0))
            ToggleChildDisplay();
        if (VRTools.GetKeyDown(KeyCode.B) || VRTools.IsButtonToggled(1))
        {
            RaquetteController.ShowMode = (RaquetteController.ShowMode + 1) % 4;
            Debug.Log($"RaquetteController.ShowMode {RaquetteController.ShowMode}");
        }
            
    }

    private void ToggleChildDisplay()
    {
        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = !child.GetComponent<Renderer>().enabled;
    }
}
