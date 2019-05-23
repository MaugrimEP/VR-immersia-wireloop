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
        VectorManager.VECTOR_MANAGER.DrawVector(transform.position, dynRaquette.Element.Position - transform.position, Color.gray);
        avatarInputController.PositionOffset = dynRaquette.Element.Position - transform.position;
        avatarInputController.ForceOutput = dynRaquette.Element.Force;
        //transform.position = dynRaquette.Element.Position;
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
