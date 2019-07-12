using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceVirtuose : MonoBehaviour
{
    [Header("GENERAL")]
    public InputController ic;
    public Transform Head;
    public GameObject BackgroundPanel;
    public GameObject CircleMenuElementPrefab;
    public GameObject PlaneGO;
    public float activationCooldown;

    private float nextActivation;
    private Plane plane;
    private Vector3 activationVirtPos;

    private Transform debugSphereTransform;

    [Header("BUTTONS")]
    public Color NormalButtonColor;
    public Color HighlightButtonColor;

    [Header("INFORMAL CENTER")]
    public Image InformalCenterBackground;
    public Text ItemName;
    public Text ItemDescription;
    public Image ItemIcon;

    protected int currentMenuItemIndex;
    protected int previousMenuItemIndex;
    protected float calculatedMenuIndex;
    protected float currentSelectionAngle;
    protected Vector3 currentMousePosition;
    public List<CircularMenuElement> menuElements = new List<CircularMenuElement>();

    protected static UserInterface instance;
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

    protected virtual void Start()
    {
        if (VRTools.IsClient())
        {
            gameObject.SetActive(false);
            return;
        }

        Initialize();

        Mesh planeMesh = PlaneGO.GetComponent<MeshFilter>().mesh;
        Vector3 p1 = planeMesh.vertices[0];
        Vector3 p2 = planeMesh.vertices[1];
        Vector3 p3 = planeMesh.vertices[2];

        plane = new Plane(p1, p2, p3);

        nextActivation = VRTools.GetTime();

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = Color.green;
        debugSphereTransform = sphere.transform;
        debugSphereTransform.localScale = Vector3.one * 0.05f;

    }

    public void Initialize()
    {
        float rotationalIncrementalValue = 360f / menuElements.Count;
        float currentRotationValue = 0f;
        float fillPercentageValue = 1f / menuElements.Count;

        for (int i = 0; i < menuElements.Count; ++i)
        {
            GameObject menuElementGameObject = Instantiate(CircleMenuElementPrefab);
            menuElementGameObject.name = $"{i} : {currentRotationValue}";
            menuElementGameObject.transform.SetParent(BackgroundPanel.transform);

            MenuButton menuButton = menuElementGameObject.GetComponent<MenuButton>();

            menuButton.Recttransform.localScale = Vector3.one;
            menuButton.Recttransform.localPosition = Head.forward * 0.5f;
            menuButton.Recttransform.rotation = Head.rotation * Quaternion.Euler(0f, 0f, currentRotationValue);
            menuButton.IconRecttransform.rotation = Head.rotation;
            currentRotationValue += rotationalIncrementalValue;

            menuButton.BackgroundImage.fillAmount = fillPercentageValue + 0.001f;
            menuElements[i].ButtonBackground = menuButton.BackgroundImage;
            menuElements[i].ButtonBackground.color = NormalButtonColor;

            menuButton.IconImage.sprite = menuElements[i].ButtonIcon;
            //menuButton.IconRecttransform.rotation = Quaternion.identity;
        }

        BackgroundPanel.SetActive(false);
    }

    protected virtual void Update()
    {
        if ((!Active))
        {
            if (ic.virtuoseManager.IsButtonPressed(1))
            {
                if (VRTools.GetTime() > nextActivation)
                {
                    nextActivation = VRTools.GetTime() + activationCooldown;
                    activationVirtPos = ic.GetVirtuosePose().Position;
                    Activate();
                }
            }
            return;
        }

        GetCurreentMenuElement();
        if (ic.virtuoseManager.IsButtonPressed(1))
        {
            if (VRTools.GetTime() > nextActivation)
            {
                nextActivation = VRTools.GetTime() + activationCooldown;
                Select();
            }
        }
    }

    protected virtual void GetCurreentMenuElement()
    {
        float rotationalIncrementalValue = 360f / MenuElements.Count;

        (Vector3 virtPos, Quaternion virtRot) = ic.GetVirtuosePose();
        Vector3 pointOfPlane = plane.ClosestPointOnPlane(virtPos);

        Vector3 center = plane.ClosestPointOnPlane(activationVirtPos);
        float distance = Vector3.Distance(center, pointOfPlane); // = sinus * magnitude
        float sinus = distance / Vector3.Distance(center, virtPos);
        float angle = Mathf.Asin(sinus) * Mathf.Rad2Deg;

        currentMousePosition = new Vector2(pointOfPlane.x, pointOfPlane.y);

        debugSphereTransform.transform.position = pointOfPlane;

        //currentMousePosition = new Vector2(Input.mousePosition.x - Screen.width / 2f, Input.mousePosition.y - Screen.height / 2f);

        currentSelectionAngle = 90 + rotationalIncrementalValue + Mathf.Atan2(currentMousePosition.y, currentMousePosition.x) * Mathf.Rad2Deg;
        currentSelectionAngle = (currentSelectionAngle + 360f) % 360f;

        //Debug.Log($"currentMousePosition {currentMousePosition}");

        currentMenuItemIndex = (int)(currentSelectionAngle / rotationalIncrementalValue);

        if (currentMenuItemIndex != previousMenuItemIndex)
        {
            menuElements[previousMenuItemIndex].ButtonBackground.color = NormalButtonColor;

            previousMenuItemIndex = currentMenuItemIndex;

            menuElements[currentMenuItemIndex].ButtonBackground.color = HighlightButtonColor;
            InformalCenterBackground.color = HighlightButtonColor;
            RefreshInformalCenter();
        }
    }

    protected virtual void RefreshInformalCenter()
    {
        ItemName.text = menuElements[currentMenuItemIndex].Name;
        ItemDescription.text = menuElements[currentMenuItemIndex].Description;
        ItemIcon.sprite = menuElements[currentMenuItemIndex].ButtonIcon;
    }

    protected virtual void Select()
    {
        DoSomething();
        Deactivate();
    }

    protected virtual void DoSomething()
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
