using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour
{
    public VRInput scaleUp;
    public VRInput scaleDown;
    public Transform raquetteTransform;
    public float sizeStep;

    void Update()
    {
        if (scaleUp.IsToggled() || VRTools.IsButtonToggled(4))
        {
            raquetteTransform.transform.localScale += Vector3.one * sizeStep;
            VRTools.Log($"Size = {raquetteTransform.transform.localScale}");
        }

        if (scaleDown.IsToggled() || VRTools.IsButtonToggled(3))
        {
            raquetteTransform.transform.localScale -= Vector3.one * sizeStep;
            VRTools.Log($"Size = {raquetteTransform.transform.localScale}");
        }
    }
}
