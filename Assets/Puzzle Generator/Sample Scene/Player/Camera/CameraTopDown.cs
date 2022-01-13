using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTopDown : MonoBehaviour, IPlayerCamera
{
    public Transform Player;


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
        // Calculate the forward direction (not taking y into account)
        Vector3 camForward = new Vector3(Player.position.x, 0.0f, Player.position.z) - new Vector3(transform.position.x, 0.0f, transform.position.z);
        camForward.Normalize();

        // Modify velocity to be matched with the direction of the camera (ignoring y)
        Vector3 velocityDir = camForward;
        velocityDir += transform.right * side;
        velocityDir += Vector3.up * vertical;
        velocityDir.Normalize();

        return velocityDir;
    }
}
