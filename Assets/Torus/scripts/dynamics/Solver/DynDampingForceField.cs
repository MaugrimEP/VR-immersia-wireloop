using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynDampingForceField : DynIForceField
{
    public List<DynElement> Elements;
    public float Dampling;

    public DynDampingForceField(List<DynElement> elements, float dampling)
    {
        Elements = elements;
        Dampling = dampling;
    }

    public void AddForce()
    {
        foreach (DynElement e in Elements)
            e.Force += -Dampling * e.Velocity;
    }
}
