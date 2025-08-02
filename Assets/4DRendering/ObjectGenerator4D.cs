using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using EzySlice;
using static UnityEngine.GraphicsBuffer;
using System;
using Unity.VisualScripting;

public class ObjectGenerator4D : MonoBehaviour
{

    #region variables

    [SerializeField] Transform slicePlaneDebug;
    [SerializeField] GameObject animatedModelPrefab;
    [SerializeField] GameObject zzTempGarbageModelPrefab;
    [SerializeField] float timeInSeconds;
    [SerializeField] int numSlices;


    [SerializeField] Material[] zzTempBallMat;
    [SerializeField][HideInInspector] Material zzTempMat;



    [SerializeField] GameObject zzTempBallPrefab;

    List<GameObject> sliceInstances = new List<GameObject>();
    List<GameObject> zzTempVertBalls = new List<GameObject>();

    Vector3 slicePlanePrevPosition;
    Quaternion slicePlanePrevRotation;


    #endregion


    #region runtime

    private void Awake()
    {
        slicePlanePrevPosition = slicePlaneDebug.localPosition;
        slicePlanePrevRotation = slicePlaneDebug.rotation;

        _InspectorButtonReset();
        DrawObjectAsVerts();
        MakeObject();
    }

    private void Update()
    {
        if (slicePlaneDebug.localPosition != slicePlanePrevPosition || 
            slicePlaneDebug.rotation != slicePlanePrevRotation)
        {
            _InspectorButtonReset();
            DrawObjectAsVerts();
            MakeObject();
        }
        slicePlanePrevPosition = slicePlaneDebug.localPosition;
        slicePlanePrevRotation = slicePlaneDebug.rotation;
    }

    #endregion

    #region helper

    private void ClearSliceInstances()
    {
        for (int i = 0; i < sliceInstances.Count; i++)
        {
            DestroyImmediate(sliceInstances[i]);
        }
        sliceInstances.Clear();
    }
    private void zzTempClearBallInstances()
    {
        foreach (var vert in zzTempVertBalls)
        {
            DestroyImmediate(vert);
        }
        zzTempVertBalls.Clear();
    }

