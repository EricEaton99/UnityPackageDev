using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;



[ExecuteInEditMode]
public class BTDaisyWheel : MonoBehaviour
{
    // Variables
    #region Inspector Variables



    [SerializeField] GameObject optionUnselected;

    [SerializeField][HideInInspector] string numberOfOptionsPropertyName, optionsInnerRadiusPropertyName, optionsOuterRadiusPropertyName, gapWidthPropertyName;

    [SerializeField] public GameObject[] optionIcons;
    [SerializeField][Range(0f, 3f)] float optionIconSize;
    [SerializeField] bool allignOptionIcons;

    [SerializeField][Range(0f, 1f)] float optionsWidth, optionsSpacing;
    [SerializeField] bool haveDefaultOption;
    [SerializeField][Range(0f, 1f)] float optionsCenterSpacing;
    [SerializeField] GameObject defaultOptionIcon;

    [SerializeField] bool useDividers;
    [SerializeField] Sprite dividerIcon;

    [SerializeField, HideInInspector] string optionsAlphaPropertyName, optionsRingAlphaPropertyName, optionsRingSpreadPropertyName;
    [SerializeField][Range(0f, 1f)] float optionsAlpha, optionsSelectedAlpha, optionsRingAlpha, optionsRingSpread;

    #endregion

    #region private variables


    int numberOfOptions;
    float[] radialMenuAngles;
    float optionAngleFull;
    float optionIconScale, optionSelectedIconScale;
    float radialMenuInnerRadius;

    List<GameObject> dividers = new List<GameObject>();
    List<GameObject> options = new List<GameObject>();
    #endregion

    // Radial Menu Creation
    #region Helper Functions


    private float[] CalculateRadialMenuAngles()
    {
        float[] radialMenuAngles = new float[numberOfOptions];

        float currentAngle = Mathf.PI / 2;

        float incrementAngle = optionAngleFull = 2f * Mathf.PI / numberOfOptions;

        currentAngle += incrementAngle / 2;

        for (int i = 0; i < numberOfOptions; i++)
        {
            radialMenuAngles[i] = currentAngle;
            currentAngle -= incrementAngle;
        }
        return radialMenuAngles;
    }

    private void SetLocalVars()
    {
        //Menu
        numberOfOptions = optionIcons.Length;
        radialMenuInnerRadius = 1 - optionsWidth / 2;
        optionIconScale = optionsWidth * optionIconSize;
        optionSelectedIconScale = optionIconScale * (1 + optionsSpacing);
        //both: menu for fast lookup and option for creation
        radialMenuAngles = CalculateRadialMenuAngles();
    }

    private float angleToTransformRotation(float angle)
    {
        return angle * Mathf.Rad2Deg - 90f;
    }

    #endregion

    #region Make Radial Menu UI Elements

