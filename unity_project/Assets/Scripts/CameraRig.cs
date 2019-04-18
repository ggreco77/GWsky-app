using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour
{
    void Update()
    {
        RotateMouse();
    }

    /*void RotateMouse():
     * Rotates the camera around its current position using mouse controls. */
    void RotateMouse()
    {
        //Get x and y inputs from Unity
        float y = Input.GetAxis("Mouse X");
        float x = Input.GetAxis("Mouse Y");
        
        //Create rotation vector
        Vector3 rotateValue = new Vector3(x, y * -1, 0);
        //Apply rotation vector
        transform.eulerAngles = transform.eulerAngles - rotateValue;
    }
}
