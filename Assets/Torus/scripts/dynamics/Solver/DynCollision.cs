using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynCollision
{
    protected readonly float restitution;

    public DynCollision(float _restitution)
    {
        restitution = _restitution;
    }

    public abstract void SolveCollision();
}
