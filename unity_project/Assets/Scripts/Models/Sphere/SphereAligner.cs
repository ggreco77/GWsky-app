using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
/* NOTE: A task is an asynchronous function which may be redirected to the same or a different thread.
         In Unity's case, tasks can only be executed on the main thread whenever they contain any reference to any
         class or variable in UnityEngine (since Unity is not thread-safe), as is in our case. */
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;

/// <summary>
/// Component aligning a photosphere with the real-world star locations, based on the device's sensors.
/// Also aligns a sphere of cardinal points.
/// </summary>
/// Component requires three child spheres to work: a photosphere, a cardinal points sphere, and an empty sphere copy of the first one.
class SphereAligner : MonoBehaviour {
    // Minimum and maximum numbers of compass reading before the compass values can be accepted.
    // This is done because the compass is inherently imprecise and often has wild variance
    const int MIN_COMPASS_READINGS = 3;
    const int MAX_COMPASS_READINGS = 12;

    // Minimum compass accuracy in degrees, based on Unity reading
    const int MIN_COMPASS_ACCURACY = 2;

    // Maximum number of alignment attempts before dropping out
    const int MAX_ATTEMPTS = 5;
    // Current number of alignment attempts
    int _curr_attempts = 0;
    // Whether UTC time needs to be queried again
    bool _query_UTC = true;

    // Radius of the photosphere to align
    float _sphere_radius;

    // Coordinates of the two points in the ICRS system used for photosphere alignment.
    // First point is chosen as one of the poles to ensure most precise positioning of the poles,
    // Second point MUST be chosen close enough to first one (see sphere alignment methods)
    Vector2 _p_a = new Vector2(0, 90);
    Vector2 _p_b = new Vector2(0, 89.999f);

    // Reference to photosphere to align
    Transform _sphere;
    // Reference to cardinal points sphere
    Transform _north_sphere;

    // Pure transform of the photosphere to align
    Transform _sphere_destination;
    // Pure transform of the cardinal points sphere to align
    Transform _north_sphere_destination;
    // Pure transform of the empty clone sphere to align (used for point b)
    Transform _sphere_b;

    // Reference to camera object in the scene
    Transform _camera;
    // Reference to the UI of the application while looking at the photosphere
    LookAroundUI _lookaround_UI;

    // Boolean to keep track of errors while computing sphere alignment
    bool _error = false;
    // Mutex for exclusive access to function which computes sphere alignment
    int _lock = 1;
    // Whether to repeat sphere alignment as soon as possible after completion of the current alignment attempt
    bool _requeue = false;

    // These fields are externally visible since they may be useful for logs or to be shown to the user
    //Universal Time
    public DateTime UTC { get; private set; }
    //Greenwhich Mean Sidereal Time
    public TimeSpan GMST { get; private set; }
    //Local (Mean) Sidereal Time
    public TimeSpan LST { get; private set; }
    //Local Hour Angle
    public double LHA { get; private set; }
    //GPS, in format (latitude, longitude)
    public Vector2 GPS { get; private set; }
    //North deviation from Magnetic to True heading
    public float NorthDev { get; private set; }
    //Heading to True North as a 3D Vector of roll, yaw, pitch
    public Vector3 RelNorth { get; private set; }
    //Equatorial->Local Coordinates Converted

    public float SecondRotAngle { get; private set; }

    public Vector2 Az_H { get; private set; }

