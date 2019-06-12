using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    /// <summary>
    /// Enum to choose what mode we are in, Unity to dont interact, Simulator to connect with the virtuose simulator, SingleArm to connect with the virtuose arm
    /// </summary>
    public enum ArmSelection
    {
        Unity, Simulator, SingleArm125, SingleArm126
    }
    public enum MassInertiaMode
    {
        Default, InertiesInventor, ComputedInertie
    }

    public VirtuoseManager virtuoseManager;
    public ArmSelection armSelection;
    /// <summary>
    /// If HapticEnable is True, then the output to the virtuose will use the class value, if it's False, the input will be the output
    /// </summary>
    public bool HapticEnable;
    public VirtuoseAPI.VirtCommandType modeVirtuose;

    #region value read from the virtuose, they should be in unity coordinate system
    [HideInInspector]
    public Vector3 virtuose_Position;
    [HideInInspector]
    public Quaternion virtuose_Rotation;
    [HideInInspector]
    public Vector3 virtuose_Force;
    [HideInInspector]
    public Vector3 virtuose_Torque;
    #endregion

    [Range(VirtuoseAPIHelper.MIN_MASS, VirtuoseAPIHelper.MAX_MASS)]
    public float mass    = 0.2f;
    public float density = 1350; // in kg.m^-3
    private readonly float[] defaultInertie = new float[] 
                                            { 0.02f, 0f  , 0f ,
                                            0f  , 0.02f, 0f ,
                                            0f  , 0f  ,0.02f};

    private readonly float[] inertiesInventor = new float[] { 77067f *  0.000001f, 0f  , 0f ,
                                                  0f  , 52828f * 0.000001f, 0f ,
                                                  0f  , 0f  , 24441f * 0.000001f};
    public MassInertiaMode modeInertie;
    public RaquetteController rc;

    private string GetIP()
    {
        switch (armSelection)
        {
            case ArmSelection.Unity:
                return "";
            case ArmSelection.Simulator:
                return "127.0.0.1";
            case ArmSelection.SingleArm125:
                return "131.254.154.16#5125";
            case ArmSelection.SingleArm126:
                return "131.254.18.52#5126";
            default:
                return "";
        }
    }

    public bool UseVirtuose()
    {
        return armSelection != ArmSelection.Unity;
    }

    public (float[] inertie, float mass) GetMassAndInertie()
    {
        float[] appliedInertie = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        float appliedMass = 0.2f;

        if (rc.SendForce())
        {
            (appliedInertie, appliedMass) = (new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, mass);
        }
        else
        {
            switch (modeInertie)
            {
                case MassInertiaMode.ComputedInertie:
                    (InertiaMatrix inertiaMatrix, float massFromInertia) = InertiaMatrix.GetRaquette(density: density);
                    inertiaMatrix = 0.1f * inertiaMatrix;
                    massFromInertia = 0.1f * massFromInertia;
                    (appliedInertie, appliedMass) = (inertiaMatrix.GetMatrix1D(), massFromInertia);
                    break;
                case MassInertiaMode.InertiesInventor:
                    (appliedInertie, appliedMass) = (inertiesInventor, 0.2f);
                    break;
                case MassInertiaMode.Default:
                    (appliedInertie, appliedMass) = (defaultInertie, mass);
                    break;
            }
        }
        return (appliedInertie, appliedMass);
    }

    private void Awake () {
        Application.targetFrameRate = 100;

        (float[] appliedInertie, float appliedMass) = GetMassAndInertie();

        Debug.Log($"appliedInertie : {Utils.ArrayToString(appliedInertie)}, appliedMass = {appliedMass}"); //TODO to remove verbose

        if (UseVirtuose())
        {//init the virtuoseManager component
            (virtuoseManager.inerties, virtuoseManager.mass) = (appliedInertie, appliedMass);
            virtuoseManager.BaseFramePosition = Vector3.zero;
            virtuoseManager.powerOnKey = KeyCode.P;
            virtuoseManager.CommandType = modeVirtuose;
            virtuoseManager.Arm.Ip = GetIP();
        }
    }

    private void Update () {
    }
}
