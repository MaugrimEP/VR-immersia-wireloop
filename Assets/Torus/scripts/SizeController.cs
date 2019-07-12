using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour
{
    public VRInput scaleUp;
    public VRInput scaleDown;
    public Transform raquetteTransform;

    public Vector2 MinMaxSize;
    public float ChangeSizeSpeed;

    void Update()
    {
        if (scaleUp.IsPressed() || VRTools.IsButtonPressed(4))
        {
            raquetteTransform.transform.localScale = (raquetteTransform.transform.localScale + Vector3.one * ChangeSizeSpeed * VRTools.GetDeltaTime()).ClampVector3(MinMaxSize.x, MinMaxSize.y);
        }

        if (scaleDown.IsPressed() || VRTools.IsButtonPressed(3))
        {
            raquetteTransform.transform.localScale = (raquetteTransform.transform.localScale - Vector3.one * ChangeSizeSpeed * VRTools.GetDeltaTime()).ClampVector3(MinMaxSize.x, MinMaxSize.y);
        }
    }
}
