using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricityManager : MonoBehaviour
{
    /// <summary>
    /// if the target start (target end) is a transform, then the electricity start point(end point) will follow the target start(target end) position;
    /// else if it is a vector3 it will just stay at the given position
    /// </summary>
    private static ElectricityManager me;
    public Material electricityMat;

    void Start()
    {
        me = this;
    }
    public void DrawElectricity(Transform targetStart, Transform targetEnd)
    {
        GameObject emptyContainer = new GameObject();
        emptyContainer.transform.parent = transform;

        GameObject StartPoint = new GameObject();
        GameObject EndPoint = new GameObject();

        StartPoint.transform.parent = emptyContainer.transform;
        EndPoint.transform.parent = emptyContainer.transform;

        StartPoint.transform.position = targetStart.position;
        EndPoint.transform.position = targetEnd.position;

        Electric electricComp =  emptyContainer.AddComponent<Electric>();
        electricComp.targetStart = targetStart;
        electricComp.targetEnd = targetEnd;
        electricComp.elecMat = electricityMat;
        electricComp.transformPointA = StartPoint.transform;
        electricComp.transformPointB = EndPoint.transform;
    }

    public void DrawElectricity(Transform targetStart, Vector3 targetEnd, float lifeTime = -1)
    {
        GameObject emptyContainer = new GameObject();
        emptyContainer.transform.parent = transform;

        GameObject StartPoint = new GameObject();
        GameObject EndPoint = new GameObject();

        StartPoint.transform.parent = emptyContainer.transform;
        EndPoint.transform.parent = emptyContainer.transform;

        StartPoint.transform.position = targetStart.position;
        EndPoint.transform.position = targetEnd;

        Electric electricComp = emptyContainer.AddComponent<Electric>();
        electricComp.targetStart = targetStart;
        electricComp.targetEnd = null;
        electricComp.elecMat = electricityMat;
        electricComp.transformPointA = StartPoint.transform;
        electricComp.transformPointB = EndPoint.transform;

        if(lifeTime != -1)
            Destroy(emptyContainer, lifeTime);
    }

    public void AddChild(GameObject go)
    {
        go.transform.parent = transform;
    }

    public void Clear()
    {
        List<GameObject> electricityElements = new List<GameObject>();
        foreach (Transform element in transform)
            electricityElements.Add(element.gameObject);
        foreach (GameObject element in electricityElements)
            Destroy(element);
    }


    #region static wrapper
    public static void DrawElectricityS(Transform targetStart, Transform targetEnd)
    {
        me.DrawElectricity(targetStart, targetEnd);
    }


    public static void DrawElectricityS(Transform targetStart, Vector3 targetEnd, float lifeTime = -1)
    {
        me.DrawElectricity(targetStart, targetEnd, lifeTime);
    }

    public static void ClearS()
    {
        me.Clear();
    }

    public static void AddChildS(GameObject go)
    {
        me.AddChild(go);
    }
    #endregion
}
