using System;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Add command line parameter :
/// - SERVER=:port
/// - HOST=:port
/// - CLIENT=address:port
/// </summary>
public class SelectionArgumentLine
{
    public const string ARM_MODE = "ARM_MODE";
    public const string BASE_FRAME_POSITION = "BASE_FRAME";
    public const string USE_WAND = "WAND";

    /// <summary>
    /// use example:
    /// ARM_MODE="127.0.0.1"
    /// </summary>
    /// <returns></returns>
    public static string FromCommandeLine()
    {
        string[] arguments = System.Environment.GetCommandLineArgs();

        foreach (string argument in arguments)
        {
            if (argument.Contains(ARM_MODE))
            {
                string addr = argument.Split('=')[1];
                if (addr.Contains(USE_WAND))
                {
                    return USE_WAND;
                }
                VRTools.Log($"[ArmSelectionArgumentLine] Arm addr fetch from commande line parameters : {addr}");
                return addr;
            }
        }
        return "";
    }

    public static (bool argHere, Vector3 BaseFrame) GetBaseFramePosition()
    {
        string[] arguments = System.Environment.GetCommandLineArgs();


        foreach (string argument in arguments)
        {
            if (argument.Contains(BASE_FRAME_POSITION))
            {
                string BaseFrame = argument.Split('=')[1];
                
                VRTools.Log($"[ArmSelectionArgumentLine] Base Frame Position fetch from commande line parameters : {BaseFrame}");

                Vector3 BaseFrameV3 = Vector3.zero;
                try
                {
                    BaseFrameV3 = Utils.StringToVector3(BaseFrame);
                }
                catch{}

                return (true, BaseFrameV3);
            }
        }
        return (false, Vector3.zero);
    }
}
