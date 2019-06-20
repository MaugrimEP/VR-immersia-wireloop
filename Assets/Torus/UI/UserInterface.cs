using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [Header("GENERAL")]
    public Camera UICamera;
    public GameObject BackgroundPanel;
    public GameObject CircleMenuElementPrefab;
    public InputController ic;

    [Header("BUTTONS")]
    public Color NormalButtonColor;
    public Color HighlightButtonColor;

    [Header("INFORMAL CENTER")]
    public Image InformalCenterBackground;
    public Text ItemName;
    public Text ItemDescription;
    public Image ItemIcon;

    private int currentMenuItemIndex;
    private int previousMenuItemIndex;
    private float calculatedMenuIndex;
    private float currentSelectionAngle;
    private Vector3 currentMousePosition;
    public List<CircularMenuElement> menuElements = new List<CircularMenuElement>();

    private static UserInterface instance;
    public static UserInterface Instance { get { return instance; } }
    public bool Active { get { return BackgroundPanel.activeSelf; } }
    public List<CircularMenuElement> MenuElements
    {
        get
        {
            return menuElements;
        }
        set
        {
            menuElements = value;
        }
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        float rotationalIncrementalValue = 360f / menuElements.Count;
        float currentRotationValue = 0f;
        float fillPercentageValue = 1f / menuElements.Count;

        for(int i = 0; i < menuElements.Count; ++i)
        {
            GameObject menuElementGameObject = Instantiate(CircleMenuElementPrefab);
            menuElementGameObject.name = $"{i} : {currentRotationValue}";
            menuElementGameObject.transform.SetParent(BackgroundPanel.transform);

            MenuButton menuButton = menuElementGameObject.GetComponent<MenuButton>();

            menuButton.Recttransform.localScale = Vector3.one;
            menuButton.Recttransform.localPosition = Vector3.one;
            menuButton.Recttransform.rotation = UICamera.transform.rotation * Quaternion.Euler(0f, 0f, currentRotationValue);
            menuButton.IconRecttransform.rotation = UICamera.transform.rotation;
            {
                //menuButton.Recttransform.rotation *= UICamera.transform.rotation;
                //menuButton.IconRecttransform.rotation *= UICamera.transform.rotation;
                Debug.Log($"{UICamera.transform.rotation.eulerAngles}");
            }
            currentRotationValue += rotationalIncrementalValue;

            menuButton.BackgroundImage.fillAmount = fillPercentageValue + 0.001f;
            menuElements[i].ButtonBackground = menuButton.BackgroundImage;
            menuElements[i].ButtonBackground.color = NormalButtonColor;

            menuButton.IconImage.sprite = menuElements[i].ButtonIcon;
            //menuButton.IconRecttransform.rotation = Quaternion.identity;
        }

        BackgroundPanel.SetActive(false);
    }

    private void Update()
    {
        if ((!Active))
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Activate();
            }
            return;
        }


        GetCurreentMenuElement();
        if (Input.GetMouseButton(0))
        {
            Select();
        }
    }

    private void GetCurreentMenuElement()
    {
        float rotationalIncrementalValue = 360f / MenuElements.Count;
        currentMousePosition = new Vector2(Input.mousePosition.x - Screen.width / 2f, Input.mousePosition.y - Screen.height / 2f);

        currentSelectionAngle = 90 + rotationalIncrementalValue + Mathf.Atan2(currentMousePosition.y, currentMousePosition.x) * Mathf.Rad2Deg;
        currentSelectionAngle = (currentSelectionAngle + 360f) % 360f;

        currentMenuItemIndex = (int)(currentSelectionAngle / rotationalIncrementalValue);

        if(currentMenuItemIndex != previousMenuItemIndex)
        {
            menuElements[previousMenuItemIndex].ButtonBackground.color = NormalButtonColor;

            previousMenuItemIndex = currentMenuItemIndex;

            menuElements[currentMenuItemIndex].ButtonBackground.color = HighlightButtonColor;
            InformalCenterBackground.color = HighlightButtonColor;
            RefreshInformalCenter();
        }
    }

    private void RefreshInformalCenter()
    {
        ItemName.text = menuElements[currentMenuItemIndex].Name;
        ItemDescription.text = menuElements[currentMenuItemIndex].Description;
        ItemIcon.sprite = menuElements[currentMenuItemIndex].ButtonIcon ;
    }

    private void Select()
    {
        DoSomething();
        Deactivate();
    }

    private void DoSomething()
    {
        if (Debug.isDebugBuild)
            Debug.Log($"SelectedIndex : {currentMenuItemIndex}");
    }

    public void Activate()
    {
        if (Active) return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
        BackgroundPanel.SetActive(true);
        RefreshInformalCenter();
    }

    public void Deactivate()
    {
        BackgroundPanel.SetActive(false);
    }
}
