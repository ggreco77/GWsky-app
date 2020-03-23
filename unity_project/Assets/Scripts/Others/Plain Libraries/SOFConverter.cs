using UnityEngine;

/// <summary>
/// Provides methods for conversions between different Systems Of Reference.
/// </summary>
public static class SOFConverter {
    /// <summary>
    /// Convert a point expressed in 2D coordinates on a equirectangle to a 3D point on a sphere of given radius.
    /// </summary>
    /// Input coordinates are expressed in (RA, Dec).
    /// Output coordinates are expressed as 3D Cartesian.
    /// <param name="rect"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static Vector3 EquirectangularToAbsoluteSphere(Vector2 rect, float radius = 1.0f) {
        // Get azimuth and altitude of the point as angles in radians (the negative sign is due to astronomical orientation
        // vs. spherical orientation in Math)
        float az = (float)MathExtension.ToRadians(-rect.x);
        float h = (float)MathExtension.ToRadians(-rect.y);
        // Convert from spherical coordinates to Cartesian coordinates (again, with changes due to astronomical orientation
        // vs. spherical orientation in Math)
        Vector3 sphere_pos = radius * new Vector3(Mathf.Cos(-h + Mathf.PI/2),
                                                  -Mathf.Sin(-h + Mathf.PI/2) * Mathf.Sin(az),
                                                  -Mathf.Sin(-h + Mathf.PI/2) * Mathf.Cos(az));
        // Make an initial orientation for lineup (due to Unity)
        sphere_pos = Quaternion.Euler(0, -90, 0) * sphere_pos;

        // Return the computed coordinates
        return sphere_pos;
    }

    /// <summary>
    /// Convert a point expressed in 2D coordinates on a equirectangle to a 3D point for a given input sphere.
    /// </summary>
    /// Input coordinates are expressed in (RA, Dec).
    /// Output coordinates are expressed as 3D Cartesian.
    /// <param name="rect"></param>
    /// <param name="radius"></param>
    /// <param name="followed_sphere"></param>
    /// <returns></returns>
    public static Vector3 EquirectangularToSphere(Vector2 rect, Transform followed_sphere, float radius = 1.0f) {
        // First, compute the absolute cartesian coordinates of the point
        Vector3 sphere_pos = EquirectangularToAbsoluteSphere(rect, radius);

        // Then, rotate the point as per sphere orientation
        sphere_pos = followed_sphere.rotation * sphere_pos;

        // Return the computed coordinates
        return sphere_pos;
    }

    /// <summary>
    /// Convert a right-handed system into a left-handed system.
    /// </summary>
    /// This is useful to convert to and from Unity system (which is left-handed).
    /// <param name="q"></param>
    /// <returns></returns>
    public static Quaternion RightToLeftHanded(Quaternion q) {
        // Inverting last two quaternion coordinates produces the desired conversion
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    public static float RelativeAngleOnSphere(Vector3 axis, Vector3 v1, Vector3 v2) {
        // Compute angle between the two vectors. Formulae are taken
        // from https://stackoverflow.com/questions/14066933/direct-way-of-computing-clockwise-angle-between-2-vectors
        float dot = Vector3.Dot(v1, v2);
        float det = Vector3.Dot(axis, Vector3.Cross(v1, v2));
        return (float)MathExtension.ToDegrees(Mathf.Atan2(det, dot));
    }
}
