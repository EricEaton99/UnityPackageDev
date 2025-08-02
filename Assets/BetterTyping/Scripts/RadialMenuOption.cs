using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BetterTyping
{
    #region public Enums, structs, and storage and helper classes 

    public enum OptionState
    {
        Hidden,
        Unselected,
        Selected,
        Length     //leave at end
    };

    public enum MenuGradientType
    {
        CircualrBubble,
        CircularRamp,
        ExponentialRamp,
        Linear,
    };




    [System.Serializable]
    public struct OptionMenuProporties
    {
        public GameObject optionIconPrefab;
        public UnityEvent<int, ControllerInputOptions, InputActionPhase> optionCallbackEvent;
    }
    [System.Serializable]
    public struct StaticInputOptions
    {
        public ControllerInputOptions inputOption;
        public UnityEvent<int, ControllerInputOptions> optionCallbackEvent;
    }
    [System.Serializable]
    public struct MenuSettings
    {
        [Range(0f, 1f)] public float optionsWidth, optionsSpacing;
        public Color optionUnselectedColor, optionSelectedColor;
        [Range(0f, 1f)] public float optionsUnselectedAlpha, optionsSelectedAlpha;
        public float selectionAnimationDurration;
    }

    [System.Serializable]
    public struct OptionIconSettings
    {
        [Range(0f, 1f)] public float optionIconUnselectedAlpha;
        //public Color optionIconUnselectedColorMultiplyer, optionIconSelectedColorMultiplyer;
        public float optionIconScale;
        [Range(0f, 3f)] public float optionIconSelectedScaleMultiplyer;
        public bool allignOptionIcons;
    }

    [System.Serializable]
    public struct MenuGradientSettings
    {
        [Range(0f, 1f)] public float menuGradientInnerRadius;
        [Range(0f, 2f)] public float menuGradientOuterRadius;
        [Range(0f, 1f)] public float menuGradientStrength;
        public MenuGradientType menuGradientType;
        public Color menuGradientColor;
    }

    [System.Serializable]
    public struct OptionDeviderSettings
    {
        public bool useDividers;
    }
    [System.Serializable]
    public struct DefaultOptionSettings
    {
        public bool haveDefaultOption;
        public GameObject defaultOptionPrefab;
        [Range(0f, 1f)] public float defaultOptionWidth;
        public OptionMenuProporties defaultOptionProporties;
    }

    public struct OptionStateProportyValues
    {
        public float bgAlpha;
        public Color bgColor;
        public float iconAlpha;
        public float iconScale;

        public OptionStateProportyValues(float bgAlpha, Color bgColor, float iconAlpha, float iconScale)
        {
            this.bgAlpha = bgAlpha;
            this.bgColor = bgColor;
            this.iconAlpha = iconAlpha;
            this.iconScale = iconScale;
        }
    }

    public class OptionStateProporties
    {
        public float[] bgAlpha;
        public Color[] bgColor;
        public float[] iconAlpha;
        public float[] iconScale;

        public OptionStateProportyValues[] values = new OptionStateProportyValues[(int)OptionState.Length];

        public OptionStateProporties(float[] bgAlpha, Color[] bgColor, float[] iconAlpha, float[] iconScale)
        {
            SetOptionStates(bgAlpha, bgColor, iconAlpha, iconScale);
        }

        public void SetOptionStates(float[] bgAlpha, Color[] bgColor, float[] iconAlpha, float[] iconScale)
        {
            int length = (int)OptionState.Length;
            if (bgAlpha.Length != length || bgColor.Length != length || iconAlpha.Length != length || iconScale.Length != length)
            {
                Debug.LogError("length mismatch for OptionState array lengths");
            }

            this.bgAlpha = bgAlpha;
            this.bgColor = bgColor;
            this.iconAlpha = iconAlpha;
            this.iconScale = iconScale;

            for(int i= 0; i < length; i++)
            {
                Debug.Log($"index {i}");
                values[i] = new OptionStateProportyValues(bgAlpha[i], bgColor[i], iconAlpha[i], iconScale[i]);
            }

        }
    }

    public class RadialMenuData
    {
        public MenuSettings menuSettings;
        public OptionIconSettings optionIconSettings;
        public MenuGradientSettings menuGradientSettings;
        public DefaultOptionSettings defaultOptionSettings;

        public int numberOfOptions;
        public float optionAngleFull;
        public float radialMenuInnerRadius;

        public OptionStateProporties optionStateProporties;


        public RadialMenuData(MenuSettings menuSettings, OptionIconSettings optionIconSettings, MenuGradientSettings menuGradientSettings, DefaultOptionSettings defaultOptionSettings, int numberOfOptions, float optionAngleFull, float radialMenuInnerRadius, OptionStateProporties optionStateProporties)
        {
            this.menuSettings = menuSettings;
            this.optionIconSettings = optionIconSettings;
            this.menuGradientSettings = menuGradientSettings;
            this.defaultOptionSettings = defaultOptionSettings;
            this.numberOfOptions = numberOfOptions;
            this.optionAngleFull = optionAngleFull;
            this.radialMenuInnerRadius = radialMenuInnerRadius;
            this.optionStateProporties = optionStateProporties;
        }
    }

    public class HelperFunctions
    {
        public static float AngleToTransformRotation(float angle)
        {
            return angle * Mathf.Rad2Deg - 90f;
        }

    }

    #endregion


    public class RadialMenuOption : MonoBehaviour
    {
        GameObject optionInstance;
        GameObject iconInstance;

        float optionAngle;
        private bool isDefaultOption;
        int optionNum;



        // HideInInspector
        [SerializeField] string numberOfOptions_Name, optionsInnerRadius_Name, optionsOuterRadius_Name, gapWidth_Name;
        [SerializeField] string optionsAlpha_Name, optionsGradientStrength_Name, optionsGradientInnerRadius_Name, optionsGradientOuterRadius_Name;
        [SerializeField] string optionsGradientMode_Name, optionsGradientColor_Name, optionsMenuColor_Name;

        RadialMenuData menuData;

        Coroutine animationCoroutine;


        private float animationProgress = 0f;
        private OptionState animationStartState = OptionState.Unselected;
        private OptionState animationTargetState = OptionState.Unselected;


        #region Helper Functions


        private void SetAllImagesAlpha(GameObject root, float alpha)
        {
            Color color = new Color(1f, 1f, 1f, alpha);

            Image[] imageList = root.GetComponentsInChildren<Image>();
            for (int i = 0; i < imageList.Length; i++)
            {
                imageList[i].color = color;
            }
        }

        #endregion



        #region Setup



        private GameObject makeOptionIcon(GameObject objectIconPrefab)
        {
            GameObject optionIcon = Instantiate(objectIconPrefab, optionInstance.transform, false);
            optionIcon.name = $"Option {optionNum + 1} Icon";

            SetAllImagesAlpha(optionIcon, menuData.optionStateProporties.iconAlpha[(int)OptionState.Unselected]);

            if (optionIcon.GetComponent<RectTransform>() == null) optionIcon.AddComponent<RectTransform>();

            Vector2 pos = new Vector2(0f, menuData.radialMenuInnerRadius + menuData.menuSettings.optionsWidth/2);
            optionIcon.GetComponent<RectTransform>().anchoredPosition = pos;

            float iconScale = menuData.optionStateProporties.iconScale[(int)OptionState.Unselected];
            //optionIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(menuData.optionIconScale, menuData.optionIconScale);
            optionIcon.GetComponent<RectTransform>().localScale = new Vector2(iconScale, iconScale);

            if (menuData.optionIconSettings.allignOptionIcons)
            {
                optionIcon.GetComponent<RectTransform>().rotation = Quaternion.identity;
            }
            else if (0.001f - Mathf.PI < optionAngle && optionAngle < -0.001f)
            {
                optionIcon.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, HelperFunctions.AngleToTransformRotation(optionAngle) + 180);
            }

            return optionIcon;
        }

        private GameObject makeRadialMenuOption(GameObject objectIconPrefab)
        {
            optionInstance.name = $"Option {optionNum}";


            float scale = 2;
            optionInstance.GetComponent<RectTransform>().sizeDelta = new Vector2(scale, scale);

            optionInstance.GetComponent<Image>().material = new Material(optionInstance.GetComponent<Image>().material);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsAlpha_Name, menuData.optionStateProporties.bgAlpha[(int)OptionState.Unselected]);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsOuterRadius_Name, 1);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsInnerRadius_Name, menuData.radialMenuInnerRadius);
            optionInstance.GetComponent<Image>().material.SetColor(optionsMenuColor_Name, menuData.menuSettings.optionUnselectedColor);

            optionInstance.GetComponent<Image>().material.SetFloat(optionsGradientInnerRadius_Name, menuData.menuGradientSettings.menuGradientInnerRadius);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsGradientOuterRadius_Name, menuData.menuGradientSettings.menuGradientOuterRadius);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsGradientStrength_Name, menuData.menuGradientSettings.menuGradientStrength);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsGradientMode_Name, (float)menuData.menuGradientSettings.menuGradientType);
            optionInstance.GetComponent<Image>().material.SetColor(optionsGradientColor_Name, menuData.menuGradientSettings.menuGradientColor);

            optionInstance.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, HelperFunctions.AngleToTransformRotation(optionAngle));
            
            optionInstance.GetComponent<Image>().material.SetFloat(numberOfOptions_Name, menuData.numberOfOptions);
            optionInstance.GetComponent<Image>().material.SetFloat(gapWidth_Name, menuData.menuSettings.optionsSpacing);

            iconInstance = makeOptionIcon(objectIconPrefab);
            iconInstance.transform.SetParent(optionInstance.transform, false);

            return optionInstance;
        }

        public void SetDefaultOption(float defaultOptionWidth)
        {
            optionInstance.GetComponent<Image>().material.SetFloat(optionsOuterRadius_Name, menuData.radialMenuInnerRadius * defaultOptionWidth);
            optionInstance.GetComponent<Image>().material.SetFloat(optionsInnerRadius_Name, 0);
            optionInstance.GetComponent<Image>().material.SetFloat(numberOfOptions_Name, 1);
            optionInstance.name = $"Option {optionNum} (default)";


            Vector2 pos = Vector2.zero;
            iconInstance.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        public void Setup(GameObject optionInstance, RadialMenuData menuData, float angle, int optionNum, GameObject objectIconPrefab)
        {
            this.optionInstance = optionInstance;
            this.menuData = menuData;
            this.optionNum = optionNum;
            this.optionAngle = angle - this.menuData.optionAngleFull / 2;

            makeRadialMenuOption(objectIconPrefab);
        }


        #endregion


        #region Set State

        public void SetState(OptionState stateEnum)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateOption(stateEnum));
        }

        public void SetState(OptionState stateEnum, bool immediate)
        {
            if (!immediate)
            {
                SetState(stateEnum);
                return;
            }

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            SetOptionProporties(stateEnum);
            animationStartState = animationTargetState;

        }

        public OptionState GetState()
        {
            return animationTargetState;
        }


        private OptionStateProportyValues GetOptionProporties()
        {
            float bgAlpha = optionInstance.GetComponent<Image>().material.GetFloat(optionsAlpha_Name);
            Color bgColor = optionInstance.GetComponent<Image>().material.GetColor(optionsMenuColor_Name);
            float iconAlpha = iconInstance.GetComponent<Image>().color.a;
            float iconScale = iconInstance.GetComponent<RectTransform>().localScale.x;

            return new OptionStateProportyValues(bgAlpha , bgColor, iconAlpha, iconScale);
        }

        private OptionStateProportyValues GetOptionProporties(OptionState state)
        {
            return menuData.optionStateProporties.values[(int)state];
        }

        private void SetOptionProporties(float bgAlpha, Color bgColor, float iconAlpha, float iconScale)
        {
            optionInstance.GetComponent<Image>().material.SetFloat(optionsAlpha_Name, bgAlpha);
            //optionInstance.GetComponent<Image>().material.SetFloat(optionsAlpha_Name, gradientAlpha);
            optionInstance.GetComponent<Image>().material.SetColor(optionsMenuColor_Name, bgColor);
            SetAllImagesAlpha(iconInstance, iconAlpha);

            //if (iconInstance.GetComponent<Image>() != null) iconInstance.GetComponent<Image>().color = new Color(1f, 1f, 1f, iconAlpha);
            iconInstance.GetComponent<RectTransform>().localScale = new Vector2(iconScale, iconScale);
        }

        private void SetOptionProporties(OptionStateProportyValues optionProperties)
        {
            SetOptionProporties(optionProperties.bgAlpha, optionProperties.bgColor, optionProperties.iconAlpha, optionProperties.iconScale);
        }

        private void SetOptionProporties(OptionState state)
        {
            SetOptionProporties(GetOptionProporties(state));
        }



        private float GetAnimationDuration(OptionState startingState, OptionState targetState, float startingProgress)
        {
            if (startingState == OptionState.Hidden && targetState == OptionState.Hidden) return 0f;
            if (startingState == OptionState.Hidden || targetState == OptionState.Hidden) return menuData.menuSettings.selectionAnimationDurration;

            float start = startingProgress;
            float end = (startingState != targetState) ? 1f : 0f;
            return Mathf.Abs(end - start) * menuData.menuSettings.selectionAnimationDurration;
        }


        private IEnumerator AnimateOption(OptionState targetState)
        {
            animationTargetState = targetState;

            float duration = GetAnimationDuration(animationStartState, targetState, animationProgress);
            float time = 0f;


            OptionStateProportyValues startValues = GetOptionProporties();
            OptionStateProportyValues currentvalues = new OptionStateProportyValues();
            OptionStateProportyValues targetValues = GetOptionProporties(targetState);

            //  Debug.Log($"start state {animationStartState} -< target state {animationTargetState},\t\t start scale {startValues[3]} -> target scale {targetValues[3]}");


            while (time < duration)
            {
                time += Time.deltaTime;
                animationProgress = time / duration;

                currentvalues.bgAlpha = Mathf.Lerp(startValues.bgAlpha, targetValues.bgAlpha, animationProgress);
                currentvalues.bgColor = Color.Lerp(startValues.bgColor, targetValues.bgColor, animationProgress);
                currentvalues.iconAlpha = Mathf.Lerp(startValues.iconAlpha, targetValues.iconAlpha, animationProgress);
                currentvalues.iconScale = Mathf.Lerp(startValues.iconScale, targetValues.iconScale, animationProgress);

                SetOptionProporties(currentvalues);

                yield return null;
            }

            SetOptionProporties(targetValues);
            animationProgress = 0f;
            animationStartState = targetState;
        }

        #endregion
    }
}