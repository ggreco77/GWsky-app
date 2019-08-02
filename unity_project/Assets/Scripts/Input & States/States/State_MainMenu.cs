using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class State_MainMenu : State {
    public State_MainMenu(Master master)
        : base(master)
    {
    }

    Canvas _canvas;

    public override void Enter()
    {
        _canvas = master.mainmenu_canvas;
        _canvas.enabled = true;
        master.BG_canvas.enabled = true;

        //Buttons in Main Menu.
        Button[] buttons = new Button[4];
        buttons[0] = _canvas.transform.Find("Select GW Event Button").gameObject.GetComponent<Button>() as Button;
        buttons[1] = _canvas.transform.Find("Options Button").gameObject.GetComponent<Button>() as Button;
        buttons[2] = _canvas.transform.Find("Credits Button").gameObject.GetComponent<Button>() as Button;
        buttons[3] = _canvas.transform.Find("Quit Button").gameObject.GetComponent<Button>() as Button;

        //Preemptively clear button of listeners.
        foreach (Button button in buttons)
            button.onClick.RemoveAllListeners();

        //Select GW Event Button
        buttons[0].onClick.AddListener(delegate
        {
            master.state_machine.IssueChangeState(new State_GWEventSelection(master));
        });

        //Options Button
        buttons[1].onClick.AddListener(delegate
        {
            master.state_machine.IssueChangeState(new State_Options(master));
        });

        //Credits Button
        buttons[2].onClick.AddListener(delegate
        {
            master.state_machine.IssueChangeState(new State_Credits(master));
        });

        //Quit Button
        buttons[3].onClick.AddListener(delegate
        {
            Application.Quit();
        });
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        //Disable canvases.
        _canvas.enabled = false;
        master.BG_canvas.enabled = false;
    }
}
