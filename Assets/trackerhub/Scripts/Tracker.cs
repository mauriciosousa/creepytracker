using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Windows.Kinect;

public enum CalibrationProcess
{
    None, 
    FindCenter,
    FindForward
}

public class Sensor
{
    public string SensorID;
    public BodiesMessage lastBodiesMessage;

    public Dictionary<string, SensorBody> bodies;

    private GameObject _sensorGameObject;
    public GameObject SensorGameObject { get { return _sensorGameObject; } }

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

    public Sensor(string sensorID, GameObject sensorGameObject)
    {
        bodies = new Dictionary<string, SensorBody>();
        _active = true;
        SensorID = sensorID;
        lastBodiesMessage = null;
        _sensorGameObject = sensorGameObject;
        SensorGameObject.name = sensorID;
        center1 = new Vector3();
        center2 = new Vector3();
        up1 = new Vector3();
        up2 = new Vector3();
        _floorValues = new List<Vector3>();
    }

    internal Vector3 pointSensorToScene(Vector3 p)
    {
        return SensorGameObject.transform.localToWorldMatrix.MultiplyPoint(p);
    }

    internal void updateBodies()
    {
        BodiesMessage bodiesMessage = lastBodiesMessage;

        if (bodiesMessage == null) return;

        foreach (KeyValuePair<string, SensorBody> sb in bodies)
        {
            sb.Value.updated = false;
        }

        // refresh bodies position
        foreach (Skeleton sk in bodiesMessage.Bodies)
        {
            SensorBody b;

            if (int.Parse(sk.bodyProperties[BodyPropertiesTypes.Confidence]) < TrackerProperties.Instance.confidenceTreshold)
            {
                if (bodies.ContainsKey(sk.ID))
                {
                    b = bodies[sk.ID];
                    b.updated = true;
                    b.lastUpdated = DateTime.Now;
                }
                continue;
            }

            if (bodies.ContainsKey(sk.ID))
            {   //existing bodies
                b = bodies[sk.ID];
            }
            else
            {   // new bodies
                b = new SensorBody(sk.ID, SensorGameObject.transform);
                bodies[sk.ID] = b;
                b.sensorID = SensorID;
            }

            b.LocalPosition = CommonUtils.pointKinectToUnity(sk.jointsPositions[Windows.Kinect.JointType.SpineBase]);
            b.updated = true;
            b.lastUpdated = DateTime.Now;
            b.skeleton = sk;
        }

        // remove bodies no longer present
        List<string> keysToRemove = new List<string>();
        foreach (KeyValuePair<string, SensorBody> sb in bodies)
        {
            if (!sb.Value.updated)
            {
                GameObject.Destroy(sb.Value.gameObject);
                keysToRemove.Add(sb.Key);
            }
        }
        foreach (String key in keysToRemove)
        {
            bodies.Remove(key);
        }
    }

    internal void calibrationStep1()
    {
        center1 = CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[TrackerProperties.Instance.centerJoint]);
        up1 = CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[TrackerProperties.Instance.upJointB]) - CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[TrackerProperties.Instance.upJointA]);

        _floorValues.Add(CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]));
        _floorValues.Add(CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]));
    }

    internal void calibrationStep2()
    {
        center2 = CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[TrackerProperties.Instance.centerJoint]);
        up2 = CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[TrackerProperties.Instance.upJointB]) - CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[TrackerProperties.Instance.upJointA]);

        _floorValues.Add(CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootLeft]));
        _floorValues.Add(CommonUtils.pointKinectToUnity(lastBodiesMessage.Bodies[0].jointsPositions[Windows.Kinect.JointType.FootRight]));

        // Begin calibration calculations

        Vector3 up = (up1 + up2) / 2.0f;
        Vector3 forward = center2 - center1;
        Vector3 right = Vector3.Cross(up, forward);
        forward = Vector3.Cross(right, up);

        SensorGameObject.transform.position = Vector3.zero;
        SensorGameObject.transform.rotation = Quaternion.identity;

        GameObject centerGO = new GameObject();
        centerGO.transform.parent = SensorGameObject.transform;
        centerGO.transform.rotation = Quaternion.LookRotation(forward, up);
        centerGO.transform.position = center1;

        centerGO.transform.parent = null;
        SensorGameObject.transform.parent = centerGO.transform;
        centerGO.transform.position = Vector3.zero;
        centerGO.transform.rotation = Quaternion.identity;

        SensorGameObject.transform.parent = null;
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
        SensorGameObject.transform.position += positiondelta;
    }
}

