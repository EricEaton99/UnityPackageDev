using BetterTyping;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace BetterTyping
{
    public class RadialMenu : MonoBehaviour
    {
        #region public variables

        public int numberOfOptions;

        #endregion


        #region serialized variables

        [SerializeField] GameObject optionPrefab;

        [SerializeField] OptionMenuProporties[] optionMenuProportiesList;
        [SerializeField] ControllerInputOptions[] radialInputOptions;
        [SerializeField] StaticInputOptions[] staticInputOptions;

        [SerializeField] MenuSettings menuSettings;
        [SerializeField] OptionIconSettings optionIconSettings;
        [SerializeField] MenuGradientSettings menuGradientSettings;
        [SerializeField] OptionDeviderSettings optionDeviderSettings;
        [SerializeField] DefaultOptionSettings defaultOptionSettings;

        #endregion


        #region private variables

        int currentOption;
        float[] radialMenuAngles;
        float optionAngleFull;
        float optionIconScale, optionSelectedIconScale;
        float radialMenuInnerRadius;

        List<GameObject> dividerInstances = new List<GameObject>();
        List<GameObject> optionInstances = new List<GameObject>();
        GameObject defaultOptionInstance;

        RadialMenuData menuData;

        Coroutine FadeMenuCoroutine;

        #endregion


        private void Awake()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            GenerateRadialMenu();
        }


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

        private int PositionToOption(Vector2 position)
        {
            float rotation = Mathf.Atan2(position.y, position.x);
            //convert from radians to clockwise
            float projectedRotation = -(rotation - Mathf.PI / 2) / (2 * Mathf.PI);
            if (projectedRotation < 0) projectedRotation++;
            int optionNum = Mathf.RoundToInt(projectedRotation * numberOfOptions);
            if (optionNum >= numberOfOptions) optionNum = 0;

            return optionNum;
        }


        private OptionStateProporties GenerateOptionStateProporties()
        {
            float[] bgAlpha = new float[(int)OptionState.Length];
            bgAlpha[(int)OptionState.Hidden] = 0.0f;
            bgAlpha[(int)OptionState.Unselected] = menuSettings.optionsUnselectedAlpha; //menuData.optionsAlpha
            bgAlpha[(int)OptionState.Selected] = menuSettings.optionsSelectedAlpha;

            Color[] bgColor = new Color[(int)OptionState.Length];
            bgColor[(int)OptionState.Hidden] = Color.clear;
            bgColor[(int)OptionState.Unselected] = menuSettings.optionUnselectedColor;
            bgColor[(int)OptionState.Selected] = menuSettings.optionSelectedColor;

            float[] iconAlpha = new float[(int)OptionState.Length];
            iconAlpha[(int)OptionState.Hidden] = 0.0f;
            iconAlpha[(int)OptionState.Unselected] = optionIconSettings.optionIconUnselectedAlpha;
            iconAlpha[(int)OptionState.Selected] = 1.0f;

            float[] iconScale = new float[(int)OptionState.Length];
            iconScale[(int)OptionState.Hidden] = 0.0f;
            iconScale[(int)OptionState.Unselected] = optionIconScale;
            iconScale[(int)OptionState.Selected] = optionSelectedIconScale;

            return new OptionStateProporties(bgAlpha, bgColor, iconAlpha, iconScale);
        }

        private void SetLocalVars()
        {
            numberOfOptions = optionMenuProportiesList.Length;
            currentOption = -1;
            radialMenuInnerRadius = 1 - menuSettings.optionsWidth;
            optionIconScale = menuSettings.optionsWidth * optionIconSettings.optionIconScale;
            optionSelectedIconScale = optionIconScale * optionIconSettings.optionIconSelectedScaleMultiplyer;
            radialMenuAngles = CalculateRadialMenuAngles();

            OptionStateProporties optionStateProporties = GenerateOptionStateProporties();

            //Debug.Log($"icon scale: unselected {optionStateProporties.iconScale[(int)OptionState.Unselected]}, selected {optionStateProporties.iconScale[(int)OptionState.Selected]}");


            menuData = new RadialMenuData(
                 menuSettings,
                 optionIconSettings,
                 menuGradientSettings,
                 defaultOptionSettings,
                 numberOfOptions,
                 optionAngleFull,
                 radialMenuInnerRadius,
                 optionStateProporties);

        }

        #endregion


        #region Generate Radial Menu

        private void ResetRadialMenu()
        {
            foreach (var opt in dividerInstances)
            {
                if (opt != null) DestroyImmediate(opt);
            }
            dividerInstances.Clear();

            foreach (var opt in optionInstances)
            {
                if (opt != null) DestroyImmediate(opt);
            }
            optionInstances.Clear();

            if (defaultOptionInstance != null) DestroyImmediate(defaultOptionInstance);

        }

        public void GenerateRadialMenu()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            ResetRadialMenu();

            SetLocalVars();

            for (int i = 0; i < numberOfOptions; i++)
            {
                //if (useDividers) dividerInstances.Add(makeRadialMenuDivider(radialMenuAngles[i], i));

                GameObject optionInstance = Instantiate(optionPrefab, this.gameObject.transform);
                optionInstance.GetComponent<RadialMenuOption>().Setup(optionInstance, menuData, radialMenuAngles[i], i, optionMenuProportiesList[i].optionIconPrefab);
                optionInstances.Add(optionInstance);

                //Debug.Log($"added instance {i}");
            }

            if (defaultOptionSettings.haveDefaultOption)
            {
                GameObject optionInstance = Instantiate(defaultOptionSettings.defaultOptionPrefab, this.gameObject.transform);
                optionInstance.GetComponent<RadialMenuOption>().Setup(optionInstance, menuData, radialMenuAngles[0], -1, defaultOptionSettings.defaultOptionProporties.optionIconPrefab);
                optionInstance.GetComponent<RadialMenuOption>().SetDefaultOption(defaultOptionSettings.defaultOptionWidth);
                defaultOptionInstance = optionInstance;
            }

            //hide Munu
            SelectOption(0, OptionState.Hidden, true);
        }

        public void _ResetRadialMenu()
        {
            ResetRadialMenu();
        }

        #endregion


        #region Actions


        public OptionState GetCurrentState()
        {
            //Debug.Log($" i {currentOption} , default state {defaultOptionInstance.GetComponent<RadialMenuOption_7_22_9I00>().GetState()}, option[i] state {optionInstances[currentOption].GetComponent<RadialMenuOption_7_22_9I00>().GetState()}, option[i+1] {optionInstances[currentOption+1].GetComponent<RadialMenuOption_7_22_9I00>().GetState()}");

            //handle default case
            if (currentOption < 0 || defaultOptionInstance.GetComponent<RadialMenuOption>().GetState() == OptionState.Selected) return OptionState.Unselected;


            return optionInstances[currentOption].GetComponent<RadialMenuOption>().GetState();
        }

        public int GetCurrentOption()
        {
            return currentOption;
        }

        private void SelectDefaultOption(OptionState state, bool immediate)
        {
            OptionState defaultOptionState = OptionState.Hidden;

            //logic is different than for non-default options
            switch (state)
            {
                case OptionState.Hidden:
                    defaultOptionState = OptionState.Hidden;
                    break;
                case OptionState.Unselected:
                    defaultOptionState = OptionState.Selected;
                    break;
                case OptionState.Selected:
                    defaultOptionState = OptionState.Unselected;
                    break;
            }

            defaultOptionInstance.GetComponent<RadialMenuOption>().SetState(defaultOptionState, immediate);
        }

        private OptionState[] GetNextStatesFromCurrent(int optionNum, OptionState state)
        {
            switch (state)
            {
                case OptionState.Hidden:
                    return Enumerable.Repeat(state, numberOfOptions).ToArray();
                case OptionState.Unselected:
                    return Enumerable.Repeat(state, numberOfOptions).ToArray();
                case OptionState.Selected:
                    OptionState[] nextStates = Enumerable.Repeat(OptionState.Unselected, numberOfOptions).ToArray();
                    if (optionNum >= 0) nextStates[optionNum] = OptionState.Selected;
                    return nextStates;
                default:
                    Debug.LogError($"GetNextStatesFromCurrent: unexpected state {state}");
                    return null;
            }

        }

        public void SelectOption(int optionNum, OptionState state, bool immediate)
        {
            if (optionInstances.Count == 0) return;
            //if(optionNum  == currentOption && state == GetCurrentState()) return;

            currentOption = optionNum;

            OptionState[] nextStates = GetNextStatesFromCurrent(optionNum, state);


            for (int i = 0; i < numberOfOptions; i++)
            {
                optionInstances[i].GetComponent<RadialMenuOption>().SetState(nextStates[i], immediate);
            }


            if (defaultOptionSettings.haveDefaultOption)
            {
                SelectDefaultOption(state, immediate);
            }
        }


        public void SelectOption(Vector2 position, OptionState state)
        {
            int optionNum = PositionToOption(position);
            SelectOption(optionNum, state, false);
        }

        //!!HERE V split this into 2 functions
        public void OptionInputActionCallback(ControllerInputOptions controllerInputOption, InputActionPhase phase)
        {

            //Radial Menu options
            int i;
            if ((i = Array.IndexOf(radialInputOptions, controllerInputOption)) >= 0)
            {
                if (GetCurrentState() == OptionState.Unselected)
                {
                    //default option
                    defaultOptionSettings.defaultOptionProporties.optionCallbackEvent.Invoke(currentOption, controllerInputOption, phase);
                    return;
                }

                optionMenuProportiesList[i].optionCallbackEvent.Invoke(currentOption, controllerInputOption, phase);
                return;
            }
            Debug.LogWarning($"{controllerInputOption} not implemented");
        }

        public void StaticInputActionCallback(ControllerInputOptions controllerInputOption)
        {
            //Static options
            foreach (var staticOption in staticInputOptions)
            {
                if (staticOption.inputOption == controllerInputOption)
                {
                    staticOption.optionCallbackEvent.Invoke(currentOption, controllerInputOption);
                    return;
                }
            }

            Debug.LogWarning($"{controllerInputOption} not implemented");
        }


        public void Enable()
        {
            this.gameObject.SetActive(true);

            if (FadeMenuCoroutine != null) StopCoroutine(FadeMenuCoroutine);
            SelectOption(0, OptionState.Unselected, false);
        }
        public void Disable()
        {
            SelectOption(0, OptionState.Hidden, false);

            if (FadeMenuCoroutine != null) StopCoroutine(FadeMenuCoroutine);
            FadeMenuCoroutine = StartCoroutine(WaitAndDisable());
        }
        public void DisableImmediate()
        {
            SelectOption(0, OptionState.Hidden, true);
            this.gameObject.SetActive(false);
        }
        private IEnumerator WaitAndDisable()
        {
            yield return new WaitForSeconds(menuSettings.selectionAnimationDurration);

            this.gameObject.SetActive(false);
        }

        #endregion
    }




    #region UI Customization

    [CustomEditor(typeof(RadialMenu))]
    public class MenuEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RadialMenu menu = (RadialMenu)target;

            if (GUILayout.Button("Generate Wheel"))
            {
                menu.GenerateRadialMenu();
            }

            if (GUILayout.Button("Select Option"))
            {
                //Debug.LogError("click");

                int optionToChange = (int)(menu.numberOfOptions * UnityEngine.Random.value);
                OptionState state = (OptionState)((int)OptionState.Length * UnityEngine.Random.value);
                if (UnityEngine.Random.value > 0.3)
                {
                    state = OptionState.Selected;
                }
                menu.SelectOption(optionToChange, state, false);
                //}
            }

            if (GUILayout.Button("Clear Wheel"))
            {
                menu._ResetRadialMenu();
            }
        }
    }



    #endregion


}