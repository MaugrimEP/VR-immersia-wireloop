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
}
