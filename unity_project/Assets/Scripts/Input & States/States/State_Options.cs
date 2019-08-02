using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class State_Options : State
{
    public State_Options(Master master)
        : base(master)
    {}

    Canvas _canvas;

    public override void Enter()
    {
        _canvas = master.options_canvas;
        _canvas.enabled = true;
        master.BG_canvas.enabled = true;

        //Get Main Menu Button
        Button mainmenu_button = _canvas.transform.Find("To Main Menu Button").gameObject.GetComponent<Button>() as Button;
        mainmenu_button.onClick.RemoveAllListeners();
        mainmenu_button.onClick.AddListener(delegate
        {
            master.state_machine.IssueChangeState(new State_MainMenu(master));
        });

        Button GPS_button = _canvas.transform.Find("GPS/Ask GPS Permission").gameObject.GetComponent<Button>() as Button;
        GPS_button.onClick.RemoveAllListeners();
        #if !PLATFORM_ANDROID
            GPS_button.enabled = false;
        #else
            GPS_button.enabled = true;
            GPS_button.onClick.AddListener(delegate
            {
                    SensorExtension.ResetLocationPermission();
                    Permission.RequestUserPermission(Permission.CoarseLocation);
            });
        #endif

        TextMeshProUGUI GPS_text = _canvas.transform.Find("GPS/GPS Text").gameObject.GetComponent<TextMeshProUGUI>() as TextMeshProUGUI;
        string str_enabled = SensorExtension.UnityInputSensorsStart().Result ? "<color=\"green\">Enabled</color>" : "<color=\"red\">Disabled</color>";
        GPS_text.text = "GPS:   " + str_enabled;
    }

    public override void Exit()
    {
        master.BG_canvas.enabled = false;
        _canvas.enabled = false;
    }
}