    /// <summary>
    /// Init method, should be called by Master. Unfortunately Constructors do not work well with Unity, so a Init function has to be called.
    /// </summary>
    /// <param name="camera"></param>
    public void Init(Transform camera, LookAroundUI lookaround_UI) {
        // Fetch references to the three required spheres in Unity scene
        _sphere = transform.Find("Photosphere");
        _north_sphere = transform.Find("North Sphere");

        // Pass reference from parameters to local variables
        _camera = camera;
        _lookaround_UI = lookaround_UI;

        // Initialize the Universal time as a default value
        UTC = new DateTime(2000, 1, 1);

        // Create a sphere B GameObject, child of the GameObject holding this component,
        // used for positioning of the second point
        _sphere_b = new GameObject("Sphere B").transform;
        _sphere_b.position = _sphere.position;
        _sphere_b.rotation = _sphere.rotation;
        _sphere_b.localScale = _sphere.localScale;
        _sphere_b.parent = transform;
        // Add the sphere mesh to the clone sphere
        MeshFilter filter = _sphere_b.gameObject.AddComponent<MeshFilter>();
        filter.mesh = Resources.Load<Mesh>("3D Models/Sphere");

        // Create a destination sphere GameObject, child of the GameObject holding this component,
        // onto which to attach the pure transform of the photosphere. Copy to this transform the transform of the sphere
        _sphere_destination = new GameObject("Sphere Destination").transform;
        _sphere_destination.position = _sphere.position;
        _sphere_destination.rotation = _sphere.rotation;
        _sphere_destination.localScale = _sphere.localScale;
        _sphere_destination.parent = transform;

        // Create a destination sphere GameObject, child of the GameObject holding this component,
        // onto which to attach the pure transform of the cardinal points sphere. Copy to this transform the transform of the sphere
        _north_sphere_destination = new GameObject("North Sphere Destination").transform;
        _north_sphere_destination.position = _north_sphere.position;
        _north_sphere_destination.rotation = _north_sphere.rotation;
        _north_sphere_destination.localScale = _north_sphere.localScale;
        _north_sphere_destination.parent = transform;

        // Fetch the sphere radius as the scale of the photosphere
        _sphere_radius = _sphere.localScale.x;
    }

    /// <summary>
    /// Contact NIST server and retrieve UTC time.
    /// </summary>
    /// Asynchronous because it has to wait the server response.
    /// <returns></returns>
    async Task<DateTime> GetNISTDate() {
        UTC = default;
        // URL of the server to contact as a string
        string server = "time.nist.gov";
        // response body of the server
        string string_response;

        // Attempt a connection...
        try {
            // Create a new TCP client
            TcpClient tcpClient = new TcpClient();
            // connect Asynchronously to the server, port 13
            var task = tcpClient.ConnectAsync(server, 13);
            // Await closure of connection OR a 3 seconds timeout
            Task completed_task = await Task.WhenAny(task, Task.Delay(3000));
            // If there hasn't been a timeout...
            if (completed_task == task) {
                // Get the stream from the connection
                var reader = new StreamReader(tcpClient.GetStream());
                // Get the body to the response string from the stream
                string_response = reader.ReadToEnd();
                // Close the stream (and thus the connection)
                reader.Close();
            }
            // Otherwise...
            else {
                // Print a warning message
                Log.Print("Warning! Could not Connect to NIST Server. Timeout.", Log.Colors.Warning);
                // Set internal error status to true
                _error = true;
                // Set to requery time
                _query_UTC = true;
                // Return
                return UTC;
            }
        }
        // If there has been some uncaught kind of exception...
        catch (Exception e) {
            // Print a warning message displaying the message of the exception
            Log.Print("Warning! Could not Connect to NIST Server. " + e.Message, Log.Colors.Warning);
            // Set internal error status to true
            _error = true;
            // Set to requery time
            _query_UTC = true;
            // Return
            return UTC;
        }

        // If the UTC signature is in the response string...
        if (string_response.Length > 47
            && string_response.Substring(38, 9).Equals("UTC(NIST)")) {
            // Parse the date into appropriate variables
            int jd = int.Parse(string_response.Substring(1, 5));
            int yr = int.Parse(string_response.Substring(7, 2));
            int mo = int.Parse(string_response.Substring(10, 2));
            int dy = int.Parse(string_response.Substring(13, 2));
            int hr = int.Parse(string_response.Substring(16, 2));
            int mm = int.Parse(string_response.Substring(19, 2));
            int sc = int.Parse(string_response.Substring(22, 2));

            // Compensate the number of years (this is a convention)
            if (jd > 51544)
                yr += 2000;
            else
                yr += 1999;

            // Copy the UTC into the appropriate variable
            UTC = new DateTime(yr, mo, dy, hr, mm, sc);
            // Since time has been fetched, no need to fetch it again in the time vicinity
            _query_UTC = false;
        }
        // If for some reason signature is not there...
        else {
            // Print a warning message displaying the message of the exception
            Log.Print("Warning! Could not retrieve NIST Time. Unexpected server response body!", Log.Colors.Warning);
            // Set internal error status to true
            _error = true;
            // Set to requery time
            _query_UTC = true;
            // Return
            return UTC;
        }

        // Return a valid UTC time
        return UTC;
    }

