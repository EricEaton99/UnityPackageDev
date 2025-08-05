using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using EzySlice;
using static UnityEngine.GraphicsBuffer;
using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class ObjectGenerator4D : MonoBehaviour
{

    #region variables

    [SerializeField] Transform slicePlaneDebug;
    [SerializeField] GameObject animatedModelPrefab;
    [SerializeField] GameObject zzTempGarbageModelPrefab;
    [SerializeField][Range(0f, 0.3f)] float angularReductionThreshold, distanceReductionThreshold;
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

    public void ResetObject()
    {
        ClearSliceInstances();
        zzTempClearBallInstances();
    }

    public void MakeObject()
    {
        DrawObjectAsVerts();

        GameObject instance = MakeObject(animatedModelPrefab, slicePlaneDebug, numSlices);
        sliceInstances.Add(instance);
    }

    private void Awake()
    {
        slicePlanePrevPosition = slicePlaneDebug.localPosition;
        slicePlanePrevRotation = slicePlaneDebug.rotation;

        ResetObject();
        MakeObject();
    }

    private void Update()
    {
        if (slicePlaneDebug.localPosition != slicePlanePrevPosition || 
            slicePlaneDebug.rotation != slicePlanePrevRotation)
        {
            ResetObject();
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
        Debug.Log("_InspectorButtonAction");


        ResetObject();

        Transform4D tf4D = GetComponent<Transform4D>();
        Mesh mesh = Mesh4DSliceGenerator.Get3DSliceOf4DObject(animatedModelPrefab, tf4D, numSlices);

        GameObject instance = Instantiate(zzTempGarbageModelPrefab);
        //ring.GetComponent<MeshFilter>().sharedMesh.Clear();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        instance.GetComponent<MeshFilter>().sharedMesh = mesh;
        sliceInstances.Add(instance);


        //ResetObject();
        //DrawObjectAsVerts();
        //MakeObject();

    }


    #endregion

    #region visualization


    private void DrawObjectAsVerts()
    {
        List<List<Vector3>> verts = GetObjectVerts(animatedModelPrefab, slicePlaneDebug, numSlices);
        //sliceInstances = GetSlices(animatedModelPrefab, slicePlaneDebug, numSlices);

        foreach (var ring in verts)
        {
            zzTempPutBallsAtVerts(ring);
        }
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

    private GameObject MakePlaneAtFrame(Vector4 eulerRotation, Vector4 position, float animW)
    {
        Vector4 r = eulerRotation;
        Vector4 p = position;
        Vector3 r3 = new Vector3(r.x,r.y,r.z);
        Vector3 p3 = new Vector3(p.x, p.y, p.z);

        float MagSqrNormal_3D = r.x * r.x + r.y * r.y + r.z * r.z;

        if (r.x * r.x + r.y * r.y + r.z * r.z == 0)
        {
            if (animW == p.w)
            {
                //the entire object is the object at animW
                return null;

            }
            else
            {
                //there is no intersection
                return null;
            }
        }
        else {
            Vector3 c_3D = p3  +  (r.w * (animW - p.w) / MagSqrNormal_3D) * r3;
            GameObject plane = new GameObject();
            plane.transform.rotation = Quaternion.Euler(r3);
            plane.transform.position = c_3D;
            return plane;
        }
    }


    private void RemoveInsignificantVerts(List<Vector3> verts)
    {
        //perform a traversal of the list and remove all verts that have an angle of less than threshold

        int i = 1;

        while(i < verts.Count - 1)
        {
            if (Vector3.Distance(verts[i - 1], verts[i]) < distanceReductionThreshold)
            {
                verts.RemoveAt(i);
                continue;
            }

            //Vector3 entryDirection = verts[i - 1] - verts[i];
            //Vector3 exitDirection = verts[i] - verts[i + 1];
            //if (Vector3.Dot(entryDirection.normalized, exitDirection.normalized) < 1 - angularReductionThreshold)
            //{
            //    verts.RemoveAt(i);
            //    continue;
            //}
            i++;
        }
    }

    private GameObject MakeObject(GameObject animatedModelPrefab, Transform slicePlane, int numSlices)
    {
        List<List<Vector3>> verts = GetObjectVerts(animatedModelPrefab, slicePlane, numSlices); 

        Debug.Log($"number of rings = {verts.Count}");

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
            MeshTools.GetTrisConnectingRings(vertsArray, ringStarts[i], ringStarts[i + 1], ringStarts[i + 2], tris);
        }
        MeshTools.GetTrisConnectingRings(vertsArray, ringStarts[numRings-2], ringStarts[numRings-1], ringEnd, tris);

        //add end-cap tris to tris list
        MeshTools.GetRingTriFan(ringStarts[numRings - 1], ringEnd, tris, false);
        //GetRingTriFan(ringStarts[0], ringStarts[1], tris, true);

        //convert tris array to list
        int[] trisArray = tris.ToArray();

        GameObject ring = Instantiate(zzTempGarbageModelPrefab);
        //ring.GetComponent<MeshFilter>().sharedMesh.Clear();
        ring.GetComponent<MeshFilter>().sharedMesh.vertices = vertsArray;
        ring.GetComponent<MeshFilter>().sharedMesh.triangles = trisArray;
        ring.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
        ring.GetComponent<MeshFilter>().sharedMesh.RecalculateTangents();

        return ring;
    }

    #endregion

    #region slices and verts


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

        return upperHull;
    }

    public List<Vector3> GetSliceVerts(Mesh sharedMesh, Transform slicePlane)
    {
        GameObject slice = GetObjectSlice(sharedMesh, slicePlane);

        if (slice == null)
        {
            DestroyImmediate(slice);
            return null;
        }

        Mesh mesh = slice.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(slice);

        if (mesh == null) return null;
        return GetVertsRing(mesh, mesh.triangles);
    }
    public List<List<Vector3>> GetObjectVerts(GameObject animatedModelPrefab, Transform slicePlane, int numSlices)
    {
        return GetObjectVerts(animatedModelPrefab, slicePlane, numSlices, 0.01f, 0.99f);
    }


    public List<List<Vector3>> GetObjectVerts(GameObject animatedModelPrefab, Transform slicePlane, int numSlices, float sliceStart, float sliceEnd)
    {
        List<List<Vector3>> slices = new List<List<Vector3>>();

        float step = (sliceEnd - sliceStart) / (numSlices - 1);

        for (int i = 0; i < numSlices; i++)
        {
            float sliceProgress = sliceStart + i * step;

            Mesh mesh = MakeMeshAtFrame(animatedModelPrefab, sliceProgress);

            List<Vector3> sliceVerts = GetSliceVerts(mesh, slicePlane);
            if (sliceVerts == null)
            {
                int finalNumSlices = 4;
                if (numSlices == finalNumSlices || i <= 0) break;
                List<List<Vector3>> finalSlices = GetObjectVerts(animatedModelPrefab, slicePlane, finalNumSlices, sliceProgress - step * (finalNumSlices - 1) / finalNumSlices, sliceProgress);
                slices.AddRange(finalSlices);
                break;
            }

            RemoveInsignificantVerts(sliceVerts);

            OffsetVerts(sliceVerts, slicePlane.up * sliceProgress);
            //slice.GetComponent<Transform>().localPosition = new Vector3(0, sliceProgress, 0);
            slices.Add(sliceVerts);
        }
        return slices;
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
            menu.ResetObject();
        }
    }
}