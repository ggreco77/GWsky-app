using UnityEngine;
using System;

/// <summary>
/// Static class containing convenience Math methods and extending the standard Math library.
/// </summary>
public static class MathExtension {
    /// <summary>
    /// Return the decimal part of a number.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static double DecimalPart(double x) {
        // Subtract the integer part of the number from the number, return the result
        return x - Math.Truncate(x);
    }

    /// <summary>
    /// Convert an angle in radians into degrees.
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static double ToDegrees(double radians) {
        // Convert radians to degrees by proportion
        return radians / Math.PI * 180.0;
    }

    /// <summary>
    /// Convert an angle in degrees into radians.
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static double ToRadians(double degrees) {
        // Convert degrees to radians by proportion
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Restrict an angle in degrees into range [-180; 180].
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
    public static double DegRestrict(double degrees) {
        // If the angle is beyond an half circle, subtract a full circle from it
        if (degrees > 180)
            degrees -= 360;
        // If on the other hand the angle is below an half circle, add a full circle from it
        if (degrees < -180)
            degrees += 360;

        // Return the angle
        return degrees;
    }

    /// <summary>
    /// Sort the input array the median of an array of numbers.
    /// </summary>
    /// <param name="sourceNumbers"></param>
    /// <returns></returns>
    public static T GetMedian<T>(T[] source_elems, Comparison<T> sort_comparator) {
        // Check consistence of the input array, throw an Exception otherwise
        if (source_elems == null || source_elems.Length == 0)
            throw new System.Exception("Median of empty array not defined.");

        // Sort the list onto a new array (in order not to modify the original one)
        T[] sorted_elems = (T[])source_elems.Clone();
        Array.Sort(sorted_elems, sort_comparator);

        // Compute the median (in case of even number of elements, take the one to the left)
        T median = sorted_elems[sorted_elems.Length / 2];

        // Return the median
        return median;
    }
}