    /// <summary>
    /// Contact NOAA server and retrieve Magnetic North deviation.
    /// </summary>
    /// Asynchronous because it has to wait the server response.
    /// <returns></returns>
    async Task<float> GetNorthDeviation(float lat, float lon) {
        // URL of the server to contact as a string
        string server = "http://www.ngdc.noaa.gov/geomag-web/calculators/calculateDeclination";
        // Build query string
        string query = server + "?lat1=" + lat.ToString("0.000000") + "&lon1=" + lon.ToString("0.000000") + "&resultFormat=csv";
        // response body of the server
        string string_response;

        // North deviation westwards
        float deviation = 0;

        // Attempt a connection...
        try {
            // Create a new HTTP Web Request
            HttpWebRequest request = WebRequest.Create(new Uri(query)) as HttpWebRequest;
            request.Method = "GET";
            // connect Asynchronously to the server
            var task = Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            // Await closure of connection OR a 3 seconds timeout
            Task completed_task = await Task.WhenAny(task, Task.Delay(5000));
            // If there hasn't been a timeout...
            if (completed_task == task) {
                // Get the stream from the connection
                var reader = new StreamReader(task.Result.GetResponseStream());
                // Get the body to the response string from the stream
                string_response = reader.ReadToEnd();
                // Close the stream (and thus the connection)
                reader.Close();
            }
            // Otherwise...
            else {
                // Print a warning message
                Log.Print("Warning! Could not Connect to NOAA Server. Timeout.", Log.Colors.Warning);
                // Set internal error status to true
                _error = true;
                // Return
                return deviation;
            }
        }
        // If there has been some uncaught kind of exception...
        catch (Exception e) {
            // Print a warning message displaying the message of the exception
            Log.Print("Warning! Could not Connect to NOAA Server. " + e.Message, Log.Colors.Warning);
            // Set internal error status to true
            _error = true;
            // Return
            return deviation;
        }

        // If the UTC signature is in the response string...
        if (string_response.Substring(72, 18) == "Declination Values") {
            while (string_response[0] == '#') {
                var lines = Regex.Split(string_response, "\r\n|\r|\n").Skip(1);
                string_response = string.Join(Environment.NewLine, lines.ToArray());
            }
            Debug.Log(string_response);
            string[] values = string_response.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
            deviation = float.Parse(values[4]);
        }
        // If for some reason signature is not there...
        else {
            // Print a warning message displaying the message of the exception
            Log.Print("Warning! Could not retrieve NIST Time. Unexpected server response body!", Log.Colors.Warning);
            // Set internal error status to true
            _error = true;
            // Return
            return deviation;
        }

        // Return a valid UTC time
        return deviation;
    }

    /// <summary>
    /// Start location service, retrieve GPS and North True Heading, close location service.
    /// </summary>
    /// Asynchronous because of location service initialization and compass reading.
    /// <returns>Returns values as a tuple containing the GPS and North Heading.</returns>
    async Task<Tuple<Vector2, Vector3>> GetGPSAndRelativeNorth() {
        // Initialize the GPS and North Heading to empty vectors
        GPS = new Vector2(0, 0);
        RelNorth = new Vector3(0, 0, 0);

        // Starts location service through Unity, checks GPS and compass availability. Function is asynchronous
        await SensorExtension.UnityLocationSensorsStart();
        // After enabling the service, its status is tested to ensure location can be read
        _error = !SensorExtension.TestLocation();

        // If there hasn't been an error...
        if (!_error) {
            // Get GPS data
            GPS = new Vector2(Input.location.lastData.latitude,
                              Input.location.lastData.longitude);

            // Wait for a good compass reading. Asynchronous function
            await WaitGoodCompassReading();
        }

        // Stop service after query
        SensorExtension.UnityLocationSensorsStop();

        // Return both GPS and True North Heading
        return new Tuple<Vector2, Vector3>(GPS, RelNorth);
    }

