using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

/// <summary>
/// Static class containing methods related to functionalities of the compass.
/// </summary>
public static class CompassExtension {
    /// <summary>
    /// Computes the magnetic North heading as a 3D vector (angles in degrees) with tilt compensation.
    /// </summary>
    /// Tilt compensation is required because Unity's built-in compass reading is not compensated, thus
    /// Unity's heading is only correct if the phone is laid horizontally.
    /// See https://cache.freescale.com/files/sensors/doc/app_note/AN4248.pdf for the formulae used here.
    /// <param name="magnetometer">Raw magnetometer data</param>
    /// <param name="accelerometer">Raw gyroscope data</param>
    /// <returns></returns>
    public static Vector3 CompensatedHeading(Vector3 magnetometer, Vector3 accelerometer) {
        // Convert coordinate systems for coherence between formulae and phone sensors
        Vector3 mag = new Vector3(magnetometer.x, magnetometer.y, -magnetometer.z);
        Vector3 acc = new Vector3(accelerometer.y, -accelerometer.x, -accelerometer.z);

        // Compute phone roll (relative to gravity)
        float roll = (float) Math.Atan2(acc.y, acc.z);
        // Compute phone pitch (relative to gravity)
        float pitch = (float) Math.Atan(-acc.x /
                                        (acc.y * Math.Sin(roll) + acc.z * Math.Cos(roll)));
        // Compute jaw (relative to magnetic North)
        float yaw = (float)Math.Atan2(mag.z * Math.Sin(roll) - mag.y * Math.Cos(roll),
                                      mag.x * Math.Cos(pitch) + mag.y * Math.Sin(pitch) * Math.Sin(roll) + mag.z * Math.Sin(pitch) * Math.Cos(roll));

        // Return the computed angles, converted to degrees for convenience
        return new Vector3((float)MathExtension.ToDegrees(pitch),
                           (float)MathExtension.DegRestrict(MathExtension.ToDegrees(yaw) + 90),
                           (float)MathExtension.ToDegrees(roll));
    }
}
