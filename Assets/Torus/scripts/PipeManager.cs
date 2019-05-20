using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{

    public GameObject PipePrefab;
    public Vector3 InitialPipePosition;
    public int PipeCount;

    // Use this for initialization
    void Start()
    {
    }

    public void DrawLevel()
    {
        ClearPipe();

        Vector3[] directions = { Vector3.left, Vector3.up};

        Vector3 currentPosition = InitialPipePosition;
        for (int i = 0; i < PipeCount; ++i)
        {
            Vector3 direction = directions[Random.Range(0, directions.Length)];
            DrawPipe(currentPosition, currentPosition + direction);
            currentPosition += direction;
        }
    }

    public void ClearPipe()
    {
        List<Transform> childs = new List<Transform>();
        foreach (Transform child in transform)
        {
            childs.Add(child);
        }
        foreach (Transform child in childs)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void DrawPipe(Vector3 v1, Vector3 v2)
    {
        Vector3 spawnPos = Vector3.Lerp(v1, v2, 0.5f);
        float distance = Vector3.Distance(v1, v2);
        GameObject instantiated = Instantiate(PipePrefab, spawnPos, Quaternion.identity);
        instantiated.transform.localScale += Vector3.forward * distance;
        instantiated.transform.LookAt(v2);
        instantiated.transform.parent = this.transform;
    }

}
