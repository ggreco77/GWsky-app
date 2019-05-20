﻿using UnityEngine;
using System.Collections;
using TMPro;

public class CameraRig : MonoBehaviour
{
    bool _gyro_supported = false;
    DebugMessages _debug_messages;

    public float yaw = 0.0f;
    public float pitch = 0.0f;

    public void Init(DebugMessages debug_messages)
    {
        _debug_messages = debug_messages;
        TestGyroscope();
    }

    public void Rotate()
    {
        if (_gyro_supported)
            RotateGyroscope();
        else
            RotateMouse();
    }

    /*void TestGyroscope():
    * Tests if the system running the app has a working gyroscope. */
    void TestGyroscope()
    {
        if (!SystemInfo.supportsGyroscope)
        {
            _gyro_supported = false;
            _debug_messages.Print("No Gyroscope has been detected!", DebugMessages.Colors.Warning);
        }
        else
        {
            _gyro_supported = true;
            //Enable gyroscope usage
            Input.gyro.enabled = true;

            _debug_messages.PrintClear("No errors detected.", DebugMessages.Colors.Neutral);
        }
    }

    /*void RotateGyroscope():
    * Rotates the camera around its current position using a gyroscope. */
    void RotateGyroscope()
    {
        transform.rotation = RightToLeftHanded(Input.gyro.attitude);
    }

    void RotateMouse()
    {
        yaw += Input.GetAxis("Mouse X");
        pitch -= Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    Quaternion RightToLeftHanded(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

}
