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

    public float force = 3;

    public float stiffness = 10;

    public Vector3 BaseFramePosition;

    [Range(MIN_MASS, MAX_MASS)]
    public float mass = 0.2f;
    [Range(MIN_INERTIE, MAX_INERTIE)]
    public float inertie = 0.1f;

    public GameObject target;

    bool[] buttonsPressed = new bool[4];
    bool[] buttonsToggled = new bool[4];

    float[] forces = { 0, 0, 0, 0, 0, 0 };
    float[] positions = { 0, 0, 0, 0, 0, 0, 1 };
    float[] speeds = { 0, 0, 0, 0, 0, 0 };

    bool isMaster;

    Vector3 lastFramePosition;
    Quaternion lastFrameRotation;

    public VirtuoseAPI.VirtCommandType commandType = VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE;

    /// <summary>
    /// Vector3(x, y, z) + Quaternion(x, y, z, w)
    /// </summary>
    const int POSITIONS_COMPONENTS = 7;

    /// <summary>
    /// In newton.
    /// </summary>
    const float MAX_FORCE = 5;

    /// <summary>
    /// In kg.
    /// </summary>
    const float MIN_MASS = 0.001f;
    const float MAX_MASS = 1;

    /// <summary>
    /// In Kg / m
    /// </summary>
    const float MIN_INERTIE = 0;
    const float MAX_INERTIE = 1;

    /// <summary>
    /// In meters.
    /// </summary>
    const float MAX_DISTANCE_PER_FRAME = 0.3f;

    const float MAX_DOT_DIFFERENCE = 0.05f;


    Rigidbody targetRigidbody;
    InfoCollision infoCollision;

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

            if (target)
            {
                targetRigidbody = target.GetComponentInChildren<Rigidbody>();
                targetRigidbody.LogErrorIfNull();

                infoCollision = target.GetComponentInChildren<InfoCollision>();
                infoCollision.LogErrorIfNull();

                (Vector3 position, Quaternion rotation) pose = Virtuose.Pose;

                targetRigidbody.position = pose.position;
                targetRigidbody.rotation = pose.rotation;

                lastFramePosition = pose.position;
                lastFrameRotation = pose.rotation;
            }

            //Disable watchdog to allow to pause editor without having timeout error.
            if (Application.isEditor)
                Virtuose.ControlConnexion(false);

            Initialized = true;
        }
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
        Virtuose.CommandType = commandType;
        Virtuose.Power = true;

        if (commandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
        {
            Virtuose.Pose = (Vector3.zero, Quaternion.identity);

            ExecLogOnError(
               VirtuoseAPI.virtSetSpeed, speeds);


            if (mass > MAX_MASS)
                VRTools.LogWarning("[Warning][VirtuoseManager] Mass is aboved authorized threshold (" +  mass + ">" + MAX_MASS + ")");
            else if(mass < 0)
                VRTools.LogWarning("[Warning][VirtuoseManager] Mass must be > 0.");

            mass = Mathf.Clamp(mass, MIN_MASS, MAX_MASS); //Use 1g as minimum, completely arbitrary.

            if (inertie > MAX_INERTIE)
                VRTools.LogWarning("[Warning][VirtuoseManager] Inertie is above authorized threshold (" + inertie + ">" + MAX_INERTIE + ")");
            else if(inertie < 0)
                VRTools.LogWarning("[Warning][VirtuoseManager] Inertie must be >= 0.");

            inertie = Mathf.Clamp(inertie, MIN_INERTIE, MAX_INERTIE);

            float[] inerties = {
                inertie, 0, 0,
                0, inertie, 0,
                0, 0, inertie }; // Haption CObject.SetInertie();

            ExecLogOnError(
                VirtuoseAPI.virtAttachVO, mass, inerties);
        }

        //string filePath = Path.Combine(Application.dataPath, name + "_haption.log");
        //VRTools.Log(filePath);
        //IntPtr filePathPtr = Marshal.StringToHGlobalUni(filePath);

        //ExecLogOnError(
        //    VirtuoseAPI.virtSetOutputFile, filePathPtr);

       //Marshal.FreeHGlobal(filePathPtr);
    }

    void UpdateMassAndInertie()
    {

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

        if (VRTools.GetFrameCount() % 1000 == 0)
            VRTools.Log("Deltatime " + VRTools.GetDeltaTime());
        
    }
    
    void FixedUpdate()
    {
        if (VRTools.IsMaster() && Initialized && target)
        {
            //VirtuoseAPI.VirtCommandType.COMMAND_TYPE_ADMITTANCE is deprecated
            if (Arm.IsConnected  
                && commandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_VIRTMECH)
            {
                // GetPositions();
                //SetPositions();
                SetRigidbodyPositions();
            }
        }
    }

    void UpdateArm()
    {       
        int buttonState = 0;
        for(int b = 0; b < 3; b++)
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetButton, b, ref buttonState);

            buttonsToggled[b] = buttonsPressed[b] != GetButtonState(buttonState);
            buttonsPressed[b] = GetButtonState(buttonState);
            if (buttonsToggled[b])
                VRTools.Log("Toggled " + b);
        }

        if(commandType == VirtuoseAPI.VirtCommandType.COMMAND_TYPE_IMPEDANCE)
            SetForce();
    }

    public bool IsButtonPressed(int button = 2)
    {
        return buttonsPressed[button];
    }

    public bool IsButtonToggled(int button = 2)
    {
        return buttonsToggled[button];
    }

   void GetPositions()
    {
        ExecLogOnError(
            VirtuoseAPI.virtGetPosition, positions);

        Vector3 position = VirtuoseToUnityPosition(positions);
        Quaternion rotation = VirtuoseToUnityRotation(positions);

        VRTools.Log("virtGetPosition: " + position.ToString("F4") + "Rotation: " + rotation.ToString("F4"));

        //ExecLogOnError(
        //    VirtuoseAPI.virtGetAvatarFrame, positions);

        //position = VirtuoseToUnityPosition(positions);
        //rotation = VirtuoseToUnityRotation(positions);

        //VRTools.Log("virtGetAvatarFrame: " + position.ToString("F4") + "Rotation: " + rotation.ToString("F4"));


        //ExecLogOnError(
        //     VirtuoseAPI.virtGetAvatarPosition, pos);

        //position = VirtuoseToUnityPosition(positions);
        //rotation = VirtuoseToUnityRotation(positions);

        //VRTools.Log("virtGetAvatarPosition: " + position.ToString("F4") + "Rotation: " + rotation.ToString("F4"));

        //ExecLogOnError(
        //    VirtuoseAPI.virtGetBaseFrame, positions);

        //position = VirtuoseToUnityPosition(positions);
        //rotation = VirtuoseToUnityRotation(positions);

        //VRTools.Log("virtGetBaseFrame: " + position.ToString("F4") + "Rotation: " + rotation.ToString("F4"));

        //ExecLogOnError(
        //     VirtuoseAPI.virtGetObservationFrame, positions);

        //position = VirtuoseToUnityPosition(positions);
        //rotation = VirtuoseToUnityRotation(positions);

        //VRTools.Log("virtGetObservationFrame: " + position.ToString("F4") + "Rotation: " + rotation.ToString("F4"));

        //ExecLogOnError(
        //    VirtuoseAPI.virtGetPhysicalPosition, positions);

        //position = VirtuoseToUnityPosition(positions);
        //rotation = VirtuoseToUnityRotation(positions);

        //VRTools.Log("virtGetPhysicalPosition: " + position.ToString("F4") + "Rotation: " + rotation.ToString("F4"));

    }

    void SetForce()
    {
        for (int f = 0; f < forces.Length; f++)
        {
            if(Mathf.Abs(forces[f]) > MAX_FORCE)
                VRTools.LogError("[Error][VirtuoseManager] Force clamped because outside of limit");

            forces[f] = Mathf.Clamp(forces[f], -MAX_FORCE, MAX_FORCE);
        }

        ExecLogOnError(
            VirtuoseAPI.virtSetForce, forces);
    }



    void SetPositions()
    {
        ExecLogOnError(
            VirtuoseAPI.virtGetPosition, positions);

        ExecLogOnError(
            VirtuoseAPI.virtGetSpeed, speeds);

        Vector3 position = VirtuoseToUnityPosition(positions);
        Quaternion rotation = VirtuoseToUnityRotation(positions);

       // VRTools.Log("GetPositions: " + positions.ToFlattenString());
        VRTools.Log("Position: " + position.ToString("F4"));
        VRTools.Log("Rotation: " + rotation.ToString("F4"));
        //VRTools.Log("Speeds: " + speeds.ToFlattenString());

        Vector3 offset = (IsButtonPressed()) ? -Vector3.up * 0.001f : Vector3.zero;
        positions = ConvertUnityToVirtuose(position + offset, rotation);
      //  VRTools.Log("Converted: " + positions.ToFlattenString());

        float distance = 0;
        float dot = 0;

        if (lastFramePosition != Vector3.zero)
            distance = Vector3.Distance(lastFramePosition, position);

        if (lastFrameRotation != Quaternion.identity)
            dot = Quaternion.Dot(lastFrameRotation, rotation);

        ExecLogOnError(
            VirtuoseAPI.virtSetPosition, positions);

        ExecLogOnError(
           VirtuoseAPI.virtSetSpeed, speeds);

        if (distance > MAX_DISTANCE_PER_FRAME)
        {
            VRTools.LogWarning("[Warning][VirtuoseManager] Haption arm new position is aboved the authorized threshold distance (" + distance + ">" + MAX_DISTANCE_PER_FRAME + ")");
            Virtuose.Power = false;
        }
        if (dot < 1 - MAX_DOT_DIFFERENCE)
        {
            VRTools.LogWarning("[Warning][VirtuoseManager] Haption arm new rotation is aboved authorized the threshold dot (" + dot + " : " + MAX_DOT_DIFFERENCE + ")");
            Virtuose.Power = false;
        }

        lastFramePosition = position;
        lastFrameRotation = rotation;
    }

    void SetTargetPositions()
    {
        ExecLogOnError(
            VirtuoseAPI.virtGetPosition, positions);

        if (target != null)
        {
            target.transform.position = VirtuoseToUnityPosition(positions);
            target.transform.rotation = VirtuoseToUnityRotation(positions);
        }
    }

    void SetRigidbodyPositions()
    {
        if (target != null)
        {
            (Vector3 position, Quaternion rotation) = Virtuose.Pose;

            targetRigidbody.MovePosition(position);
            targetRigidbody.MoveRotation(rotation);

         //   VRTools.Log("Wanted position " + position.ToString("F4") + " rigidb position " + targetRigidbody.position.ToString("F4") + " target transform position " + target.transform.position.ToString("F4") + " V " +targetRigidbody.velocity.ToString("F4"));
         //  VRTools.Log("Wanted rotation " + rotation.ToString("F4") + " real rotation " + targetRigidbody.rotation.ToString("F4"));

            float distance = 0;
            float dot = 0;

            Vector3 normal = target.transform.position - targetRigidbody.position;
            //When there is a collision the rigidbody position is at the virtuose arm position but the transform.position is impacted by the scene physic.
            Vector3 newPosition = infoCollision.IsCollided ? target.transform.position + stiffness * normal : targetRigidbody.position ;
            Quaternion newRotation = infoCollision.IsCollided ? target.transform.rotation : targetRigidbody.rotation;

            distance = Vector3.Distance(position, newPosition);
            dot = Quaternion.Dot(rotation, newRotation);

            //Add extra protection to avoid high velocity movement.
            if (distance > MAX_DISTANCE_PER_FRAME)
            {
                VRTools.LogWarning("[Warning][VirtuoseManager] Haption arm new position is aboved the authorized threshold distance (" + distance + ">" + MAX_DISTANCE_PER_FRAME + "). Power off.");
                Virtuose.Power = false;
            }

            if (dot < 1 - MAX_DOT_DIFFERENCE)
            {
                VRTools.LogWarning("[Warning][VirtuoseManager] Haption arm new rotation is aboved authorized the threshold dot (" + (1 - dot) + " : " + MAX_DOT_DIFFERENCE + "). Power off.");
                Virtuose.Power = false;
            }

            Virtuose.Pose = (newPosition, newRotation);

            lastFramePosition = position;
            lastFrameRotation = rotation;
        }
    }

    /// <summary>
    /// Unity
    /// Y       
    /// |   Z   
    /// | /
    /// |/___X
    /// 
    /// Virtuose
    /// Z       
    /// |   Y   
    /// | /
    /// |/___X
    /// </summary>
    /// <param name="positions"></param>
    /// <returns></returns>
    Vector3 VirtuoseToUnityPosition(float[] positions, int axe = 0)
    {
        //Need to check the size of the array.
        if (positions.Length >= (axe + 1) * POSITIONS_COMPONENTS)
            return new Vector3
                (
                    positions[axe * POSITIONS_COMPONENTS + 1],
                    positions[axe * POSITIONS_COMPONENTS + 2],
                   -positions[axe * POSITIONS_COMPONENTS + 0]
                );

        VRTools.LogError("[Error][VirtuoseManager] Wrong array length for the pose.");
        return Vector3.zero;
    }

    Quaternion VirtuoseToUnityRotation(float[] positions, int axe = 0)
    {
        if(positions.Length >= (axe + 1) * POSITIONS_COMPONENTS)
            return new Quaternion(positions[axe * POSITIONS_COMPONENTS + 3], positions[axe * POSITIONS_COMPONENTS + 4], positions[axe * POSITIONS_COMPONENTS + 5], positions[axe * POSITIONS_COMPONENTS + 6]);

        VRTools.LogError("[Error][VirtuoseManager] Wrong pose length .");
        return Quaternion.identity;
    }
    /// <summary>
    /// [ x y z qx qy qz qw ] 
    /// /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    float[] ConvertUnityToVirtuose(Vector3 position, Quaternion rotation)
    {
        float[] positions = { 0, 0, 0,  0, 0, 0, 0 };
        positions[0] = - position.z;
        positions[1] = position.x;
        positions[2] = position.y;

        positions[3] = rotation.x;
        positions[4] = rotation.y;
        positions[5] = rotation.z;
        positions[6] = rotation.w;

        return positions;
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
        ExecLogOnError(
            VirtuoseAPI.virtSetPowerOn, 0);
        ExecLogOnError(
            VirtuoseAPI.virtDetachVO);

        int errorCode = VirtuoseAPI.virtClose(Arm.Context);
        if (errorCode == 0)
            VRTools.Log("[VirtuoseManager] Disconnection successful with the arm " + Arm.Ip);
        
        else
            VRTools.LogError("[Error][VirtuoseManager] Disconnection error with arm " + Arm.Ip + GetError());

        //System.Runtime.InteropServices.Marshal.FreeHGlobal(arm.Context);
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
            //Debug.Break();
        }
    }

    public string GetError()
    {
        int errorCode = VirtuoseAPI.virtGetErrorCode(Arm.Context);
        return " (error " + errorCode + " : " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(VirtuoseAPI.virtGetErrorMessage(errorCode)) + ")";
    }


}