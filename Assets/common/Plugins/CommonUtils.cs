using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class CommonUtils
{
    internal static bool EnumContains(List<string> list, string v)
    {
        foreach (string s in list)
        {
            if (s == v) return true;
        }
        return false;
    }

    internal static Vector3 _convertToVector3(Kinect.CameraSpacePoint p)
    {
        return new Vector3(p.X, p.Y, p.Z);
    }

    internal static string convertVectorToStringRPC(Vector3 v)
    {
        return "" + v.x + MessageSeparators.L3 + v.y + MessageSeparators.L3 + v.y;
    }

    internal static string convertQuaternionToStringRPC(Quaternion v)
    {
        return "" + v.w + MessageSeparators.L3 + v.x + MessageSeparators.L3 + v.y + MessageSeparators.L3 + v.y;
    }

    internal static Vector3 convertRpcStringToVector3(string v)
    {
        string[] p = v.Split(MessageSeparators.L3);
        return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
    }

    internal static Quaternion convertRpcStringToQuaternion(string v)
    {
        string[] p = v.Split(MessageSeparators.L3);
        return new Quaternion(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[2]), float.Parse(p[0]));
    }

    internal static Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }
}
