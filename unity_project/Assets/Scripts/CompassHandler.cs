using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class CompassHandler {

    //Compensate compass heading (magnetic North)
    public static float CompensatedHeading(Vector3 magnetometer, Vector3 accelerometer)
    {
        // See https://cache.freescale.com/files/sensors/doc/app_note/AN4248.pdf for the formulae used here

        //Values are computed in radians, because of how the Math library operates
        float roll = (float) Math.Atan2(accelerometer.y, accelerometer.z);
        
        float pitch = (float) Math.Atan(-accelerometer.x /
                                        (accelerometer.y * Math.Sin(roll) + accelerometer.z * Math.Cos(roll)));
        float yaw = (float)Math.Atan2(magnetometer.z * Math.Sin(roll) - magnetometer.y * Math.Cos(roll),
                                       magnetometer.x * Math.Cos(pitch) + magnetometer.y * Math.Sin(pitch) * Math.Sin(roll) + magnetometer.z * Math.Sin(pitch) * Math.Cos(roll));

        //Convert result to degrees for convenience
        return (float) MathExtension.ToDegrees(yaw);
    }
}
