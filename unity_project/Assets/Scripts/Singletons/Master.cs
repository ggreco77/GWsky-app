﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Master : MonoBehaviour {

    public VirtualObjects vo;
    public CameraRig main_camera;
    public Transform sphere;
    public DebugMessages debug_messages;
    public LookUI look_UI;
    public GWEventDatabase GW_event_db;
    public StateMachine state_machine;
    public Canvas selection_canvas;
    public Canvas BG_canvas;

	// Application Initialization
	void Start () {
        //To allow the navigation bar to be shown.
        Screen.fullScreen = false;

        //Screen is always oriented horizontally, for better view.
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        //Get a reference to each gameObject in the scene.
        vo = GameObject.Find("Virtual Objects").GetComponent<VirtualObjects>() as VirtualObjects;
        main_camera = GameObject.Find("Main Camera").GetComponent<CameraRig>() as CameraRig;
        sphere = GameObject.Find("Sphere").transform;
        debug_messages = GameObject.Find("Debug Messages Canvas").GetComponent<DebugMessages>() as DebugMessages;
        look_UI = new LookUI(this, GameObject.Find("LookAround UI").GetComponent<Canvas>());
        GW_event_db = GameObject.Find("GW Event Database").GetComponent<GWEventDatabase>() as GWEventDatabase;
        state_machine = GameObject.Find("State Machine").GetComponent<StateMachine>() as StateMachine;
        selection_canvas = GameObject.Find("Selection Canvas").GetComponent<Canvas>() as Canvas;
        BG_canvas = GameObject.Find("BG Canvas").GetComponent<Canvas>() as Canvas;

        //Run initialization functions for the referenced gameObjects.
        state_machine.Init(this);
        debug_messages.Init();
        main_camera.Init(debug_messages);
        GW_event_db.Init(debug_messages);

        //Load all event summaries.
        GW_event_db.LoadEventSummaries();

        //Disable gameObjects as needed when the application begins.
        StartDisable();

        //Set initial application state
        state_machine.IssueChangeState(new State_GWEventSelection(this));
    }

    //Function to disable gameObjects as needed when the application begins.
    public void StartDisable()
    {
        selection_canvas.enabled = false;
        look_UI.canvas.enabled = false;
        BG_canvas.enabled = false;

    }
}

public struct LookUI
{
    public LookUI(Master master, Canvas canvas)
    {
        this.canvas = canvas;
        telescope_button = canvas.transform.Find("Change Telescope Button").gameObject.GetComponent<ChangeTelescopeButton>() as ChangeTelescopeButton;
        select_GW_button = canvas.transform.Find("Change GW Event Button").gameObject.GetComponent<Button>() as Button;

        select_GW_button.onClick.AddListener(delegate
        {
            //Change application state
            master.state_machine.IssueChangeState(new State_GWEventSelection(master));
        });
    }

    public Canvas canvas;

    public ChangeTelescopeButton telescope_button;
    public Button select_GW_button;
}