using UnityEngine;
using System.Collections;
using System;
using Windows.Kinect;

public class HumanSkeleton : MonoBehaviour
{

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

	private AdaptiveDoubleExponentialFilterVector3 headKalman;
	private AdaptiveDoubleExponentialFilterVector3 neckKalman;
	private AdaptiveDoubleExponentialFilterVector3 spineShoulderKalman;
	private AdaptiveDoubleExponentialFilterVector3 spineMidKalman;
	private AdaptiveDoubleExponentialFilterVector3 spineBaseKalman;

	private AdaptiveDoubleExponentialFilterVector3 leftShoulderKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftElbowKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftWristKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftHandKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftThumbKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftHandTipKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftHipKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftKneeKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftAnkleKalman;
	private AdaptiveDoubleExponentialFilterVector3 leftFootKalman;

	private AdaptiveDoubleExponentialFilterVector3 rightShoulderKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightElbowKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightWristKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightHandKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightThumbKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightHandTipKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightHipKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightKneeKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightAnkleKalman;
	private AdaptiveDoubleExponentialFilterVector3 rightFootKalman;

	public Tracker tracker;
	public int ID;

	private bool canSend = false;

	private bool mirror = false;
	private Vector3 lastForward;

	//private GameObject forwardGO;

	private GameObject floorForwardGameObject;

	void Start ()
	{
		CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider> ();
		collider.radius = 0.25f;
		collider.height = 1.75f;

		head = createSphere ("head", 0.3f);
		leftShoulder = createSphere ("leftShoulder");
		rightShoulder = createSphere ("rightShoulder");
		leftElbow = createSphere ("leftElbow");
		rightElbow = createSphere ("rightElbow");
		leftHand = createSphere ("leftHand");
		rightHand = createSphere ("rightHand");
		spineMid = createSphere ("spineMid", 0.2f);
		leftHip = createSphere ("leftHip");
		rightHip = createSphere ("rightHip");
		leftKnee = createSphere ("leftKnee");
		rightKnee = createSphere ("rightKnee");
		leftFoot = createSphere ("leftFoot");
		rightFoot = createSphere ("rightFoot");

		headKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		neckKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		spineShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		spineMidKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		spineBaseKalman = new AdaptiveDoubleExponentialFilterVector3 ();

		leftShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftElbowKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftWristKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftHandKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftThumbKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftHandTipKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftHipKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftKneeKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftAnkleKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		leftFootKalman = new AdaptiveDoubleExponentialFilterVector3 ();

		rightShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightElbowKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightWristKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightHandKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightThumbKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightHandTipKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightHipKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightKneeKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightAnkleKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		rightFootKalman = new AdaptiveDoubleExponentialFilterVector3 ();

		canSend = true;

		lastForward = Vector3.zero;

		//forwardGO = new GameObject();
		//forwardGO.name = "ForwardOld";
		//forwardGO.transform.parent = transform;
		//GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		//cylinder.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
		//cylinder.transform.position += new Vector3(0, 0, 0.25f);
		//cylinder.transform.up = Vector3.forward;
		//cylinder.transform.parent = forwardGO.transform;

		floorForwardGameObject = (GameObject)Instantiate (Resources.Load ("Prefabs/FloorForwardPlane"));
		floorForwardGameObject.name = "Forward";
		floorForwardGameObject.tag = "nocolor";
		floorForwardGameObject.transform.parent = transform;

	}

	private GameObject createSphere (string name, float scale = 0.1f)
	{
		GameObject gameObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		gameObject.GetComponent<SphereCollider> ().enabled = false;
		gameObject.transform.parent = transform;
		gameObject.transform.localScale = new Vector3 (scale, scale, scale);
		gameObject.name = name;
		return gameObject;
	}

	private Vector3 calcUnfilteredForward ()
	{
		Vector3 spineRight = (mirror ? tracker.getJointPosition (ID, JointType.ShoulderLeft) : tracker.getJointPosition (ID, JointType.ShoulderRight)) - tracker.getJointPosition (ID, JointType.SpineShoulder);
		Vector3 spineUp = tracker.getJointPosition (ID, JointType.SpineShoulder) - tracker.getJointPosition (ID, JointType.SpineMid);

		return Vector3.Cross (spineRight, spineUp);
	}

	private Vector3 calcForward ()
	{
		Vector3 spineRight = rightShoulderKalman.Value - spineShoulderKalman.Value;
		Vector3 spineUp = spineShoulderKalman.Value - spineMidKalman.Value;

		return Vector3.Cross (spineRight, spineUp);
	}

