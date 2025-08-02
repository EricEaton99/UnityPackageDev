using BetterTyping;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestInputController : InputController
{
    // Start is called before the first frame update
    IA_TestController testController;
    [SerializeField] BetterTyping.RadialMenuInputController radialMenuControls;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TypingCallbacks typingCallbacks;

    void Awake()
    {
        Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
        testController = new IA_TestController();
        testController.controller.openTypingMenu.performed += ctx => EnterTypingMode();
        testController.Enable();

        if (radialMenuControls == null) Debug.LogError($"{radialMenuControls.name} is not set in inspector");

        inputField.interactable = false;

        SetInputActionsOnAwake();
    }


    protected override void SetInputActionsOnAwake()
    {
        Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
        inputActions = testController;
    }

    void EnterTypingMode()
    {
        Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
        typingCallbacks.SetInputField(inputField);
        radialMenuControls.EnableInputController(this);
    }


    protected override void OnEnableInputController()
    {
        Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
        inputField.interactable = false;
    }
    protected override void OnDisableInputController()
    {
        Debug.Log($"{this.GetType().Name}: {System.Reflection.MethodBase.GetCurrentMethod().Name}()");
        inputField.interactable = true;
        inputField.Select();

    }
}
