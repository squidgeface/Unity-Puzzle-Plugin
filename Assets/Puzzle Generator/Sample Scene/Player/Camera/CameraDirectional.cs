using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirectional : MonoBehaviour, IPlayerCamera
{
    public Transform Direction;


    /// <summary>
    ///     Modifies the players velocity direction to be relative to the direction of the camera (ignoring y)
    /// </summary>
    /// <param name="side">The left/right input value (-1, 0, 1)</param>
    /// <param name="vertical">The up/down input value (-1, 0, 1)</param>
    /// <returns>The modified direction</returns>
    public Vector3 CalculateMovementDirection(float side, float vertical)
    {
        return CalculateMovementDirectionSprint(side, vertical);
    }

    /// <summary>
    ///     Modifies the players velocity direction to be relative to the direction of the camera (ignoring y)
    /// </summary>
    /// <param name="side">The left/right input value (-1, 0, 1)</param>
    /// <param name="vertical">The up/down input value (-1, 0, 1)</param>
    /// <returns>The modified direction</returns>
    public Vector3 CalculateMovementDirectionSprint(float side, float vertical)
    {
        // Modify velocity to be matched with the direction of the camera (ignoring y)
        Vector3 velocityDir = Direction.forward;
        velocityDir += Direction.right * side;
        velocityDir += Direction.up * vertical;
        velocityDir.Normalize();

        return velocityDir;
    }
}
