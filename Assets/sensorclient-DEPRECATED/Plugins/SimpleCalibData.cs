using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class SimpleCalibData {

    private List<Vector3> _spineCalibPoints;
    private List<Vector3> _hipCalipPoints;

    public SimpleCalibData() {
        _spineCalibPoints = new List<Vector3>();
        _hipCalipPoints = new List<Vector3>();
    }

    public void addData(Kinect.Body body)
    {
        _spineCalibPoints.Add(CommonUtils._convertToVector3(body.Joints[Kinect.JointType.SpineMid].Position));
        _hipCalipPoints.Add(CommonUtils._convertToVector3(body.Joints[Kinect.JointType.SpineBase].Position));
    }

    public void calcVectors(Vector3 [] data)
    {
        List<Vector3> A = new List<Vector3>();
        List<Vector3> B = new List<Vector3>();
        
        for (int i = 1; i < data.Length - 1; i++)
        {
            if (Vector3.Distance(data[0], data[i]) <= Vector3.Distance(data[data.Length], data[i]))
                A.Add(data[i]);
            else
                B.Add(data[i]);
        }

        Vector3 centerA = CommonUtils.CenterOfVectors(A.ToArray());
        Vector3 centerB = CommonUtils.CenterOfVectors(B.ToArray());





    }
    

}
