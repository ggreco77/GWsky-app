using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
/*NOTE: A task is an asynchronous function which may be redirected to the same or a different thread.
        In Unity's case, tasks can only be executed on the main thread whenever they contain any reference to any
        class or variable in UnityEngine (since Unity is not thread-safe), as is in our case. */
using System.Threading.Tasks;

// MAKE IT STATIC!!!
public class SphereAligner : MonoBehaviour {
    // Minimum number of compass reading before the values can be accepted
    const int MIN_COMPASS_READINGS = 4;
    const int MAX_COMPASS_READINGS = 12;

    const int MIN_COMPASS_ACCURACY = 2; // degrees

    const float SPHERE_RADIUS = 10f;

    Vector2 p_a = new Vector2(0, 90);
    Vector2 p_b = new Vector2(0.001f, 89.999f);

    // Reference to sphere object in the scene
    Transform _sphere;
    // Reference to cardinal points sphere object in scene
    Transform _north_sphere;

    Transform _sphere_destination;
    Transform _north_sphere_destination;

    // Reference to camera object in the scene
    Transform _camera;

    LookUI _look_UI;

    // Boolean to keep track of errors while computing sphere alignment
    bool _error = false;
    // Mutex for function which computes sphere alignment
    int _lock = 1;
    // Whether to repeat sphere alignment just after completion
    bool _requeue = false;

    // These fields are public since they may be useful for logs or to be shown to the user
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
    //Heading to True North
    public Vector3 RelNorth { get; private set; }
    //Accuracy of the Heading
    public float RelNorthAccuracy { get; private set; }
    //Equatorial->Local Coordinates Converted
    public Vector2 az_h { get; private set; }

    //Init function, should be called by Master. Unfortunately Constructors do not work well with Unity, so a Init function has to be called.
    public void Init(Transform sphere, Transform north_sphere, Transform camera, LookUI look_UI) {
        // Copy reference to sphere
        _sphere = sphere;
        _north_sphere = north_sphere;
        _camera = camera;
        _look_UI = look_UI;

        UTC = new DateTime(2000, 1, 1);

        _sphere_destination = new GameObject("Sphere Destination").transform;
        _sphere_destination.position = _sphere.position;
        _sphere_destination.rotation = _sphere.rotation;
        _sphere_destination.localScale = _sphere.localScale;

        _north_sphere_destination = new GameObject("North Sphere Destination").transform;
        _north_sphere_destination.position = _north_sphere.position;
        _north_sphere_destination.rotation = _north_sphere.rotation;
        _north_sphere_destination.localScale = _north_sphere.localScale;
    }

