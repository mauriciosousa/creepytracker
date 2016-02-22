﻿using UnityEngine;
using System.Collections;

public class TrackerProperties : MonoBehaviour {

    private static TrackerProperties _singleton;

    public int rpcPort = 57743;
    public int broadcastPort = 53804;

    [Range(0, 1)]
    public float mergeDistance = 0.3f;

    [Range(0, 25)]
    public int confidenceTreshold = 20;

    public Windows.Kinect.JointType centerJoint = Windows.Kinect.JointType.SpineShoulder;
    public Windows.Kinect.JointType upJointA = Windows.Kinect.JointType.SpineBase;
    public Windows.Kinect.JointType upJointB = Windows.Kinect.JointType.SpineShoulder;

    public string configFilename = "configSettings.txt";

    private TrackerProperties() { }

    public static TrackerProperties Instance
    {
        get
        {
            return _singleton;
        }
    }

    void Start()
    {
        _singleton = this;
    }
}