using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynSystem
{
    public List<DynElement> Elements;
    public List<DynIForceField> ForceFields;
    public DynISolver Solver;
    public float Dt;
    public List<DynCollision> Collisions;

    public bool HandleCollisions;
    public float Restitution;

    public DynSystem()
    {
        Collisions = new List<DynCollision>();
        ForceFields = new List<DynIForceField>();
        Elements = new List<DynElement>();
        Dt = 0.1f;
        Restitution = 0.0f;
        HandleCollisions = true;
        Solver = new DynEulerExplicitSolver();
    }

    public bool HasCollision()
    {
        return Collisions.Count != 0;
    }

    public void ComputeSimuationStep()
    {
        //Compute DynamicElement's force
        foreach (DynElement e in Elements)
            e.Force = Vector3.zero;
        foreach (DynIForceField f in ForceFields)
            f.AddForce();

        //integrate position and velocity of DynamicElement
        Solver.Solve(Dt, Elements);

        //dectect and resolve collisions
        if (HandleCollisions)
        {
            DetectCollisions();
            SolveCollisions();
        }
    }

    public void Clear()
    {
        Elements.Clear();
        ForceFields.Clear();
    }

    /// <summary>
    /// will not be used here, we will detect the collision with unity engine and add the collision ourself to the list
    /// </summary>
    private void DetectCollisions()
    {
        
    }

    private void SolveCollisions()
    {
        while (Collisions.Count!=0)
        {
            DynCollision collision = Collisions[Collisions.Count - 1];
            collision.SolveCollision();
            Collisions.RemoveAt(Collisions.Count - 1);
        }
    }
}
