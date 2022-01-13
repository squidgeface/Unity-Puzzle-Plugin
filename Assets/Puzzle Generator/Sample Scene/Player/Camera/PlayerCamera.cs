using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerCamera
{
    /// <summary>
    ///     Modifies the players velocity direction to work with the given camera
    ///     This is useful for cinematic scenes where different control schemes are more appropriate
    /// </summary>
    /// <param name="side">The left/right input value (-1, 0, 1)</param>
    /// <param name="vertical">The up/down input value (-1, 0, 1)</param>
    /// <returns>The modified direction</returns>
    public Vector3 CalculateMovementDirection(float side, float vertical);

    /// <summary>
    ///     Modifies the players velocity direction to work with the given camera while sprinting
    ///     This is useful for cinematic scenes where different control schemes are more appropriate
    /// </summary>
    /// <param name="side">The left/right input value (-1, 0, 1)</param>
    /// <param name="vertical">The up/down input value (-1, 0, 1)</param>
    /// <returns>The modified direction</returns>
    public Vector3 CalculateMovementDirectionSprint(float side, float vertical);
}
