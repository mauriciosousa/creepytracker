using UnityEngine;
using System.Collections;

public class RPCServer : MonoBehaviour {

    public string port;

    public Texture onlineTex;
    public Texture offlineTex;

    public bool showNetworkOptions;

    void Start()
    {
        showNetworkOptions = false;
    }

    void Update()
    {

    }

    void OnGUI()
    {
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            GUI.DrawTexture(new Rect(Screen.width - 48, 1, 48, 48), offlineTex);
        }
        else
        {
            GUI.DrawTexture(new Rect(Screen.width - 48, 1, 48, 48), onlineTex);
            if (Network.peerType == NetworkPeerType.Server)
            {
                GUI.Label(new Rect(Screen.width - 20, 30, 48, 48), "" + Network.connections.Length);
            }
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

                GUI.Label(new Rect(left, top, 100, 25), "port:");
                port = GUI.TextField(new Rect(left + 60, top, 50, 25), port);

                top += 30;

                if (GUI.Button(new Rect(Screen.width - 90, top, 90, 25), "Start"))
                {
                    Network.InitializeServer(32, int.Parse(port), true);
                    showNetworkOptions = false;
                }

            }
            else
            {
                if (Network.peerType == NetworkPeerType.Server)
                {
                    GUI.Label(new Rect(Screen.width - 48 - 58, 10, 100, 25), "I'm a server");
                    if (GUI.Button(new Rect(Screen.width - 120, 155, 90, 25), "Stop"))
                    {
                        Network.Disconnect(250);
                    }
                }
            }
        }
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        DoNotify n = gameObject.GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.INFO, "Network", "New Connection", 5000);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);

        DoNotify n = gameObject.GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.IMPORTANT, "Network", "Lost Connection", 5000);
    }
}
