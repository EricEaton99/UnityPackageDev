using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class BTInteractions : MonoBehaviour
{
    static IInputActionCollection2 suspendedInputActions;
    IA_radialMenu inputActions;


    private void Awake()
    {
        inputActions = new IA_radialMenu();

        inputActions.typing.LB.started += ctx => ExitBetterTyping();
    }


    public void EnterBetterTyping(IInputActionCollection2 inputActions)
    {
        Debug.Log("Entering BetterTyping");
        suspendedInputActions = inputActions;
        suspendedInputActions.Disable();
        this.inputActions.Enable();
    }

    public void ExitBetterTyping()
    {
        Debug.Log("Exiting BetterTyping");
        suspendedInputActions.Enable();
        inputActions.Disable();
    }
}
