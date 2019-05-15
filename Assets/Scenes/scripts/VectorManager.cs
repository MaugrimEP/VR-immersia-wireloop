using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorManager : MonoBehaviour
{

    public Material vectorMat;
    public float vectorRadius;

    public void DrawVector(Vector3 position,Vector3 val)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.GetComponent<Renderer>().material = vectorMat;

        cylinder.transform.LookAt(val + position);
        cylinder.transform.Rotate(Vector3.right, 90.0f);

        cylinder.transform.localScale = new Vector3(vectorRadius, 1,vectorRadius);
        cylinder.transform.position = Vector3.Lerp(position, position + val, 0.5f);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position + val;
        cube.GetComponent<Renderer>().material = vectorMat;
        cube.transform.localScale = Vector3.one * vectorRadius * 2;


        cylinder.transform.parent = transform;
        cube.transform.parent = transform;
    }
}