    /// <summary>
    /// Keeps querying compass until a good reading is found. Gets the median value from the list of compass results.
    /// </summary>
    /// <returns></returns>
    async Task WaitGoodCompassReading() {
        // Initialize list of headings
        List<Vector3> norths = new List<Vector3>();
        // Number of attempts made
        int count = 0;

        // Get magnetic deviation from true north
        await GetNorthDeviation(GPS.x, GPS.y);

        do {
            // Add a new heading to list, queried from the compass
            norths.Add(CompassExtension.CompensatedHeading(Input.compass.rawVector,
                                                           Input.gyro.gravity));
            // Increase the number of attempts
            count++;

            //Delay for some time before the next reading
            await Task.Delay(TimeSpan.FromSeconds(0.1));
        }
        // Read at least MIN_COMPASS_READINGS times and until accuracy is positive (see Unity documentation)
        while (count < MIN_COMPASS_READINGS &&
               (count < MAX_COMPASS_READINGS || Input.compass.headingAccuracy < MIN_COMPASS_ACCURACY &&
                                                Input.compass.headingAccuracy > 0));

        // Set the relative North as the median element, based on North computation (since it is the most critical value of the three)
        RelNorth = MathExtension.GetMedian(norths.ToArray(), delegate (Vector3 v1, Vector3 v2) {
                return v1.y.CompareTo(v2.y);
            });
        // Add deviation of magnetic north to obtain true heading
        RelNorth = new Vector3(RelNorth.x, RelNorth.y - NorthDev, RelNorth.z);
    }

    /// <summary>
    /// Converts a Vector2 containing (RA, Dec) ICRS coordinates into a (az, h) Vector2 in local coordinates.
    /// </summary>
    /// <param name="RA_Dec"></param>
    /// <returns></returns>
    Vector2 ICRSToLocal(Vector2 RA_Dec) {
        // Converted local coordinates
        Vector2 az_h;

        // Compute GMST
        // Difference in days from standard date
        double D = (UTC - new DateTime(2000, 1, 1, 12, 0, 0)).TotalDays;
        // GMST expressed as a single number
        double GMST_n = (18.697374558 + 24.06570982441908 * D) % 24;

        // Parse the single value into a time span and write it to the appropriate property
        int hours = (int)GMST_n;
        int minutes = (int)(MathExtension.DecimalPart(GMST_n) * 60);
        int seconds = (int)(MathExtension.DecimalPart(GMST_n * 60) * 60);
        GMST = new TimeSpan(0, hours, minutes, seconds);
        
        // LST = GMST - longitude west = GMST + longitude east. Longitude is converted by dividing by 15
        // since 360 / 24 = 15
        LST = GMST + TimeSpan.FromHours(GPS.y / 15);

        //Local Hour Angle, obtained from LST and RA. Conversion is done by multiplying by 15. Value in hours is
        //preemptively reduced to range 0h - 24h
        LHA = ((LST.TotalHours - (RA_Dec.x / 15) + 24) % 24) * 15;

        // Express angles necessary for the conversion in radians, due to Math library functions working with radians
        float LHA_rad = (float)MathExtension.ToRadians(LHA);
        float Dec_rad = (float)MathExtension.ToRadians(RA_Dec.y);
        float lat_rad = (float)MathExtension.ToRadians(GPS.x);

        // Compute altitude and azimuth as per http://star-www.st-and.ac.uk/~fv/webnotes/chapter7.htm
        float h = (float)Math.Asin(Math.Sin(Dec_rad) * Math.Sin(lat_rad) + Math.Cos(Dec_rad) * Math.Cos(lat_rad) * Math.Cos(LHA_rad));
        float az;
        try {
            az = (float)Math.Atan2(-Math.Sin(LHA_rad) * Math.Cos(Dec_rad) / Math.Cos(h),
                                   (Math.Sin(Dec_rad) - Math.Sin(lat_rad) * Math.Sin(h)) / (Math.Cos(lat_rad) * Math.Cos(h)));
            // This takes care of cases in which altitude is exactly 90° or -90°, since azimuth would not uniquely be defined otherwise
            } catch (DivideByZeroException) {
            az = 0;
        }

        // Since az is in range [-180; 180] as per atan2, convert it to range [0; 360] as per standards
        if (az < 0)
            az += (float)Math.PI * 2;

        // Convert angles back into degrees for convenience (and because Unity rotates using degrees)
        az_h = new Vector2((float)MathExtension.ToDegrees(az), (float)MathExtension.ToDegrees(h));

        // Return the converted coordinates
        return az_h;
    }

