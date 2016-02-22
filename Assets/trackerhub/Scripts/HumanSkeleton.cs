using UnityEngine;
using System.Collections;
using System;
using Windows.Kinect;

public class HumanSkeleton : MonoBehaviour {

    private GameObject head;
    private GameObject leftShoulder;
    private GameObject rightShoulder;
    private GameObject leftElbow;
    private GameObject rightElbow;
    private GameObject leftHand;
    private GameObject rightHand;
    private GameObject spineMid;
    private GameObject leftHip;
    private GameObject rightHip;
    private GameObject leftKnee;
    private GameObject rightKnee;
    private GameObject leftFoot;
    private GameObject rightFoot;

    public Tracker tracker;
    public int ID;

    void Start ()
    {
        head = createSphere("head", 0.3f);
        leftShoulder = createSphere("leftShoulder");
        rightShoulder = createSphere("rightShoulder");
        leftElbow = createSphere("leftElbow");
        rightElbow = createSphere("rightElbow");
        leftHand = createSphere("leftHand");
        rightHand = createSphere("rightHand");
        spineMid = createSphere("spineMid", 0.2f);
        leftHip = createSphere("leftHip");
        rightHip = createSphere("rightHip");
        leftKnee = createSphere("leftKnee");
        rightKnee = createSphere("rightKnee");
        leftFoot = createSphere("leftFoot");
        rightFoot = createSphere("rightFoot");
    }

    private GameObject createSphere(string name, float scale = 0.1f)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObject.transform.parent = transform;
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
        gameObject.name = name;
        return gameObject;
    }

    void Update ()
    {
        updateSkeleton();
    }

    public void updateSkeleton()
    {
        if (tracker.humanHasBodies(ID))
        {
            head.transform.position = tracker.getJointPosition(ID, JointType.Head);
            leftShoulder.transform.position = tracker.getJointPosition(ID, JointType.ShoulderLeft);
            rightShoulder.transform.position = tracker.getJointPosition(ID, JointType.ShoulderRight);
            leftElbow.transform.position = tracker.getJointPosition(ID, JointType.ElbowLeft);
            rightElbow.transform.position = tracker.getJointPosition(ID, JointType.ElbowRight);
            leftHand.transform.position = tracker.getJointPosition(ID, JointType.HandLeft);
            rightHand.transform.position = tracker.getJointPosition(ID, JointType.HandRight);
            spineMid.transform.position = tracker.getJointPosition(ID, JointType.SpineMid);
            leftHip.transform.position = tracker.getJointPosition(ID, JointType.HipLeft);
            rightHip.transform.position = tracker.getJointPosition(ID, JointType.HipRight);
            leftKnee.transform.position = tracker.getJointPosition(ID, JointType.KneeLeft);
            rightKnee.transform.position = tracker.getJointPosition(ID, JointType.KneeRight);
            leftFoot.transform.position = tracker.getJointPosition(ID, JointType.FootLeft);
            rightFoot.transform.position = tracker.getJointPosition(ID, JointType.FootRight);
        }
    }
}
