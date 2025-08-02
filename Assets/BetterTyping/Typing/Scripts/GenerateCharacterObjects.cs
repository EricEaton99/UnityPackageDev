using BetterTyping;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GenerateCharacterObjects : MonoBehaviour
{
    [SerializeField] GameObject baseObject;
    [SerializeField] Sprite[] sprites;
    [SerializeField] GameObject spriteObjectPrefab;


    public void GenerateOptions()
    {
        foreach (var sprite in sprites)
        {
            GameObject instance = Instantiate(baseObject, this.transform);

            instance.name = baseObject.name + "_" + sprite.name;

            GameObject spriteObject = Instantiate(spriteObjectPrefab, instance.transform);

            spriteObject.name = sprite.name;

            spriteObject.GetComponent<Image>().sprite = sprite;
            spriteObject.GetComponent<Image>().transform.localScale = spriteObjectPrefab.transform.localScale;
            spriteObject.GetComponent<Image>().transform.position = spriteObjectPrefab.transform.position;
        }
    }
}




#region UI Customization

[CustomEditor(typeof(GenerateCharacterObjects))]
public class generateCharacterObjectsMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GenerateCharacterObjects menu = (GenerateCharacterObjects)target;

        if (GUILayout.Button("Generate Sprite Assets"))
        {
            menu.GenerateOptions();
        }
    }
}



#endregion