    /// <summary>
    /// Private asynchronous method used to align the photosphere.
    /// </summary>
    /// <returns></returns>
    async Task AlignSphereInternal() {
        // Initially, method should not be requeued
        _requeue = false;
        // Get a key to enter critical section code
        int key = System.Threading.Interlocked.Exchange(ref _lock, 0);
        // If function is not executing already...
        if (key == 1) {
            // No errors at the beginning of execution
            _error = false;

            // Start camera movement tracking
            _camera.GetComponent<CameraRig>().StartTracking();
            // Compute alignment to obtain local coordinates for points a and b
            Tuple<Vector2, Vector2> local_coords = await ComputeSphereAlignment();
            // Stop camera movement tracking. If camera tracking yields a positive result...
            if (_camera.GetComponent<CameraRig>().StopTracking()) {
                // Too much movement! Print an error
                Log.Print("Failed to align sphere! Too unstable! " + _camera.GetComponent<CameraRig>().TrackingValue, Log.Colors.Error);
                // And requeue the alignment
                _requeue = true;
                // If time needs to be queried again, ensure to wait at least 4 seconds (requirement by NIST servers)
                if (_query_UTC)
                    await Task.Delay(TimeSpan.FromSeconds(4.0f));
            }
            // Else, if an error has been raised during coordinates computation, retry
            else if (_error) {
                _requeue = true;
                // If time needs to be queried again, ensure to wait at least 4 seconds (requirement by NIST servers)
                if (_query_UTC)
                    await Task.Delay(TimeSpan.FromSeconds(4.0f));
            }
            // If camera has not moved much and no errors were raised during coordinates computation...
            else {
                // Align sphere based on obtained rotation
                ApplySphereAlignment(local_coords);
                // Print a bunch of debug info
                PrintDebugInfo();
                }

            // Allow another call for this function. Return point
            _lock = 1;
        }
        else
            // If function is already executing, print an error and return without doing anything
            Log.Print("Warning! Sphere Alignment was requested again before completion!", Log.Colors.Warning);
    }

