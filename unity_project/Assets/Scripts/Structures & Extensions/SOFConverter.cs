using UnityEngine;

public static class SOFConverter
{
    public static Vector3 EquirectangularToSphere(Vector2 rect, float radius, Transform followed_sphere) {
        float az = (float)MathExtension.ToRadians(-rect.x);
        float h = (float)MathExtension.ToRadians(-rect.y);
        // Get point position on the sphere as cartesian coordinates
        Vector3 sphere_pos = radius * new Vector3(Mathf.Cos(-h + Mathf.PI/2),
                                                  -Mathf.Sin(-h + Mathf.PI/2) * Mathf.Sin(az),
                                                  -Mathf.Sin(-h + Mathf.PI/2) * Mathf.Cos(az));
        // Make an initial orientation for lineup
        sphere_pos = Quaternion.Euler(0, -90, 0) * sphere_pos;

        // Rotate points as per sphere orientation
        sphere_pos = followed_sphere.rotation * sphere_pos;

        return sphere_pos;
    }
}
