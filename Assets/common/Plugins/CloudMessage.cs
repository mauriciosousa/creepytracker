using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudMessage {
	
	public List<Point3DRGB> Points { get; internal set; }
	public string KinectId { get; internal set; }
	public int id;
	public CloudMessage(string message)
	{
		string[] pdu = message.Split(MessageSeparators.L1);
		KinectId = pdu[0];
		id = int.Parse (pdu [1]);
		Points = new List<Point3DRGB> ();
		List<string> lines = new List<string> (pdu [2].Split (MessageSeparators.L2));
		foreach (string line in lines) {
			string[] values = line.Split(MessageSeparators.L3);
			if(values.Length >= 6){
				float x = float.Parse(values[0]);
				float y = float.Parse(values[1]);
				float z = float.Parse(values[2]);
				float r = float.Parse(values[3]);
				float g = float.Parse(values[4]);
				float b = float.Parse(values[5]);
				Point3DRGB pt = new Point3DRGB(new Vector3(x,y,z),new Color(r/255,g/255,b/255));
				Points.Add(pt);
			}

		}
	}

	public static string createRequestMessage(int mode)
	{
		return "CloudMessage" + MessageSeparators.L0 + Network.player.ipAddress + MessageSeparators.L1 + (mode) + MessageSeparators.L1 + TrackerProperties.Instance.listenPort;
	}
}
