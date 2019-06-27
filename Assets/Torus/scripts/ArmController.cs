using System.Collections;
using System.Collections.Generic;
using MiddleVR_Unity3D;
using UnityEngine;

public class ArmController : MonoBehaviour
{
    public InputController inputController;
    public GameObject ParticulesContrainer;
    private List<CollectableController> collectableControllers;

    private vrCommand VRResetCollectables;
    private static int id;

    void Start()
    {
        ++id;

        collectableControllers = new List<CollectableController>();
        foreach (Transform child in ParticulesContrainer.transform)
            collectableControllers.Add(child.GetComponent<CollectableController>());

        VRResetCollectables = new vrCommand($"ArmController_{name}_{id}", ResetCollectables);
    }

    [VRCommand]
    private vrValue ResetCollectables(vrValue _)
    {
        foreach (CollectableController collectable in collectableControllers)
            collectable.Start();
        return null;
    }

    void Update()
    {
        if (inputController.Button(1) || inputController.Button(2))
        {
            VRResetCollectables.Do();
        }
    }
}
