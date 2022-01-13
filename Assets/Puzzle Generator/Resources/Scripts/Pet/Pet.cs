using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pet : MonoBehaviour
{
    public enum LookState
    {
        LOOKFOWARD,     // Look forward in the direction the player is moving
        LOOKTARGET,     // Look at points of interest
    }

    private enum BehaviourState
    {
        WAIT,           // Stays in place 
        LEAD,           // (Attempts to) Lead the player to a place
        STICK,          // Sticks close to the player 
        CHASE,          // Chases after the player
        FOLLOW,         // Follow the player
        SEARCH,         // Find points of interest 
    }

    private enum GizmoType 
    { 
        Never,          // Never shows gizmos
        SelectedOnly,   // Show only when selected
        Always          // Always show gizmo
    }

    [System.Serializable]
    private struct DebugGizmo
    {
        // States
        public GizmoType ShowWaitEnter;
        public GizmoType ShowWaitExit;
        public GizmoType ShowFollowEnter;
        public GizmoType ShowFollowExit;

        // Others
        public GizmoType ShowViewRadius;
        public GizmoType ShowArrivalRadius;
        public GizmoType ShowAvoidDistance;
        public GizmoType ShowTargetPosition;
    }

    [Header("Pet Properties")]
    [SerializeField] private GameObject LookTarget;
    [SerializeField] private GameObject MoveTarget;
    [SerializeField] private GameObject Player;

    [Space(10)]
    [SerializeField] private float DistanceToTarget;
    [SerializeField] private float DistanceToPlayer;

    [Space(5)]
    [SerializeField] private float CurrentSpeed;
    [SerializeField] private float MaxSpeed;
    [SerializeField] private float MaxChaseSpeed;
    [SerializeField] private float MaxSteerForce;
    [SerializeField] private float MaxRotationSpeed;

    [Space(5)]
    [SerializeField] private LayerMask InteractableLayer;
    [SerializeField] private LayerMask ObstacleLayers;
    [SerializeField] private float AvoidDistance;
    [SerializeField] private float ViewRadius;

    [Header("Pet State Properties")]
    [SerializeField] private LookState CurrentLookState;
    [SerializeField] private BehaviourState CurrentBehaviourState;

    [Space(10)]
    [SerializeField] private float ArrivalRadius;
    [SerializeField] private float WaitRadiusEnter;
    [SerializeField] private float WaitRadiusExit;
    [SerializeField] private float FollowRadiusEnter;
    [SerializeField] private float FollowRadiusExit;

    [Space(10)]
    [SerializeField] private float StateChangeCooldown;
    [SerializeField] private float StateChangeTimer;
    [SerializeField] private bool StateChange;


    [Header("Debug")]
    [SerializeField] private bool StopStateAutoChange;
    [SerializeField] private DebugGizmo ToggleDebugGizmos;

    private Vector3 Velocity;
    private Vector3 PreviousVelocity;
    private Vector3 TargetPosition;

    bool IsActive = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Add references if no reference
        if (!Player) 
        {
            Player = GameObject.FindGameObjectWithTag("Player").gameObject; 
        }

        if(!LookTarget)
        {
            LookTarget = GameObject.Find("PetTarget");
        }

        if (!MoveTarget)
        {
            MoveTarget = LookTarget;
        }

        // Initiate variables
        Velocity = new Vector3(0.0f, 1.0f, 0.0f);
        CurrentLookState = LookState.LOOKTARGET;
        CurrentSpeed = 10.0f;
    }

    // Update is called once per frame
    private void Update()
    {
        // Velocity calculator
        Velocity = (transform.position - PreviousVelocity) / Time.deltaTime;
        PreviousVelocity = transform.position;

        // Update distances and positions
        DistanceToTarget = Vector3.Distance(transform.position, TargetPosition);
        DistanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        TargetPosition = MoveTarget.transform.position;

        UpdateBehaviourState();

        // Determine look state
        switch (CurrentLookState)
        {
            case LookState.LOOKFOWARD:
                // If it's not a vector 0 set to new direction, otherwise keep at current direction
                Quaternion rotation = Quaternion.LookRotation(Velocity.normalized == Vector3.zero ? transform.forward : Velocity.normalized);
                
                // Lerp the rotation so its not instant
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, MaxRotationSpeed * Time.deltaTime);
                break;

            case LookState.LOOKTARGET:

                Vector3 direction = LookTarget.transform.position - transform.position;
                Quaternion rotation2 = Quaternion.LookRotation(direction);

                // Lerp the rotation so its not instant
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation2, MaxRotationSpeed * Time.deltaTime);
                break;

            default:
                Debug.LogWarning("You're not supposed to be here", gameObject);
                break;
        }

        // Don't move if waiting or sticking, otherwise move
        if (CurrentBehaviourState != BehaviourState.STICK)
        {
            MoveTo(TargetPosition);
        }
    }

    /// <summary>
    /// Updates the pet's behaviour state based on the enviroment around it
    /// </summary>
    private void UpdateBehaviourState()
    {
        // Stops the pet from updating states (For debugging)
        if (StopStateAutoChange)
        {
            return;
        }

        StateChangeTimer += Time.deltaTime;

        // Auto change state based on conditions
        if (CurrentBehaviourState != BehaviourState.WAIT && 
            DistanceToTarget < WaitRadiusEnter)
        {
            SetCurrentBehaviourState(BehaviourState.WAIT);
        }
        else if (CurrentBehaviourState != BehaviourState.LEAD &&
                 CurrentBehaviourState != BehaviourState.WAIT &&
                 FindInteractable())
        {
            SetCurrentBehaviourState(BehaviourState.LEAD);
        }
        else if (CurrentBehaviourState != BehaviourState.FOLLOW &&
                 CurrentBehaviourState != BehaviourState.LEAD &&
                 DistanceToTarget < FollowRadiusEnter && 
                 DistanceToTarget > WaitRadiusExit)
        {
            SetCurrentBehaviourState(BehaviourState.FOLLOW);
        }
        else if (CurrentBehaviourState != BehaviourState.CHASE && 
                 CurrentBehaviourState != BehaviourState.LEAD &&
                 DistanceToPlayer > FollowRadiusExit)
        {
            SetCurrentBehaviourState(BehaviourState.CHASE);
            MoveTarget = LookTarget;
        }

        // Stops state from being updated every frame
        if (!StateChange)
        {
            return;
        }

        StateChange = false;

        // Determines behaviour state
        switch (CurrentBehaviourState)
        {
            case BehaviourState.WAIT:
            case BehaviourState.STICK:
            case BehaviourState.SEARCH:
                CurrentLookState = LookState.LOOKTARGET;
                break;

            case BehaviourState.LEAD:
            case BehaviourState.CHASE:
            case BehaviourState.FOLLOW:
                CurrentLookState = LookState.LOOKFOWARD;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Calculates and moves to a point specified by position
    /// </summary>
    /// <param name="position">The position to move to</param>
    private void MoveTo(Vector3 position)
    {
        // Get the position offset and steer towards that direction
        Vector3 direction = position - transform.position;
        Vector3 acceleration = SteerTowards(direction);

        if (CollisionCheck())
        {
            acceleration += SteerTowards(ObstacleRays()) * 10.0f;
        }

        // Add the acceleration to current velocity
        Velocity += acceleration * Time.deltaTime;

        // Get the speed and direction of movement
        CurrentSpeed = Velocity.magnitude;
        direction = Velocity / CurrentSpeed;

        // Clamp speed to max speed
        switch (CurrentBehaviourState)
        {
            case BehaviourState.WAIT:
                CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0.0f, MaxSpeed);

                // Stop threshold
                if (CurrentSpeed < 0.1f)
                {
                    CurrentSpeed = 0.0f;
                }

                break;

            case BehaviourState.CHASE:
                CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0.01f, MaxChaseSpeed);
                break;

            default:
                CurrentSpeed = Mathf.Clamp(CurrentSpeed, 1f, MaxSpeed);
                break;
        }

        // Calculate new velocity given direction and speed
        Velocity = direction * CurrentSpeed;

        // Update position and direction
        transform.position += Velocity * Time.deltaTime;
    }

    /// <summary>
    /// Calculates the steer force needed to move in a direction with arrival if
    /// the object is within the arrival radius
    /// </summary>
    /// <param name="direction">Desired direction to move to</param>
    /// <returns>Steer force needed to go in the provided direction (Clamped to max steer force)</returns>
    private Vector3 SteerTowards(Vector3 direction)
    {
        // Switch between arrival and non arrival calculations based on distance to target
        Vector3 v = DistanceToTarget < ArrivalRadius ? 
                    direction.normalized * (MaxChaseSpeed) * (DistanceToTarget / ArrivalRadius) : 
                    direction.normalized * MaxChaseSpeed;       

        return Vector3.ClampMagnitude(v - Velocity, MaxSteerForce);
    }

    /// <summary>
    /// Cast out a sphere ray with radius of the object bounds and given obstacle layers to detect
    /// if the object is on course for a collision
    /// </summary>
    /// <returns>True if a collision occurs</returns>    
    private bool CollisionCheck()
    {
        return Physics.SphereCast(transform.position, 1.0f, Velocity, out _, AvoidDistance, ObstacleLayers);
    }

    /// <summary>
    /// Raycasts a set amount of evenly distributed rays out in a sphere shape to find an unobstructed path
    /// </summary>
    /// <returns>An unobstructed direction vector</returns>
    private Vector3 ObstacleRays()
    {
        // Get all the directions of the rays
        Vector3[] rayDirections = Helper.Directions;

        // Iterate through every direction
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(rayDirections[i]);

            // Check for collision of fish bounds
            if (!Physics.SphereCast(new Ray(transform.position, dir), 1.0f, AvoidDistance, ObstacleLayers))
            {
                return dir;
            }
        }

        // If no unobstructed direction was found, continue on current path 
        return Velocity;
    }

    /// <summary>
    /// Checks to see if there is any object in the interactable layer
    /// If there is, return it
    /// </summary>
    /// <returns>Returns true if interactable found</returns>
    private bool FindInteractable()
    {
        GameObject InteractableTarget = null;

        // Check to see if there's any interactable objects in the object's view range
        foreach (Collider c in Physics.OverlapSphere(transform.position, ViewRadius, InteractableLayer))
        {
            // Put interactable logic here
            InteractableTarget = c.gameObject;
        }

        // Set new move target if interactable found, otherwise ignore
        MoveTarget = InteractableTarget != null ? InteractableTarget : MoveTarget;

        return InteractableTarget;
    }

    /// <summary>
    /// Sets the current behaviour state
    /// 
    /// Can only occur once the switch cooldown is finished
    /// </summary>
    /// <param name="BS">Behaviour State to pass in</param>
    private void SetCurrentBehaviourState(BehaviourState BS)
    {
        if (StateChangeTimer >= StateChangeCooldown)
        {
            StateChangeTimer = 0;
            CurrentBehaviourState = BS;
            StateChange = true;
        }
    }

    public bool GetActive()
    {
        return IsActive;
    }

    public void SetActive(bool _active)
    {
        IsActive = _active;
    }

    public void SetTarget(GameObject _target)
    {
        MoveTarget = _target;
        Player = MoveTarget;
        LookTarget = MoveTarget;
    }

    #region Gizmos
    // Always draws gizmos
    private void OnDrawGizmos()
    {
        if (ToggleDebugGizmos.ShowTargetPosition == GizmoType.Always)
        {
            // Target position
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(TargetPosition, 1.0f);
        }

        if (ToggleDebugGizmos.ShowAvoidDistance == GizmoType.Always)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, Velocity.normalized * AvoidDistance);
        }

        if (ToggleDebugGizmos.ShowWaitEnter == GizmoType.Always)
        {
            // Wait enter radius
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            Gizmos.DrawSphere(TargetPosition, WaitRadiusEnter);
        }

        if (ToggleDebugGizmos.ShowWaitExit == GizmoType.Always)
        {
            // Wait exit radius
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
            Gizmos.DrawSphere(TargetPosition, WaitRadiusExit);
        }

        if (ToggleDebugGizmos.ShowFollowEnter == GizmoType.Always)
        {
            // Follow enter radius
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.2f);
            Gizmos.DrawSphere(TargetPosition, FollowRadiusEnter);
        }

        if (ToggleDebugGizmos.ShowFollowExit == GizmoType.Always)
        {
            // Follow exit radius
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.1f);
            Gizmos.DrawSphere(TargetPosition, FollowRadiusExit);
        }

        if (ToggleDebugGizmos.ShowArrivalRadius == GizmoType.Always)
        {
            // Arrival enter radius
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.05f);
            Gizmos.DrawSphere(TargetPosition, ArrivalRadius);
        }

        if (ToggleDebugGizmos.ShowViewRadius == GizmoType.Always)
        {
            // Arrival enter radius
            Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.05f);
            Gizmos.DrawSphere(transform.position, ViewRadius);
        }
    }

    // Only draws when selected
    void OnDrawGizmosSelected()
    {
        // Target position
        if (ToggleDebugGizmos.ShowTargetPosition == GizmoType.SelectedOnly)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(TargetPosition, 1.0f);
        }

        // Avoidance ray
        if (ToggleDebugGizmos.ShowAvoidDistance == GizmoType.SelectedOnly)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, Velocity.normalized * AvoidDistance);
        }

        // Wait enter radius
        if (ToggleDebugGizmos.ShowWaitEnter == GizmoType.SelectedOnly)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            Gizmos.DrawSphere(TargetPosition, WaitRadiusEnter);
        }

        // Wait exit radius
        if (ToggleDebugGizmos.ShowWaitExit == GizmoType.SelectedOnly)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
            Gizmos.DrawSphere(TargetPosition, WaitRadiusExit);
        }

        // Follow enter radius
        if (ToggleDebugGizmos.ShowFollowEnter == GizmoType.SelectedOnly)
        {
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.2f);
            Gizmos.DrawSphere(TargetPosition, FollowRadiusEnter);
        }

        // Follow exit radius
        if (ToggleDebugGizmos.ShowFollowExit == GizmoType.SelectedOnly)
        {
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.1f);
            Gizmos.DrawSphere(TargetPosition, FollowRadiusExit);
        }

        // Arrival enter radius
        if (ToggleDebugGizmos.ShowArrivalRadius == GizmoType.SelectedOnly)
        {
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.05f);
            Gizmos.DrawSphere(TargetPosition, ArrivalRadius);
        }

        if (ToggleDebugGizmos.ShowViewRadius == GizmoType.SelectedOnly)
        {
            // Arrival enter radius
            Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.05f);
            Gizmos.DrawSphere(transform.position, ViewRadius);
        }
    }
    #endregion
}
