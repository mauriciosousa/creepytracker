using UnityEngine;
using System.Collections;

enum MenuAction
{
    Settings,
    Sensors,
    Humans,
    Devices,
    NetworkSettings,
    About,
    None
}

public class TrackerUI : MonoBehaviour {

    public Texture checkTexture;
    public Texture uncheckTexture;

    public Texture sensorTextureOn;
    public Texture sensorTextureOff;

    public Texture settingsTextureOn;
    public Texture settingsTextureOff;

    public Texture networkTextureOn;
    public Texture networkTextureOff;

    public Texture aboutOn;
    public Texture aboutOff;

    [Range(20, 100)]
    public int iconSize;

    private MenuAction _menuAction;

    private Tracker _userTracker;

	private GUIStyle _titleStyle;


    private string newUnicastAddress;
    private string newUnicastPort;

    void Start()
    {
        _userTracker = gameObject.GetComponent<Tracker>();
        _menuAction = MenuAction.None;

		_titleStyle = new GUIStyle ();
		_titleStyle.fontStyle = FontStyle.Bold;
		_titleStyle.normal.textColor = Color.white;

        newUnicastAddress = "";
        newUnicastPort = "";
    }
	
	void Update () {
        
	}

    void OnGUI()
    {
        int top = 5;
        int left = 20;

        displayMenuButton(MenuAction.Sensors, sensorTextureOn, sensorTextureOff, new Rect(left, top, iconSize, iconSize));
        GUI.Label(new Rect(left + iconSize, top + iconSize - 20, 10, 25), "" + _userTracker.Sensors.Count);
        left += iconSize + iconSize / 2;
        
        //displayMenuButton(MenuAction.Devices, deviceTex_on, deviceTex_off, new Rect(left, top, iconSize, iconSize));
        //left += iconSize + iconSize / 2;
        displayMenuButton(MenuAction.Settings, settingsTextureOn, settingsTextureOff, new Rect(left, top, iconSize, iconSize));

        



        left = Screen.width - iconSize - 10;
        displayMenuButton(MenuAction.NetworkSettings, networkTextureOn, networkTextureOff, new Rect(left, top, iconSize, iconSize));

        if (_menuAction == MenuAction.Sensors)
        {
            top = iconSize + iconSize / 2;
            left = 20;

            GUI.Box(new Rect(left - 10, top - 10, 200, _userTracker.Sensors.Count == 0 ? 50 : 65 * _userTracker.Sensors.Count), "");


            if (_userTracker.Sensors.Count > 0)
            {
				GUI.Label(new Rect(left, top, 200, 25), "Sensors:", _titleStyle);
				top += 35;

                foreach (string sid in _userTracker.Sensors.Keys)
                {
                    if (GUI.Button(new Rect(left, top, 20, 20), _userTracker.Sensors[sid].Active ? checkTexture : uncheckTexture, GUIStyle.none))
                    {
                        _userTracker.Sensors[sid].Active = !_userTracker.Sensors[sid].Active;
                    }
                    GUI.Label(new Rect(left + 40, top, 100, 25), sid);

                    top += 35;
                }
            }
            else
            {
                GUI.Label(new Rect(left, top, 1000, 40), "No connected sensors.");
            }
        }

        if (_menuAction == MenuAction.Settings)
        {
            top = iconSize + iconSize / 2;
            left = iconSize + 50;

            GUI.Box(new Rect(left, top - 10, 200, 100), "");
            left += 10;

            GUI.Label(new Rect(left, top, 500, 35), "Calibration: ", _titleStyle);
            top += 40;

            if (_userTracker.CalibrationStatus == CalibrationProcess.FindCenter)
            {  
                if (GUI.Button(new Rect(left, top, 150, 35), "(1/2) Find Center"))
                {
                    if (_userTracker.CalibrationStep1())
                    {
                        _userTracker.CalibrationStatus = CalibrationProcess.FindForward;
                    }
                }
            }
            else if (_userTracker.CalibrationStatus == CalibrationProcess.FindForward)
            {
                if (GUI.Button(new Rect(left, top, 150, 35), "(2/2) Find Forward"))
                {
                    _userTracker.CalibrationStatus = CalibrationProcess.FindCenter;
                    _userTracker.CalibrationStep2();
                    _menuAction = MenuAction.None;
                }
            }


        }

        if (_menuAction == MenuAction.NetworkSettings)
        {
            top = iconSize + iconSize / 2;
            left = Screen.width - 250;

            GUI.Box(new Rect(left, top - 10, 240, 140), "");
            left += 10;

            GUI.Label(new Rect(left, top, 200, 25), "Broadcast Settings:", _titleStyle);
            left += 10;
            top += 35;

            GUI.Label(new Rect(left, top, 150, 25), "Sensors port:");
            left += 100;

            TrackerProperties.Instance.listenPort = int.Parse(GUI.TextField(new Rect(left, top, 50, 20), "" + TrackerProperties.Instance.listenPort));
            left += 55;
            if (GUI.Button(new Rect(left, top, 50, 25), "Reset"))
            {
                _userTracker.resetListening();
                _userTracker.Save();

				DoNotify n = gameObject.GetComponent<DoNotify>();
				n.notifySend(NotificationLevel.INFO, "Udp Listening", "Listening to port " + TrackerProperties.Instance.listenPort, 2000);
            }
            top += 35;

            left = Screen.width - 250 + 20;
            GUI.Label(new Rect(left, top, 150, 25), "Broadcast port:");
            left += 100;

            TrackerProperties.Instance.broadcastPort = int.Parse(GUI.TextField(new Rect(left, top, 50, 20), "" + TrackerProperties.Instance.broadcastPort));
            left += 55;
            if (GUI.Button(new Rect(left, top, 50, 25), "Reset"))
            {
                _userTracker.resetBroadcast();
                _userTracker.Save();

				DoNotify n = gameObject.GetComponent<DoNotify>();
				n.notifySend(NotificationLevel.INFO, "Udp Broadcast", "Sending to port " + TrackerProperties.Instance.broadcastPort, 2000);
            }

            // Unicast Settings
            top += 80;
            left = Screen.width - 250;

            GUI.Box(new Rect(left, top - 10, 240, 140), "");
            left += 10;

            GUI.Label(new Rect(left, top, 200, 25), "Unicast Settings:", _titleStyle);
            left += 10;
            top += 35;

            int addressTextFieldSize = 110;
            newUnicastAddress = GUI.TextField(new Rect(left, top, addressTextFieldSize, 20), newUnicastAddress);
            GUI.Label(new Rect(left + addressTextFieldSize + 3, top, 10, 25), ":");
            newUnicastPort = GUI.TextField(new Rect(left + addressTextFieldSize + 10, top, 50, 20), newUnicastPort);
            left += addressTextFieldSize + 1 + 15 + 50;
            if (GUI.Button(new Rect(left, top, 40, 25), "Add"))
            {
                _userTracker.addUnicast(newUnicastAddress, newUnicastPort);
                newUnicastAddress = "";
                newUnicastPort = "";
            }

            left = Screen.width - 250 + 20;
            foreach(string ip in _userTracker.UnicastClients)
            {
                top += 30;
                GUI.Label(new Rect(left, top, 160, 25), ip);
                if (GUI.Button(new Rect(left, top, 20, 20), "R"))
                {
                    _userTracker.removeUnicast(ip);
                }
            }

        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.name.Contains("Human"))
                    {
                        _userTracker.showHumanBodies = int.Parse(hit.collider.gameObject.name.Remove(0, "Human ".Length));
                    }
                    else
                        _userTracker.showHumanBodies = -1;
                }
                else
                    _userTracker.showHumanBodies = -1;
            }
            else
                _userTracker.showHumanBodies = -1;
        }
    }

    void displayMenuButton(MenuAction button, Texture texon, Texture texoff, Rect rect)
    {
        if (GUI.Button(rect, _menuAction == button ? texon : texoff, GUIStyle.none))
            if (_menuAction == button) _menuAction = MenuAction.None;
            else _menuAction = button;
    }
}