	public void updateSkeleton ()
	{
		if (tracker.humanHasBodies (ID)) {
			// Update Forward (mirror or not to mirror?)

			Vector3 forward = calcUnfilteredForward ();

			if (lastForward != Vector3.zero) {
				Vector3 projectedForward = new Vector3 (forward.x, 0, forward.z);
				Vector3 projectedLastForward = new Vector3 (lastForward.x, 0, lastForward.z);

				if (Vector3.Angle (projectedLastForward, projectedForward) > 90)
 {                //if (Vector3.Angle(projectedLastForward, -projectedForward) < Vector3.Angle(projectedLastForward, projectedForward)) // the same as above
					mirror = !mirror;
					forward = calcUnfilteredForward ();
					projectedForward = new Vector3 (forward.x, 0, forward.z);
				}

				// Front for sure?

				Vector3 elbowHand1 = tracker.getJointPosition (ID, JointType.HandRight) - tracker.getJointPosition (ID, JointType.ElbowRight);
				Vector3 elbowHand2 = tracker.getJointPosition (ID, JointType.HandLeft) - tracker.getJointPosition (ID, JointType.ElbowLeft);

				if (Vector3.Angle (elbowHand1, -projectedForward) < 30 || Vector3.Angle (elbowHand2, -projectedForward) < 30) {
					mirror = !mirror;
					forward = calcUnfilteredForward ();
				}
			}

			lastForward = forward;

			// Update Joints

			try {
				headKalman.Value = tracker.getJointPosition (ID, JointType.Head);
				neckKalman.Value = tracker.getJointPosition (ID, JointType.Neck);
				spineShoulderKalman.Value = tracker.getJointPosition (ID, JointType.SpineShoulder);
				spineMidKalman.Value = tracker.getJointPosition (ID, JointType.SpineMid);
				spineBaseKalman.Value = tracker.getJointPosition (ID, JointType.SpineBase);

				if (mirror) {
					rightShoulderKalman.Value = tracker.getJointPosition (ID, JointType.ShoulderLeft);
					rightElbowKalman.Value = tracker.getJointPosition (ID, JointType.ElbowLeft);
					rightWristKalman.Value = tracker.getJointPosition (ID, JointType.WristLeft);
					rightHandKalman.Value = tracker.getJointPosition (ID, JointType.HandLeft);
					rightThumbKalman.Value = tracker.getJointPosition (ID, JointType.ThumbLeft);
					rightHandTipKalman.Value = tracker.getJointPosition (ID, JointType.HandTipLeft);
					rightHipKalman.Value = tracker.getJointPosition (ID, JointType.HipLeft);
					rightKneeKalman.Value = tracker.getJointPosition (ID, JointType.KneeLeft);
					rightAnkleKalman.Value = tracker.getJointPosition (ID, JointType.AnkleLeft);
					rightFootKalman.Value = tracker.getJointPosition (ID, JointType.FootLeft);

					leftShoulderKalman.Value = tracker.getJointPosition (ID, JointType.ShoulderRight);
					leftElbowKalman.Value = tracker.getJointPosition (ID, JointType.ElbowRight);
					leftWristKalman.Value = tracker.getJointPosition (ID, JointType.WristRight);
					leftHandKalman.Value = tracker.getJointPosition (ID, JointType.HandRight);
					leftThumbKalman.Value = tracker.getJointPosition (ID, JointType.ThumbRight);
					leftHandTipKalman.Value = tracker.getJointPosition (ID, JointType.HandTipRight);
					leftHipKalman.Value = tracker.getJointPosition (ID, JointType.HipRight);
					leftKneeKalman.Value = tracker.getJointPosition (ID, JointType.KneeRight);
					leftAnkleKalman.Value = tracker.getJointPosition (ID, JointType.AnkleRight);
					leftFootKalman.Value = tracker.getJointPosition (ID, JointType.FootRight);
				} else {
					leftShoulderKalman.Value = tracker.getJointPosition (ID, JointType.ShoulderLeft);
					leftElbowKalman.Value = tracker.getJointPosition (ID, JointType.ElbowLeft);
					leftWristKalman.Value = tracker.getJointPosition (ID, JointType.WristLeft);
					leftHandKalman.Value = tracker.getJointPosition (ID, JointType.HandLeft);
					leftThumbKalman.Value = tracker.getJointPosition (ID, JointType.ThumbLeft);
					leftHandTipKalman.Value = tracker.getJointPosition (ID, JointType.HandTipLeft);
					leftHipKalman.Value = tracker.getJointPosition (ID, JointType.HipLeft);
					leftKneeKalman.Value = tracker.getJointPosition (ID, JointType.KneeLeft);
					leftAnkleKalman.Value = tracker.getJointPosition (ID, JointType.AnkleLeft);
					leftFootKalman.Value = tracker.getJointPosition (ID, JointType.FootLeft);

					rightShoulderKalman.Value = tracker.getJointPosition (ID, JointType.ShoulderRight);
					rightElbowKalman.Value = tracker.getJointPosition (ID, JointType.ElbowRight);
					rightWristKalman.Value = tracker.getJointPosition (ID, JointType.WristRight);
					rightHandKalman.Value = tracker.getJointPosition (ID, JointType.HandRight);
					rightThumbKalman.Value = tracker.getJointPosition (ID, JointType.ThumbRight);
					rightHandTipKalman.Value = tracker.getJointPosition (ID, JointType.HandTipRight);
					rightHipKalman.Value = tracker.getJointPosition (ID, JointType.HipRight);
					rightKneeKalman.Value = tracker.getJointPosition (ID, JointType.KneeRight);
					rightAnkleKalman.Value = tracker.getJointPosition (ID, JointType.AnkleRight);
					rightFootKalman.Value = tracker.getJointPosition (ID, JointType.FootRight);
				}

				head.transform.position = headKalman.Value;
				leftShoulder.transform.position = leftShoulderKalman.Value;
				rightShoulder.transform.position = rightShoulderKalman.Value;
				leftElbow.transform.position = leftElbowKalman.Value;
				rightElbow.transform.position = rightElbowKalman.Value;
				leftHand.transform.position = leftHandKalman.Value;
				rightHand.transform.position = rightHandKalman.Value;
				spineMid.transform.position = spineMidKalman.Value;
				leftHip.transform.position = leftHipKalman.Value;
				rightHip.transform.position = rightHipKalman.Value;
				leftKnee.transform.position = leftKneeKalman.Value;
				rightKnee.transform.position = rightKneeKalman.Value;
				leftFoot.transform.position = leftFootKalman.Value;
				rightFoot.transform.position = rightFootKalman.Value;

				// update forward

				Vector3 fw = calcForward ();
				Vector3 pos = spineMid.transform.position;

				//forwardGO.transform.forward = fw;
				//forwardGO.transform.position = pos;

				floorForwardGameObject.transform.forward = new Vector3 (fw.x, 0, fw.z);
				floorForwardGameObject.transform.position = new Vector3 (pos.x, 0.001f, pos.z);
				floorForwardGameObject.transform.parent = transform;

			} catch (Exception e) {
				Debug.Log (e.Message + "\n" + e.StackTrace);
			}
		}
	}

