using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine;

public class VirtuoseManager : MonoBehaviour
{
    public VirtuoseArm Arm;
    public VirtuoseAPIHelper Virtuose;

    public Vector3 BaseFramePosition;

    bool[] buttonsPressed = new bool[4];
    bool[] buttonsToggled = new bool[4];

    bool isMaster;

    public VirtuoseAPI.VirtCommandType CommandType = VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE;

    [Range(VirtuoseAPIHelper.MIN_MASS, VirtuoseAPIHelper.MAX_MASS)]
    public float mass = 0.2f;//0.05
    //[Range(VirtuoseAPIHelper.MIN_INERTIE, VirtuoseAPIHelper.MAX_INERTIE)]
    public float[] inerties = new float[] { 0.1f, 0f  , 0f ,
                                            0f  , 0.1f, 0f ,
                                            0f  , 0f  ,0.1f};

    public KeyCode powerOnKey = KeyCode.P;

    public bool Initialized
    {
        get; private set;
    }

    void OnEnable()
    { 
        if (VRTools.IsMaster())
            StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        //Wait one frame to avoid virtuose timeout, due to first frame lag in MiddleVR.
        yield return null;

        OpenArm();
        if (Arm.IsConnected)
        {
            InitArm();

            //Disable watchdog to allow to pause editor without having timeout error.
            if (Application.isEditor)
                Virtuose.ControlConnexion(false);

            Initialized = true;
        }
    }
    
    public IEnumerator WaitVirtuoseConnexion(Action action)
    {
        while (!Arm.IsConnected)
            yield return null;

        action();
    }

    void Start()
    {
        isMaster = VRTools.IsMaster(); //need to cache the value for OnDisable when MiddleVR is already
    }

    void OnDisable()
    {
        if (Initialized)
        {
            if (Application.isEditor)
                Virtuose.ControlConnexion(true);

            if (isMaster)
                DisconnectArm();

            Initialized = false;
        }
    }


    /// <summary>
    /// Just for ip to know if it's scale one arm.
    /// </summary>
    /// <returns></returns>
    public bool IsScaleOne()
    {
        return Virtuose.DeviceID == VirtuoseAPIHelper.DeviceType.DEVICE_SCALE1;
    }

    void OpenArm()
    {
        Virtuose = new VirtuoseAPIHelper(Arm);
        (int majorVersion, int minorVersion) = Virtuose.APIVersion;
        VRTools.Log("[VirtuoseManager] Virtuose API version : " + majorVersion + "." + minorVersion);

        Virtuose.Open(Arm.Ip);

        if (Arm.IsConnected)
        {
            (majorVersion, minorVersion) = Virtuose.ControllerVersion;
            VRTools.Log("[VirtuoseManager] Virtuose controller version : " + majorVersion + "." + minorVersion);
        }
    }

