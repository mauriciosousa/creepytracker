using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System;
using System.Linq;

public static class MessageSeparators {
    public const char L1 = '#'; // top level separator -> bodies
    public const char L2 = '/'; // -> body attributes
    public const char L3 = ':'; // -> 3D values
    public const char SET = '=';
}

public class BodyBuffer
{
    private Kinect.Body _originalBody;

    private Dictionary<string, string> _properties;
    private Dictionary<string, Vector3> _propertiesPoints;
    private Dictionary<string, Quaternion> _propertiesRotations;

    private void _setup()
    {
        _properties = new Dictionary<string, string>();
        _propertiesPoints = new Dictionary<string, Vector3>();
        _propertiesRotations = new Dictionary<string, Quaternion>();
    }

    public BodyBuffer(Kinect.Body body)
    {
        _originalBody = body;

        _setup();

        _addProperty(BodyProperties.Uid.ToString(), body.TrackingId.ToString());
        _addProperty(BodyProperties.HandLeftC.ToString(), body.HandLeftConfidence.ToString());
        _addProperty(BodyProperties.HandLeftS.ToString(), body.HandLeftState.ToString());
        _addProperty(BodyProperties.HandRightC.ToString(), body.HandRightConfidence.ToString());
        _addProperty(BodyProperties.HandRightS.ToString(), body.HandRightState.ToString());

        _addProperty(BodyPropertiesPoints.HeadP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.Head].Position));
        _addProperty(BodyPropertiesPoints.NeckP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.Neck].Position));

        _addProperty(BodyPropertiesPoints.SpineShoulderP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.SpineShoulder].Position));
        _addProperty(BodyPropertiesPoints.SpineMidP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.SpineMid].Position));
        _addProperty(BodyPropertiesPoints.SpineBaseP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.SpineBase].Position));

        _addProperty(BodyPropertiesPoints.ShoulderLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.ShoulderLeft].Position));
        _addProperty(BodyPropertiesPoints.ElbowLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.ElbowLeft].Position));
        _addProperty(BodyPropertiesPoints.WristLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.WristLeft].Position));
        _addProperty(BodyPropertiesPoints.HandLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.HandLeft].Position));
        _addProperty(BodyPropertiesPoints.ThumbLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.ThumbLeft].Position));
        _addProperty(BodyPropertiesPoints.HandTipLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.HandTipLeft].Position));

        _addProperty(BodyPropertiesPoints.ShoulderRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.ShoulderRight].Position));
        _addProperty(BodyPropertiesPoints.ElbowRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.ElbowRight].Position));
        _addProperty(BodyPropertiesPoints.WristRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.WristRight].Position));
        _addProperty(BodyPropertiesPoints.HandRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.HandRight].Position));
        _addProperty(BodyPropertiesPoints.ThumbRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.ThumbRight].Position));
        _addProperty(BodyPropertiesPoints.HandTipRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.HandTipRight].Position));

        _addProperty(BodyPropertiesPoints.HipLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.HipLeft].Position));
        _addProperty(BodyPropertiesPoints.KneeLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.KneeLeft].Position));
        _addProperty(BodyPropertiesPoints.AnkleLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.AnkleLeft].Position));
        _addProperty(BodyPropertiesPoints.FootLeftP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.FootLeft].Position));

        _addProperty(BodyPropertiesPoints.HipRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.HipRight].Position));
        _addProperty(BodyPropertiesPoints.KneeRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.KneeRight].Position));
        _addProperty(BodyPropertiesPoints.AnkleRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.AnkleRight].Position));
        _addProperty(BodyPropertiesPoints.FootRightP.ToString(), CommonUtils._convertToVector3(body.Joints[Kinect.JointType.FootRight].Position));

    }

    public BodyBuffer(string body)
    {
        _setup();
        List<string> bodyAttributes = new List<string>(body.Split(MessageSeparators.L2));
        foreach (string attr in bodyAttributes)
        {
            string[] statement = attr.Split(MessageSeparators.SET);
            if (statement.Length == 2)
            {
                if (CommonUtils.EnumContains((List<string>)Enum.GetNames(typeof(BodyProperties)).ToList(), statement[0]))
                {
                    _addProperty(statement[0], statement[1]);
                }
                else if (CommonUtils.EnumContains((List<string>)Enum.GetNames(typeof(BodyPropertiesPoints)).ToList(), statement[0]))
                {
                    _addProperty(statement[0], (Vector3)CommonUtils.convertRpcStringToVector3(statement[1]));
                }
                else if (CommonUtils.EnumContains((List<string>)Enum.GetNames(typeof(BodyPropertiesOrientations)).ToList(), statement[0]))
                {
                    _addProperty(statement[0], (Quaternion)CommonUtils.convertRpcStringToQuaternion(statement[1]));
                }
            }
        }
    }

    private void _addProperty(string key, string value)
    {
        if (!_properties.ContainsKey(key))
        {
            _properties[key] = value;
        }
    }

    private void _addProperty(string key, Vector3 value)
    {
        if (!_propertiesPoints.ContainsKey(key))
        {
            _propertiesPoints[key] = value;
        }
    }

    private void _addProperty(string key, Quaternion value)
    {
        if (!_propertiesRotations.ContainsKey(key))
        {
            _propertiesRotations[key] = value;
        }
    }

    public string queryProperty(string key)
    {
        return _properties[key];
    }

    public Vector3 queryPropertyPoint(string key)
    {
        return _propertiesPoints[key];
    }

    public Quaternion queryPropertyOrientations(string key)
    {
        return _propertiesRotations[key];
    }

}

