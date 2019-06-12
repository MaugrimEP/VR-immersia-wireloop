using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandController : MonoBehaviour
{
    public Transform handPosition;

    void Start()
    {

    }

    void Update()
    {
        if (VRTools.IsButtonToggled(0))
        {
            GameObject sphere = VectorManager.DrawSphereS(handPosition.position, Vector3.one * 0.1f, Color.red);
            VectorManager.DrawTextS(sphere.transform, handPosition, Vector3.down * 0.2f, Utils.Vector3Text(handPosition.position, 2), Color.white);
        }
    }
}
