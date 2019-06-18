using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoucingBallController : MonoBehaviour
{
    public InputController ic;
    public GameObject BouncingBall;
    public Transform spawnPoint;
    public GameObject container;


    private void Start()
    {
        SpawnBall();
        /*
        if (VRTools.IsClient())
        {
            this.enabled = false;
            return;
        }*/
    }

    void Update()
    {
        /*
        if (ic.virtuoseManager.IsButtonPressed(1))
        {
            SpawnBall();
        }
        */
    }

    public void Clear()
    {
        foreach(Transform c in container.transform)
        {
            Destroy(c.gameObject);
        }

    }

    private void SpawnBall()
    {
        Clear();
        GameObject o = Instantiate(BouncingBall, spawnPoint.position, spawnPoint.rotation);
        o.transform.localScale = Vector3.one * 0.1f;
        o.transform.parent = container.transform;
    }
}
