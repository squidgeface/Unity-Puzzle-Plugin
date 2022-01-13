using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Boid Properties")]
    public BoidProperties BP;
    public Transform BoidTarget;

    // To pass into compute shader
    [HideInInspector] public Vector3 SeperationForce;
    [HideInInspector] public Vector3 AlignmentForce;
    [HideInInspector] public Vector3 CohesionForce;
    [HideInInspector] public int Neighbours;

    public Vector3 BoidDirection;
    public Vector3 BoidVelocity;
    private Vector3 BoidPosition;

    private Transform BoidTransform;

    [Header("Debug Properties"), Space(15)]
    public GizmoType ShowViewDistance;
    public Color ViewColour;
    [Space(10)]
    public GizmoType ShowSeperationDistance;
    public Color SeperationColour;

    public enum GizmoType { Never, SelectedOnly, Always }

    private void Awake()
    {
        // Store the boid transforms
        BoidTransform = transform;
    }

    private void Start()
    {
        // Store the boid's position and direction
        BoidPosition = BoidTransform.position;
        BoidDirection = BoidTransform.forward;

        // Set a starting speed for the boid
        BoidVelocity = transform.forward * (BP.MinSpeed + BP.MaxSpeed) / 2;
    }

    // Updates the boid
    public void UpdateBoid()
    {

        // Reset acceleration
        Vector3 Acceleration = Vector3.zero;

        // Steer towards target
        if (BoidTarget != null)
        {
            float NoTargetRadius = BP.NoTargetingRadius * BP.NoTargetingRadius;

            // Don't apply targeting force if boid is within no target radius
            if (!((BoidTarget.position - BoidPosition).sqrMagnitude < NoTargetRadius))
            {
                Acceleration = SteerTowards((BoidTarget.position - BoidPosition).normalized  * NoTargetRadius) * BP.TargetWeight;
            }
        }

        // Only apply force if there are neighbours
        if (Neighbours != 0)
        {
            // Get average center point of cohesion force
            CohesionForce /= Neighbours;

            // Add all the forces together
            Acceleration += SteerTowards(CohesionForce - BoidPosition) * BP.CohesionWeight +
                            SteerTowards(SeperationForce) * BP.SeperationWeight +
                            SteerTowards(AlignmentForce) * BP.AlignmentWeight;
        }

        // Check if boid is going to collide with obstacle
        if (CollisionCheck())
        {
            Acceleration += SteerTowards(ObstacleRays()) * BP.AvoidWeight;
        }

        // Add accelation to velocity
        BoidVelocity += Acceleration * Time.deltaTime;

        float Speed = BoidVelocity.magnitude;
        Vector3 Dir = BoidVelocity / Speed;

        // Clamp the speed to a max
        Speed = Mathf.Clamp(Speed, BP.MinSpeed, BP.MaxSpeed);

        // Set boid velocity to it's speed
        BoidVelocity = Dir * Speed;

        // Update boid position and direction
        BoidTransform.position += BoidVelocity * Time.deltaTime;
        BoidTransform.forward = Dir;

        // Store position and direction
        BoidPosition = BoidTransform.position;
        BoidDirection = Dir;
    }

    // Check if boid is headed for a collision 
    bool CollisionCheck()
    {
        // Cast out a sphere with radius of boid bounds and given obstacle layers, Ignore out paramter 
        return Physics.SphereCast(BoidPosition, 1.0f, BoidDirection, out _, BP.AvoidDistance, BP.ObstacleLayers);
    }

    // Raycast out in a sphere and check for unobstructed path
    Vector3 ObstacleRays()
    {
        // Get all the directions of the rays
        Vector3[] RayDirections = Helper.Directions;

        // Iterate through every direction
        for (int i = 0; i < RayDirections.Length; i++)
        {
            Vector3 Dir = BoidTransform.TransformDirection(RayDirections[i]);

            // Check for collision of fish bounds
            if (!Physics.SphereCast(new Ray(BoidPosition, Dir), 1.0f, BP.AvoidDistance, BP.ObstacleLayers))
            {
                return Dir;
            }
        }

        // If no unobstructed direction was found, continue on current path 
        return BoidDirection;
    }

    // Moves the boid towards desired vector
    Vector3 SteerTowards(Vector3 Direction)
    {
        Vector3 V = Direction.normalized * BP.MaxSpeed - BoidVelocity;
        return Vector3.ClampMagnitude(V, BP.MaxSteerForce);
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (ShowViewDistance == GizmoType.Always) { DrawGizmos(ViewColour, BP.ViewDistance); }
        if (ShowSeperationDistance == GizmoType.Always) { DrawGizmos(SeperationColour, BP.SeperationDistance); }
    }

    void OnDrawGizmosSelected()
    {
        if (ShowViewDistance == GizmoType.SelectedOnly) { DrawGizmos(ViewColour, BP.ViewDistance); }
        if (ShowSeperationDistance == GizmoType.SelectedOnly) { DrawGizmos(SeperationColour, BP.SeperationDistance); }
    }

    void DrawGizmos(Color C, float F)
    {
        Gizmos.color = C;
        Gizmos.DrawSphere(transform.position, F);
    }
    #endregion
}
