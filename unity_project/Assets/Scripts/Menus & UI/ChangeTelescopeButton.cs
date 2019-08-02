using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTelescopeButton : Button {

    int _tex_n = -1;
    Transform _sphere;
    EventData _event_data;

	public void Set(Transform sphere, GWEventDatabase GW_event_db)
    {
        _sphere = sphere;
        _event_data = GW_event_db.evt_data;

        EventData event_data = GW_event_db.evt_data;
        Button button = gameObject.GetComponent<Button>() as Button;

        //Preemptively clear button of listeners.
        button.onClick.RemoveAllListeners();

        //Set first image and setup listeners to this button for changing image.
        if (GW_event_db != null &&
            GW_event_db.GetEventNames().Length > 0)
        {   
            GW_event_db.ApplyEventPhotosphere(_sphere, event_data, 0);

            _tex_n = 0;
            DebugMessages.Print("Debug: Showing \"" + event_data.internal_name + ", Telescope " + _tex_n + "\".", DebugMessages.Colors.Neutral);

            button.onClick.AddListener(delegate
            {
                _tex_n = (_tex_n + 1) % _event_data.photospheres.Length;

                GW_event_db.ApplyEventPhotosphere(_sphere, event_data, _tex_n);
                DebugMessages.Print("Debug: Showing \"" + event_data.internal_name + ", Telescope " + _tex_n + "\".", DebugMessages.Colors.Neutral);
            });
        }
        else
            DebugMessages.Print("No events could be loaded!", DebugMessages.Colors.Error);
    }
}
