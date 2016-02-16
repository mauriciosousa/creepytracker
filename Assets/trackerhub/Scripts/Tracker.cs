using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public enum CalibrationProcess
{
    None, 
    FindCenter,
    FindForward
}

public class Sensor
{
    public string SensorID;
    public BodiesMessage Bodies;

    private GameObject _sensorGameObject;

    private bool _active;

    private DateTime _lastUpdate;

    public Vector3 center;
    public Vector3 center2;
    public Vector3 up;
    public Vector3 up2;
    public Vector3 forward;
    public Vector3 right;

    private List<Vector3> _floorValues;

    public bool Active
    {
        get
        {
            return _active;
        }

        set
        {
            _active = value;
            _sensorGameObject.SetActive(_active);
        }
    }

    public Sensor(string sensorID, GameObject sensorGameObject)
    {
        _active = true;
        SensorID = sensorID;
        Bodies = null;
        _sensorGameObject = sensorGameObject;
        _sensorGameObject.name = sensorID;
        center = new Vector3();
        up = new Vector3();
        up2 = new Vector3();
        center2 = new Vector3();
        forward = new Vector3();
        right = new Vector3();
        _floorValues = new List<Vector3>();
    }

    internal void updateBodies(BodiesMessage bodies)
    {
        Bodies = bodies;
        _lastUpdate = DateTime.Now;
    }

    internal void calcCenter()
    {
        center = Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.Head];
    }

    internal void calcUp()
    {
        up = Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder] - Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineBase];
        //_floorValues.Add(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]);
        //_floorValues.Add(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]);

        _floorValues.Add(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]);
        _floorValues.Add(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]);

    }

    internal void calcCenter2()
    {
        center2 = Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.Head];
        forward = center2 - center;
        up2 = Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder] - Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineBase];

        Debug.Log("center: " + center);
        Debug.Log("center2: " + center2);
        Debug.Log("forward: " + forward);
        Debug.Log("up: " + up);
        Debug.Log("up2: " + up2);

        _floorValues.Add(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]);
        _floorValues.Add(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]);

    }

    internal void applyCalibration()
    {
        up.Normalize();
        up2.Normalize();
        up = (up + up2) / 2.0f;

        up = new Vector3(-up.x, up.y, up.z);

        forward.Normalize();

        forward = new Vector3(-forward.x, forward.y, forward.z);

        right = Vector3.Cross(forward, up);
        forward = Vector3.Cross(right, up);

        Matrix4x4 m = new Matrix4x4();

        m[0, 0] = right.x;
        m[1, 0] = right.y;
        m[2, 0] = right.z;
        m[3, 0] = 0;

        m[0, 1] = up.x;
        m[1, 1] = up.y;
        m[2, 1] = up.z;
        m[3, 1] = 0;

        m[0, 2] = forward.x;
        m[1, 2] = forward.y;
        m[2, 2] = forward.z;
        m[3, 2] = 0;

        m[0, 3] = center.x;
        m[1, 3] = center.y;
        m[2, 3] = center.z;
        m[3, 3] = 1;

        m = m.inverse;




        Vector3 minv = new Vector3();
        float min = 100000;

        foreach (Vector3 v in _floorValues)
        {
            Vector3 tmp = m.MultiplyPoint(v);

            if (tmp.y < min)
                minv = tmp;
        }

        /*GameObject minGO = CommonUtils.newGameObject(minv);
        minGO.transform.parent = _sensorGameObject.transform;
        minGO.name = "Floor";*/


        _sensorGameObject.transform.position = new Vector3(m[0, 3], m[1, 3] - minv.y, m[2, 3]);
        _sensorGameObject.transform.rotation = MatrixToRotation(m);
        _sensorGameObject.transform.localScale = new Vector3(-1, 1, 1);


        

    }

    private static Quaternion MatrixToRotation(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }
}

public class Tracker : MonoBehaviour {


    private Dictionary<string, Sensor> _sensors;
    public Dictionary<string, Sensor> Sensors
    {
        get
        {
            return _sensors;
        }
    }

    public CalibrationProcess calibrationStatus;

    void Start () {
        _sensors = new Dictionary<string, Sensor>();
        calibrationStatus = CalibrationProcess.FindCenter;
    }
	
	void Update () {
        
        foreach (Sensor s in Sensors.Values)
        {
            if (s.Active)
            {
                //foreach (ServerBody b in s.Bodies.Bodies)
                {
                    //Debug.Log(b.bodyProperties[BodyPropertiesTypes.HandRightState]);
                    //head = b.jointsPositions[Windows.Kinect.JointType.Head];
                }
            }
        }



	}

    internal void setNewFrame(BodiesMessage bodies)
    {
        if (!Sensors.ContainsKey(bodies.KinectId))
        {
            Debug.Log("New Sensor: bodies.KinectId");
            Sensors[bodies.KinectId] = new Sensor(bodies.KinectId, (GameObject) Instantiate(Resources.Load("Prefabs/SensorPrefab"), new Vector3(), new Quaternion()));
        }
        Sensors[bodies.KinectId].updateBodies(bodies);
    }

    internal void findCenter()
    {
        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.Bodies.Bodies.Count == 1 && sensor.Active)
            {
                sensor.calcCenter();
                sensor.calcUp();
            }
        }
    }

    internal void findForward()
    {
        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.Bodies.Bodies.Count == 1 && sensor.Active)
            {
                sensor.calcCenter2();
                sensor.applyCalibration();
            }
        }

    }
}
