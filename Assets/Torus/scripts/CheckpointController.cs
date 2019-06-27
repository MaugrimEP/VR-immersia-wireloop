using System.Collections;
using System.Collections.Generic;
using MiddleVR_Unity3D;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public InputController inputController;
    public GameObject ParticulesContrainer;
    public GameObject CirclePrefab;
    private List<CollectableController> collectableControllers;

    private vrCommand VRResetCollectables;
    private static int id;

    private void SpawnCircles()
    {
        foreach (Transform child in ParticulesContrainer.transform)
        {
            CollectableController particule = Instantiate(CirclePrefab, child.position, child.rotation).GetComponent<CollectableController>();
            collectableControllers.Add(particule.GetComponent<CollectableController>());
        }
    }

    void Start()
    {
        ++id;

        collectableControllers = new List<CollectableController>();
        SpawnCircles();

        VRResetCollectables = new vrCommand($"ArmController_{name}_{id}", ResetCollectables);
    }

    [VRCommand]
    private vrValue ResetCollectables(vrValue _)
    {
        foreach (CollectableController collectable in collectableControllers)
            collectable.Play();
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
