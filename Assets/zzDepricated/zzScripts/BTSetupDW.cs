using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Globalization;
using JetBrains.Annotations;

public class BTSetupDW : MonoBehaviour
{
    [SerializeField] private string tsvFilePath;        //noted on HARD-CODING: '|' is used as deliminator in tsvFilePath
    [SerializeField] private GameObject controllerInputIcon;

        
    #region BT Options

    [SerializeField] private float DaisyWheelRotationOffset = 0.0f;
    [SerializeField] private bool DaisyWheelInvertDirection = false;

    #endregion

    private string[][] DWStringOptions;
    private float[][] zoneDividerAngles;

    IA_radialMenu typingControls;


    #region Cosmetic Assets

    #endregion

    /*
     * WHat all we need:
     *      DW zones and the ability to move them around - options: rotate to pos. hide and show. * bars and pointer
     *      zone buttons and visual feedback for them - options: hide and show
     *      Misc UI elements:
     *          Esc, DW cycle L, DW cycle R
     */

    //Hot-swap the currently selected DW zone



    void Awake()
    {
        SetDWStringOptions();
        SetupDWZones();
        //SetupZoneOptions();
        //SetupMiscUIElems();
        SetupBTInteractions();
    }


    private void SetDWStringOptions()
    {
        DWStringOptions = TsvReader.ReadTsv(tsvFilePath);
    }

    /// <summary>
    /// helper function to standardize the calculation of the number of DW zones
    /// </summary>
    /// <param name="dw"></param>
    /// <returns></returns>
    private int GetNumDWZones(int dw)
    {
        return DWStringOptions[dw].Length - 1;
    }



    #region SetupDWZones
    //Setup the DW zones with 2-10 (default: 8) selections options and 1-4 (default: 4) options in each zone
    //default layout is clockwise
    private void CalculateDWZoneDivisionAngles(int dw)
    {
        int numDWSides = GetNumDWZones(dw);
        zoneDividerAngles[dw] = new float[numDWSides];

        float currentAngle = Mathf.PI / 2 + DaisyWheelRotationOffset;
        float incrementAngle = 2* Mathf.PI / numDWSides;

        string debug = $"Angle set {dw}: ";
        for (int i = 0; i < numDWSides; i++)
        {
            zoneDividerAngles[dw][i] = currentAngle;
            currentAngle += DaisyWheelInvertDirection ? -incrementAngle : incrementAngle;
            if (currentAngle < -Mathf.PI) currentAngle += 2 * Mathf.PI;
            if (currentAngle > Mathf.PI) currentAngle -= 2 * Mathf.PI;

            debug += $"{zoneDividerAngles[dw][i] / Mathf.PI} PI, ";
        }
        Debug.Log(debug);

    }

    /// <summary>
    /// use angles to instantiate DWZoneDivider for each zone
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void MakeZoneDividorFan(int dw)
    {
        int numZones = GetNumDWZones(dw);
        float dividerOffset = Mathf.PI / numZones;

        for (int i = 0; i < numZones; i++)
        {
            
        }
    }

    private void SetupDWZones()
    {
        int numDWs = DWStringOptions.Length;
        zoneDividerAngles = new float[numDWs][];

        for (int dw = 0; dw < numDWs; dw++)
        {
            CalculateDWZoneDivisionAngles(dw);
            //MakeZoneDividorFan(dw);
        }
    }
    #endregion


    #region SetupZoneOptions
    private void SetupZoneOptions()
    {
        throw new NotImplementedException();
    }
    #endregion

    private void SetupMiscUIElems()
    {
        throw new NotImplementedException();
    }

    private void SetupBTInteractions()
    {
        typingControls = new IA_radialMenu();

        //typingControls.typing.L9.started += ctx => BTInteractions.OnUncenterL9();
        //typingControls.typing.L9.performed += ctx => L9position = ctx.ReadValue<Vector2>();
        //typingControls.typing.L9.canceled += ctx => OnRecenterL9();
        //OnRecenterL9();

        //typingControls.typing.A.performed += ctx => OnL9ActionButtonPress(0);
        //typingControls.typing.B.performed += ctx => OnL9ActionButtonPress(1);
        //typingControls.typing.RB.performed += ctx => OnL9ActionButtonPress(2);

        //typingControls.typing.Lclk.started += ctx => OnL9clkButtonPress();
        //timeOfLastLclk = Time.time;

        //typingControls.typing.LB.performed += ctx => OnRBButtonPress();
    }
}
