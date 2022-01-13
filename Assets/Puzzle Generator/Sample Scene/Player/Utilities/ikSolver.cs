using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ikSolver : MonoBehaviour
{
    [Header("Controlling Transforms")]
    public Transform Base;
    public Transform Target;
    public Transform Pole;

    [Header("Technical Details")]
    public int ChildrenCount = 3;
    public int Iterations = 10;
    public float Threshold = 0.01f;
    public bool InverseSolve = false;

    [Header("Rotational Offset")]
    public float OffsetValue = 90.0f;
    public Vector3 OffsetUp = Vector3.up;


    private List<Transform> Points;
    private List<float> Lengths;

    private int PointCount;
    private List<Vector3> positions;

    private Quaternion RotationalOffset;

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    /// <summary>
    /// Initialises all the values
    /// </summary>
    void Init()
    {
        // Generate the offset
        OffsetUp.Normalize();
        RotationalOffset = Quaternion.AngleAxis(OffsetValue, OffsetUp);

        // Initialise values
        Points = new List<Transform>();
        Lengths = new List<float>();

        // Reset values
        Points.Clear();
        Lengths.Clear();

        Transform current = gameObject.transform;

        // Retrieve all the points in the joint structure
        while (current != null && Points.Count < ChildrenCount)
        {
            Points.Add(current);
            current = current.childCount == 0 ? null : current.GetChild(0);
        }

        PointCount = Points.Count;

        // Retrieve the lengths between each point
        for (int i = 0; i < PointCount - 1; i++)
        {
            float dist = Vector3.Distance(Points[i].position, Points[i + 1].position);
            Lengths.Add(dist);
        }

        // Create a copy of the positions
        positions = CopyPositions();

    }

    // Update is called once per frame
    void Update()
    {
        if (Target == null || !Target.gameObject.activeSelf)
        {
            FollowUpdate();
        }
        else
        {
            PoleTargetUpdate();
        }
    }

    /// <summary>
    ///     Applies only the forward solver
    /// </summary>
    void FollowUpdate()
    {
        positions[0] = Base.position;

        ForwardSolve();

        // Calculate rotations
        CalculateRotations();

        // Assign the positions to the points
        AssignPositions();
    }

    /// <summary>
    ///     Solves for the base, pole and target
    /// </summary>
    void PoleTargetUpdate()
    {
        positions[0] = Base.position;

        for (int j = 1; j < PointCount - 1; ++j)
        {
            positions[j] = Pole.position;
        }

        // Target is within the radius, use inverse kinematics
        for (int i = 0; i < Iterations; ++i)
        {
            BackwardSolve();
            ForwardSolve();

            // Break out if the end point has reached the target
            float distance = (positions[PointCount - 1] - Target.position).sqrMagnitude;
            if (distance < Threshold * Threshold) { break; }
        }

        CalculateRotations();

        // Assign the positions to the points
        AssignPositions();
    }

    /// <summary>
    ///     Loops through each bone and faces them towards the next one
    /// </summary>
    void CalculateRotations()
    {
        Vector3 up = Base.up;

        for (int i = 0; i < PointCount - 1; ++i)
        {
            Vector3 p0 = positions[i];
            Vector3 p1 = positions[i + 1];

            Points[i].rotation = Quaternion.LookRotation(p1 - p0, up) * RotationalOffset;
        }
    }

    /// <summary>
    ///     Points all bones straight in a certain direction
    /// </summary>
    /// <param name="direction">A unit directional vector of the direction to point in</param>
    void PointDirection(Vector3 direction)
    {
        float currentRadius = 0;

        for (int i = 0; i < PointCount - 1; ++i)
        {
            currentRadius += Lengths[i];
            positions[i + 1] = Points[0].position + direction * currentRadius;
        }
    }

    /// <summary>
    ///     Copies the positions of all the points in the joint
    /// </summary>
    /// <returns>List of the copied positions</returns>
    List<Vector3> CopyPositions()
    {
        List<Vector3> tempPositions = new List<Vector3>();

        foreach (Transform transform in Points)
        {
            tempPositions.Add(transform.position);
        }

        return tempPositions;
    }

    /// <summary>
    ///     Assigns the list of positions to the points in the joint
    /// </summary>
    void AssignPositions()
    {
        for (int i = 0; i < PointCount; ++i)
        {
            Points[i].position = positions[i];
        }
    }

    /// <summary>
    ///     Pulls the points back towards the base
    /// </summary>
    void ForwardSolve()
    {
        int count = PointCount;

        for (int i = 1; i < count; ++i)
        {
            // Retrieve positions of the two points
            Vector3 p0 = positions[i - 1];
            Vector3 p1 = positions[i];

            // Find the direction and length of the bone
            Vector3 direction = (p1 - p0).normalized;
            float length = Lengths[i - 1];

            // Constrain to the magnitude
            positions[i] = p0 + (direction * length);
        }
    }

    /// <summary>
    ///     Pushes the points towards the target point
    /// </summary>
    void BackwardSolve()
    {
        // Set the leaf point to the target position
        positions[PointCount - 1] = Target.position;

        for (int i = PointCount - 2; i > 0; i--)
        {
            // Retrieve positions of the two points
            Vector3 p0 = positions[i];
            Vector3 p1 = positions[i + 1];

            // Find the direction and length of the bone
            Vector3 direction = (p0 - p1).normalized;
            float length = Lengths[i];

            // Constrain to the magnitude
            positions[i] = p1 + (direction * length);
        }
    }

    //// Draws cubes inbetween each point
    //void OnDrawGizmos()
    //{       
    //    if (Points == null || Lengths == null)
    //    {
    //        Init();
    //    }

    //    for (int i = 0; i < PointCount - 1; i++)
    //    {
    //        // Current and next points
    //        Vector3 p1 = Points[i].position;
    //        Vector3 p2 = Points[i + 1].position;

    //        // Distance and scale between points
    //        float dist = Vector3.Distance(p1, p2);
    //        float scale = dist * 0.1f;

    //        // Draw the cube
    //        Handles.matrix = Matrix4x4.TRS(p1, Quaternion.FromToRotation(Vector3.up, p2 - p1), new Vector3(scale, dist, scale));
    //        Handles.color = Color.green;
    //        Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
    //    }
    //}
}
