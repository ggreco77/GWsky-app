using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class State_LookAroundInfo : State {
    public State_LookAroundInfo(Master master)
        : base(master)
    {}

    public override void Update()
    {
        //Run update functions for the referenced gameObjects.
        master.main_camera.Rotate();
    }

    public override void Enter()
    {
        master.look_UI.canvas.enabled = true;
        master.info_canvas.enabled = true;
        master.look_UI.telescope_button.interactable = false;
        master.look_UI.select_GW_button.interactable = false;

        //Get Back Button
        Button back_button = master.info_canvas.transform.Find("Container/Back Button").gameObject.GetComponent<Button>() as Button;
        back_button.onClick.RemoveAllListeners();
        back_button.onClick.AddListener(delegate
        {
            master.state_machine.IssueChangeState(new State_LookAround(master));
        });
    }

    public override void Exit()
    {
        master.look_UI.telescope_button.interactable = true;
        master.look_UI.select_GW_button.interactable = true;
        master.look_UI.canvas.enabled = false;
        master.info_canvas.enabled = false;
    }
}
