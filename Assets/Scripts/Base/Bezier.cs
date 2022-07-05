// <summary>
using UnityEngine;
/// Easy and simple Bezier curves for Unity
/// </summary>
/// <remarks>http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/ </remarks>
public static class Bezier
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t">Time, between 0 and 1</param>
    /// <param name="p0">Source</param>
    /// <param name="p1">Control point 1</param>
    /// <param name="p2">Control point 1</param>
    /// <param name="p3">Destination</param>
    /// <returns></returns>
    public static Vector3 Cubic(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // [x,y]=(1–t)^3*P0+3(1–t)^2*t*P1+3(1–t)t^2*P2+t^3P3

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; //first term
        p += 3 * uu * t * p1; //second term
        p += 3 * u * tt * p2; //third term
        p += ttt * p3; //fourth term

        return p;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t">Time, between 0 and 1</param>
    /// <param name="p0">Source</param>
    /// <param name="p1">Control point</param>
    /// <param name="p2">Destination</param>
    /// <returns></returns>
    public static Vector3 Quadratic(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // [x,y]=(1–t)^2*P0+2(1–t)*t*P1+t^2*P2

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; //first term
        p += 2 * u * t * p1; //second term
        p += tt * p2; //third term

        return p;
    }
}