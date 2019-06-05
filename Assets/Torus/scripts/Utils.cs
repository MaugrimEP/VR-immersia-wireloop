using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector3 Pow(Vector3 v, float power)
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

    public static List<Transform> getAllChilds(Transform me)
    {
        List<Transform> childs = new List<Transform>();
        foreach (Transform child in me)
        {
            childs.AddRange(getAllChilds(child));
        }

        return childs;
    }

    public static float MeanCollisonSeparation(Collision collision)
    {
        float separation = 0;
        for (int i = 0; i < collision.contactCount; ++i)
        {
            separation += collision.GetContact(i).separation;
        }
        separation /= collision.contactCount;
        return Mathf.Abs(separation);
    }

    public static Vector3 ClampDisplacement(Vector3 displacementVector, float clampDistance)
    {
        float travelDistance = displacementVector.magnitude;

        if (travelDistance > clampDistance)
        {// need to size down the distance
            displacementVector = Vector3.Lerp(Vector3.zero, displacementVector, clampDistance / travelDistance);
            //displacementVector /= (travelDistance / clampDistance);
        }
        return displacementVector;
    }

    public static Vector3 ClampDisplacement(Vector3 oldPosition, Vector3 newPosition, float clampDistance)
    {
        return oldPosition + ClampDisplacement(newPosition - oldPosition, clampDistance);
    }

    public static Vector3 ClampVector3(Vector3 v, float max)
    {
        return new Vector3(Mathf.Clamp(v.x, -max, max), Mathf.Clamp(v.y, -max, max), Mathf.Clamp(v.z, -max, max));
    }

    public static Vector3 U2VVector3(Vector3 v)
    {
        return new Vector3(-v.z, v.x, v.y);
    }

    public static Vector3 U2VTorque(Vector3 t)
    {
        return new Vector3(t.x, t.y, t.z);
    }

    public static Vector3 V2UVector3(Vector3 v)
    {
        return new Vector3(v.y, v.z, -v.x);
    }

    public static Quaternion ClampRotation(Quaternion rotation, float maxDot)
    {
        float dot = Quaternion.Dot(Quaternion.identity, rotation);
        if (dot > maxDot)
        {
            rotation = Quaternion.Slerp(Quaternion.identity, rotation, maxDot / dot);
        }
        return rotation;
    }

    #region Wrapper on virtuose input/output
    public static (Vector3 Position, Quaternion Rotation) V2UPosRot(Vector3 Position, Quaternion Rotation)
    {
        Vector3 newPosition = new Vector3(Position.x, Position.y, Position.z);
        Quaternion newRotation = new Quaternion(-Rotation.y, -Rotation.z, Rotation.x, Rotation.w);

        return (newPosition, newRotation);
    }

    public static (Vector3 Force, Vector3 Torque) V2UForceTorque(Vector3 Force, Vector3 Torque)
    {
        return (Force, Torque);
    }

    public static (Vector3 Position, Quaternion Rotation) U2VPosRot(Vector3 Position, Quaternion Rotation)
    {
        Vector3 newPosition = new Vector3(Position.x, Position.y, Position.z);
        //TODO : la rotation
        Vector3 eulerRotation = Rotation.eulerAngles;
        Quaternion newRotation = Quaternion.Euler(-eulerRotation.z, eulerRotation.x, eulerRotation.y);
        return (newPosition, newRotation.normalized);
    }

    public static (Vector3 Force, Vector3 Torque) U2VForceTorque(Vector3 Force, Vector3 Torque)
    {
        return (new Vector3(-Force.z, Force.x, Force.y), new Vector3(Torque.z, Torque.x, Torque.y));
    }

    public static Vector3 Angles(Vector3 v1, Vector3 v2)
    {
        return new Vector3(
            Vector2.SignedAngle(new Vector2(v1.y, v1.z), new Vector2(v2.y, v2.z)),
            Vector2.SignedAngle(new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z)),
            Vector2.SignedAngle(new Vector2(v1.x, v1.y), new Vector2(v2.x, v2.y))
            );
    }

    public static Color RandomColor()
    {
        return Random.ColorHSV();
    }
    #endregion

}
