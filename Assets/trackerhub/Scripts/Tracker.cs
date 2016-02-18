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
    private GameObject _capsule;

    private bool _active;

    private DateTime _lastUpdate;

    private Vector3 center1;
    private Vector3 center2;
    private Vector3 up1;
    private Vector3 up2;

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

    public Vector3 CalibAuxPoint
    {
        get
        {
            return center1;
        }
    }

    public Sensor(string sensorID, GameObject sensorGameObject, GameObject personGO)
    {
        _active = true;
        SensorID = sensorID;
        Bodies = null;
        _sensorGameObject = sensorGameObject;
        _sensorGameObject.name = sensorID;
        center1 = new Vector3();
        center2 = new Vector3();
        up1 = new Vector3();
        up2 = new Vector3();
        _floorValues = new List<Vector3>();

        _capsule = personGO;
        _capsule.SetActive(false);
        _capsule.transform.parent = _sensorGameObject.transform;
        _capsule.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    internal Vector3 pointSensorToScene(Vector3 p)
    {
        return _sensorGameObject.transform.localToWorldMatrix.MultiplyPoint(p);
    }

    internal Vector3 pointKinectToUnity(Vector3 p)
    {
        return new Vector3(-p.x, p.y, p.z);
    }

    internal void updateBodies(BodiesMessage bodies)
    {
        Bodies = bodies;

        if (Bodies.NumberOfBodies > 0)
        {
            _capsule.transform.localPosition = pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder]);

            _capsule.SetActive(true);
        }
        else
        {
            _capsule.SetActive(false);
        }

    }

    internal void calibrationStep1()
    {
        center1 = pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder]);
        up1 = pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder]) - pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineBase]);

        _floorValues.Add(pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]));
        _floorValues.Add(pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]));
    }

    internal void calibrationStep2()
    {
        center2 = pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder]);
        up2 = pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineShoulder]) - pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.SpineBase]);

        _floorValues.Add(pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]));
        _floorValues.Add(pointKinectToUnity(Bodies.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]));

        // Begin calibration calculations

        Vector3 up = (up1 + up2) / 2.0f;
        Vector3 forward = center2 - center1;

        GameObject centerGO = new GameObject();
        centerGO.transform.parent = _sensorGameObject.transform;
        centerGO.transform.rotation = Quaternion.LookRotation(forward, up);
        centerGO.transform.position = center1;

        centerGO.transform.parent = null;
        _sensorGameObject.transform.parent = centerGO.transform;
        centerGO.transform.position = Vector3.zero;
        centerGO.transform.rotation = Quaternion.identity;

        _sensorGameObject.transform.parent = null;
        GameObject.Destroy(centerGO);

        Vector3 minv = new Vector3();
        float min = float.PositiveInfinity;

        foreach (Vector3 v in _floorValues)
        {
            Vector3 tmp = pointSensorToScene(v);

            if (tmp.y < min)
                minv = tmp;
        }

        move(new Vector3(0, -minv.y, 0));
    }

    internal void move(Vector3 positiondelta)
    {
        _sensorGameObject.transform.position += positiondelta;
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
                
            }
        }
	}

    internal void setNewFrame(BodiesMessage bodies)
    {
        if (!Sensors.ContainsKey(bodies.KinectId))
        {
            Debug.Log("New Sensor: " + bodies.KinectId);
            Sensors[bodies.KinectId] = new Sensor(bodies.KinectId, (GameObject) Instantiate(Resources.Load("Prefabs/KinectSensorPrefab"), new Vector3(), new Quaternion()), GameObject.CreatePrimitive(PrimitiveType.Sphere));
        }
        Sensors[bodies.KinectId].updateBodies(bodies);
    }

    internal void CalibrationStep1()
    {
        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.Bodies.Bodies.Count == 1 && sensor.Active)
            {
                sensor.calibrationStep1();
            }
        }
    }

    internal void CalibrationStep2()
    {

        Vector3 avgCenter = new Vector3();
        int sensorCount = 0;

        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.Bodies.Bodies.Count == 1 && sensor.Active)
            {
                sensor.calibrationStep2();

                avgCenter += sensor.pointSensorToScene(sensor.CalibAuxPoint);
                sensorCount += 1;
            }
        }

        avgCenter /= sensorCount;

        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.Bodies.Bodies.Count == 1 && sensor.Active)
            {
                sensor.move(avgCenter - sensor.pointSensorToScene(sensor.CalibAuxPoint));   
            }
        }
    }
}
