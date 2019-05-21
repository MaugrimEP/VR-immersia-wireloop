using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ToggleChildDisplay();
    }

    private void ToggleChildDisplay()
    {
        foreach (Transform child in transform)
            child.GetComponent<Renderer>().enabled = !child.GetComponent<Renderer>().enabled;
    }
}
