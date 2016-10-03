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
    private List<byte[]> _stringsToParse; // TMA: Store the bytes from the socket instead of converting to strings. Saves time.
    private byte[] _receivedBytes;
    private int number = 0;

    void Start()
    { }

    public void udpRestart()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
        }

        _stringsToParse = new List<byte[]>();
        
		_anyIP = new IPEndPoint(IPAddress.Any, TrackerProperties.Instance.listenPort);
        
        _udpClient = new UdpClient(_anyIP);

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);

		Debug.Log("[UDPListener] Receiving in port: " + TrackerProperties.Instance.listenPort);
    }

    public void ReceiveCallback(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient.EndReceive(ar, ref _anyIP);
        
		_stringsToParse.Add(receiveBytes);

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
    }

    void Update()
    {
        while (_stringsToParse.Count > 0)
        {
            try
            {
                byte[] toProcess = _stringsToParse.First();
                if(toProcess != null)
                {
                    // TMA: THe first char distinguishes between a BodyMessage and a CloudMessage
                    if (Convert.ToChar(toProcess[0]) == 'B') {
                        try
                        {
                            string stringToParse = Encoding.ASCII.GetString(toProcess);
                            string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
                            BodiesMessage b = new BodiesMessage(splitmsg[1]);
                            gameObject.GetComponent<Tracker>().setNewFrame(b);
                        }
                        catch (BodiesMessageException e)
                        {
                            Debug.Log(e.Message);
                        }
                    }
                    else if(Convert.ToChar(toProcess[0]) == 'C')
                    {
                        string stringToParse = Encoding.ASCII.GetString(toProcess);
                        string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
                        CloudMessage c = new CloudMessage(splitmsg[1], toProcess);
                        gameObject.GetComponent<Tracker>().setNewCloud(c);
                    }
                }
                _stringsToParse.RemoveAt(0);
            }
            catch (Exception exc) { _stringsToParse.RemoveAt(0); }
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
