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
        Unity, Simulator, SingleArm125, SingleArm126, ImmersiaLeftArm, ImmersiaRightArm
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
    /// <summary>
    /// Set in the Awake, so use it in the start or after
    /// </summary>
    public bool UseWand;
    [HideInInspector]
    public string ARM_IP;
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
    public float mass = 0.2f;
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

    private Transform HandNode;

    private void SetIP()
    {
        string commandeLineIP = SelectionArgumentLine.FromCommandeLine();
        Debug.Log($"Commande line IP: {commandeLineIP}");

        //if we use the wand
        if (commandeLineIP.Equals(SelectionArgumentLine.USE_WAND))
        {
            UseWand = true;
            ARM_IP = "";
            return;
        }

        //if an IP was given
        if (commandeLineIP != "")
        {
            ARM_IP = commandeLineIP;
            return;
        }

        //if nothing was given, use the build parameter
        switch (armSelection)
        {
            case ArmSelection.Unity:
                ARM_IP = "";
                return;
            case ArmSelection.Simulator:
                ARM_IP = "127.0.0.1";
                return;
            case ArmSelection.SingleArm125:
                ARM_IP = "131.254.154.16#5125";
                return;
            case ArmSelection.SingleArm126:
                ARM_IP = "131.254.18.52#5126";
                return;
            case ArmSelection.ImmersiaLeftArm:
                ARM_IP = "131.254.154.172#6001";
                return;
            case ArmSelection.ImmersiaRightArm:
                ARM_IP = "131.254.154.172#6003";
                return;
            default:
                ARM_IP = "";
                return;
        }
    }

    public bool UseVirtuose()
    {
        return !ARM_IP.Equals("") && !UseWand;
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

    private void Awake()
    {
        SetIP();
        HandNode = GameObject.Find("HandNode").GetComponent<Transform>();

        Application.targetFrameRate = 100;

        (float[] appliedInertie, float appliedMass) = GetMassAndInertie();

        if (Debug.isDebugBuild) VRTools.Log($"appliedInertie : {Utils.ArrayToString(appliedInertie)}, appliedMass = {appliedMass}");

        if (UseVirtuose())
        {//init the virtuoseManager component
            (virtuoseManager.inerties, virtuoseManager.mass) = (appliedInertie, appliedMass);
            virtuoseManager.BaseFramePosition = Vector3.zero;
            virtuoseManager.powerOnKey = KeyCode.P;
            virtuoseManager.CommandType = modeVirtuose;
            virtuoseManager.Arm.Ip = ARM_IP;
        }
    }

    private void Start()
    {
        if (VRTools.IsClient())
        {
            virtuoseManager.enabled = false;
        }
    }

    public void SetSpeedIdentity()
    {
        if (UseVirtuose())
            virtuoseManager.Virtuose.Speed = virtuoseManager.Virtuose.Speed;
    }

    public (Vector3 Position, Quaternion Rotation) GetVirtuosePose()
    {
        if (UseVirtuose())
            return virtuoseManager.Virtuose.Pose;
        if (UseWand)
            return (HandNode.position, HandNode.rotation);
        return (Vector3.zero, Quaternion.identity);
    }

    public bool IsScaleOne()
    {
        return armSelection == ArmSelection.ImmersiaLeftArm || armSelection == ArmSelection.ImmersiaRightArm;
    }

    public (Vector3 Position, Quaternion Rotation) GetVirtuosePoseRaw()
    {
        if (UseVirtuose())
            return virtuoseManager.Virtuose.RawPose;
        else
            return (Vector3.zero, Quaternion.identity);
    }

    public void SetVirtuosePose(Vector3 position, Quaternion rotation)
    {
        if (!UseVirtuose()) return;
        if (!HapticEnable) { SetVirtuosePoseIdentity(); return; }
        virtuoseManager.Virtuose.Pose = (position, rotation);
    }

    public void SetVirtuosePoseRaw(Vector3 position, Quaternion rotation)
    {
        if (!UseVirtuose()) return;
        if (!HapticEnable) { SetVirtuosePoseIdentity(); return; }
        virtuoseManager.Virtuose.RawPose = (position, rotation);
    }

    public void VirtAddForceIdentity()
    {
        if (!UseVirtuose()) return;
        virtAddForce(Vector3.zero, Vector3.zero);
    }

    public void virtAddForce(Vector3 force, Vector3 torque)
    {
        if (!UseVirtuose()) return;
        if (!HapticEnable) { VirtAddForceIdentity(); return; }
        virtuoseManager.Virtuose.virtAddForce = (force, torque);
    }

    public void SetVirtuosePoseIdentity()
    {
        if (!UseVirtuose()) return;
        virtuoseManager.Virtuose.RawPose = virtuoseManager.Virtuose.RawPose;
    }

    public void SetPower(bool powerState)
    {
        if (!UseVirtuose()) return;
        virtuoseManager.Virtuose.Power = powerState;
    }

    public bool Button(int button = 2)
    {
        return virtuoseManager.IsButtonPressed(button);
    }
}