    private GameObject makeRadialMenuDivider(float angle, int optionNum)
    {
        float radius = 1 - optionsWidth / 2;
        float scale = optionsWidth;
        Vector2 pos = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

        GameObject divider = new GameObject($"Divider {optionNum + 1}");
        divider.transform.SetParent(transform, false);

        divider.AddComponent<Image>();
        divider.GetComponent<Image>().sprite = dividerIcon;

        if (divider.GetComponent<RectTransform>() == null) divider.AddComponent<RectTransform>();
        divider.GetComponent<RectTransform>().anchoredPosition = pos;
        divider.GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);
        divider.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, angleToTransformRotation(angle));

        return divider;
    }

    private GameObject makeOptionIcon(float angle, int optionNum)
    {
        Vector2 pos = new Vector2(0f, radialMenuInnerRadius);

        GameObject optionIcon = Instantiate(optionIcons[optionNum], transform, false);
        optionIcon.name = $"Option {optionNum + 1} Icon";

        if (optionIcon.GetComponent<Image>() != null) optionIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, optionsRingAlpha);


        if (optionIcon.GetComponent<RectTransform>() == null) optionIcon.AddComponent<RectTransform>();
        optionIcon.GetComponent<RectTransform>().anchoredPosition = pos;
        optionIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(optionIconScale, optionIconScale);

        if (allignOptionIcons)
        {
            optionIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, -angleToTransformRotation(angle));
        }
        else if (0.001f - Mathf.PI < angle && angle < -0.001f)
        {
            optionIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, angle + 180);
        }

        return optionIcon;
    }

    private GameObject makeRadialMenuOption(float angle, int optionNum)
    {
        float optionAnglel = angle - optionAngleFull / 2;
        GameObject optionInstance = Instantiate(optionUnselected, transform, false);

        optionInstance.name = $"Option {optionNum + 1}";



        float scale = 2;
        optionInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);

        optionInstance.GetComponent<Image>().material.SetFloat(optionsAlphaPropertyName, optionsAlpha);
        optionInstance.GetComponent<Image>().material.SetFloat(optionsRingAlphaPropertyName, optionsRingAlpha);
        optionInstance.GetComponent<Image>().material.SetFloat(optionsInnerRadiusPropertyName, 1 - optionsWidth);
        optionInstance.GetComponent<Image>().material.SetFloat(optionsRingSpreadPropertyName, optionsRingSpread);

        optionInstance.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, angleToTransformRotation(optionAnglel));
        optionInstance.GetComponent<Image>().material.SetFloat(numberOfOptionsPropertyName, numberOfOptions);
        optionInstance.GetComponent<Image>().material.SetFloat(gapWidthPropertyName, optionsSpacing);

        GameObject optionIcon = makeOptionIcon(optionAnglel, optionNum);
        optionIcon.transform.SetParent(optionInstance.transform, false);

        return optionInstance;
    }

    private GameObject makeCenterIcon()
    {
        float scale = optionsWidth * optionIconSize;

        GameObject optionIcon = Instantiate(defaultOptionIcon, transform, false);
        optionIcon.name = $"Center Icon";

        if (optionIcon.GetComponent<Image>() != null) optionIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, optionsRingAlpha);

        if (optionIcon.GetComponent<RectTransform>() == null) optionIcon.AddComponent<RectTransform>();
        optionIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);

        return optionIcon;
    }

    private GameObject makeRadialMenuCenter()
    {
        GameObject center = Instantiate(optionUnselected, transform, false);

        center.GetComponent<Image>().material = new Material(optionUnselected.GetComponent<Image>().material);
        center.name = $"Center (default)";

        //center.GetComponent<Image>().fillAmount = 1f;

        float scale = 2;
        center.GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);

        center.GetComponent<Image>().material.SetFloat(optionsAlphaPropertyName, optionsAlpha);
        center.GetComponent<Image>().material.SetFloat(optionsRingAlphaPropertyName, optionsRingAlpha);
        center.GetComponent<Image>().material.SetFloat(optionsRingSpreadPropertyName, optionsRingSpread);

        center.GetComponent<Image>().material.SetFloat(numberOfOptionsPropertyName, 1f);
        center.GetComponent<Image>().material.SetFloat(gapWidthPropertyName, 0f);
        center.GetComponent<Image>().material.SetFloat(optionsInnerRadiusPropertyName, 0f);
        center.GetComponent<Image>().material.SetFloat(optionsOuterRadiusPropertyName, 1 - optionsWidth - optionsCenterSpacing);

        GameObject centerIcon = makeCenterIcon();
        centerIcon.transform.SetParent(center.transform, false);

        return center;
    }

    #endregion

    #region Generate Radial Menu

    private void ResetRadialMenu()
    {
        //both
        foreach (var opt in dividers)
        {
            if (opt != null) DestroyImmediate(opt);
        }
        dividers.Clear();

        foreach (var opt in options)
        {
            if (opt != null) DestroyImmediate(opt);
        }
        options.Clear();

    }


    public void GenerateRadialMenu()
    {
        ResetRadialMenu();

        SetLocalVars();

        for (int i = 0; i < numberOfOptions; i++)
        {
            //menu
            if (useDividers) dividers.Add(makeRadialMenuDivider(radialMenuAngles[i], i));
            //option
            options.Add(makeRadialMenuOption(radialMenuAngles[i], i));
        }

        if (haveDefaultOption) options.Add(makeRadialMenuCenter());

        setHighlightedOption(0);
    }

    #endregion

    private void Awake()
    {
        GenerateRadialMenu();   //this is needed to set the private vars. particularly the dividers and options
    }

    // Radial Menu Update
    #region Set Radial Menu State

    private void setHighlightedOption(int option)
    {

        options[option].GetComponent<Image>().material = new Material(optionUnselected.GetComponent<Image>().material);

        Image[] images = options[option].GetComponentsInChildren<Image>();
        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                image.color = new Color(1f, 1f, 1f, 1f);
            }
        }


        options[option].GetComponent<Image>().material.SetFloat(optionsAlphaPropertyName, optionsSelectedAlpha);
        float scale = options[option].GetComponent<RectTransform>().sizeDelta.x * 1.1f;
        options[option].GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);
    }




    /// <summary>
    /// Animate the:
    ///     Alpha
    ///     Size
    /// Play sount
    /// </summary>

    [SerializeField] float optionSelectAnimationDuration; // Duration of the fade effect
    [SerializeField] bool optionDeselectInstantanious; // Duration of the fade effect


    [SerializeField] int currentOptionSelected;



    private void SetOptionValues(int optionNum, float bgAlpha, float iconAlpha, float scale)
    {
        options[optionNum].GetComponent<Image>().material.SetFloat(optionsAlphaPropertyName, bgAlpha);
        options[optionNum].GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);

        Image[] images = options[optionNum].GetComponentsInChildren<Image>();
        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                images[i].color = new Color(1f, 1f, 1f, iconAlpha);
            }
        }
    }

    private float[] GetOptionValues(int optionNum)
    {
        float[] properties = new float[3];

        properties[0] = options[optionNum].GetComponent<Image>().material.GetFloat(optionsAlphaPropertyName);
        properties[2] = options[optionNum].GetComponent<RectTransform>().sizeDelta.x;

        Image[] images = options[optionNum].GetComponentsInChildren<Image>();
        if (images != null)
        {
            properties[1] = images[0].color.a;
        }

        return properties;
    }


    struct FadeOptionValues
    {
        int optionNum;

        FadeOptionValues(int _optionNum)
        {
            optionNum = _optionNum;

        }
    }


    #endregion


