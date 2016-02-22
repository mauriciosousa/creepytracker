using UnityEngine;
using System.Collections;

enum MenuAction
{
    Settings,
    Sensors,
    Humans,
    Devices,
    None
}

public class TrackerUI : MonoBehaviour {

    public Texture checkTex;
    public Texture uncheckTex;

    public Texture deviceTex_on;
    public Texture deviceTex_off;

    public Texture humanTex_on;
    public Texture humanTex_off;

    public Texture sensorTex_on;
    public Texture sensorTex_off;

    public Texture settingsTex_on;
    public Texture settingsTex_off;

    public Texture network_connected;
    public Texture network_disconnected;
    public Texture network_receiving_only;
    public Texture network_sendinging_only;

    [Range(20, 100)]
    public int iconSize;

    private MenuAction _menuAction;

    private Tracker _userTracker;

    void Start()
    {
        _userTracker = gameObject.GetComponent<Tracker>();
        _menuAction = MenuAction.None;

        
    }
	
	void Update () {
        
	}

    void OnGUI()
    {
        int top = 5;
        int left = 20;

        displayMenuButton(MenuAction.Sensors, sensorTex_on, sensorTex_off, new Rect(left, top + iconSize / 2, iconSize, iconSize));
        GUI.Label(new Rect(left + iconSize, top + iconSize, 10, 25), "" + _userTracker.Sensors.Count);
        left += iconSize + iconSize / 2;
        
        //displayMenuButton(MenuAction.Devices, deviceTex_on, deviceTex_off, new Rect(left, top, iconSize, iconSize));
        //left += iconSize + iconSize / 2;
        displayMenuButton(MenuAction.Settings, settingsTex_on, settingsTex_off, new Rect(left, top, iconSize, iconSize));


        if (_menuAction == MenuAction.Sensors)
        {
            top = 5 + 2*iconSize;
            left = 20;

            GUI.Box(new Rect(left - 10, top - 10, 200, _userTracker.Sensors.Count == 0 ? 50 : 50 * _userTracker.Sensors.Count), "");

            if (_userTracker.Sensors.Count > 0)
            {
                foreach (string sid in _userTracker.Sensors.Keys)
                {
                    if (GUI.Button(new Rect(left, top, 20, 20), _userTracker.Sensors[sid].Active ? checkTex : uncheckTex, GUIStyle.none))
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
            top = 5 + 2 * iconSize;
            left = iconSize + 50;

            GUI.Box(new Rect(left, top - 10, 200, 100), "");
            left += 10;

            GUI.Label(new Rect(left, top, 500, 35), "Calibration: ");
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




        //RaycastHit hit;
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out hit))
        //    if (hit.collider != null)
        //    {
        //        Debug.Log(hit.transform.gameObject.name);
        //    }


    }

    void displayMenuButton(MenuAction button, Texture texon, Texture texoff, Rect rect)
    {
        if (GUI.Button(rect, _menuAction == button ? texon : texoff, GUIStyle.none))
            if (_menuAction == button) _menuAction = MenuAction.None;
            else _menuAction = button;
    }
}
