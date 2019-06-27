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
    private const string ARM_MODE = "ARM_MODE";
    private const string USE_WAND = "WAND";

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
                VRTools.Log($"[ArmSelectionArgumentLine] Arm addr fetch from commande line parameters : {addr}");
                return addr;
            }
        }
        return "";
    }

    public static (bool arg,bool useWand) UseWand()
    {
        string[] arguments = System.Environment.GetCommandLineArgs();

        foreach (string argument in arguments)
        {
            if (argument.Contains(USE_WAND))
            {
                string useWand = argument.Split('=')[1];
                VRTools.Log($"[ArmSelectionArgumentLine] UseWand fetch from commande line parameters : {useWand}");
                return (true,useWand.Equals("1"));
            }
        }
        return (false, false);
    }
}
