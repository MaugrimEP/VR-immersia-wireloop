using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToggleGameObjects : MonoBehaviour 
{
    public GameObject[] elementsToHide;
    bool[] elementsToHideDefaultVisibility;

    public KeyCode ToggleGameObjectsKey;
    public int ToggleGameObjectsButton = -1;

    void Reset()
    {
        if(elementsToHide.Length == 0)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (Transform t in gameObject.transform)
                if (t.gameObject.activeSelf)
                    gameObjects.Add(t.gameObject);
            elementsToHide = gameObjects.ToArray();
        }
    }

    void Start()
    {
        elementsToHideDefaultVisibility = new bool[elementsToHide.Length];
        for (int e = 0; e < elementsToHide.Length; e++)
            elementsToHideDefaultVisibility[e] = elementsToHide[e].activeSelf;
    }

    void Update()
    {
        if (VRTools.GetKeyDown(ToggleGameObjectsKey) ||
           (ToggleGameObjectsButton >= 0 && VRTools.IsButtonToggled((uint) ToggleGameObjectsButton)))
        {
            ToggleSelectedElementVisibility();
        }
    }


    public void ToggleSelectedElementVisibility()
    {
        foreach (GameObject element in elementsToHide)
            element.SetActive(!element.activeSelf);
    }

    public void SetSelectedElementVisibility(bool state)
    {
        foreach (GameObject element in elementsToHide)
            element.SetActive(state);
    }
    
    public void HideSelectedElement()
    {
        SetSelectedElementVisibility(false);
    }

    public void DisplaySelectedElement()
    {
        SetSelectedElementVisibility(true);
    }

    public void DefaultVisibilitySelectedEleement()
    {
        //Need to be init
        if (elementsToHideDefaultVisibility == null)
            return;

        for (int e = 0; e < elementsToHide.Length; e++)
             elementsToHide[e].SetActive(elementsToHideDefaultVisibility[e]);
    }
}
