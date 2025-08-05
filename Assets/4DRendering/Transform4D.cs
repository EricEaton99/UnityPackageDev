using BetterTyping;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Transform4D : MonoBehaviour
{
    public float positionW = 0f;
    public float rotationW = 0f;
    public float scaleW = 1f;


    public void SetPosition4D(Vector4 position4D)
    {
        transform.position = GetVector4XYZ(position4D);
        positionW = position4D.w;
    }
    public Vector4 GetPosition4D()
    {
        return MakeVector4XYZW(transform.position, positionW);
    }

    public void SetRotation4D(Vector4 rotation4D)
    {
        transform.rotation = Quaternion.Euler(GetVector4XYZ(rotation4D));
        rotationW = rotation4D.w;
    }
    public Vector4 GetRotation4D()
    {
        return MakeVector4XYZW(transform.rotation.eulerAngles, rotationW);
    }

    public GameObject GetPlaneAtW(float animW)
    {
        Vector4 r = GetRotation4D();
        Vector4 p = GetPosition4D();
        Vector3 r3 = new Vector3(r.x, r.y, r.z);
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
        else
        {
            Vector3 c_3D = p3 + (r.w * (animW - p.w) / MagSqrNormal_3D) * r3;
            GameObject plane = new GameObject();
            plane.transform.rotation = Quaternion.Euler(r3);
            plane.transform.position = c_3D;
            return plane;
        }
    }


    public static Vector3 GetVector4XYZ(Vector4 v4)
    {
        return new Vector3(v4.x, v4.y, v4.z);
    }
    public static Vector4 MakeVector4XYZW(Vector3 v3, float w)
    {
        return new Vector4(v3.x, v3.y, v3.z, w);
    }
}
