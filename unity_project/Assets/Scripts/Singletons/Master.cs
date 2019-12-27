using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Master : MonoBehaviour {

    public VirtualObjects vo;
    public CameraRig main_camera;
    public Transform sphere;
    public Transform north_sphere;
    public SphereAligner sphere_aligner;
    public LookUI look_UI;
    public GWEventDatabase GW_event_db;
    public StateMachine state_machine;
    public Canvas selection_canvas;
    public Canvas BG_canvas;
    public Canvas mainmenu_canvas;
    public Canvas credits_canvas;
    public Canvas options_canvas;
    public Canvas info_canvas;
    public SphereText sphere_text;

    // Application Initialization
    void Start () {
        //To allow the navigation bar to be shown.
        Screen.fullScreen = false;

        //Screen is always oriented horizontally, for better view and coding simplicity.
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        //Get a reference to each gameObject in the scene.
        vo = GameObject.Find("Virtual Objects").GetComponent<VirtualObjects>() as VirtualObjects;
        main_camera = GameObject.Find("Main Camera").GetComponent<CameraRig>() as CameraRig;
        sphere = GameObject.Find("Sphere").transform;
        north_sphere = GameObject.Find("North Sphere").transform;
        look_UI = new LookUI(this, GameObject.Find("LookAround UI").GetComponent<Canvas>());
        GW_event_db = GameObject.Find("GW Event Database").GetComponent<GWEventDatabase>() as GWEventDatabase;
        sphere_aligner = GameObject.Find("Sphere").GetComponent<SphereAligner>() as SphereAligner;
        state_machine = GameObject.Find("State Machine").GetComponent<StateMachine>() as StateMachine;
        selection_canvas = GameObject.Find("Selection Canvas").GetComponent<Canvas>() as Canvas;
        mainmenu_canvas = GameObject.Find("Main Menu Canvas").GetComponent<Canvas>() as Canvas;
        options_canvas = GameObject.Find("Options Canvas").GetComponent<Canvas>() as Canvas;
        credits_canvas = GameObject.Find("Credits Canvas").GetComponent<Canvas>() as Canvas;
        info_canvas = GameObject.Find("Info Canvas").GetComponent<Canvas>() as Canvas;
        BG_canvas = GameObject.Find("BG Canvas").GetComponent<Canvas>() as Canvas;
        sphere_text = GameObject.Find("Sphere Canvas").GetComponent<SphereText>() as SphereText;

        //Run initialization functions for the referenced gameObjects.
        sphere_aligner.Init(sphere, north_sphere, main_camera.gameObject.transform, look_UI);
        state_machine.Init(this, sphere_text);
        main_camera.Init();

        //Init Texts for sphere
        sphere_text.Init(GameObject.Find("Main Camera").GetComponent<Camera>());
        // Add cardinal points
        sphere_text.AddText("North Text", "North", new Vector2(0, 0), north_sphere);
        sphere_text.AddText("South Text", "South", new Vector2(180, 0), north_sphere);
        sphere_text.AddText("East Text", "East", new Vector2(90, 0), north_sphere);
        sphere_text.AddText("West Text", "West", new Vector2(270, 0), north_sphere);
        sphere_text.AddText("Bottom Text", "Zenith", new Vector2(0, -90), north_sphere);
        sphere_text.AddText("Top Text", "Nadir", new Vector2(0, 90), north_sphere);

        //Load all event summaries.
        GW_event_db.LoadEventSummaries();

        //Disable gameObjects as needed when the application begins.
        StartDisable();

        //Set initial application state (Main Menu)
        state_machine.IssueChangeState(new State_MainMenu(this));
    }

    //Function to disable gameObjects as needed when the application begins.
    public void StartDisable()
    {
        selection_canvas.enabled = false;
        look_UI.canvas.enabled = false;
        sphere_text.enabled = false;
        BG_canvas.enabled = false;
        mainmenu_canvas.enabled = false;
        options_canvas.enabled = false;
        credits_canvas.enabled = false;
        info_canvas.enabled = false;
    }
}

public struct LookUI
{
    public LookUI(Master master, Canvas canvas)
    {
        this.canvas = canvas;
        telescope_button = canvas.transform.Find("Change Telescope Button").gameObject.GetComponent<ChangeTelescopeButton>() as ChangeTelescopeButton;
        select_GW_button = canvas.transform.Find("Change GW Event Button").gameObject.GetComponent<Button>() as Button;
        info_button = canvas.transform.Find("Info Button").gameObject.GetComponent<Button>() as Button;
        calibration_required_text = canvas.transform.Find("Calibration Required Text").gameObject.GetComponent<TextMeshProUGUI>() as TextMeshProUGUI;

        select_GW_button.onClick.AddListener(delegate
        {
            //Change application state
            master.state_machine.IssueChangeState(new State_GWEventSelection(master));
        });

        info_button.onClick.AddListener(delegate
        {
            //Change application state
            master.state_machine.IssueChangeState(new State_LookAroundInfo(master));
        });
    }

    public void FirstCalibrationDone() {
        calibration_required_text.gameObject.SetActive(false);
    }

    public Canvas canvas;

    public ChangeTelescopeButton telescope_button;
    public Button select_GW_button;
    public Button info_button;
    public TextMeshProUGUI calibration_required_text;
}