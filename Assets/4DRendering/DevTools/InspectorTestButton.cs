using BetterTyping;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class InspectorTestButton : MonoBehaviour
{
    public UnityEvent testCallback, resetCallback;
    public void TestScript()
    {
        testCallback.Invoke();
    }
    public void ResetScript()
    {
        resetCallback.Invoke();
    }
}



[CustomEditor(typeof(InspectorTestButton))]
public class MenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InspectorTestButton menu = (InspectorTestButton)target;


        if (GUILayout.Button("Test Script"))
        {
            menu.TestScript();
        }

        if (GUILayout.Button("Reset Script"))
        {
            menu.ResetScript();
        }
    }
}