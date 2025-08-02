using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Unity.VisualScripting;

namespace BetterTyping
{
    public abstract class InputController : MonoBehaviour
    {
        protected InputController suspendedInputController;
        protected IInputActionCollection2 inputActions;


        /// <summary>
        /// Add this to your Awake() function and set the inputActions in it
        /// </summary>
        protected abstract void SetInputActionsOnAwake();

        public void EnableInputController(InputController suspendedInputController)
        {
            if (suspendedInputController == this || suspendedInputController == null)
            {
                Debug.LogError("EnableInputController: suspendedInputController is null or this");
                return;
            }
            if (inputActions == null)
            {
                Debug.LogError("EnableInputController: SetInputActionsOnAwake has not been called or has not initialized inputActions");
                return;
            }

            this.suspendedInputController = suspendedInputController;
            this.suspendedInputController.DisableInputController();
            inputActions.Enable();

            OnEnableInputController();
        }

        public void DisableInputController()
        {
            inputActions.Disable();
            OnDisableInputController();
        }
        public void ReturnInputController()
        {
            if (suspendedInputController != null)
            {
                suspendedInputController.EnableInputController(this);
                suspendedInputController = null;
            }
        }

        protected abstract void OnEnableInputController();
        protected abstract void OnDisableInputController();
    }



    public enum ControllerInputOptions
    {
        buttonNorth, buttonSouth, buttonEast, buttonWest,
        DPadUp, DPadDown, DPadLeft, DPadRight,
        LeftBumper, RightBumper,
        LeftTrigger, RightTrigger,
        LeftJoystickClick, RightJoystickClick,
    }

    public class RadialMenuInputController : InputController
    {

        #region private Variables


        IA_radialMenu radialMenuInputActions;


        Vector2 LeftScrollwheelposition = Vector2.zero; 
        Vector2 RightScrollwheelposition = Vector2.zero; 
        bool LeftScrollwheelactive = false;
        bool RightScrollwheelactive = false;

        [SerializeField] UnityEvent<Vector2, InputActionPhase> offHandJoystickCallbackEvent;

        float timeOfLastLclk;
        const float dblClickTime = 0.4f;

        #endregion


        [SerializeField] BetterTyping.RadialMenu radialMenu;


        void Awake()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");

            if (radialMenu == null) Debug.LogError("RadialMenu must be linked in inspector");

            SetupButtonAndThumbstickInput();