public class SensorBody
{
    public string ID;
    public string sensorID;
    public DateTime lastUpdated;
    private Vector3 position;
    public Skeleton skeleton;
    public GameObject gameObject;
    public bool updated;
    public Vector3 LocalPosition
    {
        get
        {
            return position;
        }

        set
        {
            position = value;
            gameObject.transform.localPosition = position;
        }
    }
    public Vector3 WorldPosition
    { get { return gameObject.transform.position; } }

    public int Confidence
    {  get { return int.Parse(skeleton.bodyProperties[BodyPropertiesTypes.Confidence]); } }

    public SensorBody(string ID, Transform parent)
    {
        this.ID = ID;
        gameObject = new GameObject();// GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.name = this.ID;
        gameObject.transform.parent = parent;
    }
}

public class Human
{
    public int ID;
    public List<SensorBody> bodies;
    public GameObject gameObject;
    public DateTime timeOfDeath;
    public string seenBySensor;
    private HumanSkeleton skeleton;

    private Vector3 position;
    public Vector3 Position
    {
        get
        {
            return position;
        }

        set
        {
            position = value;
            gameObject.transform.position = position;
        }
    }

    public Human(GameObject gameObject, Tracker tracker)
    {
        ID = CommonUtils.getNewID();
        bodies = new List<SensorBody>();
        this.gameObject = gameObject;
        this.gameObject.name = "Human " + ID;

        skeleton = this.gameObject.GetComponent<HumanSkeleton>();
        skeleton.tracker = tracker;
        skeleton.ID = ID;
        skeleton.updateSkeleton();
    }

