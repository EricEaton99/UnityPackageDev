using BetterTyping;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class cubeCallbacks : MonoBehaviour
{

    [SerializeField] private Material refMat;
    [SerializeField] BetterTyping.RadialMenu radialMenu;

    public void SetMaterialByOptionNum(int optionNum, ControllerInputOptions inputButton)
    {

        Color color = Color.magenta;

        switch (inputButton)
        {
            case ControllerInputOptions.buttonSouth:
                color = Color.red;
                break; 
            case ControllerInputOptions.buttonEast:
                color = Color.green;
                break;
            case ControllerInputOptions.RightBumper:
                color = Color.blue;
                break;
            case ControllerInputOptions.RightTrigger:
                color = Color.yellow;
                break;

        }

        color.a = (optionNum + 1) / 4f;

        gameObject.GetComponent<MeshRenderer>().material.color = color;
    }
}
