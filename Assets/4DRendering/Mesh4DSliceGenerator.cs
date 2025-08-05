using EzySlice;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Mesh4DSliceGenerator
{
    public struct SliceData
    {
        public Transform4D transform4D;
        public GameObject animatedModelPrefab;
        public int numSlices;
        public int numHighResSlices;

        public SliceData(Transform4D transform4D, GameObject animatedModelPrefab, int numSlices, int numHighResSlices)
        {
            this.transform4D = transform4D;
            this.animatedModelPrefab = animatedModelPrefab;
            this.numSlices = numSlices;
            this.numHighResSlices = numHighResSlices;
        }
    }


    public static List<List<Vector3>> GetObjectVerts(SliceData sliceData, float sliceStart, float sliceEnd, bool increasedDensityArea)
    {
        List<List<Vector3>> slices = new List<List<Vector3>>();

        float step = (sliceEnd - sliceStart) / (sliceData.numSlices - 1);

        for (int i = 0; i < sliceData.numSlices; i++)
        {
            float sliceProgress = sliceStart + i * step;

            Mesh mesh = MeshExtractionTools.GetMeshAtFrame(sliceData.animatedModelPrefab, sliceProgress);


            List<Vector3> sliceVerts = MeshExtractionTools.GetSliceVerts(mesh, sliceData.transform4D, sliceProgress);
            if (sliceVerts == null)
            {
                if(!increasedDensityArea)
                {
                    float highResSliceStart = sliceProgress - step + step / sliceData.numHighResSlices;
                    List<List<Vector3>> highResSlices = GetObjectVerts(sliceData, highResSliceStart, sliceProgress, true);
                    slices.AddRange(highResSlices);
                }
                continue;
            }

            MeshTools.RemoveInsignificantVerts(sliceVerts, 0.05f, 0.05f);

            MeshTools.OffsetVerts(sliceVerts, sliceData.transform4D.transform.up * sliceProgress);
            slices.Add(sliceVerts);
        }
        return slices;
    }

    public static Mesh Get3DSliceOf4DObject(GameObject animatedModelPrefab, Transform4D transform4D, int numSlices)
    {
        SliceData sliceData = new SliceData(transform4D, animatedModelPrefab, numSlices, 4);


        //make object

        List<List<Vector3>> verts = GetObjectVerts(sliceData, 0f, 1f, false);

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


        //add verts for bottom end cap. THIS HAS TO BE BEFORE THE VERTS ARRAY IS MADE BUT MUST BE LEFT OUT OF THE TRI RING CREATION
        vertsListArray.Add(vertsListArray[0]);

        //convert the verts list<array> to a verts array
        Vector3[] vertsArray = vertsListArray.SelectMany(array => array).ToArray();


        //add ring tris to tris list
        List<int> tris = new List<int>();
        for (int i = 0; i < numRings - 2; i++)      //IMPORTANT THAT THIS IS NUM RINGS SINCE WE ADDED AN EXTRA RING OF VERTS AT THE END FOR THE BOTTOM ENDCAP
        {
            MeshTools.GetTrisConnectingRings(vertsArray, ringStarts[i], ringStarts[i + 1], ringStarts[i + 2], tris);
        }
        MeshTools.GetTrisConnectingRings(vertsArray, ringStarts[numRings - 2], ringStarts[numRings - 1], ringEnd, tris);

        //add end-cap tris to tris list
        MeshTools.GetRingTriFan(ringStarts[numRings - 1], ringEnd, tris, false);

        //add bottom end cap
        MeshTools.GetRingTriFan(ringEnd, ringEnd + ringStarts[1], tris, true);

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