    internal string getPDU()
    {
        return skeleton.getPDU();
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

    private CalibrationProcess _calibrationStatus;
    public CalibrationProcess CalibrationStatus
    {
        get
        {
            return _calibrationStatus;
        }

        set
        {
            _calibrationStatus = value;
        }
    }

    private Dictionary<int, Human> _humans;

    private List<Human> _deadHumans;
    private List<Human> _humansToKill;

    private UdpBroadcast _udpBroadcast;

    void Start ()
    {
        _sensors = new Dictionary<string, Sensor>();
        _humans = new Dictionary<int, Human>();
        _deadHumans = new List<Human>();
        _humansToKill = new List<Human>();
        _calibrationStatus = CalibrationProcess.FindCenter;

        _udpBroadcast = new UdpBroadcast(TrackerProperties.Instance.broadcastPort);

        _loadConfig();
        _loadSavedSensors();
        
    }

    void Update ()
    {
        foreach(Sensor s in _sensors.Values)
        {
            s.updateBodies();
        }

        mergeHumans();

        List<Human> deadHumansToRemove = new List<Human>();
        foreach (Human h in _deadHumans)
        {
            if (DateTime.Now > h.timeOfDeath.AddMilliseconds(1000))
                deadHumansToRemove.Add(h);
        }

        foreach (Human h in deadHumansToRemove)
        {
            Destroy(h.gameObject);
            _deadHumans.Remove(h);
        }

        foreach (Human h in _humansToKill)
        {
            Destroy(h.gameObject);
        }
        _humansToKill.Clear();

        // udp broadcast

        string strToSend = "" + _humans.Count;

        foreach (Human h in _humans.Values)
        {
            try
            {
                strToSend += MessageSeparators.L1 + h.getPDU();
            }
            catch (Exception e)
            { }
        }

        _udpBroadcast.send(strToSend);
    }

    private void mergeHumans()
    {
        List<SensorBody> alone_bodies = new List<SensorBody>();

        // refresh existing bodies
        foreach (Sensor s in Sensors.Values)
        {
            if (s.Active)
            {
                foreach (KeyValuePair<string, SensorBody> sensorBody in s.bodies)
                {
                    bool alone = true;

                    foreach (KeyValuePair<int, Human> h in _humans)
                    {
                        foreach (SensorBody humanBody in h.Value.bodies)
                        {
                            if (sensorBody.Value.sensorID == humanBody.sensorID && sensorBody.Value.ID == humanBody.ID)
                            {
                                humanBody.LocalPosition = sensorBody.Value.LocalPosition;
                                humanBody.lastUpdated = sensorBody.Value.lastUpdated;
                                humanBody.updated = sensorBody.Value.updated;

                                alone = false;
                                break;
                            }
                        }

                        if (!alone) break;
                    }

                    if (alone) alone_bodies.Add(sensorBody.Value);
                }
            }
        }

        // refresh existing humans
        foreach (KeyValuePair<int, Human> h in _humans)
        {
            Vector3 position = new Vector3();
            int numberOfBodies = 0;
            List<SensorBody> deadBodies = new List<SensorBody>();

            foreach (SensorBody b in h.Value.bodies)
            {
                if (b.updated && Sensors[b.sensorID].Active)
                    position = (position * (float)numberOfBodies++ + b.WorldPosition) / (float)numberOfBodies;
                else
                    deadBodies.Add(b);
            }

            foreach (SensorBody b in deadBodies)
            {
                h.Value.bodies.Remove(b);
            }

            if (h.Value.bodies.Count == 0)
            {
                h.Value.timeOfDeath = DateTime.Now;
                _deadHumans.Add(h.Value);
            }
            else
            {
                h.Value.Position = position;
            }
        }
        foreach (Human h in _deadHumans)
        {
            _humans.Remove(h.ID);
        }

        // new bodies
        foreach (SensorBody b in alone_bodies)
        {
            bool hasHuman = false;

            // try to fit in existing humans
            foreach (KeyValuePair<int, Human> h in _humans)
            {
                if (Vector3.Distance(b.WorldPosition, h.Value.Position) < TrackerProperties.Instance.mergeDistance)
                {
                    h.Value.Position = (h.Value.Position * (float)h.Value.bodies.Count + b.WorldPosition) / (float)(h.Value.bodies.Count + 1);
                    h.Value.bodies.Add(b);
                    hasHuman = true;
                    break;
                }
            }

            if (!hasHuman)
            {
                // try to fit in dead humans
                foreach (Human h in _deadHumans)
                {
                    if (Vector3.Distance(b.WorldPosition, h.Position) < TrackerProperties.Instance.mergeDistance)
                    {
                        h.Position = (h.Position * (float)h.bodies.Count + b.WorldPosition) / (float)(h.bodies.Count + 1);
                        h.bodies.Add(b);
                        hasHuman = true;
                        break;
                    }
                }

                if (!hasHuman)
                {
                    // create new human
                    Human h = new Human((GameObject)Instantiate(Resources.Load("Prefabs/Human")), this);

                    h.bodies.Add(b);
                    h.Position = b.WorldPosition;

                    _humans[h.ID] = h;
                }
            }
        }

        // bring back to life selected dead humans
        List<Human> undeadHumans = new List<Human>();
        foreach (Human h in _deadHumans)
        {
            if (h.bodies.Count > 0)
            {
                _humans[h.ID] = h;
                undeadHumans.Add(h);
            }
        }
        foreach (Human h in undeadHumans)
        {
            _deadHumans.Remove(h);
        }

        // merge humans
        List<Human> mergedHumans = new List<Human>();
        foreach (KeyValuePair<int, Human> h1 in _humans)
        {
            foreach (KeyValuePair<int, Human> h2 in _humans)
            {
                if (h1.Value.ID != h2.Value.ID && !mergedHumans.Contains(h2.Value))
                {
                    if (Vector3.Distance(h1.Value.Position, h2.Value.Position) < TrackerProperties.Instance.mergeDistance)
                    {
                        Vector3 position = (h1.Value.Position * (float)h1.Value.bodies.Count + h2.Value.Position * (float)h2.Value.bodies.Count) / (float)(h1.Value.bodies.Count + h2.Value.bodies.Count);

                        if (h1.Value.ID < h2.Value.ID)
                        {
                            h1.Value.Position = position;
                            foreach (SensorBody b in h2.Value.bodies)
                            {
                                h1.Value.bodies.Add(b);
                            }
                            mergedHumans.Add(h2.Value);
                        }
                        else
                        {
                            h2.Value.Position = position;
                            foreach (SensorBody b in h1.Value.bodies)
                            {
                                h2.Value.bodies.Add(b);
                            }
                            mergedHumans.Add(h1.Value);
                        }
                        break;
                    }
                }
            }
        }
        foreach (Human h in mergedHumans)
        {
            _humansToKill.Add(h);
            _humans.Remove(h.ID);
        }
    }

    internal void setNewFrame(BodiesMessage bodies)
    {
        if (!Sensors.ContainsKey(bodies.KinectId))
        {
            Sensors[bodies.KinectId] = new Sensor(bodies.KinectId, (GameObject) Instantiate(Resources.Load("Prefabs/KinectSensorPrefab"), Vector3.zero, Quaternion.identity));
        }

        Sensors[bodies.KinectId].lastBodiesMessage = bodies;
    }

    internal bool CalibrationStep1()
    {
        bool cannotCalibrate = false;
        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active)
            {
                sensor.calibrationStep1();
            }
            else cannotCalibrate = true;
        }