	internal string getPDU ()
	{
		if (canSend) {
			string pdu = BodyPropertiesTypes.UID.ToString () + MessageSeparators.SET + ID + MessageSeparators.L2;

			pdu += BodyPropertiesTypes.HandLeftState.ToString () + MessageSeparators.SET + "Null" + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandLeftConfidence.ToString () + MessageSeparators.SET + "Null" + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandRightState.ToString () + MessageSeparators.SET + "Null" + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandRightConfidence.ToString () + MessageSeparators.SET + "Null" + MessageSeparators.L2;

			pdu += "head" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (headKalman.Value) + MessageSeparators.L2;
			pdu += "neck" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (neckKalman.Value) + MessageSeparators.L2;
			pdu += "spineShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (spineShoulderKalman.Value) + MessageSeparators.L2;
			pdu += "spineMid" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (spineMidKalman.Value) + MessageSeparators.L2;
			pdu += "spineBase" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (spineBaseKalman.Value) + MessageSeparators.L2;

			pdu += "leftShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftShoulderKalman.Value) + MessageSeparators.L2;
			pdu += "leftElbow" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftElbowKalman.Value) + MessageSeparators.L2;
			pdu += "leftWrist" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftWristKalman.Value) + MessageSeparators.L2;
			pdu += "leftHand" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftHandKalman.Value) + MessageSeparators.L2;
			pdu += "leftThumb" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftThumbKalman.Value) + MessageSeparators.L2;
			pdu += "leftHandTip" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftHandTipKalman.Value) + MessageSeparators.L2;
			pdu += "leftHip" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftHipKalman.Value) + MessageSeparators.L2;
			pdu += "leftKnee" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftKneeKalman.Value) + MessageSeparators.L2;
			pdu += "leftAnkle" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftAnkleKalman.Value) + MessageSeparators.L2;
			pdu += "leftFoot" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (leftFootKalman.Value) + MessageSeparators.L2;

			pdu += "rightShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightShoulderKalman.Value) + MessageSeparators.L2;
			pdu += "rightElbow" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightElbowKalman.Value) + MessageSeparators.L2;
			pdu += "rightWrist" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightWristKalman.Value) + MessageSeparators.L2;
			pdu += "rightHand" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightHandKalman.Value) + MessageSeparators.L2;
			pdu += "rightThumb" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightThumbKalman.Value) + MessageSeparators.L2;
			pdu += "rightHandTip" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightHandTipKalman.Value) + MessageSeparators.L2;
			pdu += "rightHip" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightHipKalman.Value) + MessageSeparators.L2;
			pdu += "rightKnee" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightKneeKalman.Value) + MessageSeparators.L2;
			pdu += "rightAnkle" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightAnkleKalman.Value) + MessageSeparators.L2;
			pdu += "rightFoot" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (rightFootKalman.Value);

			return pdu;
		} else
			throw new Exception ("Human not initalized.");
	}

	public Vector3 getHead ()
	{
		return headKalman.Value;
	}

}
