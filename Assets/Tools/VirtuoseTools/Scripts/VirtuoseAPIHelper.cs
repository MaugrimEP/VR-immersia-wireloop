using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Helper functions for Virtuose API that work with VRTools library.
/// Manage error and log when needed.
/// Documentation is copy pasta from Virtuose official documentation.
/// </summary>
public class VirtuoseAPIHelper
{

    public static VirtuoseAPIHelper me;

    #region force and torque
    /// <summary>
    /// Add a force to the VIRTUOSE.
    /// </summary>
    public (Vector3 forces, Vector3 torques) virtAddForce
    {
        set
        {
            value.forces = Utils.ClampVector3(value.forces, 30f);
            value.torques = Utils.ClampVector3(value.torques, 3.1f);

            value.forces = UnityToVirtuoseVector3(value.forces);
            value.torques = UnityToVirtuoseVector3(value.torques);

            ExecLogOnError(
                VirtuoseAPI.virtAddForce, new float[] { value.forces.x, value.forces.y, value.forces.z, value.torques.x, value.torques.y, value.torques.z });
        }
    }
    #endregion



    #region position and rotation
    /// <summary>
    /// Current value of the control position and sends it to the Virtuose controller.
    /// If an object is attached to the Virtuose (virtAttachVO called before),
    /// then the control point is the center of the object,
    /// otherwise it is the center of the Virtuose end-effector.
    /// </summary>
    public (Vector3 position, Quaternion rotation) Pose
    {
        get
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetPosition, pose);
            return VirtuoseToUnityPose(pose);
        }
        set
        {
            Vector3 castedPosition = UnityToVirtuosePosition(value.position);
            Quaternion castedRotation = UnityToVirtuoseRotation(value.rotation);

            pose = new float[]{ castedPosition.x, castedPosition.y, castedPosition.z,
                    castedRotation.x, castedRotation.y, castedRotation.z, castedRotation.w};

            ExecLogOnError(
                VirtuoseAPI.virtSetPosition, pose);
        }
    }

    #region virtuose to unity
    public static (Vector3, Quaternion) VirtuoseToUnityPose(float[] pose, int axe = 0)
    {
        return (VirtuoseToUnityPosition(pose, axe), VirtuoseToUnityRotation(pose, axe));
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
    public static Vector3 VirtuoseToUnityPosition(float[] positions, int axe = 0)
    {
        //Need to check the size of the array.
        if (positions.Length >= (axe + 1) * POSE_COMPONENTS_NUMBER)
            return new Vector3
                (
                    positions[axe * POSE_COMPONENTS_NUMBER + 1],
                    positions[axe * POSE_COMPONENTS_NUMBER + 2],
                   -positions[axe * POSE_COMPONENTS_NUMBER + 0]
                );

        VRTools.LogError("[Error][VirtuoseManager] Wrong array length for the pose: " + positions.Length + ".");
        return Vector3.zero;
    }

    //Quaternion newRotation = new Quaternion(-Rotation.y, -Rotation.z, Rotation.x, Rotation.w);
    //x:3 y:4 z:5 w:6
    public static Quaternion VirtuoseToUnityRotation(float[] pose, int axe = 0)
    {
        if (pose.Length >= (axe + 1) * POSE_COMPONENTS_NUMBER)
        {
            Quaternion read = new Quaternion(pose[axe * POSE_COMPONENTS_NUMBER + 3], pose[axe * POSE_COMPONENTS_NUMBER + 4], pose[axe * POSE_COMPONENTS_NUMBER + 5], pose[axe * POSE_COMPONENTS_NUMBER + 6]);
            Quaternion rotation = new Quaternion(-read.y, -read.z, read.x, read.w);
            return rotation;
        }


        VRTools.LogError("[Error][VirtuoseManager] Wrong pose length: " + pose.Length + ".");
        return Quaternion.identity;
    }
    #endregion

    #region unity to virtuose

    public static Vector3 UnityToVirtuosePosition(Vector3 position)
    {
        return new Vector3(-position.z, position.x, position.y);
    }


    public static Quaternion UnityToVirtuoseRotation(Quaternion rotation)
    {
        Quaternion virtRot = new Quaternion(rotation.z, -rotation.x, -rotation.y, rotation.w);
        return virtRot.normalized;
    }

    /// <summary>
    /// Cast a Vector3 form unity axis system, to Virtuose axis system
    /// work for Force, Torque,
    /// Position ? Rotation ?
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Vector3 UnityToVirtuoseVector3Rotation(Vector3 value)
    {
        float[] poseMax = new float[6] { 0, 0, 0, 0, 0, 0 };
        ExecLogOnError(VirtuoseAPI.virtGetArticularPosition, poseMax);
        Vector3 vect = new Vector3(-value[2] * Mathf.Cos(poseMax[0]) + value[0] * Mathf.Sin(poseMax[0]), value[0] * Mathf.Cos(poseMax[0]) + value[2] * Mathf.Sin(poseMax[0]), value[1]);
        return vect;
    }

    /// <summary>
    /// Cast a Vector3 form unity axis system, to Virtuose axis system
    /// work for Force, Torque,
    /// Position ? Rotation ?
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Vector3 UnityToVirtuoseVector3(Vector3 value)
    {
        float[] poseMax = new float[6] { 0, 0, 0, 0, 0, 0 };
        ExecLogOnError(VirtuoseAPI.virtGetArticularPosition, poseMax);
        Vector3 vect = new Vector3(-value[2] * Mathf.Cos(poseMax[0]) + value[0] * Mathf.Sin(poseMax[0]), value[0] * Mathf.Cos(poseMax[0]) + value[2] * Mathf.Sin(poseMax[0]), value[1]);

        return vect;
    }

    #endregion

    #endregion

    /// <summary>
    /// Vector3(x, y, z) + Quaternion(x, y, z, w)
    /// </summary>
    const int POSE_COMPONENTS_NUMBER = 7;

    /// <summary>
    /// At the moment for Virtuose 6HF.
    /// </summary>
    const int AXES_NUMBER = 6;

    bool[] buttonsPressed = new bool[4];
    bool[] buttonsToggled = new bool[4];

    float[] pose = new float[POSE_COMPONENTS_NUMBER];
    float[] force = new float[AXES_NUMBER];

    VirtuoseArm arm;


    /// <summary>
    /// Vector3(x, y, z) + Quaternion(x, y, z, w)
    /// </summary>
    public const int POSITIONS_COMPONENTS = 7;

    /// <summary>
    /// In newton.
    /// </summary>
    public const float MAX_FORCE = 5;

    /// <summary>
    /// In kg.
    /// </summary>
    public const float MIN_MASS = 0.001f;
    public const float MAX_MASS = 1;

    /// <summary>
    /// In Kg / m
    /// </summary>
    public const float MIN_INERTIE = 0;
    public const float MAX_INERTIE = 1;

    /// <summary>
    /// In meters.
    /// </summary>
    public const float MAX_DISTANCE_PER_FRAME = 0.3f;

    public const float MAX_DOT_DIFFERENCE = 0.05f;

    /// <summary>
    /// Y       
    /// |   Z   
    /// | /
    /// |/___X
    /// </summary>
    enum ArticularScaleOne
    {
        Z = 0,
        Neg_X,
        Y,
        Base,
        Turret,
        Arm,
        ForeArm_Pitch,
        ForeArm_Roll,
        Handle_Pitch,
        Handle_Roll
    }

    enum ArticularHF6
    {
        Base,
        Turret,
        Arm,
        ForeArm_Pitch,
        ForeArm_Roll,
    }

    public enum DeviceType
    {
        DEVICE_VIRTUOSE_3D = 1,
        DEVICE_VIRTUOSE_3D_DESKTOP = 2,
        DEVICE_VIRTUOSE_6D = 3,
        DEVICE_VIRTUOSE_6D_DESKTOP = 4,
        DEVICE_VIRTUOSE_7D = 5,
        DEVICE_MAT6D = 6,
        DEVICE_MAT7D = 7,
        DEVICE_INCA_6D = 8,
        DEVICE_INCA_3D = 9,
        DEVICE_ORTHESE = 10,
        DEVICE_SCALE1 = 11,
        DEVICE_1AXE = 12,
        DEVICE_OTHER = 13
    }



    /// <param name="arm"></param>
    public VirtuoseAPIHelper(VirtuoseArm arm)
    {
        me = this;
        this.arm = arm;
    }

    /// <summary>
    ///  Virtuose library version.
    ///  Major and minor index of the software version.  
    /// </summary>
    public (int major, int minor) APIVersion
    {
        get
        {
            int major = 0, minor = 0;
            ExecLogOnError(
                VirtuoseAPI.virtAPIVersion, ref major, ref minor);

            return (major, minor);
        }
    }

    /// <summary>
    /// Version of the embedded software.
    /// Major and minor index of the controller.
    /// </summary>
    public (int major, int minor) ControllerVersion
    {
        get
        {
            int major = 0, minor = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetControlerVersion, ref major, ref minor);

            return (major, minor);
        }
    }


    /// <summary>
    /// Open a connection to the controller of the Virtuose.
    /// The host parameter corresponds to the URL (Uniform Ressource Locator) of the Virtuose controller to connect to.
    /// In the current version of the API, only one type of communication protocol is available, therefore the URL is always in the form: 
    /// "udpxdr://identification:port_number+interface" 
    /// udpxdr is the only protocol available in the current version of the API.
    /// identification should be replaced by the host name of the Virtuose controller if it can be resolved by a DNS,
    /// or else by its IP address in dotted form (e.g. "192.168.0.1").
    /// port_number should be replaced by the port number to be used by the API to connect to the Virtuose controller.
    /// The default value is 0, and in that case the API looks for a free port number starting from 3131.
    /// interface designates the physical interface to be used by the API(ignored in the case of udpxdr). 
    /// In case only identification is given, the URL is completed as follows: 
    /// ... "udpxdr://identification:0" 
    /// Note: the automatic completion is limited to udpxdr only.The initial prefix "url:" defined in the URL standard is supported but ignored.
    /// </summary>
    /// <param name="ip">ip#port (127.0.0.1#5125).</param>
    public bool Open(string ip)
    {
        arm.Ip = ip;
        arm.Context = VirtuoseAPI.virtOpen(arm.Ip);
        if (arm.Context.ToInt32() == 0)
            VRTools.LogError("[Error][VirtuoseAPIHelper] Connection error with the arm " + arm.Ip + ErrorMessage);

        else
        {
            VRTools.Log("[VirtuoseAPIHelper] Connection successful with the arm " + arm.Ip);
            arm.IsConnected = true;
        }
        return arm.IsConnected;
    }

    /// <summary>
    ///  Closing of connection to the controller of the Virtuose.
    /// </summary>
    /// <returns></returns>
    public bool Close()
    {
        int errorCode = VirtuoseAPI.virtClose(arm.Context);
        if (errorCode == 0)
            VRTools.Log("[VirtuoseAPIHelper] Disconnection successful with the arm " + arm.Ip);
        else
            VRTools.LogError("[Error][VirtuoseAPIHelper] Disconnection error with arm " + arm.Ip + ErrorMessage);

        return errorCode == 0;
    }


    /// <summary>
    /// Initialize arm with default value.
    /// </summary>
    public void InitDefault()
    {
        IndexingMode = VirtuoseAPI.VirtIndexingType.INDEXING_ALL_FORCE_FEEDBACK_INHIBITION;
        ForceFactor = 1;
        SpeedFactor = 1;
        Timestep = 0.010f;
        BaseFrame = (Vector3.zero, Quaternion.identity);
        ObservationFrame = (Vector3.zero, Quaternion.identity);
        ObservationFrameSpeed = (Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Control mode of the Virtuose device.
    /// COMMAND_TYPE_NONE No possible movement,
    /// COMMAND_TYPE_IMPEDANCE Force/position control,
    /// COMMAND_TYPE_VIRTMECH Position/force control with virtual mechanism.
    /// </summary>
    public VirtuoseAPI.VirtCommandType CommandType
    {
        get
        {
            int commandType = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetCommandType, ref commandType);

            return (VirtuoseAPI.VirtCommandType)commandType;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetCommandType, (ushort)value);
        }
    }

    public VirtuoseAPI.VirtGripperCommandType GripperCommand
    {
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetGripperCommandType, (ushort)value);
        }
    }

    /// <summary>
    /// Mode of indexing (also called offset). 
    /// INDEXING_ALL Indexing is active for both translation and rotation movements, whenever the offset button is pushed or the power is off (power button or deadman sensor off). 
    /// INDEXING_TRANS Indexing is active only on the translation movements.When power is turned on, the device is constrained along a line segment going back to the orientation it had before switching off.
    /// INDEXING_NONE Indexing is inactive.When power is turned on, the device is constrained along a line segment going back to the position it had before switching off.
    /// Other values are implemented, which correspond to the same modes but forcefeedback is inhibited during indexing.
    /// </summary>
    public VirtuoseAPI.VirtIndexingType IndexingMode
    {
        get
        {
            ushort indexingMode = 0;
            ExecLogOnError(
                 VirtuoseAPI.virtGetIndexingMode, ref indexingMode);
            return IndexingMode;
        }
        set
        {
            ExecLogOnError(
                 VirtuoseAPI.virtSetIndexingMode, (ushort)value);
        }
    }

    /// <summary>
    /// Device type stored in its embedded variator card.
    /// </summary>
    public DeviceType DeviceID
    {
        get
        {
            int deviceId = 0;
            int serialNumber = 0;
            ExecLogOnError(
                 VirtuoseAPI.virtGetDeviceID, ref deviceId, ref serialNumber);
            return (DeviceType)deviceId;
        }
    }

    /// <summary>
    /// Serial number stored in its embedded variator card.
    /// </summary>
    public int SerialNumber
    {
        get
        {
            int deviceId = 0;
            int serialNumber = 0;
            ExecLogOnError(
                 VirtuoseAPI.virtGetDeviceID, ref deviceId, ref serialNumber);
            return serialNumber;
        }
    }

    /// <summary>
    /// State of the motor power supply
    /// </summary>
    public bool Power
    {
        get
        {
            int power = 0;
            ExecLogOnError(
                 VirtuoseAPI.virtGetPowerOn, ref power);
            return power == 1;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetPowerOn, value ? 1 : 0);
        }
    }

    /// <summary>
    /// State of the safety sensor.
    /// A value of 1 means that the safety sensor is active (user present), a value of 0 means that the sensor is inactive (user absent). 
    /// </summary>
    public bool DeadMan
    {
        get
        {
            int deadMan = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetDeadMan, ref deadMan);

            return deadMan == 1;
        }
    }

    /// <summary>
    /// State of the emergency stop.
    /// A value of 1 means that the chain is closed (the system is operational), a value of that it is open (the system is stopped). 
    /// </summary>
    public bool EmergencyStop
    {
        get
        {
            int emergencyStop = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetEmergencyStop, ref emergencyStop);

            return emergencyStop == 1;
        }
    }

    /// <summary>
    /// Encoder failure code.
    /// In case of success, the virtGetFailure function returns 0. 
    /// Otherwise, it returns 1 and the virtGetErrorCode function gives access to an error code. 
    /// </summary>
    public uint Failure
    {
        get
        {
            uint failure = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetFailure, ref failure);

            return failure;
        }
    }

    /// <summary>
    /// Current alarm status.
    /// VIRT_ALARM_OVERHEAT means that one motor is overheated. Forcefeedback is automatically reduced, until the motor has cooled down to an acceptable temperature.
    /// VIRT_ALARM_SATURATE means that the motor currents have reached their maximum and are saturated.
    /// VIRT_ALARM_CALLBACK_OVERRUN means that the execution time of the callback function defined with virtSetPeriodicFunction is greater than the timestep value. 
    /// In that case, the real-time execution of the simulation cannot be guaranteed. 
    /// </summary>
    public uint Alarm
    {
        get
        {
            uint alarm = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetAlarm, ref alarm);

            return alarm;
        }
    }

    /// <summary>
    /// Status of indexing.
    /// </summary>
    public bool IsInShiftPosition
    {
        get
        {
            //A value of 1 if the offset push-button is pressed or the power if off - a value of 0 otherwise. 
            int indexing = 0;
            ExecLogOnError(
                VirtuoseAPI.virtIsInShiftPosition, ref indexing);

            return indexing == 1;
        }
    }

    /// <summary>
    /// Force scale factor which corresponds to a scaling between the forces exerted at the tip of the VIRTUOSE and those computed in the simulation.
    /// A value smaller than 1 corresponds to an amplification of the forces from the Virtuose towards the simulation.
    /// The function must be called before the selection of the control mode. 
    /// </summary>
    public float ForceFactor
    {
        get
        {
            float forceFactor = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetForceFactor, ref forceFactor);

            return forceFactor;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetForceFactor, value);
        }
    }

    /// <summary>
    /// Movement scale factor which corresponds to a scaling of the workspace of the haptic device. 
    ///  A value larger than 1.0 means that the movements of the Virtuose are amplified inside the simulation. 
    /// </summary>
    public float SpeedFactor
    {
        get
        {
            float speedFactor = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetSpeedFactor, ref speedFactor);

            return speedFactor;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetSpeedFactor, value);
        }
    }

    /// <summary>
    /// Modify the current control speed.
    /// Function modifies the current value of the control speed. If an object is attached to the VIRTUOSE(virtAttachVO called before),
    /// then the control point is the center of the object, otherwise it is the center of the virtuose endeffector.
    /// Parameter corresponds to the speed with respect to the control point expressed in the coordinates of the environment reference frame.
    /// </summary>
    public float[] Speed
    {
        get
        {
            float[] speed = new float[6] { 0, 0, 0, 0, 0, 0 };
            ExecLogOnError(
                VirtuoseAPI.virtGetSpeed, speed);
            return speed;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetSpeed, value);
        }
    }

    /// <summary>
    /// Simulation timestep.
    /// Virtuose controller of the simulation timestep. This value is used in order to guarantee the stability of the system.
    /// The function must be called before the selection of the type of control mode.
    /// Expressed in seconds. 
    /// </summary>
    public float Timestep
    {
        get
        {
            float timestep = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetTimeStep, ref timestep);

            return timestep;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetTimeStep, value);
        }
    }


    /// <summary>
    /// Timeout value used in communications with the Virtuose controller. 
    /// Expressed in seconds.
    /// </summary>
    public float Timeout
    {
        get
        {
            float timeout = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetTimeoutValue, ref timeout);
            return timeout;
        }
        set
        {
            ExecLogOnError(
                VirtuoseAPI.virtSetTimeoutValue, value);
        }
    }

    /// <summary>
    /// function disables (or enables) the watchdog control on the communication with the Virtuose controller. 
    /// The role of the watchdog control is to stop force-feedback in case of a software failure on the simulation side. 
    /// In practice, if the simulation does not update the control values during a time period of 2 seconds, 
    /// the Virtuose considers that the simulation is dead and cuts off the forcefeedback and the connection to the API. 
    /// For debugging purposes, it can be useful to disable the watchdog control, so that the simulation can be executed step-by-step without loosing the connection with the Virtuose.
    /// In that case, it is essential to call the virtClose function at the end of the simulation, 
    /// otherwise the haptic device will not be reset, and it could be impossible to establish a new connection.
    /// </summary>
    /// <param name="connexionState">state of the wanted watchdog control.</param>
    public void ControlConnexion(bool connexionState)
    {
        //The disable parameter should be set to 1 to disable the watchdog control, and to 0 to re-enable it. 
        int disable = connexionState ? 0 : 1;
        ExecLogOnError(
            VirtuoseAPI.virtDisableControlConnexion, disable);
    }

    /// <summary>
    /// Number of axis of the Virtuose that can be controlled.
    /// </summary>
    public int AxesNumber
    {
        get
        {
            int nbAxes = 0;
            ExecLogOnError(
                VirtuoseAPI.virtGetNbAxes, ref nbAxes);
            return nbAxes;
        }
    }

    /// <summary>
    /// 20/21 Ref.haptic devices - installation_manual_en_rev4.docx
    /// ID Definition State
    ///00 system OK Normal
    ///IP Reset factory IP setting(push on the reset
    ///button when the haptic device boot) Normal
    ///11 error DSP axe 1 System failed
    ///12 error DSP axe 2 System failed
    ///13 error DSP axe 3 System failed
    ///14 error DSP axe 4 System failed
    ///15 error DSP axe 5 System failed
    ///16 error DSP axe 6 System failed
    ///21 error coder axe 1 System failed
    ///22 error coder axe 2 System failed
    ///23 error coder axe 3 System failed
    ///24 error coder axe 4 System failed
    ///25 error coder axe 5 System failed
    ///26 error coder axe 6 System failed
    ///31 error read signal axe 1 System failed
    ///32 error read signal axe 2 System failed
    ///33 error read signal axe 3 System failed
    ///34 error read signal axe 4 System failed
    ///35 error read signal axe 5 System failed
    ///36 error read signal axe 6 System failed
    ///41 alarm T° PWM axe 1 Non-Fatal
    ///42 alarm T° PWM axe 2 Non-Fatal
    ///43 alarm T° PWM axe 3 Non-Fatal
    ///44 alarm T° PWM axe 4 Non-Fatal
    ///45 alarm T° PWM axe 5 Non-Fatal
    ///46 alarm T° PWM axe 6 Non-Fatal
    ///51 error PWM axe 1 System failed
    ///52 error PWM axe 2 System failed
    ///53 error PWM axe 3 System failed
    ///54 error PWM axe 4 System failed
    ///55 error PWM axe 5 System failed
    ///56 error PWM axe 6 System failed
    ///70 error relay System failed
    ///71 watchdog tram reception Non-Fatal
    /// </summary>
    public int ErrorCode
    {
        get
        {
            return VirtuoseAPI.virtGetErrorCode(arm.Context);
        }
    }

    public string ErrorMessage
    {
        get
        {
            int errorCode = VirtuoseAPI.virtGetErrorCode(arm.Context);
            return " (error " + errorCode + " : " + System.Runtime.InteropServices.Marshal.PtrToStringAnsi(VirtuoseAPI.virtGetErrorMessage(errorCode)) + ")";
        }
    }


    /// <summary>
    /// Test whether the mechanical limits of the device workspace have been reached.
    /// The bounds parameter is set by the function, as a field of bit with the following meaning:
    /// VIRT_BOUND_LEFT_AXE_1 corresponds to the left bound of axis 1, 
    /// VIRT_BOUND_RIGHT_AXE_1 corresponds to the right bound of axis 1,
    /// VIRT_BOUND_SUP_AXE_2 corresponds to the upper bound of axis 2,
    /// VIRT_BOUND_INF_AXE_2 corresponds to the lower bound of axis 2,
    /// VIRT_BOUND_SUP_AXE_3 corresponds to the upper bound of axis 3,
    /// VIRT_BOUND_INF_AXE_3 corresponds to the lower bound of axis 3,
    /// VIRT_BOUND_LEFT_AXE_4 corresponds to the left bound of axis 4,
    /// VIRT_BOUND_RIGHT_AXE_4 corresponds to the right bound of axis 4,
    /// VIRT_BOUND_SUP_AXE_5 corresponds to the upper bound of axis 5,
    /// VIRT_BOUND_INF_AXE_5 corresponds to the lower bound of axis 5,
    /// VIRT_BOUND_LEFT_AXE_6 corresponds to the left bound of axis 6,
    /// VIRT_BOUND_RIGHT_AXE_6 corresponds to the right bound of axis 6. 
    /// </summary>
    public uint IsInBound
    {
        get
        {
            uint bounds = 0;
            ExecLogOnError(
                VirtuoseAPI.virtIsInBounds, ref bounds);
            return bounds;
        }
    }

    /// <summary>
    /// Time elapsed since the last update of the Virtuose state vector.
    /// Value in CPU ticks of the time elapsed since the last update of the state vector received from the Virtuose controller. 
    /// </summary>
    public float TimeLastUpdate
    {
        get
        {
            uint time = 0;
            ExecLogOnError(
               VirtuoseAPI.virtGetTimeLastUpdate, ref time);
            return time;
        }
    }

    /// <summary>
    /// Force tensor to be applied to the object attached to the Virtuose, allowing the dynamic simulation of the scene.
    /// It corresponds to the force applied by the user to the Virtuose, in the form of a 6 component force tensor.
    /// This force is expressed with respect to the center of the object, in the coordinates of the environment reference frame
    /// </summary>
    public float[] Force
    {
        get
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetForce, force);
            return force;
        }
        set
        {
            Assert.AreEqual(value.Length, 6);

            for (int f = 0; f < value.Length; f++)
            {
                if (Mathf.Abs(value[f]) > MAX_FORCE)
                    VRTools.LogError("[Error][VirtuoseAPIHelper] Force clamped because outside of limit |" + value[f] + "| > " + MAX_FORCE);

                value[f] = Mathf.Clamp(value[f], -MAX_FORCE, MAX_FORCE);
            }

            ExecLogOnError(
                VirtuoseAPI.virtSetForce, value);
        }
    }



    /// <summary>
    /// Current value of the control position and sends it to the Virtuose controller.
    /// If an object is attached to the Virtuose (virtAttachVO called before),
    /// then the control point is the center of the object,
    /// otherwise it is the center of the Virtuose end-effector.
    /// </summary>
    public (Vector3 position, Quaternion rotation) RawPose
    {
        get
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetPosition, pose);
            return (new Vector3(pose[0], pose[1], pose[2]), new Quaternion(pose[3], pose[4], pose[5], pose[6]));
        }
        set
        {
            pose[0] = value.position.x;
            pose[1] = value.position.y;
            pose[2] = value.position.z;
            pose[3] = value.rotation.x;
            pose[4] = value.rotation.y;
            pose[5] = value.rotation.z;
            pose[6] = value.rotation.w;
            ExecLogOnError(
                VirtuoseAPI.virtSetPosition, pose);
        }
    }


    /// <summary>
    /// Indexed position of the end-effector.
    /// </summary>
    public (Vector3 position, Quaternion rotation) AvatarPose
    {
        get
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetAvatarPosition, pose);
            return VirtuoseToUnityPose(pose);
        }
    }

    /// <summary>
    /// Physical position of the Virtuose with respect to its base.
    /// </summary>
    public (Vector3 position, Quaternion rotation) PhysicalPose
    {
        get
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetPhysicalPosition, pose);
            return (VirtuosePhysicalToUnityPosition(pose), VirtuoseToUnityRotation(pose));
        }
    }

    /// <summary>
    /// Return articular value for each axe.
    /// For scale1 first three value are the position of the carrier : 0 -> Z, 1 -> -X, 2 -> Fixed Y value in meter.
    /// Then : For each axes the current angles in degree.
    /// </summary>
    public float[] Articulars
    {
        get
        {
            float[] articularValues = new float[SafeAxeNumber];
            ExecLogOnError(
                VirtuoseAPI.virtGetArticularPosition, articularValues);

            //Scale1 has translation on first 3 axes.
            int startRotationIndex = IsScaleOne ? 3 : 0;
            for (int a = 3; a < articularValues.Length; a++)
                articularValues[a] = Mathf.Rad2Deg * articularValues[a];
            return articularValues;
        }
        set
        {
            Assert.AreEqual(AxesNumber, value.Length, "Array length must have the same size as axes number.");

            for (int a = 3; a < value.Length; a++)
                value[a] = Mathf.Deg2Rad * value[a];

            ExecLogOnError(
                VirtuoseAPI.virtSetArticularPosition, value);
        }
    }
    public Vector3 ArticularsPosition
    {
        get
        {
            return VirtuosePhysicalToUnityPosition(Articulars);
        }
    }

    /// <summary>
    /// Corresponds to the articular speed. It is an array of float, with length the number of axes of the Virtuose.
    /// </summary>
    public float[] ArticularsSpeed
    {
        get
        {
            float[] articularsSpeed = new float[SafeAxeNumber];
            ExecLogOnError(
                VirtuoseAPI.virtGetArticularSpeed, articularsSpeed);

            return articularsSpeed;
        }
        set
        {
            Assert.AreEqual(AxesNumber, value.Length, "Array length must have the same size as axes number.");
            ExecLogOnError(
                VirtuoseAPI.virtSetArticularSpeed, value);
        }
    }

    public static Vector3 VirtuosePhysicalToUnityPosition(float[] position)
    {
        //Need to check the size of the array.
        if (position.Length >= 3)
            return new Vector3
                (
                  -position[1],
                    position[2],
                    position[0]
                );

        VRTools.LogError("[Error][VirtuoseManager] Wrong array length for the articulars values: " + position.Length + ".");
        return Vector3.zero;
    }

    /// <summary>
    /// Get base position which match power button.
    /// </summary>
    /// <param name="offset">Offset to match absolute tracking position.</param>
    /// <returns></returns>
    public (Vector3 position, Quaternion rotation) PhysicalBasePose
    {
        get
        {
            Vector3 position = ArticularsPosition;
            Quaternion rotation = Quaternion.AngleAxis(-Articulars[(int)ArticularScaleOne.Base], Vector3.up);
            return (position, rotation);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset">Offset to match absolute tracking position.</param>
    /// <param name="distance">Distance from the base. This distance should be read from the virtuose configuration file.</param>
    /// <returns></returns>
    public Vector3 BubblePosition(float distance = 0.60f)
    {
        (Vector3 position, Quaternion rotation) = PhysicalBasePose;
        return position + rotation * Vector3.forward * distance;
    }

    /// <summary>
    /// Check if device is a Scale1.
    /// </summary>
    public bool IsScaleOne
    {
        get
        {
            return DeviceID == VirtuoseAPIHelper.DeviceType.DEVICE_SCALE1;
        }
    }

    /// <summary>
    /// Use this if you don't want to check if the AxeNumber is 0 because there is no connexion or other fancy fantasy.
    /// </summary>
    public int SafeAxeNumber
    {
        get
        {
            int axesNumber = AxesNumber;
            //Some haption function may crash if 0 size array is given as input.
            return axesNumber <= 0 ? POSE_COMPONENTS_NUMBER : axesNumber;
        }
    }

    /// <summary>
    /// Get base position which match power button.
    /// </summary>
    /// <param name="offset">Offset to match absolute tracking position.</param>
    /// <returns></returns>
    public (Vector3 position, Quaternion rotation) ComputeBasePose(Vector3 offset = new Vector3())
    {
        float[] articulars = Articulars;
        Vector3 position = VirtuoseAPIHelper.VirtuoseToUnityPosition(articulars);
        position.x = -position.x;
        position.z = -position.z;
        position += offset;
        Quaternion rotation = Quaternion.AngleAxis(-articulars[(int)ArticularScaleOne.Base], Vector3.up);
        return (position, rotation);
    }


    /// <summary>
    /// Get base position which match power button.
    /// </summary>
    /// <param name="offset">Offset to match absolute tracking position.</param>
    /// <returns></returns>
    public (Vector3 position, Quaternion rotation) ComputePhysicalPose(Vector3 offset = new Vector3())
    {
        (Vector3 position, Quaternion rotation) = PhysicalPose;
        position.x = -position.x;
        position.z = -position.z;
        position += offset;
        return (position, rotation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset">Offset to match absolute tracking position.</param>
    /// <param name="distance">Distance from the base. This distance should be read from the virtuose configuration file.</param>
    /// <returns></returns>
    public Vector3 ComputeBubblePosition(Vector3 offset = new Vector3(), float distance = 0.60f)
    {
        (Vector3 position, Quaternion rotation) = ComputeBasePose(offset);
        return position + rotation * Vector3.forward * distance;
    }


    /// <summary>
    /// Current position and orientation of the base reference frame.
    /// </summary>
    public (Vector3 position, Quaternion rotation) BaseFrame
    {
        get
        {
            ExecLogOnError(
               VirtuoseAPI.virtGetBaseFrame, pose);
            return VirtuoseToUnityPose(pose);
        }
        set
        {
            pose = ConvertUnityToVirtuose(value.Item1, value.Item2);
            ExecLogOnError(
               VirtuoseAPI.virtSetBaseFrame, pose);
        }
    }

    /// <summary>
    /// Observation frame with respect to the reference of environment.
    /// </summary>
    public (Vector3 position, Quaternion rotation) ObservationFrame
    {
        get
        {
            ExecLogOnError(
               VirtuoseAPI.virtGetObservationFrame, pose);
            return VirtuoseToUnityPose(pose);
        }
        set
        {
            pose = ConvertUnityToVirtuose(value.Item1, value.Item2);
            ExecLogOnError(
               VirtuoseAPI.virtSetObservationFrame, pose);
        }
    }
    public float[] ConvertUnityToVirtuose(Vector3 position, Quaternion rotation)
    {
        Vector3 virtuosePos = UnityToVirtuosePosition(position);
        Quaternion virtuoseRot = UnityToVirtuoseRotation(rotation);
        float[] positions = { 0, 0, 0, 0, 0, 0, 0 };

        positions[0] = virtuosePos.x;
        positions[1] = virtuosePos.y;
        positions[2] = virtuosePos.z;

        positions[3] = virtuoseRot.x;
        positions[4] = virtuoseRot.y;
        positions[5] = virtuoseRot.z;
        positions[6] = virtuoseRot.w;

        return positions;
    }

    /// <summary>
    /// Speed of the observation reference frame. 
    /// Speed of motion of the observation reference frame with respect to the environment reference frame. 
    /// </summary>
    public (Vector3 position, Quaternion rotation) ObservationFrameSpeed
    {
        set
        {
            pose = ConvertUnityToVirtuose(value.Item1, value.Item2);
            ExecLogOnError(
               VirtuoseAPI.virtSetObservationFrameSpeed, pose);
        }
    }

    public void AttachVO(float mass, float[] inerties)
    {
        float inertie = MAX_INERTIE;
        {
            for (int i = 0; i < inerties.Length; ++i)
                inertie = Mathf.Min(inertie, inerties[i]);
        }

        Pose = (Vector3.zero, Quaternion.identity);

        Speed = new float[] { 0, 0, 0, 0, 0, 0 };

        if (mass > MAX_MASS)
            VRTools.LogWarning("[Warning][VirtuoseManager] Mass is aboved authorized threshold (" + mass + ">" + MAX_MASS + ")");
        else if (mass < 0)
            VRTools.LogWarning("[Warning][VirtuoseManager] Mass must be > 0.");

        mass = Mathf.Clamp(mass, MIN_MASS, MAX_MASS); //Use 1g as minimum, completely arbitrary.

        if (inertie > MAX_INERTIE)
            VRTools.LogWarning("[Warning][VirtuoseManager] Inertie is above authorized threshold (" + inertie + ">" + MAX_INERTIE + ")");
        else if (inertie < 0)
            VRTools.LogWarning("[Warning][VirtuoseManager] Inertie must be >= 0.");

        inertie = Mathf.Clamp(inertie, MIN_INERTIE, MAX_INERTIE);

        ExecLogOnError(
            VirtuoseAPI.virtAttachVO, mass, inerties);
    }

    public void DetachVO()
    {
        ExecLogOnError(
            VirtuoseAPI.virtDetachVO);
    }

    public Vector2 Scale1CarrierPosition
    {
        get
        {
            float[] articulars = Articulars;
            return new Vector2(-articulars[1], articulars[0]);
        }
        set
        {
            if (Power)
            {
                float[] articulars = Articulars;
                articulars[0] = value[1];
                articulars[1] = -value[0];
                Articulars = articulars;
            }
            else
            {
                //Refresh position if no power.
                Articulars = Articulars;
            }
        }
    }

    public void UpdateArm()
    {
        int buttonState = 0;
        for (int b = 0; b < 3; b++)
        {
            ExecLogOnError(
                VirtuoseAPI.virtGetButton, b, ref buttonState);

            buttonsToggled[b] = buttonsPressed[b] != GetButtonState(buttonState);
            buttonsPressed[b] = GetButtonState(buttonState);
            if (buttonsToggled[b])
                VRTools.Log("Toggled " + b);
        }
    }

    public bool Button(int button)
    {
        int state = 0;
        ExecLogOnError(
            VirtuoseAPI.virtGetButton, button, ref state);
        return state == 1;
    }

    public bool IsButtonPressed(int button = 2)
    {
        return buttonsPressed[button];
    }

    public bool IsButtonToggled(int button = 2)
    {
        return buttonsToggled[button];
    }

    public Vector2 Joystick(float[] referencearticulars, bool clamped = false)
    {
        float[] articulars = Articulars;
        float x = (referencearticulars[(int)ArticularScaleOne.Handle_Roll] - articulars[(int)ArticularScaleOne.Handle_Roll]) / 80;
        float y = (referencearticulars[(int)ArticularScaleOne.Handle_Pitch] - articulars[(int)ArticularScaleOne.Handle_Pitch]) / -70;
        if (clamped)
        {
            x = Mathf.Clamp(x, -1, 1);
            y = Mathf.Clamp(y, -1, 1);
        }
        return new Vector2(x, y);
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
    public void ExecLogOnError(virtDelegate virtMethod, IntPtr context)
    {
        int errorCode = virtMethod(context);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError(virtDelegate virtMethod)
    {
        int errorCode = virtMethod(arm.Context);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateGen<T> virtMethod, T value)
    {
        int errorCode = virtMethod(arm.Context, value);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateGen<T> virtMethod, IntPtr context, T value)
    {
        int errorCode = virtMethod(context, value);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T>(virtDelegateRefGen<T> virtMethod, ref T value)
    {
        int errorCode = virtMethod(arm.Context, ref value);
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
        int errorCode = virtMethod(arm.Context, value1, value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateRefGenRefGen<T, U> virtMethod, ref T value1, ref U value2)
    {
        int errorCode = virtMethod(ref value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateGenRefGen<T, U> virtMethod, T value1, ref U value2)
    {
        int errorCode = virtMethod(arm.Context, value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateGenRefGen<T, U> virtMethod, IntPtr context, T value1, ref U value2)
    {
        int errorCode = virtMethod(context, value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateContextRefGenRefGen<T, U> virtMethod, ref T value1, ref U value2)
    {
        int errorCode = virtMethod(arm.Context, ref value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void ExecLogOnError<T, U>(virtDelegateContextRefGenRefGen<T, U> virtMethod, IntPtr context, ref T value1, ref U value2)
    {
        int errorCode = virtMethod(context, ref value1, ref value2);
        LogError(errorCode, virtMethod.Method.Name);
    }

    public void LogError(int errorCode, string methodName)
    {
        if (errorCode == -1)
        {
            VRTools.LogError("[Error][VirtuoseManager] " + methodName + " error " + errorCode + "(" + ErrorMessage + ")");
            arm.HasError = true;
        }
    }
}

static class Extension
{
    public static string ToFlattenString(this Array array)
    {
        string flattenString = "[" + array.Length + "](";
        foreach (System.Object o in array)
            flattenString += o.ToString() + ", ";
        return flattenString;
    }

    public static void LogErrorIfNull(this Component component)
    {
        if (!component)
            VRTools.LogError("[Error] Couldn't find component " + component + ".");
    }

    public static T GetComponentLogIfNull<T>(this Behaviour behaviour) where T : Component
    {
        T component = behaviour.GetComponent<T>();
        if (!component)
            VRTools.LogError("[Error] Couldn't find component " + typeof(T) + " in " + behaviour.name + " gameObject.");
        return component;
    }

    public static T GetComponentInChildrenLogIfNull<T>(this Behaviour behaviour) where T : Component
    {
        T component = behaviour.GetComponentInChildren<T>();
        if (!component)
            VRTools.LogError("[Error] Couldn't find component " + typeof(T) + " in " + behaviour.name + " children gameObject.");
        return component;
    }

    public static T GetComponentInParentLogIfNull<T>(this Behaviour behaviour) where T : Component
    {
        T component = behaviour.GetComponentInParent<T>();
        if (!component)
            VRTools.LogError("[Error] Couldn't find component " + typeof(T) + " in " + behaviour.name + " parent gameObject.");
        return component;
    }

    public static void SetPose(this Transform transform, (Vector3 position, Quaternion rotation) pose)
    {
        transform.position = pose.position;
        transform.rotation = pose.rotation;
    }

    public static void LocalPose(this Transform transform, (Vector3 position, Quaternion rotation) pose)
    {
        transform.localPosition = pose.position;
        transform.localRotation = pose.rotation;
    }
}
