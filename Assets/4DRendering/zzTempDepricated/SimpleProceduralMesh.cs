using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SimpleProceduralMesh : MonoBehaviour
{

    void OnEnable()
    {
        MakeQuad();
    }


    public void MakeQuad()
    {

        var mesh = new Mesh
        {
            name = "Procedural Mesh"
        };

        mesh.vertices = new Vector3[] {
            Vector3.zero, Vector3.right, Vector3.up, new Vector3(1f, 1f)
        };

        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };

        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        mesh.RecalculateNormals();
        //mesh.RecalculateTangents();

        foreach(Vector3 normal in mesh.normals)
        {
            Debug.Log(normal.ToString());
        }

        //mesh.RecalculateBounds();

        mesh.uv = new Vector2[] {
            Vector2.zero, Vector2.right, Vector2.up, Vector2.one
        };

        mesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };

        GetComponent<MeshFilter>().mesh = mesh;
    }
}


[CustomEditor(typeof(SimpleProceduralMesh))]
public class SimpleProceduralMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (optional)
        DrawDefaultInspector();

        SimpleProceduralMesh menu = (SimpleProceduralMesh)target;

        if (GUILayout.Button("Generate Quad"))
        {
            menu.MakeQuad();
        }
    }
}