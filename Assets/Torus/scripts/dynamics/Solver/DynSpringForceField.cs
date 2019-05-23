using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynSpringForceField : DynIForceField
{
    public DynElement Element1;
    public DynElement Element2;
    public float Stiffness;
    public float EquilibriumLenght;
    public float Dampling;

    public static float Epislon;

    DynSpringForceField(DynElement e1, DynElement e2, float stiffness, float equilibriumLength, float dampling)
    {
        Element1          = e1;
        Element2          = e2;
        Stiffness         = stiffness;
        EquilibriumLenght = equilibriumLength;
        Dampling          = dampling;
    }

    public void AddForce()
    {
        Vector3 displacementVector = (Element2.Position - Element2.Position) / (Element1.Position - Element2.Position).magnitude;

        float displacementLength = 0.0f;
        {
            float length_k_j_i = -Stiffness * ((Element1.Position - Element2.Position).magnitude - EquilibriumLenght);
            float length_kc_j_i = -Dampling * Vector3.Dot(Element1.Velocity - Element2.Velocity, displacementVector);
            displacementLength = length_k_j_i + length_kc_j_i;
        }

        if (displacementLength <= Epislon) return;

        {
            Vector3 fj_i = displacementLength * displacementVector;
            Vector3 fi_j = -fj_i;

            Element1.Force += fj_i;
            Element2.Force += fi_j;
        }
    }
}
