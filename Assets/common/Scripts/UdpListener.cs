using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;

public class UdpListener : MonoBehaviour {

    private UdpClient _udpClient = null;
    private IPEndPoint _anyIP;
    private List<string> _stringsToParse;

    void Start()
    { }

    public void udpRestart()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
        }

        _stringsToParse = new List<string>();
        
		_anyIP = new IPEndPoint(IPAddress.Any, TrackerProperties.Instance.listenPort);
        
        _udpClient = new UdpClient(_anyIP);

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);

		Debug.Log("[UDPListener] Receiving in port: " + TrackerProperties.Instance.listenPort);
    }

    public void ReceiveCallback(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient.EndReceive(ar, ref _anyIP);
		_stringsToParse.Add(Encoding.ASCII.GetString(receiveBytes));

		_udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
    }

    void Update()
    {
        while (_stringsToParse.Count > 0)
        {
            string stringToParse = _stringsToParse.First();
            _stringsToParse.RemoveAt(0);
			if(stringToParse != null){
				string[] splitmsg = stringToParse.Split (MessageSeparators.L0);
				if(splitmsg[0] == "BodiesMessage"){
					try
            		{
						BodiesMessage b = new BodiesMessage(splitmsg[1]);
						gameObject.GetComponent<Tracker>().setNewFrame(b);
					}catch (BodiesMessageException e)
            		{
                		Debug.Log(e.Message);
            		}
				}
				if (splitmsg [0] == "CloudMessage") {
					CloudMessage c = new CloudMessage(splitmsg[1]);
					gameObject.GetComponent<Tracker>().setNewCloud(c);

				}
			}
        }
    }

    void OnApplicationQuit()
    {
        if (_udpClient != null) _udpClient.Close();
    }

    void OnQuit()
    {
        OnApplicationQuit();
    }
}
