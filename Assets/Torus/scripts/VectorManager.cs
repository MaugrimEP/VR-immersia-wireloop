using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorManager : MonoBehaviour
{
    public static VectorManager VECTOR_MANAGER;

    public float vectorRadius;
    public float lifeTime;

    private void Awake()
    {
        VECTOR_MANAGER = this;
    }

    public void DrawVector(Vector3 position, Vector3 val, Color color, string name="not assigned")
    {
        Vector3 endPoint = position + val;
        float distance = Vector3.Distance(position, endPoint);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = Vector3.Lerp(position, endPoint, 0.5f);
        cylinder.transform.localScale = new Vector3(vectorRadius, distance/2.0f, vectorRadius);
        cylinder.transform.LookAt(endPoint);
        cylinder.transform.Rotate(Vector3.right, 90.0f);


        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = Vector3.one * vectorRadius;
        cube.transform.position = endPoint;

        cylinder.GetComponent<Renderer>().material.color = cube.GetComponent<Renderer>().material.color = color;

        //add the vector under the VectorCreator
        cylinder.transform.parent = transform;
        cube.transform.parent = transform;

        //just to be able to see the data in the inspector
        {
            VectorData cubeData = cube.AddComponent<VectorData>();
            VectorData cylinderData = cylinder.AddComponent<VectorData>();

            cubeData.origin = cylinderData.origin = position;
            cubeData.value = cylinderData.value = val;
            cubeData.endPoint = cylinderData.endPoint = endPoint;
            cubeData.magnitude = cylinderData.magnitude = distance;
            cubeData.nameReference = cylinderData.nameReference = name;
        }

        Destroy(cylinder, lifeTime);
        Destroy(cube, lifeTime);
    }


    public void ClearVector()
    {
        List<GameObject> vectors = new List<GameObject>();
        foreach (Transform vector in transform)
            vectors.Add(vector.gameObject);
        foreach (GameObject vector in vectors)
            Destroy(vector);
    }
}


public class VectorData : MonoBehaviour
{
    public Vector3 origin;
    public Vector3 value;
    public Vector3 endPoint;
    public float magnitude;
    public string nameReference;
}
