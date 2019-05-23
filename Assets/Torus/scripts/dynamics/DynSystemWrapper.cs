using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynSystemWrapper : MonoBehaviour
{
    [HideInInspector]
    public DynSystem DynSystem;
    /// <summary>
    /// List we need to subscibe too, if we want to be updated after the solving
    /// </summary>
    [HideInInspector]
    public List<DynElementWrapper> ElementsWrappers;

    private void Awake()
    {
        DynSystem = new DynSystem();
        ElementsWrappers = new List<DynElementWrapper>();
    }

    private void LateUpdate()
    {
        DynSystem.Dt=VRTools.GetDeltaTime();
        if (DynSystem.HasCollision())
        {
            DynSystem.ComputeSimuationStep();
            foreach (DynElementWrapper elementWrapper in ElementsWrappers)
                elementWrapper.AfterCollision();
        }
        else
        {
            foreach (DynElementWrapper elementWrapper in ElementsWrappers)
                elementWrapper.NoCollision();
        }

    }
}
