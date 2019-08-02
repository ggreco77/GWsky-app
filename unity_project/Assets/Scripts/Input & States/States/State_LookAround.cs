using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_LookAround : State {
    public State_LookAround(Master master)
        : base(master)
    {}

    public override void Update()
    {
    }

    public override void Enter()
    {
        master.look_UI.telescope_button.gameObject.transform.parent.GetComponent<Canvas>().enabled = true;

    }

    public override void Exit()
    {
        master.look_UI.telescope_button.gameObject.transform.parent.GetComponent<Canvas>().enabled = false;
    }
}
