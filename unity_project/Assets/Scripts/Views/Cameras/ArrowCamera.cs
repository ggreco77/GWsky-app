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
    AudioSource _audio_source;

    float _radius;
    Quaternion _prev_camera_rotation;

    const int CLOSENESS_FRAMES = 100;
    const float CLOSENESS_MAX_ANGLE = 20f;
    int _curr_closeness_counter = 0;

    (float, float) _blip_frames = (15, 70);
    int _curr_blip_counter;

    public bool ClosenessCheck { get; set; } = true;

    public void Init(Camera main_camera, EventDatabase events_db, Transform photosphere, LookAroundUI look_around_UI) {
        Arrow = transform.parent.Find("Arrow");
        _main_camera = main_camera;
        _events_db = events_db;
        _photosphere = photosphere;
        _look_around_UI = look_around_UI;

        _arrow_renderer = Arrow.Find("Arrow").GetComponent<MeshRenderer>();
        _audio_source = GetComponent<AudioSource>();
        _radius = Math.Abs(transform.position.z);

        Disable();
    }

    void Update() {
        LookAtEvent();
        if (ClosenessCheck)
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

        float rot_angle = MathExtension.RelativeAngleOnSphere(axis, v1, v2);

        // Play "blip" sound
        if (_curr_blip_counter == 0) {
            _curr_blip_counter = (int)Math.Round(Mathf.Lerp(_blip_frames.Item1, _blip_frames.Item2, rot_angle / 180));
            _audio_source.Stop();
            _audio_source.Play();
        }
        _curr_blip_counter--;

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
        ClosenessCheck = false;
        _curr_blip_counter = -1;
    }

    public void Enable() {
        _arrow_renderer.enabled = true;
        ClosenessCheck = true;
        _curr_blip_counter = 0;
    }
}