    public void OffsetVerts(List<Vector3> verts, Vector3 offset)
    {
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += offset;
        }
    }

    #endregion

    #region inspector

    public void _InspectorButtonAction()
    {
        Debug.Log("_InspectorButtonAction: Slice");

        _InspectorButtonReset();
        //DrawObjectAsVerts();
        MakeObject();
    }

    public void _InspectorButtonReset()
    {

        ClearSliceInstances();
        zzTempClearBallInstances();
    }

    #endregion

    #region visualization

    private void MakeObject()
    {
        GameObject instance = MakeObject(animatedModelPrefab, slicePlaneDebug, numSlices);
        //if (instance == null) return;
        //instance.GetComponent<MeshRenderer>().material = zzTempBallMat[0];
        sliceInstances.Add(instance);
    }

    private void DrawObjectAsVerts()
    {
        List<List<Vector3>> verts = GetObjectVerts(animatedModelPrefab, slicePlaneDebug, numSlices);
        //sliceInstances = GetSlices(animatedModelPrefab, slicePlaneDebug, numSlices);

        foreach (var ring in verts)
        {
            zzTempPutBallsAtVerts(ring);
        }
    }

    private void DrawObjectAsSlices()
    {
        _InspectorButtonReset();
        sliceInstances = GetSlices(animatedModelPrefab, slicePlaneDebug, numSlices);
    }


    private void zzTempPutBallsAtVerts(List<Vector3> verts)
    {
        bool first = true;

        foreach (Vector3 v in verts)
        {
            GameObject ball = Instantiate(zzTempBallPrefab, v, Quaternion.identity);
            ball.GetComponent<MeshRenderer>().material = zzTempBallMat[0];
            if (first) ball.GetComponent<MeshRenderer>().material = zzTempBallMat[1];
            first = false;
            zzTempVertBalls.Add(ball);
        }
        zzTempVertBalls.Last().GetComponent<MeshRenderer>().material = zzTempBallMat[2];

    }

    private void zzTempMakeBallRings(Mesh mesh, int[] sliceTriangles)
    {
        List<Vector3> sliceVerts = GetVertsRing(mesh, sliceTriangles);
        zzTempPutBallsAtVerts(sliceVerts);
    }


    #endregion

    #region Mesh

    /// <summary>
    /// TODO: should have all the meshes generated here instead of in the loop
    /// </summary>
    /// <param name="animatedModelPrefab"></param>
    /// <param name="timeInSeconds"></param>
    /// <returns></returns>
    private Mesh MakeMeshAtFrame(GameObject animatedModelPrefab, float timeInSeconds)
    {
        // 1. Instantiate a temporary copy (not visible, not saved)
        GameObject instance = Instantiate(animatedModelPrefab);
        instance.hideFlags = HideFlags.HideAndDontSave;

        // 2. Access Animator and force sample
        Animator animator = instance.GetComponent<Animator>();
        AnimationClip clip = animator.runtimeAnimatorController.animationClips
                             .FirstOrDefault();     //c => c.name == clipName

        if (clip == null)
        {
            Debug.LogError("Clip not found!");
            DestroyImmediate(instance);
            return null;
        }

        clip.SampleAnimation(instance, timeInSeconds);

        // 3. Bake the skinned mesh
        SkinnedMeshRenderer smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
        Mesh bakedMesh = new Mesh();
        smr.BakeMesh(bakedMesh);

        // 4. Clean up
        DestroyImmediate(instance);
        return bakedMesh;
    }


    private void AddTopTri(int topStart, int bottomStart, List<int> tris)
    {
        tris.Add(topStart);
        tris.Add(bottomStart);
        tris.Add(topStart + 1);
    }
    private void AddBottomTri(int topStart, int bottomStart, List<int> tris)
    {
        tris.Add(bottomStart);
        tris.Add(bottomStart + 1);
        tris.Add(topStart);
    }
    private void AddTri(int v1, int v2, int v3, List<int> tris)
    {
        tris.Add(v1);
        tris.Add(v2);
        tris.Add(v3);
    }

    private void CloseConnectingRing(Vector3[] rings, int r1Start, int r2Start, int r1Index, int r2Index, List<int> tris)
    {
        float topDist = Vector3.Distance(rings[r1Start], rings[r2Index]);
        float bottomDist = Vector3.Distance(rings[r2Start], rings[r1Index]);

        if (topDist < bottomDist)
        {
            AddTri(r1Index, r2Index, r1Start, tris);
            AddTri(r2Index, r2Start, r1Start, tris);
        }
        else
        {
            AddTri(r2Index, r2Start, r1Index, tris);
            AddTri(r1Index, r2Start, r1Start, tris);
        }
    }
    private List<int> GetTrisConnectingRings(Vector3[] rings, int r1Start, int r2Start, int r2End, List<int> tris)
    {
        //assume ring2 verts are sequentially after ring1 verts. so ring1[ring1.Length] == ring2[0]
        int r1Index = r1Start;
        int r2Index = r2Start;

        float topDist, bottomDist;

        while (r1Index < r2Start - 1 && r2Index < r2End - 1)
        {
            topDist = Vector3.Distance(rings[r1Index + 1], rings[r2Index]);
            bottomDist = Vector3.Distance(rings[r2Index + 1], rings[r1Index]);

            if (topDist < bottomDist) AddTopTri(r1Index++, r2Index, tris);
            else AddBottomTri(r1Index, r2Index++, tris);
        }

        //finish tri fan
        while (r1Index < r2Start - 1) AddTopTri(r1Index++, r2Index, tris);
        while (r2Index < r2End - 1) AddBottomTri(r1Index, r2Index++, tris);

        //close remaining gap
        CloseConnectingRing(rings, r1Start, r2Start, r1Index, r2Index, tris);

        return tris;
    }

    private void GetRingTriFan(int ringStart, int ringEnd, List<int> tris, bool flip)
    {
        //assume ring2 verts are sequentially after ring1 verts. so ring1[ring1.Length] == ring2[0]
        
        for (int i = ringStart+1; i < ringEnd-1; i++)
        {
            if (flip)
            {
                AddTri(ringStart, i, i + 1, tris);
            }
            else
            {
                AddTri(i, ringStart, i + 1, tris);
            }
        }
        Debug.Log($"{ringEnd - ringStart} verts, {ringEnd - 2 - (ringStart+1)} tri fan");
    }

    private List<int> GetTrisConnectingRingsSkewed(Vector3[] ring1, Vector3[] ring2, int startIndex)
    {
        Debug.Log($"first indicies are:\n" +
            $"{ring1[0]}, {ring1[1]}, {ring1[2]}, {ring1[3]}\n" +
            $"{ring2[0]}, {ring2[1]}, {ring2[2]}, {ring2[3]}\n" +
            $"");


        List<int> tris = new List<int>();

        int minLen = Mathf.Min(ring1.Length, ring2.Length);
        int maxLen = Mathf.Max(ring1.Length, ring2.Length);

        for (int i = startIndex; i < minLen + startIndex; i++)
        {
            //make 2 tris for each vert
            //assume ring2 verts are sequentially after ring1 verts. so ring1[ring1.Length] == ring2[0]

            //top tri is 
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + ring1.Length);

            //bottom tri is
            tris.Add(i + ring1.Length);
            tris.Add(i + 1);
            tris.Add(i + ring1.Length + 1);

            int j = 6 * (i - startIndex);
            Debug.Log($"tri pair {i}, {i + ring1.Length}:\n" +
                $"{tris[j + 0]}, {tris[j + 1]}, {tris[j + 2]}\n" +
                $"{tris[j + 3]}, {tris[j + 4]}, {tris[j + 5]}\n" +
                $"");
        }

        //for (int i = startIndex; i < minLen + startIndex; i++)
        //{
        //    //make 2 tris for each vert
        //    AddTopTri(i, i + ring1.Length, tris);
        //    AddBottomTri(i + 1, i + ring1.Length, tris);

        //    //int j = 6 * (i - startIndex);
        //    //Debug.Log($"tri pair {i}, {i + ring1.Length}:\n" +
        //    //    $"{tris[j + 0]}, {tris[j + 1]}, {tris[j + 2]}\n" +
        //    //    $"{tris[j + 3]}, {tris[j + 4]}, {tris[j + 5]}\n" +
        //    //    $"");
        //}


        /*
         * from 0-min
         * from min to max but only on the max side
         */
        tris[tris.Count - 1] = 0;

        tris.Add(ring1.Length + ring2.Length - 2);
        tris.Add(0);
        tris.Add(ring1.Length);
        //tris[tris.Count - 2] = ring1.Length;

        return tris;


        //complete the rest as a fan
        if (ring1.Length > ring2.Length)
        {
            for (int i = ring2.Length + startIndex; i < ring1.Length + startIndex; i++)
            {
                //top tri only
                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(ring1.Length);     //all verts in these tris are in a fan connected to ring2[0] since ring1.Len > ring2.Len

                int j = (3 * minLen) + (3 * i);
                Debug.Log($"r1 tri {i}:\n" +
                    $"{tris[j + 0]}, {tris[j + 1]}, {tris[j + 2]}\n");
            }
        }
        else
        {
            for (int i = minLen + startIndex; i < maxLen + startIndex; i++)
            {
                //bottom tri only
                tris.Add(ring1.Length);
                tris.Add(i + 1);
                tris.Add(i + ring1.Length + 1);

                int j = (3 * minLen) + (3 * i);
                Debug.Log($"r2 tri {i}:\n" +
                    $"{tris[j + 0]}, {tris[j + 1]}, {tris[j + 2]}\n");
            }
        }


        return tris;
    }

    private GameObject MakeObject(GameObject animatedModelPrefab, Transform slicePlane, int numSlices)
    {
        List<List<Vector3>> verts = GetObjectVerts(animatedModelPrefab, slicePlane, numSlices); 

        //NOTE numRings may not equal numSlices!
        int numRings = verts.Count;
        if (numRings < 2) return null;

        //make array of each ring's starting index and find the final index
        int[] ringStarts = new int[numRings];
        int ringEnd;
        ringStarts[0] = 0;
        for (int i = 0; i < numRings - 1; i++) ringStarts[i+1] = verts[i].Count + ringStarts[i];
        ringEnd = ringStarts[numRings - 1] + verts[numRings - 1].Count;

        //convert the verts list<list> to a verts list<array>
        List<Vector3[]> vertsListArray = new List<Vector3[]>();
        foreach(List<Vector3> vertsList in verts) vertsListArray.Add(vertsList.ToArray());

        //convert the verts list<array> to a verts array
        Vector3[] vertsArray = vertsListArray.SelectMany(array => array).ToArray();


        //add ring tris to tris list
        List<int> tris = new List<int>();
        for (int i = 0; i < numRings - 2; i++)
        {
            GetTrisConnectingRings(vertsArray, ringStarts[i], ringStarts[i + 1], ringStarts[i + 2], tris);
        }
        GetTrisConnectingRings(vertsArray, ringStarts[numRings-2], ringStarts[numRings-1], ringEnd, tris);

        //add end-cap tris to tris list
        GetRingTriFan(ringStarts[numRings - 1], ringEnd, tris, false);
        GetRingTriFan(ringStarts[0], ringStarts[1], tris, true);

        //convert tris array to list
        int[] trisArray = tris.ToArray();

        GameObject ring = Instantiate(zzTempGarbageModelPrefab);
        //ring.GetComponent<MeshFilter>().sharedMesh.Clear();
        ring.GetComponent<MeshFilter>().sharedMesh.triangles = trisArray;
        ring.GetComponent<MeshFilter>().sharedMesh.vertices = vertsArray;
        ring.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
        ring.GetComponent<MeshFilter>().sharedMesh.RecalculateTangents();

        return ring;
    }

    #endregion

    #region slices and verts

    public List<GameObject> GetSlices(GameObject animatedModelPrefab, Transform slicePlane, int numSlices)
    {
        List<GameObject> slices = new List<GameObject>();


        for (int i = 0; i < numSlices; i++)
        {
            float sliceProgress = i / (float)(numSlices - 0.99);

            Mesh mesh = MakeMeshAtFrame(animatedModelPrefab, sliceProgress);

            GameObject slice = GetObjectSlice(mesh, slicePlane);
            if (slice == null) continue;

            slice.name = $"slice {sliceProgress}";
            slice.GetComponent<Transform>().localPosition = new Vector3(0, sliceProgress, 0);

            slice.GetComponent<MeshRenderer>().material = zzTempMat;

            slices.Add(slice);
        }
        return slices;
    }

    public GameObject GetObjectSlice(Mesh sharedMesh, Transform slicePlane)    //GameObject targetObject
    {
        //TODO: this is probably not right, but hey, we don't want 1-sided disks in the end anyway!
        EzySlice.Plane pl = new EzySlice.Plane(slicePlane.position, slicePlane.up);
        EzySlice.TextureRegion region = new TextureRegion(0, 0, 1, 1);
        int crossIndex = 1;

        SlicedHull hull = Slicer.Slice(sharedMesh, pl, region, crossIndex);         //targetObject.Slice(slicePlane.position, slicePlane.up);

        if (hull == null) return null;

        GameObject upperHull = hull.CreateUpperHull();

        Mesh mesh = upperHull.GetComponent<MeshFilter>().sharedMesh;
        if (mesh.subMeshCount < 2) return null;
        upperHull.GetComponent<MeshFilter>().sharedMesh.triangles = mesh.GetTriangles(1);

        //List<Vector3> sliceVerts = new List<Vector3>();
        //upperHull.GetComponent<MeshFilter>().sharedMesh.GetVertices(sliceVerts);
        //zzTempPutBallsAtVerts(sliceVerts);
        //zzTempMakeBallRings(mesh, upperHull.GetComponent<MeshFilter>().sharedMesh.triangles);

        return upperHull;
    }


    public List<List<Vector3>> GetObjectVerts(GameObject animatedModelPrefab, Transform slicePlane, int numSlices)
    {
        List<List<Vector3>> slices = new List<List<Vector3>>();

        for (int i = 0; i < numSlices; i++)
        {
            float sliceProgress = i / (float)(numSlices - 0.99);

            Mesh mesh = MakeMeshAtFrame(animatedModelPrefab, sliceProgress);

            List<Vector3> sliceVerts = GetSliceVerts(mesh, slicePlane);
            if (sliceVerts == null) continue;

            OffsetVerts(sliceVerts, Vector3.up * sliceProgress);
            //slice.GetComponent<Transform>().localPosition = new Vector3(0, sliceProgress, 0);
            slices.Add(sliceVerts);
        }
        return slices;
    }

    public List<Vector3> GetSliceVerts(Mesh sharedMesh, Transform slicePlane)
    {
        GameObject slice = GetObjectSlice(sharedMesh, slicePlane);
        if (slice == null) return null;
        Mesh mesh = slice.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(slice);
        if (mesh == null) return null;
        return GetVertsRing(mesh, mesh.triangles);
    }
    private List<Vector3> GetVertsRing(Mesh mesh, int[] sliceTriangles)
    {
        List<Vector3> verts = new List<Vector3>();

        List<Vector3> sliceVerts = new List<Vector3>();
        mesh.GetVertices(sliceVerts);
        for (int i = 0; i < sliceTriangles.Length; i += 3)
        {
            verts.Add(sliceVerts[sliceTriangles[i + 1]]);
        }
        return verts;
    }

    #endregion




}




[CustomEditor(typeof(ObjectGenerator4D))]
public class ObjectGenerator4DEditor : Editor
{

    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (optional)
        DrawDefaultInspector();

        ObjectGenerator4D menu = (ObjectGenerator4D)target;

        if (GUILayout.Button("Inspector Action"))
        {
            menu._InspectorButtonAction();
        }
        if (GUILayout.Button("Inspector Reset"))
        {
            menu._InspectorButtonReset();
        }
    }
}