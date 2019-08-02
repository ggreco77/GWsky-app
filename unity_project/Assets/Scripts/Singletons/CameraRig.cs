using UnityEngine;
using System.Collections;
using TMPro;

public class CameraRig : MonoBehaviour
{
    bool _can_rotate = false;

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
        transform.rotation = MathExtension.RightToLeftHanded(Input.gyro.attitude);
    }

}
