using EzySlice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshIntersector : MonoBehaviour
{
    Vector3[] meshVerts;
    Tri[] meshTris;


    private void VerifyMesh()
    {
        foreach (var tri in meshTris)
        {
            if (!tri.FullySetup())
            {
                Debug.LogError("Mesh not fully setup");
                //return;
            }
        }
    }

    private Tri[] MakeTris(int[] trisVerts)
    {
        string zzTemp = "";
        foreach(var tri in trisVerts) zzTemp += tri.ToString() + ", ";
        Debug.Log($"make tris: " + zzTemp);
        

        Tri[] tris = new Tri[trisVerts.Length / 3];        //3 verts in tri
        Dictionary<(int, int), List<Tri>> edgeMap = new();

        //add all reis to a list so we know what still needs to be done
        for (int i = 0; i < trisVerts.Length; i += 3)
        {
            Debug.Log($"make tri");
            Tri tri = new Tri(
                new int[] {
                    trisVerts[i],
                    trisVerts[i + 1],
                    trisVerts[i + 2]
                });

            tris[i / 3] = tri;

            //add each edge to an edge dictionary and map the edges to the Tris
            for (int e = 0; e < 3; e++)
            {
                int a = tri.verts[e];
                int b = tri.verts[(e + 1) % 3];
                var edge = (Math.Min(a, b), Math.Max(a, b));

                if (!edgeMap.ContainsKey(edge))
                    edgeMap[edge] = new List<Tri>();
                edgeMap[edge].Add(tri);
            }
        }

        //add adjacency tris
        while (edgeMap.Any())
        {
            var adjTrisEntries = edgeMap.FirstOrDefault();
            List<Tri> adjTris = adjTrisEntries.Value;

            if(adjTris.Count == 2)
            {
                adjTris[0].SetAdjTri(adjTris[1]);
            }
            else if (adjTris.Count != 2)
            {
                Debug.Log($"Bad tris  ({adjTrisEntries.Key}) ");
            }
            edgeMap.Remove(adjTrisEntries.Key);
        }

        return tris;
    }

    public void SetVertsAndTris(Vector3[] verts, int[] tris)
    {
        meshVerts = verts;
        meshTris = MakeTris(tris);

        VerifyMesh();
    }
}





public class Tri
{
    public int[] verts;
    public List<Tri> adjTris = new();

    public Tri(int[] verts)
    {
        this.verts = verts;
    }


    public void SetAdjTri(Tri tri)
    {
        tri.adjTris.Add(tri);
        adjTris.Add(tri);
    }
    public Tri GetAdjTri(int commonVertA, int commonVertB)
    {
        foreach(var tri in adjTris)
        {
            if(VertIsInTri(commonVertA) && VertIsInTri(commonVertB))
            {
                return tri;
            }
        }
        return null;
    }

    public bool FullySetup()
    {
        Debug.Log("adjTris.Count = "+adjTris.Count);
        return adjTris.Count == 3;
    }


    public bool VertIsInTri(int vert)
    {
        return verts.Contains(vert);
    }
}

