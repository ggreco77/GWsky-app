using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
/*NOTE: A task is an asynchronous function which may be redirected to the same or a different thread.
        In Unity's case, tasks can only be executed on the main thread whenever they contain any reference to any
        class or variable in UnityEngine (since Unity is not thread-safe), as is in our case. */
using System.Threading.Tasks;

public class SphereAligner : MonoBehaviour {
    // Minimum number of compass reading before the values can be accepted
    const int MIN_COMPASS_READINGS = 4;

    // Reference to sphere object in the scene
    Transform _sphere;

    // Boolean to keep track of errors while computing sphere alignment
    bool _error = false;
    // Mutex for function which computes sphere alignment (since it uses tasks and Unity redirects them to the main thread, a mutex cannot be used)
    bool _lock;

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
    public float NorthDir { get; private set; }
    //Accuracy of the Heading
    public float NorthDirAccuracy { get; private set; }
    //Equatorial->Local Coordinates Converted
    public Vector2 az_h { get; private set; }

    //Init function, should be called by Master. Unfortunately Constructors do not work well with Unity, so a Init function has to be called.
    public void Init(Transform sphere)
    {
        // Copy reference to sphere
        _sphere = sphere;

        UTC = new DateTime(2000, 1, 1);
    }

    //Contact NIST server and retrieve UTC time. Asynchronous because it has to wait server response.
    public async Task<DateTime> GetNISTDate()
    {
        //Server to contact
        string server = "time.nist.gov";
        string string_response = string.Empty;

        //Connection attempt
        try
        {
            //Create a TCP connection
            TcpClient tcpClient = new TcpClient();
            //Asynchronous connect, port 13
            var task = tcpClient.ConnectAsync(server, 13);
            //Await closure of connection OR a 3 seconds timeout
            Task completed_task = await Task.WhenAny(task, Task.Delay(3000));
            //If there hasn't been a timeout...
            if (completed_task == task)
            {
                //Get the stream from the connection and pass it to a string
                using (var reader = new StreamReader(tcpClient.GetStream()))
                {
                    string_response = reader.ReadToEnd();
                    reader.Close();
                }
            }
            //Otherwise...
            else
            {
                //Print an error message and return.
                DebugMessages.Print("Warning! Could not Connect to NIST Server. Timeout.", DebugMessages.Colors.Warning);
                _error = true;
                return UTC;
            }
        }
        //If there has been some other kind of error...
        catch (Exception e)
        {
            //Print an error message and return.
            DebugMessages.Print("Warning! Could not Connect to NIST Server. " + e.Message, DebugMessages.Colors.Warning);
            _error = true;
            return UTC;
        }

        // Check to see that the UTC signature is there
        if (string_response.Length > 47 && string_response.Substring(38, 9).Equals("UTC(NIST)"))
        {
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
    public async Task<Tuple<Vector2, float>> GetGPSAndNorthDirection()
    {
        GPS = new Vector2(0, 0);

        //Starts location service through Unity, checks GPS and compass availability. Function is asynchronous and returns
        //true if everything went ok.
        _error = !(await SensorExtension.UnityInputSensorsStart());

        //If there hasn't been an error...
        if (!_error)
        {
            //Get GPS data
            GPS = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);

            //Wait for a good compass reading. Asynchronous function
            await WaitGoodCompassReading();
        }

        // Stop service after query
        SensorExtension.UnityInputSensorsStop();

        //Returns both GPS and True North Heading
        return new Tuple<Vector2, float>(GPS, NorthDir);
    }
    //Keeps querying compass until a good reading is found. Gets the median value from the list of results.
    async Task WaitGoodCompassReading()
    {
        //Initialize list of headings.
        List<double> directions = new List<double>();
        NorthDirAccuracy = -1;
        int count = 0;
        //Read at least MIN_COMPASS_READINGS times and until accuracy is positive (see Unity documentation)
        while (NorthDirAccuracy < 0 && count < MIN_COMPASS_READINGS)
        {
            //Add new heading to list
            directions.Add(Input.compass.trueHeading);
            //Last reading is also accuracy
            NorthDirAccuracy = Input.compass.headingAccuracy;
            count++;
            //Delay for some time before next reading
            await Task.Delay(TimeSpan.FromSeconds(0.2));
        }

        //Compute median
        NorthDir = (float)MathExtension.GetMedian(directions.ToArray());
    }
    /*Converts a Vector2 containing (RA, Dec) coordinates in ICRS into a (az, h) Vector2 in local coordinates. */
    public async Task<Vector2> ICRSToLocal(Vector2 RA_Dec)
    {
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
        await GetGPSAndNorthDirection();

        // LST = GMST - longitude west = GMST + longitude east. Longitude is converted by dividing by 15
        // since 360 / 24 = 15
        LST = GMST + TimeSpan.FromHours(GPS.y / 15);

        //Local Hour Angle, obtained from LST and RA. Conversion is done by multiplying by 15. Value in hours is
        //preemptively reduced to range 0h - 24h.
        LHA = ((LST.TotalHours - RA_Dec.x + 24) % 24) * 15;

        //Compute and Convert necessary angles in radians: Math library functions work with angles in radians,
        //so conversion is necessary.
        float LHA_rad = (float)MathExtension.ToRadians(LHA);
        float Dec_rad = (float)MathExtension.ToRadians(RA_Dec.y);
        float lat_rad = (float)MathExtension.ToRadians(GPS.x);

        //Compute altitude and azimuth as per http://star-www.st-and.ac.uk/~fv/webnotes/chapter7.htm.
        float h = (float)Math.Asin(Math.Sin(Dec_rad) * Math.Sin(lat_rad) + Math.Cos(Dec_rad) * Math.Cos(lat_rad) * Math.Cos(LHA_rad));
        float az = (float)Math.Atan2(-Math.Sin(LHA_rad) * Math.Cos(Dec_rad) / Math.Cos(h),
                                     (Math.Sin(Dec_rad) - Math.Sin(lat_rad) * Math.Sin(h)) / (Math.Cos(Dec_rad) * Math.Cos(h)));

        //Convert back into degrees for convenience
        az_h = new Vector2((float)MathExtension.ToDegrees(az), (float)MathExtension.ToDegrees(h));

        //Return converted coordinates.
        return az_h;
    }

    public async void AlignSphere()
    {
        //If function is not executing already...
        if (!_lock)
        {
            //Lock so that another call to the function is not possible. This is safe because Unity can only use its
            //main thread to execute, so no racing conditions can happen between threads.
            _lock = true;
            //No errors at the beginning of execution.
            _error = false;

            //Compute alignment into a rotation vector.
            Vector2 rotation = await ComputeSphereAlignment();
            //DebugMessages.Print("Rotation: " + rotation.x + " " + rotation.y);

            //Align sphere based on obtained rotation.
            ApplySphereAlignment(rotation);

            //Allow another call for this function. Return point.
            _lock = false;
        }
        else
            //Otherwise, print an error and return without doing anything.
            DebugMessages.Print("Error! Sphere Alignment was requested again before completion!", DebugMessages.Colors.Error);
    }
    //Apply sphere alignment based on a rotation vector.
    public void ApplySphereAlignment(Vector2 rotation)
    {
        //Reset sphere transform so that sphere rotation is consistent.
        _sphere.eulerAngles = new Vector3(-90, 0, 0);

        //Rotate sphere in world space.
        _sphere.Rotate(rotation.x, rotation.y, 0, Space.World);
    }
    //Compute rotation vector used to align sphere
    public async Task<Vector2> ComputeSphereAlignment()
    {
        // Center point of photosphere, used as a reference. We compute its local coordinates.
        float c_RA = 180;
        float c_Dec = 0;

        //Compute local coordinates for our center point
        Vector2 c_az_h = await ICRSToLocal(new Vector2(c_RA, c_Dec));

        //Create rotation vector based 
        Vector2 rotation = new Vector2(c_az_h.x, c_az_h.y);

        //Get where phone is pointing in Unity's world space through gyro, ignoring height component ("Equator" orientation).
        Vector2 eq_orientation = new Vector2(Input.gyro.attitude.x, Input.gyro.attitude.y);
        //Get phone heading angle
        float phone_dir = (float)MathExtension.ToDegrees(System.Math.Atan2(eq_orientation.y, eq_orientation.x));

        rotation = rotation + new Vector2(phone_dir + NorthDir, 0);
        return rotation;
    }
}
