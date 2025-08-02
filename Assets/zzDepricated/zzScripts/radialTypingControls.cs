using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

/// <summary>
/// This makes 4 numLeftScrollwheelsides daisy wheel menus. the names they have to be in the .tsv file are labled in the SetupDaisyWheelOptions function
/// </summary>
public class radialTypingControls : MonoBehaviour
{
    [SerializeField] private GameObject daisyWheel;
    [SerializeField] private GameObject daisyWheelCenter;
    [SerializeField] private string tsvFilePath;        //noted on HARD-CODING: '|' is used as deliminator in tsvFilePath
    Quaternion daisyWheelRotation;

    [SerializeField] private Canvas daisyWheelCanvas;
    private static GameObject[] daisyWheelTextOptions = new GameObject[numLeftScrollwheelsides];
    private static GameObject daisyWheelTextCenter;


    IA_radialMenu typingControls;

    [SerializeField] const int numLeftScrollwheeltypingSets = 4;
    [SerializeField] const int numLeftScrollwheelsides = 9;
    [SerializeField] const int numLeftScrollwheelsideOptions = 3;
    [SerializeField] const float distanceFromCenter = 1.5f;

    int[] typingSetCapsMappings = new int[numLeftScrollwheeltypingSets] { 1, 0, 3, 2 };
    int[] typingSetChangeSetMappings = new int[numLeftScrollwheeltypingSets] { 2, 3, 0, 1 };
    char[,][] daisyWheelActionLookup = new char[numLeftScrollwheeltypingSets, numLeftScrollwheelsides][];    //for the redial options
    string[][] daisyWheelDefaultLookup = new string[numLeftScrollwheeltypingSets][];                 //for the center options

    public int LeftScrollwheeltypingSet
    {
        get { return _LeftScrollwheeltypingSet; }
        set
        {
            _LeftScrollwheeltypingSet = value;
            SetTextForDaisyWheel();
        }
    }
    private int _LeftScrollwheeltypingSet;


    int LeftScrollwheelselection = -1;       //-1 for centered LeftScrollwheel and 0-numLeftScrollwheelsides for specific direction
    int lastLeftScrollwheelselection = -1;
    Vector2 LeftScrollwheelposition = Vector2.zero;
    bool LeftScrollwheelactive = false;
    float timeOfLastLclk;
    const float dblClickTime = 0.4f;
    bool shiftOn = false;


    void Awake()
    {
        Debug.Log("Awake: radialTypingControls");

        typingControls = new IA_radialMenu();
        daisyWheelRotation = daisyWheel.transform.rotation;

        ButtonAndThumbstickInputSetup();
        SetupDaisyWheelTextObjects();

        int errorCode = SetupDaisyWheelOptions();
        if (errorCode < 0) Debug.LogError($"radialTypingControls SetupDaisyWheelOptions failed with error code {errorCode}");

        LeftScrollwheeltypingSet = 0;     //must be after SetupDaisyWheelTextObjects and SetupDaisyWheelOptions
    }

    private void ButtonAndThumbstickInputSetup()
    {
        typingControls.typing.LeftScrollwheel.started += ctx => OnUncenterLeftScrollwheel();
        typingControls.typing.LeftScrollwheel.performed += ctx => LeftScrollwheelposition = ctx.ReadValue<Vector2>();
        typingControls.typing.LeftScrollwheel.canceled += ctx => OnRecenterLeftScrollwheel();
        OnRecenterLeftScrollwheel();

        typingControls.typing.buttonSouth.performed += ctx => OnLeftScrollwheelActionButtonPress(0);
        typingControls.typing.buttonEast.performed += ctx => OnLeftScrollwheelActionButtonPress(1);
        typingControls.typing.RB.performed += ctx => OnLeftScrollwheelActionButtonPress(2);

        typingControls.typing.Lclk.started += ctx => OnLeftScrollwheelclkButtonPress();
        timeOfLastLclk = Time.time;

        typingControls.typing.LB.performed += ctx => OnRBButtonPress();
    }

