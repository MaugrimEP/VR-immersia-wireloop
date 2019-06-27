using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableController : MonoBehaviour
{
    public List<ParticleSystem> particleSystems;
    private void OnTriggerEnter(Collider other)
    {
        Stop();
    }

    private void Stop()
    {
        foreach (ParticleSystem ps in particleSystems)
            ps.Stop();
    }

    public void Start()
    {
        foreach (ParticleSystem ps in particleSystems)
            ps.Play();
    }
}
