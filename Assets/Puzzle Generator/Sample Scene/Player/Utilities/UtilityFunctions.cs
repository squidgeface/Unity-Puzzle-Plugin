using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityFunctions
{
    /// <summary>
    ///     Clamps a direction to a central direction by x degrees (in a cone like shape)
    /// </summary>
    /// <param name="direction">The current direction getting clamped (unit vector)</param>
    /// <param name="center">The central direction of the cone(unit vector)</param>
    /// <param name="clamp">The maximum angle in degrees from the central direction</param>
    /// <returns>The clamped directional vector</returns>
    public static Vector3 ClampDirection(Vector3 direction, Vector3 center, float clamp)
    {
        // Get angle from point (0-180)
        float angle = Vector3.Angle(center, direction);

        // Check clamp distance
        if (angle > clamp)
        {
            // Normalise to a t value (0-1 for the slerp function)
            float delta = angle - clamp;
            float t = delta / angle;
            
            // Slerp towards the central point by the normalised value
            return Vector3.Slerp(direction, center, t);
        }
        else
        {
            // Direction is already within the clamp zone
            return direction;
        }
    }

    /// <summary>
    ///     Clamps a point within a cone defined by a central point, direction, and cone angle
    /// </summary>
    /// <param name="point">The point that is being clamped</param>
    /// <param name="center">The location that the cone starts</param>
    /// <param name="direction">The central direction of the cone (Unit vector)</param>
    /// <param name="clamp">The angle of the cone</param>
    /// <returns>The point clamped to the cone</returns>
    public static Vector3 ClampPointToDirection(Vector3 point, Vector3 center, Vector3 direction, float clamp)
    {
        // Get the direction from the center to the point
        Vector3 delta = point - center;
        float length = delta.magnitude;
        Vector3 pointDirection = delta / length;

        // Clamp the direction 
        Vector3 clampedDirection = ClampDirection(pointDirection, direction, clamp);

        // Reverse transforms on the point
        return center + clampedDirection * length;
    }
}
