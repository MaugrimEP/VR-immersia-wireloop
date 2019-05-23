using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynEulerExplicitSolver : DynISolver
{
    public void Solve(float dt, List<DynElement> elements)
    {
        foreach (DynElement e in elements)
            if (!e.IsFixed)
            {
                e.Velocity += e.Force    * dt / e.Mass;
                e.Position += e.Velocity * dt / e.Mass;
            }
    }
}