#if UNITY_EDITOR
    [ContextMenu("Test Radial Menu Internals")]
    private void TestRadialMenuInternals()
    {
        Debug.Log($"--- BTDaisyWheel Test ---");
        Debug.Log($"optionIcons: {(optionIcons != null ? optionIcons.Length.ToString() : "null")}");
        Debug.Log($"numberOfOptions: {numberOfOptions}");
        Debug.Log($"optionAngleFull: {optionAngleFull}");
        Debug.Log($"dividers.Count: {dividers?.Count ?? -1}");
        Debug.Log($"options.Count: {options?.Count ?? -1}");
        Debug.Log($"optionUnselected: {(optionUnselected != null ? "set" : "null")}");
        Debug.Log($"defaultOptionIcon: {(defaultOptionIcon != null ? "set" : "null")}");
        Debug.Log($"gapWidthPropertyName: {gapWidthPropertyName}");
        Debug.Log($"numberOfOptionsPropertyName: {numberOfOptionsPropertyName}");
        Debug.Log($"optionsAlphaPropertyName: {optionsAlphaPropertyName}");
        Debug.Log($"optionsRingAlphaPropertyName: {optionsRingAlphaPropertyName}");
        Debug.Log($"optionsRingSpreadPropertyName: {optionsRingSpreadPropertyName}");
        Debug.Log($"optionsInnerRadiusPropertyName: {optionsInnerRadiusPropertyName}");
        Debug.Log($"optionsOuterRadiusPropertyName: {optionsOuterRadiusPropertyName}");
        Debug.Log($"dividerIcon: {(dividerIcon != null ? "set" : "null")}");
    }
#endif

}


#region zzTemp

[Serializable]
class BTDWOption
{
    [SerializeField] Sprite optionIcon;
    [SerializeField] public BTDWOptionOption[] optionOptions;
}


[Serializable]
class BTDWOptionOption
{
    [SerializeField] Sprite optionIcon;
    [SerializeField] public string callback;
}

#endregion


class RadialMenuOption : MonoBehaviour
{
    GameObject rootObject;
    Image mainImage;
    Image childImage;
    Transform childTransform;

    float animationDurration;

    Coroutine animateCoroutine;

    public enum OptionStates
    {
        Hidden,
        Unselected,
        Selected,
        Length     //leave at end
    };

    public void SetState(OptionStates state, bool animated)
    {
        if (!animated)
        {
            //set values directly
            return;
        }

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        animateCoroutine = StartCoroutine(AnimateOption());
    }


    private IEnumerator AnimateOption()
    {
        float time = 0;

        float start = 0f;
        float target = 1f;

        while (time < animationDurration)
        {
            time += Time.deltaTime;
            float progress = time / animationDurration;

            float bgAlpha = Mathf.Lerp(start, target, progress);

            yield return null;
        }

        // Ensure the final alpha is exactly the target

        Debug.Log($"finished");
    }
}


#region UI Customization

[CustomEditor(typeof(BTDaisyWheel))]
public class DWMenuEditor : Editor
{
    SerializedProperty useCustomOptionHighlight;
    SerializedProperty customOptionHighlight;
    SerializedProperty defaultOptionHighlightAlpha;

    void OnEnable()
    {
        //useCustomOptionHighlight = serializedObject.FindProperty("useCustomOptionHighlight");
        //customOptionHighlight = serializedObject.FindProperty("customOptionHighlight");
        //defaultOptionHighlightAlpha = serializedObject.FindProperty("defaultOptionHighlightAlpha");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BTDaisyWheel menu = (BTDaisyWheel)target;

        if (GUILayout.Button("Generate Wheel"))
        {
            menu.GenerateRadialMenu();
        }

        if (GUILayout.Button("Select Option"))
        {
            //if (!menu.BTRadialMenuSelectElement(2)) Debug.Log("BTRadialMenuSelectElement failed");
        }

        //if (useCustomOptionHighlight.boolValue)
        //{
        //    EditorGUILayout.PropertyField(customOptionHighlight);
        //}
        //else
        //{
        //    EditorGUILayout.PropertyField(defaultOptionHighlightAlpha);
        //}
    }
}

#endregion