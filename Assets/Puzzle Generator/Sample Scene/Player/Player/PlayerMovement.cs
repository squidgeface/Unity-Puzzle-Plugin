using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Sprinting,
    }

    [Header("Movement")]
    [SerializeField] private float SprintingSpeed = 25.0f;
    [SerializeField] private float BaseSpeed = 15.0f;
    [SerializeField] private float SpeedBoostDuration = 2.5f;
    [SerializeField] private float Agility = 2.0f;

    [Header("Advanced Behaviour")]
    [SerializeField] private float LegFollowMultiplier = 3.0f;
    [SerializeField] private float LegFollowPower = 4.0f;
    [SerializeField] private float LegWaveOffset = 0.5f;

    [Space(10)]
    [SerializeField] private float BodyRotationMultiplierMoving = 1.5f;
    [SerializeField] private float BodyRotationMultiplierSprinting = 3.0f;
    [SerializeField] private float BodyRotationMultiplierOverride = 5.0f;

    [Space(10)]
    [SerializeField] private float BodyIKRotationMultiplier = 3.0f;

    [Space(10)]
    [SerializeField] private Transform HeadLookTarget;
    [SerializeField] private float MaxHeadAngle = 40.0f;

    [Space(10)]
    // Changes the turn/twist amount of the body when looking in the direction it's moving in
    [SerializeField] private Vector3 UpperTurnMultipliersMovingBackwards = new Vector3(25.0f, 17.0f, 25.0f);
    [SerializeField] private Vector3 UpperTurnMultipliersMovingNeutral = new Vector3(25.0f, 25.0f, 25.0f);
    [SerializeField] private Vector3 UpperTurnMultipliersMovingForwards = new Vector3(-25.0f, 5.0f, -10.0f);

    [Space(10)]
    [SerializeField] private float MovingSideRotationAngle = 15.0f;
    [SerializeField] private float MovingDiagonalRotationAngle = 8.0f;

    [Space(10)]
    [SerializeField] private float UpperTurnNormaliseValueSprinting = 4.0f;
    [SerializeField] private float UpperTurnMaximumSprintingTurnAngle = 25.0f;

    [Space(10)]
    [SerializeField] private float ArmTurnAnimationTheshold = 40.0f;

    [Space(10)]
    [SerializeField] private bool SprintOnly = false;
    [SerializeField] private bool OverrideRotation = false;
    [SerializeField] private bool IgnoreJuiceTimeScale = false;

    [Header("Dependencies")]
    [SerializeField] private Transform BodyRotationTarget;
    [SerializeField] private Transform BoundCamera; // Important: Must be updated via UpdateCamera()

    [Space(10)]
    [SerializeField] private ikSolver LeftLegIK;
    [SerializeField] private ikSolver RightLegIK;
    [SerializeField] private ikSolver BodyIK;
    [SerializeField] private ikSolver HeadIK;

    [Space(10)]
    [SerializeField] private Transform LegsTarget;
    [SerializeField] private Transform RotationTransform;
    [SerializeField] private Transform HeadTransform;
    [SerializeField] private Transform BodyIKParent;

    private Rigidbody MainRigidBody;
    private Animator MainAnimator;

    // Modifies the acceleration speed
    private float AccelerationMultiplier = 1.0f;

    // Delta time may be different depending on "IgnoreJuiceTimeScale"
    private float DeltaTime;

    // Movement values for input
    private float SideMotion;
    private float VerticalMotion;
    private float ForwardMotion;

    // The horizontal and vertical angle between the current and target rotation
    private float VerticalAngle;
    private float HorizontalAngle;

    // The direction the player is trying to move to
    private Vector3 TargetDirection;

    // The current head rotation (used to slerp smoothly)
    private Vector3 CurrentHeadRotation;

    // The movement state of the player (idle, moving, sprinting)
    private PlayerState CurrentState;

    // The interface that holds the directional movement modifications
    private IPlayerCamera MainCamera;

    // The juice manager within the scene
    //private JuiceManager MainJuiceManager;


    // The body rotational offset. Precalculated do it doesn't need to do it every frame
    private readonly Quaternion BodyRotationOffset = Quaternion.AngleAxis(90.0f, Vector3.up) * Quaternion.AngleAxis(180.0f, Vector3.left);

    // The rotation multiplier for the bodies angle when moving forward/backward
    private readonly Dictionary<int, float> MovingRotationMultiplier = new Dictionary<int, float>() {
        { -1, -35.0f },  // degrees when moving backwards
        { 1, 20.0f },    // degrees when moving forwards
        { 0, 0.0f },
    };


    // Start is called before the first frame update
    private void Start()
    {
        UpdateCamera(BoundCamera);

        MainRigidBody = GetComponentInChildren<Rigidbody>();
        MainAnimator = GetComponentInParent<Animator>();
        //MainJuiceManager = FindObjectOfType<JuiceManager>();

        MainRigidBody.maxAngularVelocity = 0.0f;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateDeltaTime();
        UpdateInput();
        UpdateState();
        UpdateAnimator();
        UpdateHeadDirection();
    }

    // Called after transform calculations (I hope)
    private void LateUpdate()
    {
        UpdateDeltaTime();
        UpdatePelvisRotation();
        UpdateLegsTarget();
        UpdateBodyIKRotation();
    }

    // Called on the physics update tick
    private void FixedUpdate()
    {
        UpdateDeltaTime();
        UpdateMovement();
        UpdateAngleValues();
    }

    private void UpdateDeltaTime()
    {
        // If the IgnoreJuiceTimeScale is true then we update it to ignore the time scale modification
        //DeltaTime = IgnoreJuiceTimeScale ? Time.deltaTime / MainJuiceManager.TimeScale : Time.deltaTime;
        DeltaTime = IgnoreJuiceTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    /// <summary>
    ///     Gets the input of the player
    /// </summary>
    private void UpdateInput()
    {
        SideMotion = Input.GetAxis("Horizontal");
        VerticalMotion = Input.GetAxis("Vertical");
        ForwardMotion = Input.GetMouseButton(1) || Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }

    /// <summary>
    ///     Updates the CurrentState of the player depending on the input
    /// </summary>
    private void UpdateState()
    {
        // If sprint only is true then force the player to move forward
        ForwardMotion = SprintOnly ? 1.0f : ForwardMotion;

        // Check if player is sprinting
        if (ForwardMotion > 0.0f)
        {
            if (CurrentState != PlayerState.Sprinting)
            {
                CurrentState = PlayerState.Sprinting;
                MainRigidBody.velocity *= 0.0f; // Do this to avoid strange rotations
            }
        }
        // Check if player is idle swimming
        else if (ForwardMotion == 0.0f && (SideMotion != 0.0f || VerticalMotion != 0.0f))
        {
            CurrentState = PlayerState.Moving;
        }
        // Player is stil
        else
        {
            CurrentState = PlayerState.Idle;
        }
    }

    /// <summary>
    ///     Updates the horizontal and vertical delta angles
    /// </summary>
    private void UpdateAngleValues()
    {
        // Vertical:
        Vector3 dir1 = TargetDirection;
        Vector3 dir2 = -MainRigidBody.transform.right;
        VerticalAngle = (dir1.y - dir2.y) * 90.0f;

        // Horizontal:
        Vector2 dir3 = new Vector2(dir1.x, dir1.z);
        Vector2 dir4 = new Vector2(dir2.x, dir2.z);
        HorizontalAngle = Vector2.SignedAngle(dir3, dir4);
    }

    /// <summary>
    ///     Updates the animations being played based on the players state
    /// </summary>
    private void UpdateAnimator()
    {
        // Update mode to unscaled time if IgnoreJuiceTimeScale is true. TODO: This wont work in paused
        MainAnimator.updateMode = IgnoreJuiceTimeScale ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;

        switch (CurrentState)
        {
            case PlayerState.Moving:
            {
                int vertical = Mathf.RoundToInt(VerticalMotion);
                int side = Mathf.RoundToInt(SideMotion);

                MainAnimator.SetInteger("Side Motion", side);
                MainAnimator.SetInteger("Forward Motion", vertical);
                MainAnimator.SetFloat("Kicking Speed", 0.9f);
                MainAnimator.SetFloat("Arm Idle Speed", 0.6f);
                break;
            }

            case PlayerState.Sprinting:
            {
                // Default no arm movement
                int side = 0;

                // If the players target delta horizontal angle is greater than 40 degs
                if (Mathf.Abs(HorizontalAngle) > ArmTurnAnimationTheshold)
                {
                    // Play the corrosponding arm movement to "help" the player turn quicker
                    side = Math.Sign(HorizontalAngle);
                }

                MainAnimator.SetInteger("Side Motion", side);
                MainAnimator.SetInteger("Forward Motion", 0);
                MainAnimator.SetFloat("Kicking Speed", 1.25f);
                MainAnimator.SetFloat("Arm Idle Speed", 1.2f);

                // DEBUG CODE:
                // ----------------------------
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    Stroke();
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Flip();
                }
                // ----------------------------

                break;
            }

            case PlayerState.Idle:
            {
                MainAnimator.SetInteger("Forward Motion", 0);
                MainAnimator.SetFloat("Kicking Speed", 0.5f);
                MainAnimator.SetFloat("Arm Idle Speed", 0.3f);
                break;
            }

            default:
            {
                Debug.LogError("Player state not supported yet (PlayerMovement.UpdateAnimator)");
                break;
            }
        }
    }

    /// <summary>
    ///     Updates the head direction to look at the look at target
    /// </summary>
    private void UpdateHeadDirection()
    {
        // This is the target direction of the head
        Vector3 targetDir;

        // Check if there is an external target
        if (HeadLookTarget != null)
        {
            targetDir = HeadLookTarget.position - HeadIK.Base.position;
            targetDir.Normalize();
        }
        else
        {
            // Check if player is idle
            if (CurrentState == PlayerState.Idle)
            {
                targetDir = HeadIK.Base.up;
            }
            // Check if the player is moving backwards
            else if (VerticalMotion < 0.0f &&
                CurrentState == PlayerState.Moving)
            {
                // Inverse it so that the player isn't looking directly behind (looks weird)
                targetDir = -TargetDirection;
            }
            else
            {
                // The direction the player is trying to move in
                targetDir = TargetDirection;
            }
        }

        // Lock the target direction within the limits
        Vector3 lockedDir = UtilityFunctions.ClampDirection(targetDir, HeadIK.Base.up, MaxHeadAngle);

        // Slerp towards the new head direction from the current one
        CurrentHeadRotation = Vector3.Slerp(CurrentHeadRotation, lockedDir, 4.0f * DeltaTime); // TODO converty 3 to variable

        // Turn the direction into a target position
        Vector3 target = HeadIK.Base.position + CurrentHeadRotation * 10.0f;

        // Get the up position (This is important for the correct relative lookaat rotations)
        Vector3 up = -HeadIK.transform.parent.right;

        // Update neck IK
        HeadIK.Target.parent.LookAt(target, up);
     
        // Face head towards look at target
        HeadTransform.LookAt(target, up);
        HeadTransform.rotation *= Quaternion.AngleAxis(-90.0f, Vector3.up);
        HeadTransform.rotation *= Quaternion.AngleAxis(-90.0f, new Vector3(0.0f, 0.0f, 1.0f));
    }

    /// <summary>
    ///     Keeps the legs trailing behind a bit
    /// </summary>
    private void UpdateLegsTarget()
    {
        float distance = Vector3.Dot(LegsTarget.forward, RotationTransform.forward);
        float multiplier = Mathf.Pow(2.0f - distance, LegFollowPower);

        LegsTarget.position = RotationTransform.position;
        LegsTarget.rotation = Quaternion.Slerp(LegsTarget.rotation, RotationTransform.rotation, LegFollowMultiplier * multiplier * DeltaTime);
    }

    /// <summary>
    ///     Updates the pelvis rotation to point towards the velocity direction
    /// </summary>
    private void UpdatePelvisRotation()
    {
        if (OverrideRotation)
        {
            // Rotate the rigidbody relative to the velocity direction
            Quaternion targetRotation = Quaternion.LookRotation(MainRigidBody.velocity, Vector3.up) * BodyRotationOffset * BodyRotationTarget.localRotation;
            MainRigidBody.MoveRotation(Quaternion.Slerp(MainRigidBody.rotation, targetRotation, BodyRotationMultiplierOverride * DeltaTime));

            return;
        }

        switch (CurrentState)
        {
            case PlayerState.Moving:
            {
                // Vertical motion angle (Rotate forward/backwards)
                float verticalAngle = MovingRotationMultiplier[Mathf.RoundToInt(VerticalMotion)];
                Quaternion offset = Quaternion.AngleAxis(-90.0f + verticalAngle, Vector3.right);

                // Side motion angle (Rotate towards the side motion axis)
                float horizontalAngle = SideMotion * (VerticalMotion == 0.0f ? MovingSideRotationAngle : MovingDiagonalRotationAngle);
                offset *= Quaternion.AngleAxis(horizontalAngle, Vector3.up);

                // Rotate the rigidbody in the direction of the forward movement
                Quaternion targetRotation = Quaternion.LookRotation(BoundCamera.forward, Vector3.up) * offset * BodyRotationOffset;

                MainRigidBody.MoveRotation(Quaternion.Slerp(MainRigidBody.rotation, targetRotation, BodyRotationMultiplierMoving * DeltaTime));
                break;
            }

            case PlayerState.Sprinting:
            {
                // Rotate the pelvis only if the player is moving
                if (MainRigidBody.velocity != Vector3.zero)
                {
                    // Rotate the rigidbody in the direction of the velocity
                    Quaternion targetRotation = Quaternion.LookRotation(MainRigidBody.velocity, Vector3.up) * BodyRotationOffset;
                    MainRigidBody.MoveRotation(Quaternion.Slerp(MainRigidBody.rotation, targetRotation, BodyRotationMultiplierSprinting * DeltaTime));
                }

                break;
            }

            case PlayerState.Idle:
            {
                break;
            }

            default:
            {
                Debug.LogError("Player state not supported yet (PlayerMovement.UpdatePelvisRotation)");
                break;
            }
        }
    }

    /// <summary>
    ///     Rotates the upper body in the direction the player is trying to travel in
    /// </summary>
    private void UpdateBodyIKRotation()
    {
        Quaternion targetAngles = Quaternion.identity;

        switch (CurrentState)
        {
            case PlayerState.Moving:
            {
                // Use the correct multiplier
                Vector3 multiplier = Vector3.zero;
                int motion = Mathf.RoundToInt(VerticalMotion);

                if (motion == 0) { multiplier = UpperTurnMultipliersMovingNeutral; }
                else if (motion == 1) { multiplier = UpperTurnMultipliersMovingForwards; }
                else if (motion == -1) { multiplier = UpperTurnMultipliersMovingBackwards; }


                // Rotate the upper body in the direction of the input
                float x = SideMotion * multiplier.x;
                float y = -SideMotion * multiplier.y;
                float z = VerticalMotion * multiplier.z;

                targetAngles = Quaternion.Euler(x, y, z);
                break;
            }

            case PlayerState.Sprinting:
            {
                // Normalise the players angle to be within a more reasonable range
                float horizontal = HorizontalAngle / UpperTurnNormaliseValueSprinting;
                float vertical = VerticalAngle / UpperTurnNormaliseValueSprinting;

                // Clamp the values within rotational maximum
                horizontal = Mathf.Clamp(horizontal, -UpperTurnMaximumSprintingTurnAngle, UpperTurnMaximumSprintingTurnAngle);
                vertical = Mathf.Clamp(vertical, -UpperTurnMaximumSprintingTurnAngle, UpperTurnMaximumSprintingTurnAngle);

                // Rotate the body to match the horizontal and vertical delta angles of the player
                targetAngles = Quaternion.Euler(horizontal, -horizontal, vertical);

                break;
            }

            case PlayerState.Idle:
            {
                break;
            }

            default:
            {
                Debug.LogError("Player state not supported yet (PlayerMovement.UpdateBodyIKRotation)");
                break;
            }
        }

        BodyIKParent.localRotation = Quaternion.Slerp(BodyIKParent.localRotation, targetAngles, BodyIKRotationMultiplier * DeltaTime);
    }

    /// <summary>
    ///     Updates the physics of the player
    /// </summary>
    private void UpdateMovement()
    {
        switch (CurrentState)
        {
            case PlayerState.Moving:
            {
                // Get the movement direction
                TargetDirection = MainCamera.CalculateMovementDirection(SideMotion, VerticalMotion);

                // Set appropriate speed and set to the velocity
                Vector3 velocity = MainRigidBody.velocity + TargetDirection * BaseSpeed * AccelerationMultiplier * DeltaTime;
                MainRigidBody.velocity = velocity;
                break;
            }
            case PlayerState.Sprinting:
            {
                // Lerp Movement
                // ----------------------------------------------------------------------
                // Get the movement direction
                //Vector3 moveDir = MainCamera.CalculateMovementDirectionSprint(SideMotion, VerticalMotion);

                //// Set appropriate speed and set to the velocity
                //Vector3 velocity = MainRigidBody.velocity + moveDir * SprintingSpeed * AccelerationMultiplier * DeltaTime;
                //MainRigidBody.velocity = velocity;
                // ----------------------------------------------------------------------


                // Slerp Movement
                // ----------------------------------------------------------------------
                // Get the target direction
                TargetDirection = MainCamera.CalculateMovementDirectionSprint(SideMotion, VerticalMotion);

                // Get current values
                Vector3 currentDir = MainRigidBody.velocity.normalized;
                float speed = MainRigidBody.velocity.magnitude;

                // Increment current values
                currentDir = Vector3.Slerp(currentDir, TargetDirection, Agility * DeltaTime);
                speed += SprintingSpeed * AccelerationMultiplier * DeltaTime;

                // Update the velocity
                MainRigidBody.velocity = currentDir * speed;
                // ----------------------------------------------------------------------
                break;
            }
            case PlayerState.Idle:
            {

                break;
            }
            default:
            {
                Debug.LogError("Player state not supported yet (PlayerMovement.UpdateMovement)");
                break;
            }
        }
    }

    /// <summary>
    ///     Plays the flip animation
    /// </summary>
    public void Flip()
    {
        MainAnimator.SetTrigger("Flip");
        StartCoroutine(FlipSlowDown());
    }

    const float TimeToBoost = 0.85f;

    /// <summary>
    ///     Slows the player down to its normal speed x seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlipSlowDown()
    {
        yield return new WaitForSeconds(TimeToBoost);
        //MainJuiceManager.PlayAnimation("FoVSpeedUp");

        yield return new WaitForSeconds(SpeedBoostDuration - TimeToBoost);
        AccelerationMultiplier = 1.0f;
    }

    /// <summary>
    ///     Plays the stroke animation for the arms and legs
    /// </summary>
    private void Stroke()
    {
        MainAnimator.SetTrigger("Arms Wave");
        StartCoroutine(DelayedAnimation());
    }

    /// <summary>
    ///     Plays the leg animation x seconds after the initial call
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedAnimation()
    {
        yield return new WaitForSeconds(LegWaveOffset);
        MainAnimator.SetTrigger("Legs Wave");
    }

    /// <summary>
    ///     Updates the acceleration multiplier of the player. 1.0f is normal 1.5 is normal and a half
    ///     This is primarily used as an event in the players animations to give a speed boost
    /// </summary>
    /// <param name="newMultiplier">The new value of the multiplier</param>
    public void UpdateAccelerationMultiplier(float newMultiplier)
    {
        AccelerationMultiplier = newMultiplier;
    }

    /// <summary>
    ///     Updates the camera object that controls the direction of the player
    /// </summary>
    /// <param name="cameraObject">The transform of the camera object. Needs to have a PlayerCamera based component within it</param>
    public void UpdateCamera(Transform cameraObject)
    {
        MainCamera = cameraObject.GetComponent<IPlayerCamera>();
        BoundCamera = cameraObject;
    }
}
