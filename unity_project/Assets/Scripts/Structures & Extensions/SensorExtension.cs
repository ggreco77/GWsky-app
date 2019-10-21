using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public static class SensorExtension {
    public enum SensorAvailable { FALSE, TRUE, NOT_TESTED }
    const int LOCATIONSERVICE_WAIT = 20;

    static SensorAvailable _gyro_supported = SensorAvailable.NOT_TESTED;
    static SensorAvailable _location_supported = SensorAvailable.NOT_TESTED;

    public static void ResetLocationPermission()
    {
        _location_supported = SensorAvailable.NOT_TESTED;
    }

    /*void TestGyroscope():
    * Tests if the system running the app has a working gyroscope. */
    public static bool TestGyroscope()
    {
        if (_gyro_supported == SensorAvailable.NOT_TESTED)
        {
            if (!SystemInfo.supportsGyroscope)
            {
                _gyro_supported = SensorAvailable.FALSE;
                DebugMessages.Print("No Gyroscope has been detected!", DebugMessages.Colors.Warning);
            }
            else
            {
                _gyro_supported = SensorAvailable.TRUE;
                //Enable gyroscope usage
                Input.gyro.enabled = true;

                DebugMessages.PrintClear("No errors detected.", DebugMessages.Colors.Neutral);
            }
        }

        return Convert.ToBoolean((int)_gyro_supported);
    }

    //false in case of errors
    public static async Task<bool> UnityInputSensorsStart()
    {
        if (_location_supported == SensorAvailable.NOT_TESTED)
        {
            // First, check if user has location service enabled
            if (!Input.location.isEnabledByUser)
            {
                DebugMessages.Print("Warning! GPS is not enabled!", DebugMessages.Colors.Warning);
                _location_supported = SensorAvailable.FALSE;
                return false;
            }

            Input.compass.enabled = true;

            if (!Input.compass.enabled)
            {
                DebugMessages.Print("Warning! No Compass detected!", DebugMessages.Colors.Warning);
                _location_supported = SensorAvailable.FALSE;
                return false;
            }
        }

        if (_location_supported != SensorAvailable.FALSE)
        {
            // Start service before querying location
            Input.location.Start();
            // Wait until service initializes
            int max_wait = LOCATIONSERVICE_WAIT;

            while (Input.location.status == LocationServiceStatus.Initializing && max_wait > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                max_wait--;
            }

            // Service didn't initialize in 20 seconds
            if (max_wait <= 0)
            {
                DebugMessages.Print("Warning! GPS system timed out!", DebugMessages.Colors.Warning);
                return false;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                DebugMessages.Print("Warning! Failed to determine GPS Position!", DebugMessages.Colors.Warning);
                _location_supported = SensorAvailable.FALSE;
                return false;
            }
        }

        return true;
    }
    public static void UnityInputSensorsStop()
    {
        Input.location.Stop();
    }
}
