using UnityEngine;

/// <summary>
/// Controls the application at the top-most level of abstraction.
/// </summary>
public class Master : MonoBehaviour {
    // Reference to the component moving the main camera
    CameraRig _camera;
    // Reference to the component aligning the spheres with the real world
    SphereAligner _sphere_aligner;
    // Reference to texts aligned with the spheres
    SphereText _sphere_texts;
    // Reference to state machine
    StateMachine _state_machine;
    // Reference to container object for UI elements
    UIContainer _UI_container;

    // Reference to photosphere
    Transform _sphere;
    // Reference to cardinal points sphere
    Transform _north_sphere;
    // Reference to event database
    EventDatabase _events_db;

    /// <summary>
    /// Monobehaviour Method. Awake is called for all GameObject instances before the game loop.
    /// </summary>
    void Awake() {
        // Fetch valid references to the required components
        _camera = GameObject.Find("Main Camera").GetComponent<CameraRig>();
        _sphere_aligner = GameObject.Find("Sphere Container").GetComponent<SphereAligner>();
        _sphere_texts = GameObject.Find("Sphere Text Canvas").GetComponent<SphereText>();
        _sphere = GameObject.Find("Sphere Container/Photosphere").transform;
        _north_sphere = GameObject.Find("Sphere Container/North Sphere").transform;
        _state_machine = GameObject.Find("State Machine").GetComponent<StateMachine>();
        _UI_container = GameObject.Find("UI Container").GetComponent<UIContainer>();
        _events_db = GameObject.Find("Event Database").GetComponent<EventDatabase>();
    }

    /// <summary>
    /// Monobehaviour Method. Start is called before the first frame.
    /// </summary>
    void Start() {
        // Initialize UI container
        _UI_container.Init(_state_machine, _events_db, _sphere, _camera, _sphere_aligner, _sphere_texts);

        // Initialize the sphere aligner component
        _sphere_aligner.Init(_camera.transform, _UI_container.LookAroundUI);
        
        // Initialize the sphere texts components
        _sphere_texts.Init(_camera.GetComponent<Camera>(), _sphere, _north_sphere);

        // Once all preparation is done, disable all objects to start with a clean slate
        DisableAll();

        // Set first state for state machine
        _state_machine.IssueChangeState(new State_PermissionFetch(_state_machine, _UI_container));
    }

    /// <summary>
    /// 
    /// </summary>
    void DisableAll() {
        // Disable all UI objects
        _UI_container.UISphere.gameObject.SetActive(false);
        _camera.enabled = false;
        _sphere_texts.gameObject.SetActive(false);
        _sphere_aligner.gameObject.SetActive(false);
    }
}