        if (cannotCalibrate)
        {
            DoNotify n = GameObject.Find("NetworkManager").GetComponent<DoNotify>();
            n.notifySend(NotificationLevel.IMPORTANT, "Calibration error", "Incorrect user placement!", 5000);
        }

        return !cannotCalibrate;
    }

    internal void CalibrationStep2()
    {

        Vector3 avgCenter = new Vector3();
        int sensorCount = 0;

        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active)
            {
                sensor.calibrationStep2();

                avgCenter += sensor.pointSensorToScene(sensor.CalibAuxPoint);
                sensorCount += 1;
            }
        }

        avgCenter /= sensorCount;

        foreach (Sensor sensor in _sensors.Values)
        {
            if (sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active)
            {
                sensor.move(avgCenter - sensor.pointSensorToScene(sensor.CalibAuxPoint));   
            }
        }

        _saveConfig();

        DoNotify n = GameObject.Find("NetworkManager").GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
    }

    internal Vector3 getJointPosition(int id, JointType joint)
    {
        Human h = _humans[id];
        SensorBody bestBody = h.bodies[0];
        int confidence = bestBody.Confidence;

        foreach (SensorBody b in h.bodies)
        {
            int bConfidence = b.Confidence;
            if (bConfidence > confidence)
            {
                confidence = bConfidence;
                bestBody = b;
            }
        }

        h.seenBySensor = bestBody.sensorID;

        return _sensors[bestBody.sensorID].pointSensorToScene(CommonUtils.pointKinectToUnity(bestBody.skeleton.jointsPositions[joint]));
    }

    internal bool humanHasBodies(int id)
    {
        return _humans.ContainsKey(id) && _humans[id].bodies.Count > 0;
    }

    void OnGUI()
    {
        int n = 1;

        foreach (Human h in _humans.Values)
        {
            GUI.Label(new Rect(10, Screen.height - (n++ * 50), 1000, 50), "Human " + h.ID + " as seen by " + h.seenBySensor);
        }
    }

    private void _saveConfig()
    {
        string filePath = Application.dataPath + "/" + TrackerProperties.Instance.configFilename;
        ConfigProperties.clear(filePath);

        // save properties


        // save sensors
        foreach (Sensor s in _sensors.Values)
        {
            if (s.Active)
            {
                Vector3 p = s.SensorGameObject.transform.position;
                Quaternion r = s.SensorGameObject.transform.rotation;
                ConfigProperties.save(filePath, "kinect." + s.SensorID, "" + s.SensorID + ";" + p.x + ";" + p.y + ";" + p.z + ";" + r.x + ";" + r.y + ";" + r.z + ";" + r.w);
            }
        }
    }

    private void _loadConfig()
    {
        string filePath = Application.dataPath + "/" + TrackerProperties.Instance.configFilename;

        //Debug.Log(ConfigProperties.load(filePath, "exampleValue"));
    }

    private void _loadSavedSensors()
    {
        foreach (String line in ConfigProperties.loadKinects(Application.dataPath + "/" + TrackerProperties.Instance.configFilename))
        {
            string[] values = line.Split(';');

            string id = values[0];

            Vector3 position = new Vector3(
                float.Parse(values[1].Replace(',', '.')),
                float.Parse(values[2].Replace(',', '.')),
                float.Parse(values[3].Replace(',', '.')));

            Quaternion rotation = new Quaternion(
                float.Parse(values[4].Replace(',', '.')),
                float.Parse(values[5].Replace(',', '.')),
                float.Parse(values[6].Replace(',', '.')),
                float.Parse(values[7].Replace(',', '.')));

            Sensors[id] = new Sensor(id, (GameObject)Instantiate(Resources.Load("Prefabs/KinectSensorPrefab"), position, rotation));
        }
    }

    internal void resetBroadcast()
    {
        _udpBroadcast.reset(TrackerProperties.Instance.broadcastPort);
    }
}