public class BodiesMessage {

    private string _message;

    private List<BodyBuffer> _bodiesb;

    private string _kinectId;

    private int _numberOfBodies;

    public BodiesMessage(string kinectId, List<Kinect.Body> bodies)
    {
        _message = "";
        _kinectId = kinectId;

        Bodiesb = new List<BodyBuffer>();
        foreach (Kinect.Body body in bodies)
        {
            Bodiesb.Add(new BodyBuffer(body));
        }
        _numberOfBodies = bodies.Count;
        _composeMessage();
        
    }

    public BodiesMessage(string bodiesMessage)
    {
        _message = bodiesMessage;
        Bodiesb = new List<BodyBuffer>();

        List<string> bodies = new List<string>(bodiesMessage.Split(MessageSeparators.L1));
        _kinectId = bodies[0];
        bodies.RemoveAt(0);
        
        if (bodies.Count == 1 && bodies[0] == "None")
        {
            _numberOfBodies = 0;
        }
        else
        {
            _numberOfBodies = bodies.Count;
            foreach (string body in bodies)
            {
                Bodiesb.Add(new BodyBuffer(body));
            }
        }
    }

    private void _composeMessage()
    {
        _message += _kinectId + MessageSeparators.L1;

        if (Bodiesb.Count > 0)
        {
            foreach (BodyBuffer b in Bodiesb)
            {
                foreach (var v in Enum.GetValues(typeof(BodyProperties)))
                {
                    _message += v.ToString() + MessageSeparators.SET + b.queryProperty(v.ToString()) + MessageSeparators.L2;
                }
                foreach (var v in Enum.GetValues(typeof(BodyPropertiesPoints)))
                {
                    _message += v.ToString() + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(b.queryPropertyPoint(v.ToString())) + MessageSeparators.L2;
                }

                if (Bodiesb.IndexOf(b) < Bodiesb.Count - 1) _message += MessageSeparators.L1;
            }
        }
        else
        {
            _message += "None";
        }
    }

    public string KinectId
    {
        get
        {
            return _kinectId;
        }

        set
        {
            _kinectId = value;
        }
    }

    public string Message
    {
        get
        {
            return _message;
        }

        set
        {
            _message = value;
        }
    }

    public int NumberOfBodies
    {
        get
        {
            return _numberOfBodies;
        }

        set
        {
            _numberOfBodies = value;
        }
    }

    public List<BodyBuffer> Bodiesb
    {
        get
        {
            return _bodiesb;
        }

        set
        {
            _bodiesb = value;
        }
    }
}
