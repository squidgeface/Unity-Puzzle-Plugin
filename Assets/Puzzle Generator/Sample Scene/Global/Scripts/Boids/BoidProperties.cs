using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boid Properties", menuName = "Boid Properties")]
public class BoidProperties : ScriptableObject
{
    [Header("Boid Object")]
    public GameObject Object;

    [Header("Compute Shader")]
    public ComputeShader CS;
    public int NumberOfThreads = 1024;

    [Header("Boid Properties")]
    public float MinSpeed = 10;
    public float MaxSpeed = 20;
    public float MaxSteerForce = 20;
    [Space(5)]
    public float ViewDistance = 10;
    public float SeperationDistance = 5;
    [Space(5)]
    public Color FishTint;

    [Header("Targeting")]
    public float TargetRadius;
    public float NoTargetingRadius;

    [Header("Obstacle Avoidance")]
    public LayerMask ObstacleLayers;
    public float AvoidDistance = 10;

    [Header("Force Weighting")]
    public float SeperationWeight = 1;
    public float AlignmentWeight = 1;
    public float CohesionWeight = 1;
    public float TargetWeight = 1;
    public float AvoidWeight = 10;

    private void OnValidate()
    {
        // Making sure these values have a minimum so it doesn't break everything
        if (NoTargetingRadius <= 0) { NoTargetingRadius = 1; }
        if (NumberOfThreads <= 0) { NumberOfThreads = 1; }
    }
}
