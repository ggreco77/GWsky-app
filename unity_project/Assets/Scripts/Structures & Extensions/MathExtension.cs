using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MathExtension
{
    public static double DecimalPart(double x)
    {
        return x - Math.Truncate(x);
    }

    public static double ToDegrees(double radians)
    {
        return radians / Math.PI * 180.0;
    }

    public static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public static double DegRestrict(double degrees)
    {
        if (degrees > 180)
            degrees -= 360;
        if (degrees < -180)
            degrees += 360;

        return degrees;
    }

    // Courtesy of Douglas Blake, https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp/22702269
    public static Tuple<double, int> GetMedian(double[] sourceNumbers)
    {
        //Framework 2.0 version of this method. there is an easier way in F4        
        if (sourceNumbers == null || sourceNumbers.Length == 0)
            throw new System.Exception("Median of empty array not defined.");

        //make sure the list is sorted, but use a new array
        double[] sortedPNumbers = (double[])sourceNumbers.Clone();
        Array.Sort(sortedPNumbers);

        //get the median
        int size = sortedPNumbers.Length;
        int mid = size / 2;
        double median = (size % 2 != 0) ? (double)sortedPNumbers[mid] : ((double)sortedPNumbers[mid] + (double)sortedPNumbers[mid - 1]) / 2;
        return new Tuple<double, int>(median, mid);
    }

    public static double GetDiscreteStandardDeviation(double [] sourceNumbers)
    {
        //Framework 2.0 version of this method. there is an easier way in F4        
        if (sourceNumbers == null || sourceNumbers.Length == 0)
            throw new System.Exception("Standard Deviation of empty array not defined.");

        double sigma = 0.0;

        double mean = 0.0;
        foreach (double number in sourceNumbers)
            mean += number;
        mean /= sourceNumbers.Length;

        foreach (double number in sourceNumbers) {
            double diff = number - mean;
            sigma += diff * diff;
        }
        sigma = Math.Sqrt(sigma);

        return sigma;
    }

    public static Quaternion RightToLeftHanded(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
