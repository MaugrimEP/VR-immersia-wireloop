using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{
    void Update()
    {
        if (VRTools.GetKeyDown(KeyCode.B) || VRTools.IsButtonToggled(1))
        {
            RaquetteController.ShowMode = (RaquetteController.ShowMode + 1) % 6;
            Debug.Log($"RaquetteController.ShowMode {RaquetteController.ShowMode}");
        }
            
    }
}
