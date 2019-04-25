using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : MonoBehaviour {

    public CameraRig main_camera;
    public InputController input_controller;
    public Transform sphere;
    public DebugMessages debug_messages;
    public TextureButton texture_button;

	// Use this for initialization
	void Start () {
        //To allow the navigation bar to be shown.
        Screen.fullScreen = false;

        //Screen is oriented horizontally
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        //Get a reference to each gameObject in the scene.
        main_camera = GameObject.Find("Main Camera").GetComponent<CameraRig>() as CameraRig;
        input_controller = GameObject.Find("Input Controller").GetComponent<InputController>() as InputController;
        sphere = GameObject.Find("Sphere").transform;
        debug_messages = GameObject.Find("Debug Messages Canvas").GetComponent<DebugMessages>() as DebugMessages;
        texture_button = GameObject.Find("Change Texture Button Canvas").transform.Find("Button").gameObject.GetComponent<TextureButton>() as TextureButton;

        //Run initialization functions for the referenced gameObjects.
        debug_messages.Init();
        texture_button.Init(sphere);
        main_camera.Init(debug_messages);
        
	}
	
	// Update is called once per frame
	void Update () {

        //Run update functions for the referenced gameObjects.
        main_camera.Rotate();
        input_controller.CheckForExit();
	}
}