    //Contact NIST server and retrieve UTC time. Asynchronous because it has to wait server response.
    public async Task<DateTime> GetNISTDate() {
        //Server to contact
        string server = "time.nist.gov";
        string string_response = string.Empty;

        //Connection attempt
        try {
            //Create a TCP connection
            TcpClient tcpClient = new TcpClient();
            //Asynchronous connect, port 13
            var task = tcpClient.ConnectAsync(server, 13);
            //Await closure of connection OR a 3 seconds timeout
            Task completed_task = await Task.WhenAny(task, Task.Delay(3000));
            //If there hasn't been a timeout...
            if (completed_task == task) {
                //Get the stream from the connection and pass it to a string
                using (var reader = new StreamReader(tcpClient.GetStream())) {
                    string_response = reader.ReadToEnd();
                    reader.Close();
                }
            }
            //Otherwise...
            else {
                //Print an error message and return.
                DebugMessages.Print("Warning! Could not Connect to NIST Server. Timeout.", DebugMessages.Colors.Warning);
                _error = true;
                return UTC;
            }
        }
        //If there has been some other kind of error...
        catch (Exception e) {
            //Print an error message and return.
            DebugMessages.Print("Warning! Could not Connect to NIST Server. " + e.Message, DebugMessages.Colors.Warning);
            _error = true;
            return UTC;
        }

        // Check to see that the UTC signature is there
        if (string_response.Length > 47 && string_response.Substring(38, 9).Equals("UTC(NIST)")) {
            // Parse the date
            int jd = int.Parse(string_response.Substring(1, 5));
            int yr = int.Parse(string_response.Substring(7, 2));
            int mo = int.Parse(string_response.Substring(10, 2));
            int dy = int.Parse(string_response.Substring(13, 2));
            int hr = int.Parse(string_response.Substring(16, 2));
            int mm = int.Parse(string_response.Substring(19, 2));
            int sc = int.Parse(string_response.Substring(22, 2));

            //Compensate
            if (jd > 51544)
                yr += 2000;
            else
                yr += 1999;

            //Copy UTC into its field
            UTC = new DateTime(yr, mo, dy, hr, mm, sc);
        }
        //If for some reason signature is not there...
        else
        {
            //Print an error message and return.
            DebugMessages.Print("Warning! Could not retrieve NIST Time.", DebugMessages.Colors.Warning);
            _error = true;
            return UTC;
        }

        //Return UTC time.
        return UTC;
    }
    /* Start location service, retrieve GPS and North True Heading, close location service. Returns values as a
       tuple of the GPS and North Heading angle. Asynchronous because of location service initialization and compass reading. */
    public async Task<Tuple<Vector2, Vector3>> GetGPSAndRelativeNorth() {
        GPS = new Vector2(0, 0);

        //Starts location service through Unity, checks GPS and compass availability. Function is asynchronous and returns
        //true if everything went ok.
        _error = !(await SensorExtension.UnityInputSensorsStart());

        //If there hasn't been an error...
        if (!_error) {
            //Get GPS data
            GPS = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);

            //Wait for a good compass reading. Asynchronous function
            await WaitGoodCompassReading();
        }

        // Stop service after query
        SensorExtension.UnityInputSensorsStop();

