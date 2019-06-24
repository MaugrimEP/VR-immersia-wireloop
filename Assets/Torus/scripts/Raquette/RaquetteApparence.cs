using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteApparence : MonoBehaviour
{
    public List<Renderer> renderers;
    public GameObject SparksPrefab;
    public RaquetteController rc;
    public float SparksLifeTime;

    private List<SparksManager> particles;
    private List<Transform> particlesTransform;

    private void Start()
    {
        particles = new List<SparksManager>();
        particlesTransform = new List<Transform>();
        for (int i = 0; i < 4; ++i)
        {
            GameObject sparkGO = Instantiate(SparksPrefab);
            sparkGO.transform.parent = transform;
            SparksManager spark = sparkGO.GetComponent<SparksManager>();
            particles.Add(spark);
            particlesTransform.Add(sparkGO.transform);
        }
    }

    public void UpdateChildOnEnter(List<Collision> collisions)
    {
        foreach (Renderer r in renderers)
            r.material.color = Color.red;
        PlaySparks(collisions);
    }

    public void UpdateChildOnStay(List<Collision> collisions)
    {
        foreach (Renderer r in renderers)
        {
            r.material.SetColor("_EmissionColor", Color.red);
            //r.material.color = Color.red;
        }

        PlaySparks(collisions);
    }

    public void UpdateChildOnLeave(List<Collision> collisions)
    {
        if (!rc.infoCollision.IsCollided)
            foreach (Renderer r in renderers)
            {
                //r.material.color = Color.green;
                r.material.SetColor("_EmissionColor", Color.green);
            }

        StopSparks();
    }

    private void StopSparks()
    {
        foreach (SparksManager ps in particles)
            ps.StopSparks();
    }

    private void PlaySparks(List<Collision> collisions)
    {
        int i = 0;
        foreach (Collision collision in collisions)
        {
            foreach (ContactPoint cp in collision.contacts)
            {
                if (i < 4)
                {
                    particlesTransform[i].position = cp.point;
                    particlesTransform[i].LookAt(cp.point + cp.normal);
                    particles[i].PlaySparks();
                }
                ++i;
            }
        }

        for (; i < 4; ++i)
        {
            particles[i].StopSparks();
        }
    }

    private void PlaySparks(Vector3 position, Quaternion rotation)
    {
        Destroy(Instantiate(SparksPrefab, position, rotation), SparksLifeTime);
    }
}
