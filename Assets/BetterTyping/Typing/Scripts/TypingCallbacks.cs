using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterTyping
{

    public class TypingCallbacks : MonoBehaviour
    {

        #region variables

        TMP_InputField inputField;
        [SerializeField] BetterTyping.RadialMenu[] aA0_menus;
        [SerializeField] RadialMenuInputController radialMenuInputController;

        const int NUMBER_OF_OPTIONS_IN_TYPING_MENU = 8;

        enum Menus
        {
            LowerCase,
            UpperCase,
            Numbers,
            NumMenus,
        }

        const string SPACE = "space";
        const string BACKSPACE = "backspace";
        const string DELETE = "delete";
        const string ENTER = "enter";
        const string TAB = "tab";
        enum InputFieldActions
        {
            Space,
            Backspace,
            Delete,
            Enter,
            Tab,
            Inaction,
        }

        Dictionary<string, InputFieldActions> inputFieldActionsLookup = new Dictionary<string, InputFieldActions> {
            { SPACE, InputFieldActions.Space },
            { BACKSPACE, InputFieldActions.Backspace },
            { DELETE, InputFieldActions.Delete },
            { ENTER, InputFieldActions.Enter },
            { TAB, InputFieldActions.Tab },
        };

        string[,] alphabet = new string[(int)Menus.NumMenus, NUMBER_OF_OPTIONS_IN_TYPING_MENU + 1]
        { {"abcd","efgh","ijkl","mnop","qrst","uvwx","yz,.","?!@",$"{SPACE}|{BACKSPACE}|{ENTER}" },
        { "ABCD","EFGH","IJKL","MNOP","QRST","UVWX","YZ;:","\"\'&",$"{SPACE}|{BACKSPACE}|{ENTER}"},
        { "1234","5678","90$#  ","+-*/","=%^_","()[]","{}<>","~`\\|",$"{SPACE}|{BACKSPACE}|{ENTER}" } };


        float lastActionTime = 0f;
        float minTimeBetweenActions = 0.2f;
        float carotBlinkRate;
        Coroutine carotBlinkCoroutine;


        float initialDelay = 0.5f;
        float repeatRate = 0.1f;
        Coroutine holdRepeatCoroutine = null;


        Menus menuNumPrevious = Menus.LowerCase,
            menu = Menus.LowerCase;
        int shiftCaps = 0;

        #endregion



        #region input field actions and typing

        public void SetInputField(TMP_InputField inputField)
        {
            this.inputField = inputField;
        }


        private void InputFieldAddCharAtCarot(char c)
        {
            inputField.text = inputField.text.Insert(inputField.caretPosition, c.ToString());
            inputField.caretPosition++;
            Unshift();
        }
        public void CarrotMove(int amnt)
        {
            inputField.caretPosition += amnt;
            inputField.caretBlinkRate = 0;
            inputField.ForceLabelUpdate();
        }
        private void Backspace()
        {
            if (inputField.caretPosition <= 0) return;
            int caretPos = inputField.caretPosition - 1;
            inputField.text = inputField.text.Remove(caretPos, 1);
            inputField.caretPosition = caretPos;
        }
        private void Delete()
        {
            if (inputField.caretPosition >= inputField.text.Length) return;
            inputField.text = inputField.text.Remove(inputField.caretPosition, 1);
        }


        private void InputFieldActionResponse(InputFieldActions action)
        {
            if (holdRepeatCoroutine != null) StopCoroutine(holdRepeatCoroutine);
            switch (action)
            {
                case InputFieldActions.Space:
                    holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(InputFieldAddCharAtCarot, ' '));
                    break;
                case InputFieldActions.Backspace:
                    holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(Backspace));
                    break;
                case InputFieldActions.Delete:
                    holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(Delete));
                    break;
                case InputFieldActions.Enter:
                    holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(InputFieldAddCharAtCarot, '\n'));
                    break;
                case InputFieldActions.Tab:
                    holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(InputFieldAddCharAtCarot, '\t'));
                    break;
                default:
                    Debug.LogWarning($"InputFieldAction: action {action} Not implemented");
                    break;
            }
        }

        #endregion


        #region helper functions

        private int MapInputButtonToIndex(ControllerInputOptions inputButton)
        {
            switch (inputButton)
            {
                case ControllerInputOptions.buttonNorth: return 0;
                case ControllerInputOptions.buttonEast: return 1;
                case ControllerInputOptions.buttonSouth: return 2;
                case ControllerInputOptions.buttonWest: return 3;
                default: return -1;
            }
        }
        private char GetCharToType(int optionNum, ControllerInputOptions inputButton)
        {
            int buttonNumMapping = MapInputButtonToIndex(inputButton);

            if (optionNum < 0 || optionNum >= NUMBER_OF_OPTIONS_IN_TYPING_MENU) return '\0';
            if (buttonNumMapping >= alphabet[(int)menu, optionNum].Length) return '\0';

            return alphabet[(int)menu, optionNum][buttonNumMapping];
        }
        private InputFieldActions GetActionToPerform(ControllerInputOptions inputButton)
        {
            int optionNum = NUMBER_OF_OPTIONS_IN_TYPING_MENU;

            int buttonNumMapping = MapInputButtonToIndex(inputButton);

            if (buttonNumMapping >= alphabet[(int)menu, optionNum].Length)
            {
                Debug.Log($"option string length: {buttonNumMapping} >= {alphabet[(int)menu, optionNum].Length}");
                return InputFieldActions.Inaction;
            }


            string[] actions = alphabet[(int)menu, optionNum].Split('|');

            if (buttonNumMapping >= actions.Length) return InputFieldActions.Inaction;

            string action = actions[buttonNumMapping];

            return inputFieldActionsLookup[action];
        }


        private IEnumerator HoldRepeatCoroutine(Action action)
        {
            action();                // immediate on hold
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                action();            // repeat while held
                yield return new WaitForSeconds(repeatRate);
            }
        }
        private IEnumerator HoldRepeatCoroutine<T>(Action<T> action, T data)
        {
            action(data);                // immediate on hold
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                action(data);            // repeat while held
                yield return new WaitForSeconds(repeatRate);
            }
        }

        #endregion


        #region callbacks


        public void TypingCallback(int optionNum, ControllerInputOptions inputButton, InputActionPhase phase)
        {
            Debug.Log($"phase: {phase}");
            if(holdRepeatCoroutine != null) StopCoroutine(holdRepeatCoroutine);

            if (phase == InputActionPhase.Canceled) return;

            char charToType = GetCharToType(optionNum, inputButton);
            if(charToType == '\0') return;

            holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(InputFieldAddCharAtCarot, charToType));
        }

        public void DefaultTypingCallback(int optionNum, ControllerInputOptions inputButton, InputActionPhase phase)
        {
            if (holdRepeatCoroutine != null) StopCoroutine(holdRepeatCoroutine);

            if (phase == InputActionPhase.Canceled) return;

            InputFieldActions action = GetActionToPerform(inputButton);

            if (action == InputFieldActions.Inaction) return;

            InputFieldActionResponse(action);
            Debug.Log($"action: {action}");
        }

        public void OffhandJoystickCallback(Vector2 position, InputActionPhase state)
        {
            if (holdRepeatCoroutine != null) StopCoroutine(holdRepeatCoroutine);
            if (state == InputActionPhase.Canceled) return;

            int amnt = 0;
            if (position.x > 0) amnt = 1;
            else amnt = -1;

            Debug.Log($"OffhandJoystickCallback {amnt}");

            holdRepeatCoroutine = StartCoroutine(HoldRepeatCoroutine(CarrotMove, amnt));
        }

        public void ShiftCallback()
        {
            Shift();
        }

        public void NumberMenuCallback()
        {
            switch (menu)
            {
                case Menus.LowerCase:
                //passthrough
                case Menus.UpperCase:
                    SetMenu(Menus.Numbers);
                    break;
                case Menus.Numbers:
                    SetMenu(menuNumPrevious);
                    break;
            }
        }

        #endregion


        #region menu

        private void SetMenu(Menus newMenu)
        {
            int menu = (int)this.menu;
            int currentOption = aA0_menus[menu].GetCurrentOption();
            BetterTyping.OptionState currentState = aA0_menus[menu].GetCurrentState();

            aA0_menus[menu].gameObject.SetActive(false);

            menuNumPrevious = this.menu;
            menu = (int)newMenu;
            menu %= aA0_menus.Length;
            this.menu = (Menus)menu;

            aA0_menus[menu].gameObject.SetActive(true);

            radialMenuInputController.ReplaceRadialMenu(aA0_menus[menu]);

            aA0_menus[menu].SelectOption(currentOption, currentState, true);
        }

        private void IncrementMenu()
        {
            SetMenu(menu+1);
        }
        private void DecrementMenu()
        {
            SetMenu(menu - 1);
        }
        private void CapsMenuSwap()
        {
            switch (menu)
            {
                case Menus.LowerCase:
                    IncrementMenu();
                    break;
                case Menus.UpperCase:
                    DecrementMenu();
                    break;
            }
        }


        //If pressed 1 time:
        //    shiftCaps = 1
        //    toggle back after type
        //if pressed 2 times
        //    shiftCaps = 2
        //    don't toggle back after type
        //if pressed 3 times
        //    reset to pressed 0 times
        private void Shift()
        {
            if (menu == Menus.Numbers) return;

            if(shiftCaps == 0)
            {
                shiftCaps = 1;
                CapsMenuSwap();
            }
            else if(shiftCaps == 1)
            {
                shiftCaps = 2;
            }else if(shiftCaps == 2)
            {
                shiftCaps = 0;
                CapsMenuSwap();
            }
        }
        private void Unshift()
        {
            if(menu == Menus.Numbers)
            {
                shiftCaps = 0;
                return;
            }

            if (shiftCaps == 0)
            {
            }
            else if (shiftCaps == 1)
            {
                shiftCaps = 0;
                CapsMenuSwap();
            }
            else if (shiftCaps == 2)
            {
                shiftCaps = 0;
            }
        }

        #endregion
    }

}