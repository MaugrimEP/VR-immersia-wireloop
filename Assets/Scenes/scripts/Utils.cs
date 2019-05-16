using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector3 Pow(Vector3 v,float power)
    {
        return new Vector3(Mathf.Pow(v.x, power), Mathf.Pow(v.y, power), Mathf.Pow(v.z, power));
    }

    public static Vector3 SignedPow(Vector3 v, float power)
    {
        float signex = v.x < 0 ? -1 : 1;
        float signey = v.y < 0 ? -1 : 1;
        float signez = v.z < 0 ? -1 : 1;
        return new Vector3(signex * Mathf.Pow(v.x, power), signey * Mathf.Pow(v.y, power), signez * Mathf.Pow(v.z, power));
    }

    public static Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 Mul(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
}
