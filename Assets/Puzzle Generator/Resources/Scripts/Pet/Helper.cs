using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper
{
    // Amount of points to plot on sphere
    const int Points = 150;
    public static Vector3[] Directions;

    // Helper constructor to plot points uniformly on a sphere using the golden spiral method
    static Helper()
    {
        Directions = new Vector3[Points];

        float GoldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float AngleIncrement = Mathf.PI * 2 * GoldenRatio;

        for (int i = 0; i < Points; i++)
        {
            float Phi = Mathf.Acos(1 - 2 * (float)i / Points);
            float Theta = AngleIncrement * i;

            float x = Mathf.Sin(Phi) * Mathf.Cos(Theta);
            float y = Mathf.Sin(Phi) * Mathf.Sin(Theta);
            float z = Mathf.Cos(Phi);

            Directions[i] = new Vector3(x, y, z);
        }
    }
}