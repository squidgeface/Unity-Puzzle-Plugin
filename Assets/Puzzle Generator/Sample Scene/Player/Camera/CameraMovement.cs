using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour, IPlayerCamera
{
    public Transform TargetObject;

    [Space(10)]
    public float RotateSpeed = 5f;
    public float ZoomSpeed = 0.1f;
    public float Smooth = 30.0f;
    public float FollowSmooth = 3.0f;

    [Space(10)]
    public float MaxRotation = 89f;
    public float MinRotation = -89f;

    [Space(10)]
    public float MaxZoom = 10f;
    public float MinZoom = 5f;

    private float TargetDistance;
    private float TargetHorizontal;
    private float TargetVertical;

    private bool IsFocused;

    [HideInInspector] public Vector3 CurrentFocusPosition;
    [HideInInspector] public float CurrentDistance;
    [HideInInspector] public float CurrentHorizontal;
    [HideInInspector] public float CurrentVertical;

    // Called at the start of the game
    private void Start()
    {
        TargetDistance = MaxZoom;
        TargetHorizontal = 0.0f;
        TargetVertical = 20.0f;

        CurrentFocusPosition = TargetObject.position;
        CurrentDistance = TargetDistance;
        CurrentHorizontal = TargetHorizontal;
        CurrentVertical = TargetVertical;

        IsFocused = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        RotateCamera();
        MoveCamera();
    }

    /// <summary>
    ///     Moves the camera to the target values
    /// </summary>
    void MoveCamera()
    {
        // Smooth lerp to target values
        CurrentDistance = Mathf.Lerp(CurrentDistance, TargetDistance, Smooth * Time.unscaledDeltaTime);
        CurrentHorizontal = Mathf.Lerp(CurrentHorizontal, TargetHorizontal, Smooth * Time.unscaledDeltaTime);
        CurrentVertical = Mathf.Lerp(CurrentVertical, TargetVertical, Smooth * Time.unscaledDeltaTime);
        CurrentFocusPosition = Vector3.Lerp(CurrentFocusPosition, TargetObject.position, FollowSmooth * Time.unscaledDeltaTime);

        // Update position and rotation of the camera
        Vector3 worldPosition = (Vector3.forward * -CurrentDistance);
        Vector3 RotatedVec = Quaternion.AngleAxis(CurrentHorizontal, Vector3.up) * Quaternion.AngleAxis(CurrentVertical, Vector3.right) * worldPosition;

        transform.position = CurrentFocusPosition + RotatedVec;
        transform.LookAt(TargetObject.position);
    }

    /// <summary>
    ///     Rotates the target values towards the mouse
    /// </summary>
    void RotateCamera()
    {
        // Only rotate the camera if the game is focused
        if (IsFocused)
        {
            // Get the change in location of the mouse
            Vector2 deltaPosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Update horizontal component
            float targetHorizontal = TargetHorizontal + deltaPosition.x * RotateSpeed;
            TargetHorizontal = Mathf.Lerp(TargetHorizontal, targetHorizontal, Smooth * Time.unscaledDeltaTime);

            // Update vertical component
            float targetVertical = TargetVertical + -deltaPosition.y * RotateSpeed;
            targetVertical = Mathf.Clamp(targetVertical, MinRotation, MaxRotation);
            TargetVertical = Mathf.Lerp(TargetVertical, targetVertical, Smooth * Time.unscaledDeltaTime);

            // Update distance
            TargetDistance *= (1.0f - (Input.mouseScrollDelta.y * ZoomSpeed));
            TargetDistance = Mathf.Clamp(TargetDistance, MinZoom, MaxZoom);

            // Check if the game should be unfocused
            if (Input.GetKey(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                IsFocused = false;
            }
        }
        else
        {
            // Check for player interaction (so the game can be focused again)
            if (Input.GetMouseButton(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                IsFocused = true;
            }
        }
    }



    /// <summary>
    ///     Modifies the players velocity direction to be relative to the direction of the camera
    /// </summary>
    /// <param name="side">The left/right input value (-1, 0, 1)</param>
    /// <param name="vertical">The up/down input value (-1, 0, 1)</param>
    /// <returns>The modified direction</returns>
    public Vector3 CalculateMovementDirectionSprint(float side, float vertical)
    {
        // Calculate the forward direction given the cameras look direction
        Vector3 velocityDir = transform.forward;
        velocityDir += transform.right * side;
        velocityDir += transform.up * vertical;
        velocityDir.Normalize();

        // Ensure velocity isn't pushing behind the camera
        return LockVelocity(velocityDir);
    }


    /// <summary>
    ///     Modifies the players velocity direction to be relative to the direction of the camera
    /// </summary>
    /// <param name="side">The left/right input value (-1, 0, 1)</param>
    /// <param name="vertical">The up/down input value (-1, 0, 1)</param>
    /// <returns>The modified direction</returns>
    public Vector3 CalculateMovementDirection(float side, float vertical)
    {
        // Calculate the forward direction given the cameras look direction
        Vector3 velocityDir = transform.forward * vertical;
        velocityDir += transform.right * side;
        velocityDir.Normalize();

        return velocityDir;
    }

    /// <summary>
    ///     Locks the new velocity to be infront of the camera
    ///     This prevents issues where the camera flips around due to the player traveling past the cameras look range
    /// </summary>
    /// <param name="newVelocity">The new velocity of the player</param>
    /// <returns>The locked velocity</returns>
    private Vector3 LockVelocity(Vector3 newVelocity)
    {
        // Project the velocity onto a plane pointing the direction of the camera
        Vector3 planeNormal = new Vector3(Mathf.Sin(CurrentHorizontal * Mathf.Deg2Rad), 0.0f, Mathf.Cos(CurrentHorizontal * Mathf.Deg2Rad));
        Plane plane = new Plane(planeNormal, Vector3.zero);
        float distance = plane.GetDistanceToPoint(newVelocity);

        // Check if the target is behind the camera
        if (distance < 0.25f)
        {
            // Push the value back infront of the camera
            return plane.ClosestPointOnPlane(newVelocity) + planeNormal * 0.25f;
        }
        else
        {
            // Velocity is already infront of the camera
            return newVelocity;
        }
    }
}
