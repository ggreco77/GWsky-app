using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Component determining camera movement for the main camera.
/// </summary>
public class CameraRig : MonoBehaviour {
    // Maximum value of dealignment measured during tracking which is allowed for a sphere alignment
    // to be considered accurate enough
    public const int MAX_CAMERA_DEALIGNMENT = 700;
    // Camera interpolation between computed rotation and current rotation
    public const float CAMERA_SLERP = 0.05f;

    public const float MAX_ZOOM_SIZE = 60;
    public const float MIN_ZOOM_SIZE = 10;
    public const float ZOOM_MOUSE_MULTIPLIER = 8f;
    public const float ZOOM_TOUCH_MULTIPLIER = 0.2f;

    // Pure camera rotation (visual camera rotation is interpolated for a smoother visual effect)
    Quaternion _destination;
    // Camera rotation when tracking is first enabled
    Quaternion _tracking_initial_rot;
    // Whether dealignment due to camera rotation is being tracked
    bool _is_tracking = false;
    // List of tracked values
    readonly List<float> _tracking_list = new List<float>();

    // Last tracked value
    public float TrackingValue { get; private set; }

    // Pitch of the camera set via mouse
    float _pitch = 0.0f;
    // Yaw of the camera set via mouse
    float _yaw = 0.0f;

    /// <summary>
    /// Monobehaviour Method. Update is called once per frame.
    /// </summary>
    void Update() {
        // Rotate the camera, based on the movement type
        Rotate();

        // If tracking is enabled, track the camera movement
        if (_is_tracking)
            Track();
    }
    
    /// <summary>
    /// Rotates the camera.
    /// </summary>
    /// Should be called once per Unity's Update().
    void Rotate() {
        // If the device has a working gyroscope, rotate via gyroscope
        if (SensorExtension.TestGyroscope())
            RotateWithGyroscope();
        else
            // Otherwise, rotate via mouse (used for debugging)
            RotateWithMouse();
        // Zoom by either pinching or scroll wheel
        Zoom();
    }

    /// <summary>
    /// Rotates the camera around in order to match the rotation of the real-world gyroscope of the device.
    /// </summary>
    void RotateWithGyroscope() {
        // Set the destination camera rotation as the gyroscope attitude (with conversion from real-life to Unity reference systems)
        _destination = SOFConverter.RightToLeftHanded(Input.gyro.attitude);
        // Set the actual rotation of the camera as an interpolation between the current camera rotation and the gyroscope one
        transform.rotation = Quaternion.Slerp(transform.rotation, _destination, CAMERA_SLERP);
    }

    /// <summary>
    /// Rotates the camera around in order to match the position of the mouse.
    /// </summary>
    void RotateWithMouse() {
        // Increase the yaw by the mouse input on the X axis
        _yaw += Input.GetAxis("Mouse X");
        // Increase the pitch by the mouse input on the Y axis
        _pitch -= Input.GetAxis("Mouse Y");

        // Set the angle of the camera transform via pitch and yaw
        transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
    }

    void Zoom() {
        Camera camera = GetComponent<Camera>();
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView - Input.GetAxis("Mouse ScrollWheel") * ZOOM_MOUSE_MULTIPLIER, MIN_ZOOM_SIZE, MAX_ZOOM_SIZE);

        if (Input.touchCount == 2) {
            Touch touch_a = Input.GetTouch(0);
            Touch touch_b = Input.GetTouch(1);

            Vector2 prev_touch_a_pos = touch_a.position - touch_a.deltaPosition;
            Vector2 prev_touch_b_pos = touch_b.position - touch_b.deltaPosition;

            float prev_magnitude = (prev_touch_a_pos - prev_touch_b_pos).magnitude;
            float curr_magnitude = (touch_a.position - touch_b.position).magnitude;

            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView - (curr_magnitude - prev_magnitude) * ZOOM_TOUCH_MULTIPLIER, MIN_ZOOM_SIZE, MAX_ZOOM_SIZE);
        }
    }

    /// <summary>
    /// Starts dealignment tracking for sphere alignment.
    /// </summary>
    /// Tracking of the camera rotation is done to ensure that, while the sphere alignment algorithm is performed,
    /// the camera does not move or moves only very slightly. This is required for a correct alignment computation.
    public void StartTracking() {
        // Set the tracking to true
        _is_tracking = true;
        // Set the initial tracking rotation as the current camera rotation
        _tracking_initial_rot = transform.rotation;
    }

    /// <summary>
    /// 
    /// </summary>
    void Track() {
        // Add tracking distance to list
        _tracking_list.Add(System.Math.Abs(Quaternion.Angle(_tracking_initial_rot, _destination)));
    }

    /// <summary>
    /// Stops dealignment tracking for sphere alignment.
    /// </summary>
    /// <returns></returns>
    public bool StopTracking() {
        // Set the tracking to false
        _is_tracking = false;

        // Result of the tracking test
        bool test = true;

        // Set the last value of tracking as the sum of all the frame-by-frame dealignments
        TrackingValue = _tracking_list.Sum();
        // If the tracking value is beyond the maximum allowed, the test has failed (otherwise it succeeds by default)
        if (TrackingValue < MAX_CAMERA_DEALIGNMENT)
            test = false;

        // Clear the tracking list
        _tracking_list.Clear();

        // Return the test outcome
        return test;
    }

    void OnEnable() {
        GetComponent<Camera>().fieldOfView = MAX_ZOOM_SIZE;
    }

    void OnDisable() {
        GetComponent<Camera>().fieldOfView = MAX_ZOOM_SIZE;
    }
}