    void InitArm()
    {
        Virtuose.InitDefault();
        Virtuose.BaseFrame = (BaseFramePosition, Quaternion.identity);
        Virtuose.CommandType = CommandType;
        Virtuose.Power = true;

        if (CommandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
        {
            Virtuose.AttachVO(mass, inerties);
        }
    }
    
    void Update()
    {
        if (VRTools.IsMaster() && Initialized)
        {
            Virtuose.UpdateArm();
     
            if (Arm.IsConnected)
                UpdateArm();
            else if (!Arm.IsConnected)
                VRTools.LogError("[Error][VirtuoseManager] Arm not connected. Cannot execute virtuose command.");

            //Press both button to power again.
            if (Arm.IsConnected &&
                ((Virtuose.IsButtonPressed() && !Virtuose.DeadMan) ||
                VRTools.GetKeyDown(powerOnKey)))
            {
                VRTools.Log("[Info][VirtuoseManager] Force power on.");
                Virtuose.Power = true;
            }
        }
    }
    
    void UpdateArm()
    {       
        for(int b = 0; b < 3; b++)
        {
            bool buttonState = Virtuose.Button(b);   
            buttonsToggled[b] = buttonsPressed[b] != buttonState;
            buttonsPressed[b] = buttonState;
        }    
    }

    public bool IsButtonPressed(int button = 2)
    {
        return buttonsPressed[button];
    }

    public bool IsButtonToggled(int button = 2)
    {
        return buttonsToggled[button];
    }

    /// <summary>
    /// Transform int button state into boolean button state.
    /// </summary>
    /// <param name="state">0: button released, 1: button pushed</param>
    /// <returns>True if pushed, False if released</returns>
    bool GetButtonState(int state)
    {
        return state == 1;
    }

    void DisconnectArm()
    {
        Virtuose.Power = false;
        Virtuose.DetachVO();
        Virtuose.Close();
    }

    public delegate int virtDelegate(IntPtr context);
    public delegate int virtDelegateGen<T>(IntPtr context, T value);
    public delegate int virtDelegateRefGen<T>(IntPtr context, ref T value);
    public delegate int virtDelegateGenGen<T, U>(IntPtr context, T value1, U value2);
    public delegate int virtDelegateGenRefGen<T, U>(IntPtr context, T value1, ref U value2);
    public delegate int virtDelegateRefGenRefGen<T, U>(ref T value1, ref U value2);
    public delegate int virtDelegateContextRefGenRefGen<T, U>(IntPtr context, ref T value1, ref U value2);

    /// <summary>
    ///    errorCode = VirtuoseAPI.virtSetPosition(arm.Context, positions);
    ///    if (errorCode == -1)
    ///        VRTools.Log("[VirtuoseManager][Error] virtSetPosition error " + GetError());
    ///        =>
    ///     ExecLogOnError(
    ///         VirtuoseAPI.virtSetPosition, positions);
    /// </summary>
    /// <param name="virtMethod"></param>
    public void ExecLogOnError(virtDelegate virtMethod)
    {
        int errorCode = virtMethod(Arm.Context);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError(virtDelegate virtMethod, IntPtr context)
    {
        int errorCode = virtMethod(context);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateGen<T> virtMethod, T value)
    {
        int errorCode = virtMethod(Arm.Context, value);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateGen<T> virtMethod, IntPtr context, T value)
    {
        int errorCode = virtMethod(context, value);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateRefGen<T> virtMethod, ref T value)
    {
        int errorCode = virtMethod(Arm.Context, ref value);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateRefGen<T> virtMethod, IntPtr context, ref T value)
    {
        int errorCode = virtMethod(context, ref value);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateGenGen<T, U> virtMethod, IntPtr context, T value1, U value2)
    {
        int errorCode = virtMethod(context, value1, value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateGenGen<T, U> virtMethod, T value1, U value2)
    {
        int errorCode = virtMethod(Arm.Context, value1, value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateRefGenRefGen<T, U> virtMethod, ref T value1, ref U value2)
    {
        int errorCode = virtMethod(ref value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateGenRefGen<T, U> virtMethod, T value1, ref U value2)
    {
        int errorCode = virtMethod(Arm.Context, value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateGenRefGen<T, U> virtMethod, IntPtr context, T value1, ref U value2)
    {
        int errorCode = virtMethod(context, value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateContextRefGenRefGen<T, U> virtMethod, ref T value1, ref U value2)
    {
        int errorCode = virtMethod(Arm.Context, ref value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateContextRefGenRefGen<T, U> virtMethod, IntPtr context, ref T value1, ref U value2)
    {
        int errorCode = virtMethod(context, ref value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void LogError(int errorCode, string errorMessage)
    {
        if (errorCode == -1)
        {
            VRTools.LogError("[Error][VirtuoseManager] " + errorMessage + " error " + GetError());
            Arm.HasError = true;
        }
    }

    public string GetError()
    {
        int errorCode = VirtuoseAPI.virtGetErrorCode(Arm.Context);
        return " (error " + errorCode + " : " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(VirtuoseAPI.virtGetErrorMessage(errorCode)) + ")";
    }


}