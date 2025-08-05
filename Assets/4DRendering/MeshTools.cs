using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshTools
{



    public static void AddTopTri(int topStart, int bottomStart, List<int> tris)
    {
        tris.Add(topStart);
        tris.Add(bottomStart);
        tris.Add(topStart + 1);
    }
    public static void AddBottomTri(int topStart, int bottomStart, List<int> tris)
    {
        tris.Add(bottomStart);
        tris.Add(bottomStart + 1);
        tris.Add(topStart);
    }
    public static void AddTri(int v1, int v2, int v3, List<int> tris)
    {
        tris.Add(v1);
        tris.Add(v2);
        tris.Add(v3);
    }

    public static void CloseConnectingRing(Vector3[] rings, int r1Start, int r2Start, int r1Index, int r2Index, List<int> tris)
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

    public static void GetTrisConnectingRings(Vector3[] rings, int r1Start, int r2Start, int r2End, List<int> tris)
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
    }

    public static void GetRingTriFan(int ringStart, int ringEnd, List<int> tris, bool flip)
    {
        //assume ring2 verts are sequentially after ring1 verts. so ring1[ring1.Length] == ring2[0]

        for (int i = ringStart + 1; i < ringEnd - 1; i++)
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
    }



    public static void OffsetVerts(List<Vector3> verts, Vector3 offset)
    {
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += offset;
        }
    }


    public static void RemoveInsignificantVerts(List<Vector3> verts, float distanceReductionThreshold, float angularReductionThreshold)
    {

        int i = 1;

        while (i < verts.Count - 1)
        {
            if (Vector3.Distance(verts[i - 1], verts[i]) < distanceReductionThreshold)
            {
                verts.RemoveAt(i);
                continue;
            }

            Vector3 entryDirection = verts[i - 1] - verts[i];
            Vector3 exitDirection = verts[i] - verts[i + 1];
            if (Vector3.Dot(entryDirection.normalized, exitDirection.normalized) > 1 - angularReductionThreshold)
            {
                verts.RemoveAt(i);
                continue;
            }
            i++;
        }
    }


    public static List<Vector3> GetVertsRing(Mesh mesh, int[] sliceTriangles)
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
}
