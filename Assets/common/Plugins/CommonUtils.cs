using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class CommonUtils
{
    internal const int decimalsRound = 3;

    internal static Vector3 _convertToVector3(Kinect.CameraSpacePoint p)
    {
        return new Vector3(p.X, p.Y, p.Z);
    }

    internal static string convertVectorToStringRPC(Vector3 v)
    {
        return "" + Math.Round(v.x, decimalsRound) + MessageSeparators.L3 + Math.Round(v.y, decimalsRound) + MessageSeparators.L3 + Math.Round(v.z, decimalsRound);
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

    internal static string convertVectorToStringRPC(Kinect.CameraSpacePoint position)
    {
        return convertVectorToStringRPC(new Vector3(position.X, position.Y, position.Z));
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

    internal static GameObject newGameObject(Vector3 v)
    {
        GameObject go = new GameObject();
        go.transform.position = v;
        return go;
    }

    private static int userIDs = 0;
    public static int getNewID()
    {
        return ++userIDs;
    }

    internal static Vector3 pointKinectToUnity(Vector3 p)
    {
        return new Vector3(-p.x, p.y, p.z);
    }

}