            SetInputActionsOnAwake();
        }
        protected override void SetInputActionsOnAwake()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            inputActions = radialMenuInputActions;
        }

        private void Start()
        {
            //!!TODO!!: this is a bandaid fix. I need to figure out why this isn't initialized in the off position
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            radialMenuInputActions.Disable();
            radialMenu.DisableImmediate();
        }

        #region Input Action Responses
        private void OnUncenterLeftScrollwheel()
        {
            LeftScrollwheelactive = true;
        }
        private void OnRecenterLeftScrollwheel()
        {
            LeftScrollwheelactive = false;
            radialMenu.SelectOption(0, BetterTyping.OptionState.Unselected, false);
        }
        void UpdateLeftScrollwheelDaisyWheelPosition()
        {
            radialMenu.SelectOption(LeftScrollwheelposition, BetterTyping.OptionState.Selected);
        }


        private void OnUncenterRightScrollwheel()
        {
            RightScrollwheelactive = true;
        }
        private void OnRecenterRightScrollwheel()
        {
            RightScrollwheelactive = false;
            radialMenu.SelectOption(0, BetterTyping.OptionState.Unselected, false);
        }
        void UpdateRightScrollwheelDaisyWheelPosition(Vector2 v2, InputActionPhase state)
        {
            if (offHandJoystickCallbackEvent != null) offHandJoystickCallbackEvent.Invoke(v2, state);
        }


        private void OptionActionButtonPress(ControllerInputOptions controllerInputOption, InputActionPhase phase)
        {
            radialMenu.OptionInputActionCallback(controllerInputOption, phase);
        }

        private void StaticActionButtonPress(ControllerInputOptions controllerInputOption)
        {
            radialMenu.StaticInputActionCallback(controllerInputOption);
        }

        #endregion

        struct OptionPressedThings
        {
            public InputAction inputAction;
            public ControllerInputOptions controllerInputOptions;
            //public bool optionPressed;

            public OptionPressedThings(InputAction inputAction, ControllerInputOptions controllerInputOptions)
            {
                this.inputAction = inputAction;
                this.controllerInputOptions = controllerInputOptions;
                //this.optionPressed = optionPressed;
            }
        }

        OptionPressedThings[] optionPressedThings;

        private void OnStartStaticActionButtonPress(int i)
        {
            //optionPressedThings[i].optionPressed = true;
            StaticActionButtonPress(optionPressedThings[i].controllerInputOptions);
        }


        private void SetupButtonAndThumbstickInput()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");

            radialMenuInputActions = new IA_radialMenu();

            radialMenuInputActions.typing.LeftScrollwheel.started += ctx => OnUncenterLeftScrollwheel();
            radialMenuInputActions.typing.LeftScrollwheel.performed += ctx => LeftScrollwheelposition = ctx.ReadValue<Vector2>();
            radialMenuInputActions.typing.LeftScrollwheel.canceled += ctx => OnRecenterLeftScrollwheel();
            OnRecenterLeftScrollwheel();

            radialMenuInputActions.typing.RightScrollwheel.started += ctx => UpdateRightScrollwheelDaisyWheelPosition(ctx.ReadValue<Vector2>(), InputActionPhase.Started); //OnUncenterRightScrollwheel(); 
            //radialMenuInputActions.typing.RightScrollwheel.performed += ctx => RightScrollwheelposition = ctx.ReadValue<Vector2>();
            radialMenuInputActions.typing.RightScrollwheel.canceled += ctx => UpdateRightScrollwheelDaisyWheelPosition(ctx.ReadValue<Vector2>(), InputActionPhase.Canceled); //OnRecenterRightScrollwheel();
            //OnRecenterLeftScrollwheel();

            optionPressedThings = new OptionPressedThings[] {
                new OptionPressedThings(radialMenuInputActions.typing.buttonNorth, ControllerInputOptions.buttonNorth),
                new OptionPressedThings(radialMenuInputActions.typing.buttonSouth, ControllerInputOptions.buttonSouth),
                new OptionPressedThings(radialMenuInputActions.typing.buttonEast, ControllerInputOptions.buttonEast),
                new OptionPressedThings(radialMenuInputActions.typing.buttonWest, ControllerInputOptions.buttonWest),
            };
            foreach (var opt in optionPressedThings)
            {
                var localOpt = opt; // Still necessary to prevent struct capture issues
                localOpt.inputAction.started += ctx => OptionActionButtonPress(localOpt.controllerInputOptions, InputActionPhase.Started);
                //localOpt.inputAction.performed += ctx => OptionActionButtonPress(localOpt.controllerInputOptions, InputActionPhase.Performed);
                localOpt.inputAction.canceled += ctx => OptionActionButtonPress(localOpt.controllerInputOptions, InputActionPhase.Canceled);
            }


            //radialMenuInputActions.typing.buttonNorth.performed += ctx => OptionActionButtonPress(ControllerInputOptions.buttonNorth);
            //radialMenuInputActions.typing.buttonSouth.performed += ctx => OptionActionButtonPress(ControllerInputOptions.buttonSouth);
            //radialMenuInputActions.typing.buttonEast.performed += ctx => OptionActionButtonPress(ControllerInputOptions.buttonEast);
            //radialMenuInputActions.typing.buttonWest.performed += ctx => OptionActionButtonPress(ControllerInputOptions.buttonWest);
            radialMenuInputActions.typing.RB.performed += ctx => StaticActionButtonPress(ControllerInputOptions.RightBumper);
            radialMenuInputActions.typing.RT.performed += ctx => StaticActionButtonPress(ControllerInputOptions.RightTrigger);
            radialMenuInputActions.typing.LB.performed += ctx => StaticActionButtonPress(ControllerInputOptions.LeftBumper);
            radialMenuInputActions.typing.LT.performed += ctx => StaticActionButtonPress(ControllerInputOptions.LeftTrigger);
            radialMenuInputActions.typing.Lclk.performed += ctx => StaticActionButtonPress(ControllerInputOptions.LeftJoystickClick);
            //typingControls.typing.Lclkclk.performed += ctx => OnLeftScrollwheelActionButtonPress(ControllerInputOptions.);

            timeOfLastLclk = Time.time;
        }


        private void Update()
        {
            if (LeftScrollwheelactive) UpdateLeftScrollwheelDaisyWheelPosition();
            //if (RightScrollwheelactive) UpdateRightScrollwheelDaisyWheelPosition();

            //for (int i = 0; i < optionPressedThings.Length; i++)
            //{
            //    //if(optionPressedThings[i].optionPressed) OptionActionButtonPress(optionPressedThings[i].controllerInputOptions);
            //}
        }


        #region Entering and Enabling


        protected override void OnEnableInputController()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            radialMenu.Enable();
        }

        protected override void OnDisableInputController()
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            radialMenu.Disable();
        }

        #endregion


        public void ReplaceRadialMenu(BetterTyping.RadialMenu radialMenu)
        {
            Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
            this.radialMenu = radialMenu;
        }
    }


}