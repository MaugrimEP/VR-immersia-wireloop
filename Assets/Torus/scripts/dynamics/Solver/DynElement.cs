using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DynElement
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Mass;
    public bool IsFixed;
    public Vector3 Force;

    public DynElement(Vector3 position, Vector3 velocity, float mass, bool isFixed)
    {
        Position = position;
        Velocity = velocity;
        Mass = mass;
        IsFixed = isFixed;
        Force = Vector3.zero;
    }
}
