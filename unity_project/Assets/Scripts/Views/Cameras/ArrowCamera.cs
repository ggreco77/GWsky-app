using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

class ArrowCamera : MonoBehaviour {
    public Transform Arrow { get; private set; }

    Camera _main_camera;
    EventDatabase _events_db;
    Transform _photosphere;
    LookAroundUI _look_around_UI;
    MeshRenderer _arrow_renderer;

    float _radius;
    Quaternion _prev_camera_rotation;

    const int CLOSENESS_FRAMES = 100;
    const float CLOSENESS_MAX_ANGLE = 20f;
    int _curr_closeness_counter = 0;

    public bool CheckCloseness { get; set; } = true;

    public void Init(Camera main_camera, EventDatabase events_db, Transform photosphere, LookAroundUI look_around_UI) {
        Arrow = transform.parent.Find("Arrow");
        _main_camera = main_camera;
        _events_db = events_db;
        _photosphere = photosphere;
        _look_around_UI = look_around_UI;

        _arrow_renderer = Arrow.Find("Arrow").GetComponent<MeshRenderer>();

        _radius = Math.Abs(transform.position.z);

        Disable();
    }

    void Update() {
        LookAtEvent();
        if (CheckCloseness)
            CheckForCloseness();
        else
            _curr_closeness_counter = 0;
    }

    void LookAtEvent() {
        Arrow.transform.forward = SOFConverter.EquirectangularToSphere(_events_db.GetLoadedEvent().ArrowPointsTo, _photosphere).normalized;

        if (_prev_camera_rotation == null)
            transform.forward = _main_camera.transform.forward;
        else {
            Quaternion difference = _main_camera.transform.rotation * Quaternion.Inverse(_prev_camera_rotation);
            transform.rotation = difference * transform.rotation;
        }

        _prev_camera_rotation = transform.rotation;
        transform.position = -_main_camera.transform.forward.normalized * _radius;
    }

    void CheckForCloseness() {
        Vector3 v1 = (_main_camera.transform.rotation * Vector3.forward).normalized;
        Vector3 v2 = SOFConverter.EquirectangularToSphere(_events_db.GetLoadedEvent().ArrowPointsTo, _photosphere).normalized;
        Vector3 axis = Vector3.Cross(v1, v2).normalized;

        float rot_angle = SOFConverter.RelativeAngleOnSphere(axis, v1, v2);
        if (Math.Abs(rot_angle) < CLOSENESS_MAX_ANGLE)
            _curr_closeness_counter++;
        else
            _curr_closeness_counter = 0;

        if (_curr_closeness_counter == CLOSENESS_FRAMES)
            _look_around_UI.StartCoroutine(_look_around_UI.DescriptionUnlock());
    }

    public IEnumerator EnableAnimation() {
        yield return StartCoroutine(AnimatorFunctions.LinearFade(false, _arrow_renderer.material, UI.FADE_TIME));
    }

    public IEnumerator DisableAnimation() {
        yield return StartCoroutine(AnimatorFunctions.LinearFade(true, _arrow_renderer.material, UI.FADE_TIME));
    }

    public void Disable() {
        _arrow_renderer.enabled = false;
        CheckCloseness = false;
    }

    public void Enable() {
        _arrow_renderer.enabled = true;
        CheckCloseness = true;
    }
}
