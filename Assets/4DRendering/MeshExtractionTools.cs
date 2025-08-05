using EzySlice;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshExtractionTools : MonoBehaviour
{

    public static GameObject GetObjectSlice(Mesh sharedMesh, Transform slicePlane)
    {
        //TODO: find a better way to do this
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
    public static List<Vector3> GetSliceVerts(Mesh sharedMesh, Transform4D object4DTransform, float sliceProgress)
    {

        GameObject slicePlane = object4DTransform.GetPlaneAtW(sliceProgress);
        GameObject slice = GetObjectSlice(sharedMesh, slicePlane.transform);
        DestroyImmediate(slicePlane);

        if (slice == null)
        {
            DestroyImmediate(slice);
            return null;
        }

        Mesh mesh = slice.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(slice);

        if (mesh == null) return null;
        return MeshTools.GetVertsRing(mesh, mesh.triangles);
    }


    public static Mesh GetMeshAtFrame(GameObject animatedModelPrefab, float timeInSeconds)
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
}
