using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticTest : MonoBehaviour
{
    public VirtuoseArm arm;
    public GameObject linkedObject;

    public Light light;

    float[] forces = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    public int forceIndex = 0;
    public float force = 3;

    bool[] buttonsPressed = new bool[4];
    bool[] buttonsToggled = new bool[4];

    bool isMaster;

	void Start () 
    {
        isMaster = VRTools.IsMaster();
        if (isMaster)
        {
            VRTools.Log("Starting Arm");
            OpenArm();
            InitArm();
        }
	}

    void OpenArm()
    {
        int majorVersion = 0;
        int minorVersion = 0;

        int errorCode = VirtuoseAPI.virtAPIVersion(ref majorVersion, ref minorVersion);
        VRTools.Log("[VirtuoseManager][" + errorCode + "] Virtuose API version : " + majorVersion + "." + minorVersion);

        arm.Context = VirtuoseAPI.virtOpen(arm.Ip);
        if (arm.Context.ToInt32() == 0)
            VRTools.Log("[VirtuoseManager][Error] Connection error with the arm " + arm.Ip + GetError());

        else
        {
            VRTools.Log("[VirtuoseManager] Connection successful with the arm " + arm.Ip);
            arm.IsConnected = true;
        }
    }

    void InitArm()
    {
        float[] identity = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f};
        
        LogError(VirtuoseAPI.virtSetIndexingMode(arm.Context, (ushort)VirtuoseAPI.VirtIndexingType.INDEXING_ALL),
            "virtSetIndexingMode");

        LogError(VirtuoseAPI.virtSetForceFactor(arm.Context, 1.0f),
            "virtSetForceFactor");

        LogError(VirtuoseAPI.virtSetSpeedFactor(arm.Context, 1.0f),
            "virtSetSpeedFactor");

        LogError(VirtuoseAPI.virtSetTimeStep(arm.Context, 0.003f),
            "virtSetTimeStep");

        LogError(VirtuoseAPI.virtSetBaseFrame(arm.Context, identity),
            "virtSetBaseFrame");

        LogError(VirtuoseAPI.virtSetObservationFrame(arm.Context, identity),
            "virtSetObservationFrame");
        
        LogError(VirtuoseAPI.virtSetObservationFrameSpeed(arm.Context, identity),
            "virtSetObservationFrameSpeed");

        LogError(VirtuoseAPI.virtSetCommandType(arm.Context, (ushort)VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE),
            "virtSetCommandType");

        float[] positions = { 0f, 3.93f, 0.600f + 0.215f, 0f, 0f, 0f, 1f };
        //To unity {3.93f, 0.815, 0f}
        VirtuoseAPI.virtSetBaseFrame(arm.Context, positions);

        LogError(VirtuoseAPI.virtSetPowerOn(arm.Context, 1),
            "virtSetPowerOn");
    }

    void Update()
    {
        if (arm.IsConnected && !arm.HasError && isMaster)
        {
            UpdateArm();
        }
    }


    void UpdateArm()
    {
        
        int buttonState = 0;
        LogError(VirtuoseAPI.virtGetButton(arm.Context, 0, ref buttonState), "virtGetButton");
        buttonsToggled[0] = buttonsPressed[0] != GetButtonState(buttonState);
        buttonsPressed[0] = GetButtonState(buttonState);
        if(buttonsToggled[0])
        {
          //  Feedback();
            VRTools.Log("Toggled 0");
        }

        LogError(VirtuoseAPI.virtGetButton(arm.Context, 1, ref buttonState), "virtGetButton");
        buttonsToggled[1] = buttonsPressed[1] != GetButtonState(buttonState);
        buttonsPressed[1] = GetButtonState(buttonState);
        if (buttonsToggled[1])
        {
         //   Previous();
            VRTools.Log("Toggled 1");
        }

        LogError(VirtuoseAPI.virtGetButton(arm.Context, 2, ref buttonState), "virtGetButton");
        buttonsToggled[2] = buttonsPressed[2] != GetButtonState(buttonState);
        buttonsPressed[2] = GetButtonState(buttonState);
        if (buttonsToggled[2])
        {
            VRTools.Log("Toggled 2");
        }
        PressToForce(buttonsPressed[2]);

        LogError(VirtuoseAPI.virtGetButton(arm.Context, 3, ref buttonState), "virtGetButton");
        buttonsToggled[3] = buttonsPressed[3] != GetButtonState(buttonState);
        buttonsPressed[3] = GetButtonState(buttonState);
        if (buttonsToggled[3])
        {
          //  Next();
            VRTools.Log("Toggled 3");
        }
        
        SetForce();
    }

    void SetForce()
    {
        float[] positions = new float[7];
        for (int f = 0; f < forces.Length; f++)
        {
            if(Mathf.Abs(forces[f]) > 5)
                VRTools.Log("[Haptic][Error] Force clamped because outside of limit");

            forces[f] = Mathf.Clamp(forces[f], -5, 5);
        }

        int errorCode = VirtuoseAPI.virtSetForce(arm.Context, forces);
        if (errorCode == -1)
            VRTools.Log("[Haptic][Error] virtSetForce error " + GetError());

        errorCode = VirtuoseAPI.virtGetPosition(arm.Context, positions);
        if (errorCode == -1)
            VRTools.Log("[Haptic][Error] virtGetPosition error " + GetError());

        if (linkedObject != null)
            linkedObject.transform.position = ConvertVirtuoseToUnity(positions);
    }

    [ContextMenu("Next")]
    void Next()
    {
        forceIndex = (forceIndex + 1) % forces.Length;
    }

    [ContextMenu("Previous")]
    void Previous()
    {
        forceIndex = (forceIndex == 0) ? forces.Length - 1 : forceIndex - 1;
    }

    [ContextMenu("PressToForce")]
    void PressToForce(bool pressToForce)
    {
        if (pressToForce)
            forces[forceIndex] = force;
        else
            forces[forceIndex] = 0;
    }

    Vector3 ConvertVirtuoseToUnity(float[] positions)
    {
        if(positions.Length >= 3)
            return new Vector3(positions[1], positions[2], -positions[0]);
        return Vector3.zero;
    }

    //float[] ConvertVirtuoseToUnity(Vector3 position)
    //{
    //    if (positions.Length >= 3)
    //        return new Vector3(positions[1], positions[2], -positions[0]);
    //    return Vector3.zero;
    //}

    [ContextMenu("Feedback")]
    void Feedback()
    {
        light.enabled = !light.enabled;
    }

    /// <summary>
    /// Transform int button state into boolean button state.
    /// </summary>
    /// <param name="state">0: button released, 1: button pushed</param>
    /// <returns>True if pushed, False if released</returns>
    bool GetButtonState(int state)
    {
        return state == 1 ? true : false;
    }

    void OnApplicationQuit()
    {
        if (isMaster)
        {
            DisconnectArm();
        }
    }


    void DisconnectArm()
    {
        int errorCode = VirtuoseAPI.virtClose(arm.Context);
        if (errorCode == 0)
            VRTools.Log("[Haptic] Disconnection successful with the arm " + arm.Ip);
        
        else
            VRTools.Log("[Haptic][Error] Disconnection error with arm " + arm.Ip + GetError());
    }

    void LogError(int errorCode, string errorMessage)
    {
        if (errorCode == -1)
        {
            VRTools.Log("[Haptic][Error] " + errorMessage + " error " + GetError());
            arm.HasError = true;
            //Debug.Break();
        }
    }

    string GetError()
    {
        int errorCode = VirtuoseAPI.virtGetErrorCode(arm.Context);
        return " (error " + errorCode + " : " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(VirtuoseAPI.virtGetErrorMessage(errorCode)) + ")";
    }


}
