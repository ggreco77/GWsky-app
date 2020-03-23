using UnityEngine;
using System.Threading.Tasks;
using System;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Static class providing methods for sensor handling, extending those of Unity.
/// </summary>
public static class SensorExtension {
    // States of sensor availability
    public enum SensorAvailable { FALSE, TRUE, NOT_TESTED }
    // Maximum number of seconds to wait for location service initialization
    const int LOCATIONSERVICE_WAIT = 20;

    // Mutex used for exclusive access to location sensors
    static int _lock = 1;

    // Whether the gyroscope sensor is available
    static SensorAvailable _gyro_supported = SensorAvailable.NOT_TESTED;
    // Whether GPS and location is available
    static SensorAvailable _location_supported = SensorAvailable.NOT_TESTED;

    /// <summary>
    /// Tests the availability of location sensors.
    /// </summary>
    /// <returns></returns>
    public static bool TestLocation() {
        // Return false if location has not been tested yet
        if (_location_supported == SensorAvailable.NOT_TESTED)
            return false;
        // Otherwise, return whether location is supported or not
        else
            return Convert.ToBoolean((int)_location_supported);
    }

    /// <summary>
    /// Tests if the system running the app has a working gyroscope.
    /// </summary>
    /// <returns></returns>
    public static bool TestGyroscope() {
        // If the sensor has not been tested yet...
        if (_gyro_supported == SensorAvailable.NOT_TESTED) {
            // If the system does not support a gyroscope...
            if (!SystemInfo.supportsGyroscope) {
                // Set the sensor availability to false
                _gyro_supported = SensorAvailable.FALSE;
                // Write an error message to debug
                Log.Print("No Gyroscope has been detected!", Log.Colors.Warning);
            }
            // If the sensor is supported...
            else {
                // Set the sensor availability to true
                _gyro_supported = SensorAvailable.TRUE;
                // Enable gyroscope usage
                Input.gyro.enabled = true;
            }
        }

        // Return the availability of the sensor
        return Convert.ToBoolean((int)_gyro_supported);
    }

    /// <summary>
    /// Starts usage of location input sensors (that is, GPS). Contextually tests the
    /// availability of those sensors.
    /// </summary>
    /// <returns></returns>
    public static async Task UnityLocationSensorsStart() {
        // Fetch the mutex key from the lock via an atomic operation
        int key = System.Threading.Interlocked.Exchange(ref _lock, 0);
        // If the critical section is free to run...
        if (key == 1) {
            // If the location services have not been tested yet...
            if (_location_supported == SensorAvailable.NOT_TESTED) {
                // If user does not have location enabled...
                if (!Input.location.isEnabledByUser) {
                    // Write a warning message to debug
                    Log.Print("Warning! GPS is not enabled!", Log.Colors.Warning);
                    // Set the location to NOT supported
                    _location_supported = SensorAvailable.FALSE;
                }
                #if PLATFORM_ANDROID
                // If android user has not enabled coarse location for this application...
                else if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)) {
                    // Write a warning message to debug
                    Log.Print("Warning! GPS is not enabled!", Log.Colors.Warning);
                    // Set the location to NOT supported
                    _location_supported = SensorAvailable.FALSE;
                }
                #endif
                else {
                    // Start service before querying location
                    Input.location.Start();
                    
                    // Max amount of wait until service initializes, in seconds
                    int max_wait = LOCATIONSERVICE_WAIT;

                    // Wait for the service to be initialized, up until a maximum amount
                    while (Input.location.status == LocationServiceStatus.Initializing && max_wait > 0) {
                        // At each loop iteration, wait for one second...
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        // And decrease the maximum wait amount by a unit
                        max_wait--;
                    }

                    // If service did not initialize in the maximum wait time...
                    if (max_wait <= 0) {
                        // Write a warning message to debug
                        Log.Print("Warning! GPS system timed out!", Log.Colors.Warning);
                        // Set the location to NOT supported
                        _location_supported = SensorAvailable.FALSE;
                    }
                    // Connection has failed
                    else if (Input.location.status == LocationServiceStatus.Failed) {
                        // Write a warning message to debug
                        Log.Print("Warning! Failed to determine GPS Position!", Log.Colors.Warning);
                        // Set the location to NOT supported
                        _location_supported = SensorAvailable.FALSE;
                    }
                    else {
                        // Enable the compass
                        Input.compass.enabled = true;

                        // If enabling the compass has failed (see Unity documentation)...
                        if (!Input.compass.enabled) {
                            // Write a warning message to debug
                            Log.Print("Warning! No Compass detected!", Log.Colors.Warning);
                            // Set the location to NOT supported
                            _location_supported = SensorAvailable.FALSE;
                        }
                        // Finally, if every check has succeeded...
                        else {
                            // Set the location to supported
                            _location_supported = SensorAvailable.TRUE;
                        }
                    }
                }
            }

            // Free the concurrency lock
            _lock = 1;
        }
        // If sensor cannot be started due to code already in critical section, throw the corresponding Exception
        else {
            throw new ConcurrencyException("Cannot start Unity location sensors before they are fully stopped!");
        }
    }

    /// <summary>
    /// Stops usage of location sensors.
    /// </summary>
    public static void UnityLocationSensorsStop() {
        // Fetch the mutex key from the lock via an atomic operation
        int key = System.Threading.Interlocked.Exchange(ref _lock, 0);
        // If the critical section is free to run...
        if (key == 1) {
            // Stop location services
            Input.location.Stop();
            // Reset location support
            _location_supported = SensorAvailable.NOT_TESTED;

            // Free the concurrency lock
            _lock = 1;
        }
        // If sensor cannot be started due to code already in critical section, throw the corresponding Exception
        else {
            throw new ConcurrencyException("Cannot stop Unity location sensors before they are fully activated!");
        }
    }
}
