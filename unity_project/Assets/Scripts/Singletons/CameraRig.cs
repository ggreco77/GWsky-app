using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Linq;

public class CameraRig : MonoBehaviour
{
    bool _can_rotate = false;
    Quaternion _destination;

    Quaternion _trackingInitialPos;
    bool _isTracking = false;
    List<float> _trackingList = new List<float>();
    public float trackingLastValue;

    public void Init()
    {
        _can_rotate = SensorExtension.TestGyroscope();
    }

    public void Rotate()
    {
        if (_can_rotate)
            RotateWithGyroscope();
    }

    /*void RotateGyroscope():
    * Rotates the camera around its current position using a gyroscope. */
    void RotateWithGyroscope()
    {
        _destination = MathExtension.RightToLeftHanded(Input.gyro.attitude);
        transform.rotation = Quaternion.Slerp(transform.rotation, _destination, 0.05f);
    }

    public void StartTracking() {
        _isTracking = true;
        _trackingInitialPos = transform.rotation;
    }

    public bool IsTracking() {
        return _isTracking;
    }

    public void Track() {
        // Add tracking distance to list
        _trackingList.Add((float)System.Math.Abs(Quaternion.Angle(_trackingInitialPos, _destination)));
    }

    public bool StopTracking() {
        _isTracking = false;

        // Check whether linear acceleration is within reasonable standards
        bool test = true;

        trackingLastValue = _trackingList.Sum();
        if (trackingLastValue < Globals.MAX_CAMERA_DEALIGNMENT)
            test = false;

        _trackingList.Clear();

        return test;
    }

}
