using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_GWEventSelection : State {
    public State_GWEventSelection(Master master)
        : base(master)
    {}

    public override void Update()
    {
        //Run update functions for the referenced gameObjects.
        master.main_camera.Rotate();
    }
}
