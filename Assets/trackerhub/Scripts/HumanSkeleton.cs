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

    private KalmanFilterVector3 headKalman;
    private KalmanFilterVector3 neckKalman;
    private KalmanFilterVector3 spineShoulderKalman;
    private KalmanFilterVector3 spineMidKalman;
    private KalmanFilterVector3 spineBaseKalman;

    private KalmanFilterVector3 leftShoulderKalman;
    private KalmanFilterVector3 leftElbowKalman;
    private KalmanFilterVector3 leftWristKalman;
    private KalmanFilterVector3 leftHandKalman;
    private KalmanFilterVector3 leftThumbKalman;
    private KalmanFilterVector3 leftHandTipKalman;
    private KalmanFilterVector3 leftHipKalman;
    private KalmanFilterVector3 leftKneeKalman;
    private KalmanFilterVector3 leftAnkleKalman;
    private KalmanFilterVector3 leftFootKalman;

    private KalmanFilterVector3 rightShoulderKalman;
    private KalmanFilterVector3 rightElbowKalman;
    private KalmanFilterVector3 rightWristKalman;
    private KalmanFilterVector3 rightHandKalman;
    private KalmanFilterVector3 rightThumbKalman;
    private KalmanFilterVector3 rightHandTipKalman;
    private KalmanFilterVector3 rightHipKalman;
    private KalmanFilterVector3 rightKneeKalman;
    private KalmanFilterVector3 rightAnkleKalman;
    private KalmanFilterVector3 rightFootKalman;

    public Tracker tracker;
    public int ID;

    private bool canSend = false;

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

        headKalman = new KalmanFilterVector3();
        neckKalman = new KalmanFilterVector3();
        spineShoulderKalman = new KalmanFilterVector3();
        spineMidKalman = new KalmanFilterVector3();
        spineBaseKalman = new KalmanFilterVector3();

        leftShoulderKalman = new KalmanFilterVector3();
        leftElbowKalman = new KalmanFilterVector3();
        leftWristKalman = new KalmanFilterVector3();
        leftHandKalman = new KalmanFilterVector3();
        leftThumbKalman = new KalmanFilterVector3();
        leftHandTipKalman = new KalmanFilterVector3();
        leftHipKalman = new KalmanFilterVector3();
        leftKneeKalman = new KalmanFilterVector3();
        leftAnkleKalman = new KalmanFilterVector3();
        leftFootKalman = new KalmanFilterVector3();

        rightShoulderKalman = new KalmanFilterVector3();
        rightElbowKalman = new KalmanFilterVector3();
        rightWristKalman = new KalmanFilterVector3();
        rightHandKalman = new KalmanFilterVector3();
        rightThumbKalman = new KalmanFilterVector3();
        rightHandTipKalman = new KalmanFilterVector3();
        rightHipKalman = new KalmanFilterVector3();
        rightKneeKalman = new KalmanFilterVector3();
        rightAnkleKalman = new KalmanFilterVector3();
        rightFootKalman = new KalmanFilterVector3();

        canSend = true;
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
            headKalman.Value = tracker.getJointPosition(ID, JointType.Head);
            neckKalman.Value = tracker.getJointPosition(ID, JointType.Neck);
            spineShoulderKalman.Value = tracker.getJointPosition(ID, JointType.SpineShoulder);
            spineMidKalman.Value = tracker.getJointPosition(ID, JointType.SpineMid);
            spineBaseKalman.Value = tracker.getJointPosition(ID, JointType.SpineBase);

            leftShoulderKalman.Value = tracker.getJointPosition(ID, JointType.ShoulderLeft);
            leftElbowKalman.Value = tracker.getJointPosition(ID, JointType.ElbowLeft);
            leftWristKalman.Value = tracker.getJointPosition(ID, JointType.WristLeft);
            leftHandKalman.Value = tracker.getJointPosition(ID, JointType.HandLeft);
            leftThumbKalman.Value = tracker.getJointPosition(ID, JointType.ThumbLeft);
            leftHandTipKalman.Value = tracker.getJointPosition(ID, JointType.HandTipLeft);
            leftHipKalman.Value = tracker.getJointPosition(ID, JointType.HipLeft);
            leftKneeKalman.Value = tracker.getJointPosition(ID, JointType.KneeLeft);
            leftAnkleKalman.Value = tracker.getJointPosition(ID, JointType.AnkleLeft);
            leftFootKalman.Value = tracker.getJointPosition(ID, JointType.FootLeft);

            rightShoulderKalman.Value = tracker.getJointPosition(ID, JointType.ShoulderRight);
            rightElbowKalman.Value = tracker.getJointPosition(ID, JointType.ElbowRight);
            rightWristKalman.Value = tracker.getJointPosition(ID, JointType.WristRight);
            rightHandKalman.Value = tracker.getJointPosition(ID, JointType.HandRight);
            rightThumbKalman.Value = tracker.getJointPosition(ID, JointType.ThumbRight);
            rightHandTipKalman.Value = tracker.getJointPosition(ID, JointType.HandTipRight);
            rightHipKalman.Value = tracker.getJointPosition(ID, JointType.HipRight);
            rightKneeKalman.Value = tracker.getJointPosition(ID, JointType.KneeRight);
            rightAnkleKalman.Value = tracker.getJointPosition(ID, JointType.AnkleRight);
            rightFootKalman.Value = tracker.getJointPosition(ID, JointType.FootRight);

            head.transform.position = headKalman.Value;
            leftShoulder.transform.position = leftShoulderKalman.Value;
            rightShoulder.transform.position = rightShoulderKalman.Value;
            leftElbow.transform.position = leftElbowKalman.Value;
            rightElbow.transform.position = rightElbowKalman.Value;
            leftHand.transform.position =  leftHandKalman.Value;
            rightHand.transform.position = rightHandKalman.Value;
            spineMid.transform.position = spineMidKalman.Value;
            leftHip.transform.position = leftHipKalman.Value;
            rightHip.transform.position = rightHipKalman.Value;
            leftKnee.transform.position = leftKneeKalman.Value;
            rightKnee.transform.position = rightKneeKalman.Value;
            leftFoot.transform.position = leftFootKalman.Value;
            rightFoot.transform.position = rightFootKalman.Value;

        }
    }

    internal string getPDU()
    {
        if (canSend)
        {
            string pdu = "id" + MessageSeparators.SET + ID + MessageSeparators.L2;

            pdu += "head" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(headKalman.Value) + MessageSeparators.L2;
            pdu += "neck" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(neckKalman.Value) + MessageSeparators.L2;
            pdu += "spineShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(spineShoulderKalman.Value) + MessageSeparators.L2;
            pdu += "spineMid" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(spineMidKalman.Value) + MessageSeparators.L2;
            pdu += "spineBase" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(spineBaseKalman.Value) + MessageSeparators.L2;

            pdu += "leftShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftShoulderKalman.Value) + MessageSeparators.L2;
            pdu += "leftElbow" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftElbowKalman.Value) + MessageSeparators.L2;
            pdu += "leftWrist" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftWristKalman.Value) + MessageSeparators.L2;
            pdu += "leftHand" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftHandKalman.Value) + MessageSeparators.L2;
            pdu += "leftThumb" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftThumbKalman.Value) + MessageSeparators.L2;
            pdu += "leftHand" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftHandTipKalman.Value) + MessageSeparators.L2;
            pdu += "leftHip" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftHipKalman.Value) + MessageSeparators.L2;
            pdu += "leftKnee" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftKneeKalman.Value) + MessageSeparators.L2;
            pdu += "leftAnkle" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftAnkleKalman.Value) + MessageSeparators.L2;
            pdu += "leftFoot" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(leftFootKalman.Value) + MessageSeparators.L2;

            pdu += "rightShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightShoulderKalman.Value) + MessageSeparators.L2;
            pdu += "rightElbow" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightElbowKalman.Value) + MessageSeparators.L2;
            pdu += "rightWrist" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightWristKalman.Value) + MessageSeparators.L2;
            pdu += "rightHand" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightHandKalman.Value) + MessageSeparators.L2;
            pdu += "rightThumb" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightThumbKalman.Value) + MessageSeparators.L2;
            pdu += "rightHand" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightHandTipKalman.Value) + MessageSeparators.L2;
            pdu += "rightHip" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightHipKalman.Value) + MessageSeparators.L2;
            pdu += "rightKnee" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightKneeKalman.Value) + MessageSeparators.L2;
            pdu += "rightAnkle" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightAnkleKalman.Value) + MessageSeparators.L2;
            pdu += "rightFoot" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(rightFootKalman.Value);

            return pdu;
        }
        else
            throw new Exception("Human not initalized.");
    }
}
