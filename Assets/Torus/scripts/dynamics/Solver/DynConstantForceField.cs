using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynConstantForceField : DynIForceField
{
    public List<DynElement> Elements;
    public Vector3 Force;

    public DynConstantForceField(List<DynElement> elements, Vector3 force)
    {
        Elements = elements;
        Force    = force;
    }

    public void AddForce()
    {
        foreach (DynElement e in Elements)
            e.Force += Force * e.Mass;
    }
}
