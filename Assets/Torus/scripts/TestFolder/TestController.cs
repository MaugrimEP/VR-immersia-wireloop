using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    private VectorManager vm;

    private float interpenetrationDelta;
    private float lastDelta;

    private void Start()
    {
        vm = GetComponent<VectorManager>();
    }

    public void TouchPipe(Collision collision)
    {
        HandleCollision(collision);
    }

    public void LeavePipe(Collision collision)
    {
        vm.ClearVector();
    }

    public void StayPipe(Collision collision)
    {
        {
            float interpenetration = Utils.MeanCollisonSeparation(collision);
            interpenetrationDelta = interpenetration - lastDelta;
            lastDelta = interpenetration;
        }
        Debug.Log($"interpenetrationDelta = {interpenetrationDelta}");
    }

    public void HandleCollision(Collision collision)
    {
        Debug.Log($"Utils.MeanCollisonSeparation(collision) = {Utils.MeanCollisonSeparation(collision)}");
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contactPoint = collision.GetContact(i);
            //Debug.Log($"Utils.MeanCollisonSeparation(collision) = {Utils.MeanCollisonSeparation(collision)}");
            //vm.DrawVector(contactPoint.point, contactPoint.normal, Color.red, "normal");
        }
    }
}
