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
    public bool showUnityEvents = true;
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
    SerializedProperty showUnityEventsProp;
    SerializedProperty testCallbackProp;
    SerializedProperty resetCallbackProp;

    private void OnEnable()
    {
        showUnityEventsProp = serializedObject.FindProperty("showUnityEvents");
        testCallbackProp = serializedObject.FindProperty("testCallback");
        resetCallbackProp = serializedObject.FindProperty("resetCallback");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Show toggle
        EditorGUILayout.PropertyField(showUnityEventsProp, new GUIContent("Show Unity Events"));

        // Conditional display of UnityEvents
        if (showUnityEventsProp.boolValue)
        {
            EditorGUILayout.PropertyField(testCallbackProp);
            EditorGUILayout.PropertyField(resetCallbackProp);
        }

        serializedObject.ApplyModifiedProperties();

        // Draw buttons
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