    private void SetupDaisyWheelTextObjects()
    {
        GameObject textObject;

        for (int i = 0; i < numLeftScrollwheelsides; i++) {
            textObject = new GameObject();
            textObject.AddComponent<TextMeshProUGUI>();
            textObject.GetComponent <TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            textObject.transform.SetParent(daisyWheelCanvas.transform, false);
            textObject.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f, 1);

            float theta = i * (2 * Mathf.PI / numLeftScrollwheelsides);
            textObject.GetComponent<Transform>().position = new Vector3(distanceFromCenter * Mathf.Sin(theta), distanceFromCenter * Mathf.Cos(theta), 0);
            daisyWheelTextOptions[i] = textObject;
        }

        textObject = new GameObject();
        textObject.AddComponent<TextMeshProUGUI>();
        textObject.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        textObject.transform.SetParent(daisyWheelCanvas.transform, false);
        textObject.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f, 1);

        daisyWheelTextCenter = textObject;
    }

    private void SetTextForDaisyWheel()
    {
        for (int i = 0; i < numLeftScrollwheelsides; i++)
        {
            daisyWheelTextOptions[i].GetComponent<TextMeshProUGUI>().text = new string(daisyWheelActionLookup[LeftScrollwheeltypingSet, i]);
        }

        daisyWheelTextCenter.GetComponent<TextMeshProUGUI>().text = string.Join("\n", daisyWheelDefaultLookup[LeftScrollwheeltypingSet]);
    }

    private void OnLeftScrollwheelActionButtonPress(int actionButtonNum)
    {
        //char[typingSet][daisyWheelSide][inputButton]

        if (LeftScrollwheelactive)
        {
            if (actionButtonNum >= daisyWheelActionLookup[LeftScrollwheeltypingSet, LeftScrollwheelselection].Length)
            {
                Debug.Log($"actionButtonNum {actionButtonNum} is out of bounds for typing set {LeftScrollwheeltypingSet} selection {LeftScrollwheelselection}");
                return;
            }
            Debug.Log(daisyWheelActionLookup[LeftScrollwheeltypingSet, LeftScrollwheelselection][actionButtonNum]);
        }
        else
        {
            Debug.Log(daisyWheelDefaultLookup[LeftScrollwheeltypingSet][actionButtonNum]);
        }

        if(shiftOn) LeftScrollwheeltypingSet = typingSetCapsMappings[LeftScrollwheeltypingSet];
        shiftOn = false;
    }

    private void OnLeftScrollwheelclkButtonPress()
    {
        if (Time.time - timeOfLastLclk < dblClickTime)
        {
            shiftOn = false;
        }
        else
        {
            shiftOn = true;
            LeftScrollwheeltypingSet = typingSetCapsMappings[LeftScrollwheeltypingSet];
        }
        timeOfLastLclk = Time.time;



        Debug.Log($"typing set to {LeftScrollwheeltypingSet}: {new string(daisyWheelActionLookup[LeftScrollwheeltypingSet, 0])}, shift {(shiftOn ? "on":"off")}");
    }

    private void OnRBButtonPress()
    {
        LeftScrollwheeltypingSet = typingSetChangeSetMappings[LeftScrollwheeltypingSet];
    }

    private void PrintParsedDaisyWheelLookups()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==== Daisy Wheel Layout ====");

        for (int i = 0; i < numLeftScrollwheeltypingSets; i++)
        {
            sb.AppendLine($"Set {i} (Center Options: {string.Join(", ", daisyWheelDefaultLookup[i])}):");

            for (int j = 0; j < numLeftScrollwheelsides; j++)
            {
                string radialChars = daisyWheelActionLookup[i, j] != null
                    ? new string(daisyWheelActionLookup[i, j])
                    : "[null]";
                sb.AppendLine($"  Side {j + 1}: {radialChars}");
            }

            sb.AppendLine(); // spacing between sets
        }

        Debug.Log(sb.ToString());
    }

    private void PrintLeftScrollwheelselection()
    {
        if (lastLeftScrollwheelselection != LeftScrollwheelselection)
        {
            Debug.Log($"LeftScrollwheeltypingSet {LeftScrollwheeltypingSet}, LeftScrollwheelselection {LeftScrollwheelselection}");
            Debug.Log($"Rotation {LeftScrollwheelselection}: {new string(daisyWheelActionLookup[LeftScrollwheeltypingSet, LeftScrollwheelselection])}");
            lastLeftScrollwheelselection = LeftScrollwheelselection;
        }
    }

    private int SetupDaisyWheelOptions() {
        //okay, time to thimk about how we want to access the data.
        //When a button A/B/RT is pressed, I check which stping set I am in (a/b ') and what panel 0-numLeftScrollwheelsides I am in and
        //based on that, I get my letter 0-2 from the A/B/RT
        //the structure is:     char[typingSet][daisyWheelSide][inputButton]

        //noted on HARD-CODING: '|' is used as deliminator in tsvFilePath

        string[][] LeftScrollwheeltypingSetsStrings = TsvReader.ReadTsv(tsvFilePath);

        if (LeftScrollwheeltypingSetsStrings.Length < numLeftScrollwheeltypingSets)
        {
            Debug.LogError($"radialTypingControls: Mismatch in number of rows in {tsvFilePath} and numLeftScrollwheeltypingSets");
            return -1;
        }

        for (int i = 0; i < numLeftScrollwheeltypingSets; i++)
        {
            if (LeftScrollwheeltypingSetsStrings[i].Length < numLeftScrollwheelsides+1)      //+1 is because the center is the first option
            {
                Debug.LogError($"radialTypingControls: Mismatch in number of tabs in {tsvFilePath} row {i} and numLeftScrollwheelsides");
                return -2;
            }

            //handle the first case differently since it is the 0th case and uses '|' deliminators
            daisyWheelDefaultLookup[i] = LeftScrollwheeltypingSetsStrings[i][0].Split('|');

            for (int j = 1; j < numLeftScrollwheelsides+1; j++)
            {
                daisyWheelActionLookup[i,j-1] = LeftScrollwheeltypingSetsStrings[i][j].ToCharArray();
            }
        }

        //PrintParsedDaisyWheelLookups();

        return 0;
    }

    private void OnUncenterLeftScrollwheel()
    {
        LeftScrollwheelactive = true;
        daisyWheel.SetActive(true);
        daisyWheelCenter.SetActive(false);
    }

    private void OnRecenterLeftScrollwheel()
    {
        LeftScrollwheelselection = -1;
        lastLeftScrollwheelselection = -1;
        LeftScrollwheelactive = false;
        daisyWheel.SetActive(false);
        daisyWheelCenter.SetActive(true);
    }


    void UpdateLeftScrollwheelDaisyWheelPosition()
    {
        float rotation = Mathf.Atan2(LeftScrollwheelposition.y, LeftScrollwheelposition.x);
        //convert from radians to clockwise
        float projectedRotation = -(rotation - Mathf.PI/2) / (2 * Mathf.PI);
        if (projectedRotation < 0) projectedRotation++;
        LeftScrollwheelselection = Mathf.RoundToInt(projectedRotation * numLeftScrollwheelsides);
        if (LeftScrollwheelselection >= numLeftScrollwheelsides) LeftScrollwheelselection = 0;

        float angle = LeftScrollwheelselection * (360 / numLeftScrollwheelsides);
        daisyWheel.transform.rotation = Quaternion.Euler(angle, daisyWheelRotation.eulerAngles.y, daisyWheelRotation.eulerAngles.z);

        PrintLeftScrollwheelselection();
    }

    private void Update()
    {
        if (!LeftScrollwheelactive) return;

        UpdateLeftScrollwheelDaisyWheelPosition();
    }





    private void OnEnable()
    {
        typingControls.Enable();
    }

    private void OnDisable()
    {
        typingControls?.Disable();
    }

}
