using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VRInput : MonoBehaviour
{
    public KeyCode Key;
    public KeyCode[] KeysModifier;

    /// <summary>
    /// < 0 value mean that button is disabled.
    /// </summary>
    public int button = -1;

    public bool IsTriggered  { get; private set; }
   
    void Update()
    {
        IsTriggered =
            IsKeyActive2() ||
            IsButtonActive();
    }


    bool IsKeyActive2()
    {
        return VRTools.GetKeyDown(Key) && 
            KeysModifier.All(key => VRTools.GetKeyPressed(key));
    }

    bool IsKeyActive()
    {
        if (VRTools.GetKeyDown(Key))
        {
            foreach (KeyCode keyModifier in KeysModifier)
                if (!VRTools.GetKeyPressed(keyModifier))
                    return false;
            return true;
        }
        else 
            return false;
    }

    bool IsButtonActive()
    {
        return button >= 0 && VRTools.IsButtonToggled((uint)button);
    }
}