        //Returns both GPS and True North Heading
        return new Tuple<Vector2, Vector3>(GPS, RelNorth);
    }
    //Keeps querying compass until a good reading is found. Gets the median value from the list of results.
    async Task WaitGoodCompassReading() {
        //Initialize list of headings.
        List<double> directions = new List<double>();
        List<Vector3> norths = new List<Vector3>();
        RelNorthAccuracy = int.MaxValue;
        int count = 0;

        do {
            //Add new heading to list
            norths.Add(CompassHandler.CompensatedHeading(Input.compass.rawVector,
                                                         Input.gyro.gravity));
            directions.Add(norths[norths.Count - 1].y);
            count++;
            //Delay for some time before next reading
            await Task.Delay(TimeSpan.FromSeconds(0.1));
        }
        //Read at least MIN_COMPASS_READINGS times and until accuracy is positive (see Unity documentation)
        while ((count < MIN_COMPASS_READINGS || RelNorthAccuracy > MIN_COMPASS_ACCURACY) &&
               count < MAX_COMPASS_READINGS);

        //Compute median
        RelNorth = norths[MathExtension.GetMedian(directions.ToArray()).Item2];
    }

    /* Converts a Vector2 containing (RA, Dec) coordinates in ICRS into a (az, h) Vector2 in local coordinates. */
    public async Task<Vector2> ICRSToLocal(Vector2 RA_Dec) {
        az_h = new Vector2(0, 0);

        //Get updated UTC
        await GetNISTDate();

        //Compute GMST
        //Difference in days from standard date
        double D = (UTC - new DateTime(2000, 1, 1, 12, 0, 0)).TotalDays;
        //GMST expressed as a single number
        double GMST_n = (18.697374558 + 24.06570982441908 * D) % 24;

        //Parse single value into a time span
        int hours = (int)GMST_n;
        int minutes = (int)(MathExtension.DecimalPart(GMST_n) * 60);
        int seconds = (int)(MathExtension.DecimalPart(GMST_n * 60) * 60);
        GMST = new TimeSpan(0, hours, minutes, seconds);

        //Get GPS and True North Heading
        await GetGPSAndRelativeNorth();

        // LST = GMST - longitude west = GMST + longitude east. Longitude is converted by dividing by 15
        // since 360 / 24 = 15
        LST = GMST + TimeSpan.FromHours(GPS.y / 15);

        //Local Hour Angle, obtained from LST and RA. Conversion is done by multiplying by 15. Value in hours is
        //preemptively reduced to range 0h - 24h.
        LHA = ((LST.TotalHours - (RA_Dec.x / 15) + 24) % 24) * 15;

        //Compute and Convert necessary angles in radians: Math library functions work with angles in radians,
        //so conversion is necessary.
        float LHA_rad = (float)MathExtension.ToRadians(LHA);
        float Dec_rad = (float)MathExtension.ToRadians(RA_Dec.y);
        float lat_rad = (float)MathExtension.ToRadians(GPS.x);

        //Compute altitude and azimuth as per http://star-www.st-and.ac.uk/~fv/webnotes/chapter7.htm.
        float h = (float)Math.Asin(Math.Sin(Dec_rad) * Math.Sin(lat_rad) + Math.Cos(Dec_rad) * Math.Cos(lat_rad) * Math.Cos(LHA_rad));
        float az;
        try {
            az = (float)Math.Atan2(-Math.Sin(LHA_rad) * Math.Cos(Dec_rad) / Math.Cos(h),
                                   (Math.Sin(Dec_rad) - Math.Sin(lat_rad) * Math.Sin(h)) / (Math.Cos(lat_rad) * Math.Cos(h)));
            } catch (DivideByZeroException) {
            az = 0;
        }

        // Since az is in range [-180; 180] as per atan2, convert it to range [0; 360]
        if (az < 0)
            az += (float)Math.PI * 2;

        //Convert back into degrees for convenience
        az_h = new Vector2((float)MathExtension.ToDegrees(az), (float)MathExtension.ToDegrees(h));

        /*
        DebugMessages.Print("LHA: " + LHA);
        DebugMessages.Print("Dec: " + RA_Dec.y);
        DebugMessages.Print("lat: " + GPS.x);
        DebugMessages.Print("h: " + MathExtension.ToDegrees(h));
        */

        //Return converted coordinates.
        return az_h;
    }

    async Task AlignSphereInternal() {
        _requeue = false;
        int key = System.Threading.Interlocked.Exchange(ref _lock, 0);
        //If function is not executing already...
        if (key == 1) {
            //No errors at the beginning of execution.
            _error = false;

            _camera.gameObject.GetComponent<CameraRig>().StartTracking();
            //Compute alignment into a rotation vector.
            Tuple<Vector2, Vector2> local_coords = await ComputeSphereAlignment();
            if (_camera.gameObject.GetComponent<CameraRig>().StopTracking()) {
                DebugMessages.Print("Failed to align sphere! Too unstable! " + _camera.gameObject.GetComponent<CameraRig>().trackingLastValue, DebugMessages.Colors.Error);
                _requeue = true;
            }
            else {
                //Align sphere based on obtained rotation.
                ApplySphereAlignment(local_coords);
            }

            //Allow another call for this function. Return point.
            _lock = 1;
        }
        else
            //Otherwise, print an error and return without doing anything.
            DebugMessages.Print("Warning! Sphere Alignment was requested again before completion!", DebugMessages.Colors.Warning);
    }

    public async void AlignSphere() {
        do {
            await AlignSphereInternal();
        }
        while (_requeue);
    }

    //Apply sphere alignment based on a rotation vector.
    public void ApplySphereAlignment(Tuple<Vector2, Vector2> local_coords) {
        GameObject sphere_bGO = new GameObject("Sphere B");
        Transform sphere_b = sphere_bGO.transform;
        sphere_b.position = _sphere_destination.position;
        sphere_b.localScale = _sphere_destination.localScale;

        Vector2 a_az_h = local_coords.Item1;
        Vector2 b_az_h = local_coords.Item2;

        //Initially, set photosphere rotation to camera orientation
        _sphere_destination.rotation = _camera.rotation;

        // Rotate ICRS North such that it aligns with local North
        _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.up, -RelNorth.z);         // Left-Right
        _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.right, RelNorth.x);       // Up-Down
        _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.forward, -RelNorth.y);    // Round
        _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.forward, 180);            // North Alignment
        // Rotate Cardinal Points so that they match the local North
        _north_sphere_destination.rotation = _sphere_destination.rotation;
        sphere_b.rotation = _sphere_destination.rotation;

        _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.up, -p_a.y);         // + Altitude
        _sphere_destination.RotateAround(Vector3.zero, _sphere_destination.forward, -p_a.x);     // + Azimuth
        // Rotate photosphere by the computed azimuth and altitude
        _sphere_destination.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(0, 90), SPHERE_RADIUS),
                                         a_az_h.x);     // + Azimuth
        _sphere_destination.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(180, 0), SPHERE_RADIUS),
                                         a_az_h.y);     // + Altitude
        
        sphere_b.RotateAround(Vector3.zero, sphere_b.up, -p_b.y);         // + Altitude
        sphere_b.RotateAround(Vector3.zero, sphere_b.forward, -p_b.x);     // + Azimuth
        // Rotate photosphere by the computed azimuth and altitude
        sphere_b.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(0, 90), SPHERE_RADIUS),
                              b_az_h.x);     // + Azimuth
        sphere_b.RotateAround(Vector3.zero, SOFConverter.EquirectangularToAbsoluteSphere(new Vector2(180, 0), SPHERE_RADIUS),
                              b_az_h.y);     // + Altitude
        
        //Find relative angle (angle between 2 3D lines)
        Vector3 a = SOFConverter.EquirectangularToSphere(p_a, SPHERE_RADIUS, _sphere_destination);
        Vector3 b = SOFConverter.EquirectangularToSphere(p_b, SPHERE_RADIUS, _sphere_destination);
        Vector3 c = SOFConverter.EquirectangularToSphere(p_b, SPHERE_RADIUS, sphere_b);

        Vector3 u = b - a;
        Vector3 v = c - a;

        Vector3 axis = a.normalized;
        // From https://stackoverflow.com/questions/14066933/direct-way-of-computing-clockwise-angle-between-2-vectors
        float dot = Vector3.Dot(u, v);
        float det = Vector3.Dot(axis, Vector3.Cross(u, v));
        float rot_angle = (float)MathExtension.ToDegrees(Mathf.Atan2(det, dot));
        
        _sphere_destination.RotateAround(Vector3.zero,
                                         SOFConverter.EquirectangularToSphere(p_a, SPHERE_RADIUS, _sphere_destination),
                                         rot_angle);   // Second point fix
        
        // Signal that first calibration has been finished
        _look_UI.FirstCalibrationDone();

        //Print a bunch of debug info
        DebugMessages.PrintClear("Roll: " + MathExtension.DegRestrict(RelNorth.x));
        DebugMessages.Print("Pitch: " + MathExtension.DegRestrict(RelNorth.z));
        DebugMessages.Print("Yaw: " + MathExtension.DegRestrict(RelNorth.y));
        DebugMessages.Print("Ref Points: " + a_az_h.ToString() + ", " + b_az_h.ToString());
        DebugMessages.Print("Rotation: " + new Vector3(a_az_h.x, a_az_h.y, rot_angle).ToString());
        DebugMessages.Print("Magnetometer: " + Input.compass.rawVector.ToString());
        DebugMessages.Print("Accelerometer: " + Input.gyro.gravity.ToString());

        Destroy(sphere_bGO);
    }

    //Compute rotation vector used to align sphere
    public async Task<Tuple<Vector2, Vector2>> ComputeSphereAlignment() {
        //Compute local coordinates for both reference points
        Vector2 a_az_h = await ICRSToLocal(new Vector2(p_a.x, p_a.y));
        Vector2 b_az_h = await ICRSToLocal(new Vector2(p_b.x, p_b.y));
        
        return new Tuple<Vector2, Vector2>(a_az_h, b_az_h);
    }

    public void Rotate()
    {
        _sphere.rotation = Quaternion.Slerp(_sphere.rotation, _sphere_destination.rotation, Globals.LERP_ALPHA);
        _north_sphere.rotation = Quaternion.Slerp(_north_sphere.rotation, _north_sphere_destination.rotation, Globals.LERP_ALPHA);
    }


}
