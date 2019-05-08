using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : MonoBehaviour {

    public CameraRig main_camera;
    public Transform sphere;
    public DebugMessages debug_messages;
    public TextureButton texture_button;
    public GWEventHandler GW_event_handler;
    public StateMachine state_machine;

	// Application Initialization
	void Start () {
        //To allow the navigation bar to be shown.
        Screen.fullScreen = false;

        //Screen is always oriented horizontally, for better view.
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        //Get a reference to each gameObject in the scene.
        main_camera = GameObject.Find("Main Camera").GetComponent<CameraRig>() as CameraRig;
        sphere = GameObject.Find("Sphere").transform;
        debug_messages = GameObject.Find("Debug Messages Canvas").GetComponent<DebugMessages>() as DebugMessages;
        texture_button = GameObject.Find("Change Texture Button Canvas").transform.Find("Button").gameObject.GetComponent<TextureButton>() as TextureButton;
        GW_event_handler = GameObject.Find("Event Handler").GetComponent<GWEventHandler>() as GWEventHandler;
        state_machine = GameObject.Find("State Machine").GetComponent<StateMachine>() as StateMachine;

        //Run initialization functions for the referenced gameObjects.
        state_machine.Init(this);
        debug_messages.Init();
        main_camera.Init(debug_messages);
        GW_event_handler.Init(debug_messages);
        texture_button.Init(sphere, GW_event_handler, debug_messages);

        //Set initial application state
        state_machine.IssueChangeState(new State_LookAround(this));
    }
}
