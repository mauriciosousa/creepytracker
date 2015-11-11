using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Sensor
{
    private string _name;

    public Sensor(string name)
    {
        Name = name;
    }

    public string Name
    {
        get
        {
            return _name;
        }

        set
        {
            _name = value;
        }
    }
}

public class Tracker : MonoBehaviour {

    private List<Sensor> _sensors;

	void Start () {
        _sensors = new List<Sensor>();
	}
	
	void Update () {
	
	}

    internal void setNewFrame(BodiesMessage bodies)
    {
        
  


        if (bodies.NumberOfBodies > 0)
        {

        }
        else
        {
            // remove bodies
        }
    }

    private bool _sensorExists(string name)
    {
        foreach (Sensor sensor in _sensors)
        {
            if (sensor.Name == name) return true;
        }
        return false;
    }
}
