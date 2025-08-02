using BetterTyping;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Transform4D : MonoBehaviour
{
    public Vector4 position;
    //public Vector4 rotation;
    //public Vector4 scale;
    [SerializeField] float radiusTop = 1f, radiusBottom = 1f, length = 2f;
    [SerializeField] int numSides = 8;

    public GameObject spherePrefab;

    public void _MakePipe()
    {
        //MakeTri();
        MakePipe(radiusTop, radiusBottom, length, numSides);
    }


    private void MakeTri()
    {


        Vector3[] vertices = new Vector3[3];
        vertices[0] = Vector3.zero;
        vertices[1] = Vector3.up;
        vertices[2] = Vector3.right;


        int[] triangles = new int[3];
        //top tri
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        Vector2[] uv = new Vector2[3];
        uv[0] = Vector2.zero;
        uv[1] = Vector2.up;
        uv[2] = Vector2.right;


        //SetMesh(vertices, triangles, uv);
    }


    void MakePipe(float radiusTop, float radiusBottom, float length, int numSides)
    {
        if (numSides < 3) return;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[numSides * 2];
        int[] triangles = new int[numSides * 6];

        float halfLength = length / 2f;

        // Generate vertices
        for (int i = 0; i < numSides; i++)
        {
            float angle = i * Mathf.PI * 2f / numSides;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i] = new Vector3(sin * radiusTop, halfLength, cos * radiusTop); // top ring
            vertices[i + numSides] = new Vector3(sin * radiusBottom, -halfLength, cos * radiusBottom); // bottom ring
        }

        // Generate triangles (2 per segment)
        for (int i = 0; i < numSides; i++)
        {
            int next = (i + 1) % numSides;

            // Triangle 1
            triangles[i * 6 + 0] = i;
            triangles[i * 6 + 1] = i + numSides;
            triangles[i * 6 + 2] = next;

            // Triangle 2
            triangles[i * 6 + 3] = next;
            triangles[i * 6 + 4] = i + numSides;
            triangles[i * 6 + 5] = next + numSides;
        }

        // Assign mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = GetComponent<MeshFilter>();
        if (!mf) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!mr) gameObject.AddComponent<MeshRenderer>();
    }



    /// <summary>
    /// Used to ensure assignment it done in the correct order
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="triangles"></param>
    private void SetMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        //mesh.uv = uv;
        mesh.triangles = triangles;

        //mesh.normals = new Vector3[] {
        //    Vector3.back
        //};
        mesh.RecalculateNormals();




        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }





    [CustomEditor(typeof(Transform4D))]
    public class Transform4DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector fields (optional)
            DrawDefaultInspector();

            Transform4D menu = (Transform4D)target;

            if (GUILayout.Button("Generate Pipe"))
            {
                menu._MakePipe();
            }
        }
    }


}
