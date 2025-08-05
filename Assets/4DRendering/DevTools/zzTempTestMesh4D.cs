using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zzTempTestMesh4D : MonoBehaviour
{
    [SerializeField] GameObject animatedModelPrefab;
    [SerializeField] int numSlices;

    [SerializeField] GameObject zzTempGarbageModelPrefab;


    List<GameObject> testGameObjectHolder = new List<GameObject>();



    public void TestCallback()
    {
        Debug.Log("TestCallback");


        ResetCallback();

        Transform4D tf4D = GetComponent<Transform4D>();
        Mesh mesh = Mesh4DSliceGenerator.Get3DPartOfObject(animatedModelPrefab, tf4D, numSlices);

        if (mesh == null) return;

        GameObject instance = Instantiate(zzTempGarbageModelPrefab);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        instance.GetComponent<MeshFilter>().sharedMesh = mesh;
        testGameObjectHolder.Add(instance);
    }

    public void ResetCallback()
    {
        ClearTestGameObjectHolder();
    }
    private void ClearTestGameObjectHolder()
    {
        for (int i = 0; i < testGameObjectHolder.Count; i++)
        {
            DestroyImmediate(testGameObjectHolder[i]);
        }
        testGameObjectHolder.Clear();
    }
}
