using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparksManager : MonoBehaviour
{

    public GameObject Sparks;
    public new Light light;
    private ParticleSystem sparksPS;


    private void Start()
    {
        sparksPS = Sparks.GetComponent<ParticleSystem>();
        StopSparks();
    }

    public void PlaySparks()
    {
        sparksPS.Play();
        light.enabled = true;
    }

    public void StopSparks()
    {
        sparksPS.Stop();
        light.enabled = false;
    }

}
