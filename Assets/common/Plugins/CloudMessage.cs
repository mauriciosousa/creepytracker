using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CloudMessage {
	
	public List<Point3DRGB> Points_lowres { get; internal set; }
    public List<Point3DRGB> Points_highres { get; internal set; }
    public string KinectId { get; internal set; }
	public int id;
	public CloudMessage(string message, byte[] receivedBytes)
	{
        int step = 15; // TMA: Size in bytes of heading: "CloudMessage" + L0 + 2 * L1. Check the UDPListener.cs from the Client.
        byte[] buffer = new byte[4]; // Buffer for the x, y and z floats
		string[] pdu = message.Split(MessageSeparators.L1);
        float x, y, z;
        byte r, g, b;
		KinectId = pdu[0];
		id = int.Parse (pdu [1]);
        step += pdu[0].Length + pdu[1].Length;
        Points_lowres = new List<Point3DRGB> ();
        Points_highres = new List<Point3DRGB>();
        for (int i = step; i < receivedBytes.Length; i += 16) // Each point is represented by 16 bytes.
        {
            try
            {
                if (i + 15 > receivedBytes.Length) break; // Insurance.

                buffer[0] = receivedBytes[i];
                buffer[1] = receivedBytes[i + 1];
                buffer[2] = receivedBytes[i + 2];
                buffer[3] = receivedBytes[i + 3];
                x = System.BitConverter.ToSingle(buffer, 0); // x

                buffer[0] = receivedBytes[i + 4];
                buffer[1] = receivedBytes[i + 5];
                buffer[2] = receivedBytes[i + 6];
                buffer[3] = receivedBytes[i + 7];
                y = System.BitConverter.ToSingle(buffer, 0); // y

                buffer[0] = receivedBytes[i + 8];
                buffer[1] = receivedBytes[i + 9];
                buffer[2] = receivedBytes[i + 10];
                buffer[3] = receivedBytes[i + 11];
                z = System.BitConverter.ToSingle(buffer, 0); // z

                r = receivedBytes[i + 12]; // r
                g = receivedBytes[i + 13]; // g
                b = receivedBytes[i + 14]; // b

                Point3DRGB pt = new Point3DRGB(new Vector3(x, y, z), new Color((float)r / 255, (float)g / 255, (float)b / 255));

                if (receivedBytes[i + 15] == 1) Points_highres.Add(pt); // If it's a HR point, save it to the high resolution points.
                else Points_lowres.Add(pt); // If it's not, it's a low resolution point.
            }
            catch (Exception exc)
            {
                Debug.Log("Reached out of the array: " + exc.StackTrace);
                Debug.Log("Kinectid = " + KinectId);
           }
        }
    }

	public static string createRequestMessage(int mode)
	{
		return "CloudMessage" + MessageSeparators.L0 + Network.player.ipAddress + MessageSeparators.L1 + (mode) + MessageSeparators.L1 + TrackerProperties.Instance.listenPort;
	}
}
