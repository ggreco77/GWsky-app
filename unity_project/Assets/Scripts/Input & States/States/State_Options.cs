using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class State_Options : State {
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
    }

    public override void Exit()
    {
        master.BG_canvas.enabled = false;
        _canvas.enabled = false;
    }
}
