using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricityManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public (Transform startPoint, Transform endPoint) DrawElectricity(Vector3 startPos, Vector3 endPos)
    {
        GameObject emptyContainer = new GameObject();
        emptyContainer.transform.parent = transform;

        GameObject StartPoint = new GameObject();
        GameObject EndPoint = new GameObject();

        StartPoint.transform.parent = emptyContainer.transform;
        EndPoint.transform.parent = emptyContainer.transform;

        StartPoint.transform.position = startPos;
        EndPoint.transform.position = endPos;

        Electric electricComp =  emptyContainer.AddComponent<Electric>();
        electricComp.transformPointA = StartPoint.transform;
        electricComp.transformPointB = EndPoint.transform;

        return (StartPoint.transform, EndPoint.transform);
    }
}
