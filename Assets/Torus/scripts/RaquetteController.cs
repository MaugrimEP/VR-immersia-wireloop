using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaquetteController : MonoBehaviour
{
    public static int ShowMode = 4;
    public static string tagname = "Raquette";

    /// <summary>
    /// Childs that compose the raquette
    /// </summary>
    public List<Transform> raquettesChild;

    private VectorManager vectorManager;
    [HideInInspector]
    public InputController avatarInputController;
    [HideInInspector]
    public DynElementWrapper dynRaquette;
    [HideInInspector]
    public DynSystemWrapper system;

    private List<Transform> getChilds()
    {
        return raquettesChild;
    }

    private void Awake()
    {
        foreach(Transform child in getChilds()) //apply the tagname on child to filter the collisions
        {
            child.tag = tagname;
        }

        vectorManager = GameObject.Find("VectorCreator").GetComponent<VectorManager>();
        avatarInputController = GameObject.Find("Avatar").GetComponent<InputController>();
        system = GameObject.Find("DynSystemCollision").GetComponent<DynSystemWrapper>();
        dynRaquette = GetComponent<DynElementWrapper>();
    }

    public void AfterCollision()
    {
        Vector3 targedtedNextPosition = dynRaquette.Element.Position;
        Vector3 clampedNextPosition = ClampDisplacement(transform.position ,targedtedNextPosition);
        Vector3 displacementVector = clampedNextPosition - transform.position;

        VectorManager.VECTOR_MANAGER.DrawVector(transform.position, displacementVector, Color.gray);

        avatarInputController.PositionOffset = displacementVector;
        avatarInputController.ForceOutput = dynRaquette.Element.Force;
        if(avatarInputController.armSelection == InputController.ArmSelection.Unity)
        {// will update the avatar position, if we are working with the arm we update the arm position and the avatar follow the arm position
            transform.position = dynRaquette.Element.Position;
        }
    }

    private Vector3 ClampDisplacement(Vector3 previousPosition, Vector3 nextPosition)
    {
        float travelDistance = Vector3.Distance(previousPosition, nextPosition);

        float maxDisplacement = 0.02f;

        if (travelDistance > maxDisplacement)
        {// need to size down the distance
            Vector3 newDisplacementVector = nextPosition - previousPosition;
            newDisplacementVector /= travelDistance / maxDisplacement;

            nextPosition = previousPosition + newDisplacementVector;
        }

        return nextPosition;
    }

    public void NoCollision()
    {
        //avatarInputController.ResetOffsetToVirtuose();
    }

    private void addCollision(Collision collision)
    {
        system.DynSystem.Collisions.Add(new DynElementPipeCollision(1.0f, collision, dynRaquette.Element));
        system.ElementsWrappers.Add(dynRaquette);
    }

    #region Handle pipe collision interaction
    public void TouchPipe(Collision collision)
    {
        UpdateChildOnTouch();
        addCollision(collision);
    }

    public void StayPipe(Collision collision)
    {
        addCollision(collision);

    }

    public void LeavePipe(Collision collision)
    {
        UpdateChildOnLeave();
    }

    #region change the apparence of the raquette when interacting with the pipe
    private void UpdateChildOnTouch()
    {
        foreach (Transform child in getChilds())
        {
            child.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void UpdateChildOnLeave()
    {
        foreach (Transform child in getChilds())
        {
            child.GetComponent<Renderer>().material.color = Color.green;
        }
    }
    #endregion

    #endregion
}
