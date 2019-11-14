using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class CompassHandler {

    //Compensate compass heading (magnetic North)
    public static Vector3 CompensatedHeading(Vector3 magnetometer, Vector3 accelerometer) {
        // See https://cache.freescale.com/files/sensors/doc/app_note/AN4248.pdf for the formulae used here

        // Convert coordinate systems for sensors
        Vector3 mag = new Vector3(magnetometer.x, magnetometer.y, -magnetometer.z);
        Vector3 acc = new Vector3(accelerometer.y, -accelerometer.x, -accelerometer.z);

        //Values are computed in radians, because of how the Math library operates
        float roll = (float) Math.Atan2(acc.y, acc.z);
        
        float pitch = (float) Math.Atan(-acc.x /
                                        (acc.y * Math.Sin(roll) + acc.z * Math.Cos(roll)));
        float yaw = (float)Math.Atan2(mag.z * Math.Sin(roll) - mag.y * Math.Cos(roll),
                                      mag.x * Math.Cos(pitch) + mag.y * Math.Sin(pitch) * Math.Sin(roll) + mag.z * Math.Sin(pitch) * Math.Cos(roll));

        //Convert result to degrees for convenience
        return new Vector3((float)MathExtension.ToDegrees(pitch),
                           (float)MathExtension.DegRestrict(MathExtension.ToDegrees(yaw) + 90),
                           (float)MathExtension.ToDegrees(roll));
    }
}
