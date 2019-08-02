using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventButton : MonoBehaviour {

    GWEventDatabase _GW_event_db = null;
    int _index = -1;

    /* Fits the element width to the grid width, so that the element fills the canvas horizontally.
     * Function must be called once due to Unity using rt.sizeDelta to fit elements, which in this case
     * would produce this element to have width 0 because the grid is also dynamically fitted. */
    public void FitGridWidth()
    {
        /* Get the element and parent RectTransforms. */
        RectTransform rt = (RectTransform)gameObject.transform;
        RectTransform parent_rt = (RectTransform)transform.parent.transform;

        /* Fit horizontally using the parent width, do not touch height. */
        rt.sizeDelta = new Vector2(parent_rt.rect.width, rt.sizeDelta.y);
    }

    public void Init(Master master, int index, string name)
    {
        //copy parameters into local variables.
        _GW_event_db = master.GW_event_db;
        _index = index;

        FitGridWidth();

        Transform sphere = master.sphere;
        Button button = gameObject.GetComponent<Button>() as Button;

        TextMeshProUGUI text = button.gameObject.transform.Find("Text").GetComponent< TextMeshProUGUI>() as TextMeshProUGUI;
        text.text = name;

        //Add delegate function for whenever the button is pressed.
        button.onClick.AddListener(delegate
        {
            if (_GW_event_db != null &&
                _GW_event_db.GetEventNames().Length > 0)
            {
                string event_name = _GW_event_db.GetEventNames()[_index];
                _GW_event_db.evt_data = _GW_event_db.FetchEvent(event_name);
                _GW_event_db.ApplyEventPhotosphere(sphere, _GW_event_db.evt_data, 0);
                master.look_UI.telescope_button.Set(sphere, _GW_event_db);

                //Change application state (exit menu).
                master.state_machine.IssueChangeState(new State_LookAround(master));
            }
            else
                DebugMessages.Print("No events could be loaded!", DebugMessages.Colors.Error);
        });
    }
}
