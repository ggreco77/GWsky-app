using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class State_GWEventSelection : State {
    public State_GWEventSelection(Master master)
        : base(master)
    {
    }

    Canvas _canvas;
    GameObject _grid;

    public override void Enter()
    {
        _canvas = master.selection_canvas;
        _canvas.enabled = true;
        master.BG_canvas.enabled = true;

        //Get Grid object on canvas, which must the parent of all the buttons.
        _grid = _canvas.transform.Find("List Space/Grid").gameObject;

        //For each event summary...
        for (int i = 0; i < master.GW_event_db.evt_summaries.Length; i++)
        {
            EventSummary summary = master.GW_event_db.evt_summaries[i];

            //Create a new button and add it to the grid.
            EventButton button = GameObject.Instantiate(master.vo.eventButton, _grid.transform).GetComponent<EventButton>() as EventButton;
            button.Init(master, i, master.GW_event_db.evt_summaries[i].name);
        }

        //Get Main Menu Button
        Button mainmenu_button = _canvas.transform.Find("To Main Menu Button").gameObject.GetComponent<Button>() as Button;
        mainmenu_button.onClick.RemoveAllListeners();
        mainmenu_button.onClick.AddListener(delegate
        {
            master.state_machine.IssueChangeState(new State_MainMenu(master));
        });
    }

    public override void Update()
    {   
    }

    public override void Exit()
    {
        //Disable canvas.
        _canvas.enabled = false;

        //Delete all grid children (the buttons).
        foreach (Transform child in _grid.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        master.BG_canvas.enabled = false;
    }
}
