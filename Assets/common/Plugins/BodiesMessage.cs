using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System;
using System.Linq;

public static class MessageSeparators {
	public const char L0 = '$'; // header separator
    public const char L1 = '#'; // top level separator -> bodies
    public const char L2 = '/'; // -> body attributes
    public const char L3 = ':'; // -> 3D values
    public const char SET = '=';
}


public enum BodyPropertiesTypes
{
    UID,
    HandLeftState,
    HandLeftConfidence,
    HandRightState,
    HandRightConfidence,
    Confidence
}

public class Skeleton
{
    public Dictionary<BodyPropertiesTypes, string> bodyProperties;
    public Dictionary<Kinect.JointType, Vector3> jointsPositions;
    public string Message;
    public string ID
    {
        get
        {
            return bodyProperties[BodyPropertiesTypes.UID];
        }
    }

    public string sensorID;


    public void _start()
    {
        jointsPositions = new Dictionary<Windows.Kinect.JointType, Vector3>();
        bodyProperties = new Dictionary<BodyPropertiesTypes, string>();
    }

    public Skeleton(Kinect.Body body)
    {
        _start();
        Message = ""
            + BodyPropertiesTypes.UID.ToString() + MessageSeparators.SET + body.TrackingId
            + MessageSeparators.L2 + BodyPropertiesTypes.Confidence.ToString() + MessageSeparators.SET + BodyConfidence(body)
            + MessageSeparators.L2 + BodyPropertiesTypes.HandLeftState.ToString() + MessageSeparators.SET + body.HandLeftState
            + MessageSeparators.L2 + BodyPropertiesTypes.HandLeftConfidence.ToString() + MessageSeparators.SET + body.HandLeftConfidence
            + MessageSeparators.L2 + BodyPropertiesTypes.HandRightState.ToString() + MessageSeparators.SET + body.HandRightState
            + MessageSeparators.L2 + BodyPropertiesTypes.HandRightConfidence.ToString() + MessageSeparators.SET + body.HandRightConfidence;

        foreach (Kinect.JointType j in Enum.GetValues(typeof(Kinect.JointType)))
        {
            Message += "" + MessageSeparators.L2 + j.ToString() + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(body.Joints[j].Position);
        }
    }

    public Skeleton(string body)
    {
        _start();

        Message = body;
        List<string> bodyAttributes = new List<string>(body.Split(MessageSeparators.L2));
        foreach (string attr in bodyAttributes)
        {
            
            string[] statement = attr.Split(MessageSeparators.SET);
            if (statement.Length == 2)
            {
                if (Enum.IsDefined(typeof(BodyPropertiesTypes), statement[0]))
                {
                    bodyProperties[((BodyPropertiesTypes)Enum.Parse(typeof(BodyPropertiesTypes), statement[0]))] = statement[1];
                }

                if (Enum.IsDefined(typeof(Windows.Kinect.JointType), statement[0]))
                {
                    jointsPositions[((Windows.Kinect.JointType)Enum.Parse(typeof(Windows.Kinect.JointType), statement[0]))] = CommonUtils.convertRpcStringToVector3(statement[1]);
                }
            }
        }
    }

    private int BodyConfidence(Kinect.Body body)
    {
        int confidence = 0;

        foreach (Kinect.Joint j in body.Joints.Values)
        {
            if (j.TrackingState == Windows.Kinect.TrackingState.Tracked)
                confidence += 1;
        }

        return confidence;
    }
}

public class BodiesMessageException : Exception
{
    public BodiesMessageException(string message)
        : base(message) { }
}

public class BodiesMessage
{
    public string Message { get; internal set; }
    public string KinectId { get; internal set; }
    
    public List<Skeleton> _bodies;
    public int NumberOfBodies { get { return _bodies.Count; } }
    public List<Skeleton> Bodies { get { return _bodies;  } }

    private void _start()
    {
        _bodies = new List<Skeleton>();
    }

    public BodiesMessage(string bodies)
    {
        _start();
        Message = bodies;

        try
        {
            List<string> pdu = new List<string>(bodies.Split(MessageSeparators.L1));
            KinectId = pdu[0];
            pdu.RemoveAt(0);

            foreach (string b in pdu)
            {
                if (b != "None") _bodies.Add(new Skeleton(b));
            }
        }
        catch (Exception e)
        {
            throw new BodiesMessageException("Cannot instantiate BodiesMessage");
        }
    }

    public BodiesMessage(string kinectId, List<Kinect.Body> listOfBodies)
    {
        _start();
        this.KinectId = kinectId;

        Message = "" + KinectId;
        if (listOfBodies.Count == 0) Message += "" + MessageSeparators.L1 + "None";
        else
        {
            foreach (Kinect.Body b in listOfBodies)
            {
                Skeleton newBody = new Skeleton(b);
                _bodies.Add(newBody);
                Message += "" + MessageSeparators.L1 + newBody.Message;
            }
        }
    }

    public BodiesMessage()
    {
    }
}
