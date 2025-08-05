using EzySlice;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Mesh4DSliceGenerator
{
    struct SliceData
    {
        public Transform4D transform4D;
        public GameObject animatedModelPrefab;
        public int numSlices;

        public SliceData(Transform4D transform4D, GameObject animatedModelPrefab, int numSlices)
        {
            this.transform4D = transform4D;
            this.animatedModelPrefab = animatedModelPrefab;
            this.numSlices = numSlices;
        }
    }


    public static List<List<Vector3>> GetObjectVerts(GameObject animatedModelPrefab, Transform4D object4DTransform, int numSlices, float sliceStart, float sliceEnd)
    {
        List<List<Vector3>> slices = new List<List<Vector3>>();

        float step = (sliceEnd - sliceStart) / (numSlices - 1);

        for (int i = 0; i < numSlices; i++)
        {
            float sliceProgress = sliceStart + i * step;

            Mesh mesh = MeshExtractionTools.GetMeshAtFrame(animatedModelPrefab, sliceProgress);


            List<Vector3> sliceVerts = MeshExtractionTools.GetSliceVerts(mesh, object4DTransform, sliceProgress);
            if (sliceVerts == null)
            {
                int finalNumSlices = 4;
                if (numSlices == finalNumSlices || i <= 0) break;
                List<List<Vector3>> finalSlices = GetObjectVerts(animatedModelPrefab, object4DTransform, finalNumSlices, sliceProgress - step * (finalNumSlices - 1) / finalNumSlices, sliceProgress);
                slices.AddRange(finalSlices);
                break;
            }

            //RemoveInsignificantVerts(sliceVerts);

            MeshTools.OffsetVerts(sliceVerts, object4DTransform.transform.up * sliceProgress);
            //slice.GetComponent<Transform>().localPosition = new Vector3(0, sliceProgress, 0);
            slices.Add(sliceVerts);
        }
        return slices;
    }

    public static Mesh Get3DPartOfObject(GameObject animatedModelPrefab, Transform4D transform4D, int numSlices)
    {
        SliceData sliceData = new SliceData(transform4D, animatedModelPrefab, numSlices);


        //make object

        List<List<Vector3>> verts = GetObjectVerts(animatedModelPrefab, transform4D, numSlices, 0f, 1f);

        Debug.Log($"number of rings = {verts.Count}");

        //NOTE numRings may not equal numSlices!
        int numRings = verts.Count;
        if (numRings < 2) return null;


        //make array of each ring's starting index and find the final index
        int[] ringStarts = new int[numRings];
        int ringEnd;
        ringStarts[0] = 0;
        for (int i = 0; i < numRings - 1; i++) ringStarts[i + 1] = verts[i].Count + ringStarts[i];
        ringEnd = ringStarts[numRings - 1] + verts[numRings - 1].Count;

        //convert the verts list<list> to a verts list<array>
        List<Vector3[]> vertsListArray = new List<Vector3[]>();
        foreach (List<Vector3> vertsList in verts) vertsListArray.Add(vertsList.ToArray());

        //convert the verts list<array> to a verts array
        Vector3[] vertsArray = vertsListArray.SelectMany(array => array).ToArray();


        //add ring tris to tris list
        List<int> tris = new List<int>();
        for (int i = 0; i < numRings - 2; i++)
        {
            MeshTools.GetTrisConnectingRings(vertsArray, ringStarts[i], ringStarts[i + 1], ringStarts[i + 2], tris);
        }
        MeshTools.GetTrisConnectingRings(vertsArray, ringStarts[numRings - 2], ringStarts[numRings - 1], ringEnd, tris);

        //add end-cap tris to tris list
        MeshTools.GetRingTriFan(ringStarts[numRings - 1], ringEnd, tris, false);
        //GetRingTriFan(ringStarts[0], ringStarts[1], tris, true);

        //convert tris array to list
        int[] trisArray = tris.ToArray();

        Mesh mesh = new Mesh();
        mesh.vertices = vertsArray;
        mesh.triangles = trisArray;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }
}
