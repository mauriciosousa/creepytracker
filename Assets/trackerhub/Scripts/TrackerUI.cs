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

    [Range(40, 100)]
    public int iconSize;

    private MenuAction _menuAction;

    private Tracker _userTracker;

    void Start () {
        iconSize = 60;
        _userTracker = gameObject.GetComponent<Tracker>();
        _menuAction = MenuAction.None;



    }
	
	
	void Update () {
        

        // clean sensors & clean bodies
	}

    void OnGUI()
    {
        int top = 5;
        int left = 20;

        displayMenuButton(MenuAction.Sensors, sensorTex_on, sensorTex_off, new Rect(left, top + iconSize / 2, iconSize, iconSize));
        left += iconSize + iconSize / 2;
        //displayMenuButton(MenuAction.Humans, humanTex_on, humanTex_off, new Rect(left, top, iconSize, iconSize));
        //left += iconSize + iconSize / 2;
        //displayMenuButton(MenuAction.Devices, deviceTex_on, deviceTex_off, new Rect(left, top, iconSize, iconSize));
        //left += iconSize + iconSize / 2;
        displayMenuButton(MenuAction.Settings, settingsTex_on, settingsTex_off, new Rect(left, top, iconSize, iconSize));


        if (_menuAction == MenuAction.Sensors)
        {
            top = 5 + 2*iconSize;
            left = 20;
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
            left = iconSize + 20;


            GUI.Label(new Rect(left, top, 500, 35), "Calibration");
            top += 50;

            if (_userTracker.calibrationStatus == CalibrationProcess.FindCenter)
            {  
                if (GUI.Button(new Rect(left, top, 200, 35), "(1/2) Find Center"))
                {
                    _userTracker.calibrationStatus = CalibrationProcess.FindForward;
                    _userTracker.findCenter();
                    //_menuAction = MenuAction.None;
                }
            }
            else if (_userTracker.calibrationStatus == CalibrationProcess.FindForward)
            {
                if (GUI.Button(new Rect(left, top, 200, 35), "(2/2) Find Forward"))
                {
                    _userTracker.calibrationStatus = CalibrationProcess.FindCenter;
                    _userTracker.findForward();
                    _menuAction = MenuAction.None;
                }
            }


        }




        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
            if (hit.collider != null)
                Debug.Log(hit.transform.gameObject.name);



    }

    void displayMenuButton(MenuAction button, Texture texon, Texture texoff, Rect rect)
    {
        if (GUI.Button(rect, _menuAction == button ? texon : texoff, GUIStyle.none))
            if (_menuAction == button) _menuAction = MenuAction.None;
            else _menuAction = button;
    }
}
