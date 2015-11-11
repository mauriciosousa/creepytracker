using UnityEngine;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class RPCClient : MonoBehaviour {

    public string address;
    public string port;

    public Texture onlineTex;
    public Texture offlineTex;

    public bool showNetworkOptions;

    private string _peerName;
    public string PeerName
    {
        get
        {
            return _peerName;
        }

        set
        {
            _peerName = value;
        }
    }

    void Start () {
        showNetworkOptions = false;
        _peerName = System.Environment.MachineName;
	}
	
	void Update () {

	}

    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 25, 100, 25), PeerName);

        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            GUI.DrawTexture(new Rect(Screen.width - 48, 1, 48, 48), offlineTex);
        }
        else
        {
            GUI.DrawTexture(new Rect(Screen.width - 48, 1, 48, 48), onlineTex);
        }

        if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > Screen.width - 100
            && Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - 100)
        {
            showNetworkOptions = true;
        }
        if (Input.mousePosition.x < Screen.width / 2 || Input.mousePosition.y < ((3 / 4) * Screen.height))
        {
            showNetworkOptions = false;
        }

        if (showNetworkOptions)
        {
            int left = Screen.width - 200;
            int top = 50;

            if (Network.peerType == NetworkPeerType.Disconnected)
            {
                GUI.Label(new Rect(left, top, 100, 25), "address:");
                address = GUI.TextField(new Rect(left + 60, top, 100, 25), address);

                top += 30;

                GUI.Label(new Rect(left, top, 100, 25), "port:");
                port = GUI.TextField(new Rect(left + 60, top, 50, 25), port);

                top += 30;

                if (GUI.Button(new Rect(Screen.width - 90, top, 90, 25), "Connect"))
                {
                    Network.Connect(address, int.Parse(port));
                    showNetworkOptions = false;
                }

            }
            else
            {
                if (Network.peerType == NetworkPeerType.Client)
                {
                    if (GUI.Button(new Rect(left, top, 90, 25), "Log out"))
                    {
                        Network.Disconnect(250);
                    }
                }
            }
        }
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        DoNotify n = gameObject.GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.IMPORTANT, "Network", "Connection request failed", 10000);
    }

    void OnConnectedToServer()
    {
        DoNotify n = gameObject.GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.INFO, "Network", "Connected to Server", 5000);
    }

    public void sendNewFrame(List<Kinect.Body> listOfBodies)
    {
        if (Network.peerType == NetworkPeerType.Client)
        {
            BodiesMessage b = new BodiesMessage(PeerName, listOfBodies);

#pragma warning disable CS0618 // Type or member is obsolete
            GetComponent<NetworkView>().RPC("newFrameFromSensor", RPCMode.Server, b.Message);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    [RPC]
    public void newFrameFromSensor(string bodies)
    {/**/}

}
