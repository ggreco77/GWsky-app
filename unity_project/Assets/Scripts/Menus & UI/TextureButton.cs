using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureButton : MonoBehaviour {

    int _tex_n = -1;
    Transform _sphere;
    GWEventHandler _GW_event_handler;
    DebugMessages _debug_messages;

	public void Init(Transform sphere, GWEventHandler GW_event_handler, DebugMessages debug_messages)
    {
        _sphere = sphere;
        _GW_event_handler = GW_event_handler;
        _debug_messages = debug_messages;
        Button button = gameObject.GetComponent<Button>() as Button;
        Renderer renderer = _sphere.gameObject.GetComponent<Renderer>() as Renderer;

        if (_GW_event_handler != null &&
            _GW_event_handler.GetEventNames().Length > 0)
        {
            string event_name = _GW_event_handler.GetEventNames()[0];
            EventData event_data = _GW_event_handler.FetchEvent(event_name);
            _GW_event_handler.ApplyEventPhotosphere(sphere, event_data);
            _debug_messages.Print("Debug: Showing \"" + event_data.internal_name + "\".", DebugMessages.Colors.Neutral);

            _tex_n = 0;

            button.onClick.AddListener(delegate
            {
                _tex_n = (_tex_n + 1) % _GW_event_handler.GetEventNames().Length;
                event_name = _GW_event_handler.GetEventNames()[_tex_n];

                event_data = _GW_event_handler.FetchEvent(event_name);
                _GW_event_handler.ApplyEventPhotosphere(sphere, event_data);
                _debug_messages.Print("Debug: Showing \"" + event_data.internal_name + "\".", DebugMessages.Colors.Neutral);
            });
        }
        else
            _debug_messages.Print("No events could be loaded!", DebugMessages.Colors.Error);
    }
}