    /// <summary>
    /// Apply alignment to the spheres based on local coordinates for points a and b.
    /// </summary>
    /// <param name="local_coords"></param>
    void ApplySphereAlignment(Tuple<Vector2, Vector2> local_coords) {
        try {
            // Initially, set the clone sphere parameters as those of the photosphere
            _sphere_b.position = _sphere_destination.position;
            _sphere_b.rotation = _sphere_destination.rotation;
            _sphere_b.localScale = _sphere_destination.localScale;

            // Fetch the local coordinates for a and b from the input tuple
            Vector2 a_az_h = local_coords.Item1;
            Vector2 b_az_h = local_coords.Item2;

            // Initially, set photosphere rotation to camera orientation
            _sphere_destination.rotation = _camera.rotation;

            // First Point Alignment
            // Rotate ICRS North such that it aligns with local North
            _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.up, -RelNorth.z);         // Left-Right
            _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.right, RelNorth.x);       // Up-Down
            _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.forward, -RelNorth.y);    // Round
            _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.forward, 180);            // North Alignment
            // Rotate Cardinal Points sphere and clone sphere so that they also match the local North
            _north_sphere_destination.rotation = _sphere_destination.rotation;
            _sphere_b.rotation = _sphere_destination.rotation;

            // Rotate photosphere based on point a, so that point a aligns with local North
            _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.up, -_p_a.y);         // + Altitude
            _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.forward, -_p_a.x);     // + Azimuth
            // Rotate photosphere by the computed azimuth and altitude, so that point a aligns with its real world position
            _sphere_destination.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(0, 90), _sphere_radius),
                                             a_az_h.x);     // + Azimuth
            _sphere_destination.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(180, 0), _sphere_radius),
                                             a_az_h.y);     // + Altitude
            
            // Do these last two steps exactly the same on the clone sphere for point b
            _sphere_b.RotateAround(Vector3.zero, _sphere_b.up, -_p_b.y);         // + Altitude
            _sphere_b.RotateAround(Vector3.zero, _sphere_b.forward, -_p_b.x);     // + Azimuth
            _sphere_b.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(0, 90), _sphere_radius),
                                   b_az_h.x);     // + Azimuth
            _sphere_b.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(180, 0), _sphere_radius),
                                   b_az_h.y);     // + Altitude
            
            // Second Point Alignment
            // Find the three points on the 3D space corresponding to the aligned positions of a (a), b (c) and the position in
            // which b is on the photosphere after alignment of the first point (b)
            Vector3 a = SOFConverter.EquirectangularToSphere(_p_a, _sphere_destination, _sphere_radius);
            Vector3 b = SOFConverter.EquirectangularToSphere(_p_b, _sphere_destination, _sphere_radius);
            Vector3 c = SOFConverter.EquirectangularToSphere(_p_b, _sphere_b, _sphere_radius);

            // Axis of rotation
            Vector3 axis = a.normalized;

            // Compute two vectors going from point a to points b and c: these vectors form an angle, which is the angle to which
            // photosphere should be rotated around the axis passing through a to align the second point
            Vector3 u = b;
            Vector3 v = c;
            // Project vectors onto plane perpendicular to axis by subtracting their projection onto axis
            u -= Vector3.Dot(u, axis) * axis;
            v -= Vector3.Dot(v, axis) * axis;
            
            Log.Print(u.ToString());
            Log.Print(v.ToString());
            // Compute relative angle between the two vectors as seen by axis
            SecondRotAngle = MathExtension.RelativeAngleOnSphere(axis, u, v);
            
            // Rotate photosphere around axis passing through a by the computed angle
            _sphere_destination.RotateAround(Vector3.zero,
                                             SOFConverter.EquirectangularToSphere(_p_a, _sphere_destination, _sphere_radius),
                                             SecondRotAngle);
        
            // Signal that first calibration has been finished
            _lookaround_UI.FirstCalibrationDone();
        }
        // If for some reason an unhandled exception is raised...
        catch (Exception e) {
            // Print an error with the exception message
            Log.Print("Could not compute and apply photosphere alignment!" + e.Message, Log.Colors.Error);
        }
    }

    /// <summary>
    /// Print a series of info to Log for debugging purposes.
    /// </summary>
    void PrintDebugInfo() {
        Log.PrintClear("UTC:" + UTC.ToString());
        Log.Print("North Dir: " + RelNorth.ToString());
        Log.Print("Roll: " + MathExtension.DegRestrict(RelNorth.x));
        Log.Print("Pitch: " + MathExtension.DegRestrict(RelNorth.z));
        Log.Print("Yaw: " + MathExtension.DegRestrict(RelNorth.y));
        Log.Print("Magnetometer: " + Input.compass.rawVector.ToString());
        Log.Print("Accelerometer: " + Input.gyro.gravity.ToString());
        Log.Print("Second Rot Angle: " + SecondRotAngle);
    }

    /// <summary>
    /// Compute the local coordinates for points a and b.
    /// </summary>
    /// <returns></returns>
    async Task<Tuple<Vector2, Vector2>> ComputeSphereAlignment() {
        // Get GPS and True North Heading
        await GetGPSAndRelativeNorth();
        // Return prematurely on error
        if (_error)
            return default;
        // Get updated UTC, if needed
        if (_query_UTC)
            await GetNISTDate();
        // Return prematurely on error
        if (_error)
            return default;
        //Compute local coordinates for both reference points
        Vector2 a_az_h = Az_H = ICRSToLocal(new Vector2(_p_a.x, _p_a.y));
        Vector2 b_az_h = ICRSToLocal(new Vector2(_p_b.x, _p_b.y));
        
        // Return the local coordinates in a tuple
        return new Tuple<Vector2, Vector2>(a_az_h, b_az_h);
    }

    /// <summary>
    /// Public method used to issue a sphere alignment to this component.
    /// </summary>
    public async void AlignSphere() {
        // Set a new UTC time to be required
        _query_UTC = true;
        // Start a new sphere alignment and wait its completion...
        do {
            await AlignSphereInternal();
            // Once alignment is complete, increase number of attempts done so far
            _curr_attempts++;
        }
        // So long as the alignment request should be requeued
        while (_requeue && _curr_attempts < MAX_ATTEMPTS);

        // Since alignment is over, reset the counter of attempts number
        _curr_attempts = 0;
    }

    /// <summary>
    /// Method used to actually rotate photosphere and cardinal points sphere, based on computed alignment.
    /// </summary>
    void Rotate() {
        // Rotate the photosphere to its destination rotation with an interpolation factor defined in the options
        _sphere.rotation = Quaternion.Slerp(_sphere.rotation, _sphere_destination.rotation, Options.LERP_ALPHA);
        // Rotate the cardinal points sphere to its destination rotation with an interpolation factor defined in the options
        _north_sphere.rotation = Quaternion.Slerp(_north_sphere.rotation, _north_sphere_destination.rotation, Options.LERP_ALPHA);
    }

    /// <summary>
    /// Monobehaviour Method. Update is called once per frame.
    /// </summary>
    void Update() {
        // Rotate the spheres based on the computed alignment
        Rotate();
    }


}
