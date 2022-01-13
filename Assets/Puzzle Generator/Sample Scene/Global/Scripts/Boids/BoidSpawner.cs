using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [Header("Boid Properties")]
    public BoidProperties BP;
    public Transform ObjectToTarget;

    [Header("Spawn Properties")]
    public int Amount;
    public float SpawnRadius;

    [HideInInspector] public List<Boid> SpawnedBoids = new List<Boid>();

    public struct BoidData
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Vector3 AlignmentForce;
        public Vector3 CohesionForce;
        public Vector3 SeperationForce;
        public int Neighbours;

        // Returns the size of the struct
        public static int Size
        {
            get
            {
                return (sizeof(float) * 3 * 5) + sizeof(int);
            }
        }
    }

    [Header("Spawner Debug"), Space(15)]
    public GizmoType ShowSpawnRadius;
    public Color SpawnColour;

    [Header("Boid Debug")] 
    public GizmoType ShowViewDistance;
    public Color ViewColour;
    [Space(10)]
    public GizmoType ShowSeperationDistance;
    public Color SeperationColour;
    [Space(20)]
    public Color FishTintOverride;

    public enum GizmoType { Never, SelectedOnly, Always }

    private void Awake()
    {
        // If no properties specified, disable script and stop fish from being spawned
        if (!BP)
        {
            // Throw an angry fit and shout at bad programmers to fix
            Debug.LogError("Boid spawner does not have a properties referenced!", gameObject);
            enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Spawn x amount of fish on start
        for (int i = 0; i < Amount; i++)
        {
            // Find random position and rotation inside sphere given range and radius
            Vector3 RandomPosition = transform.position + Random.insideUnitSphere * SpawnRadius;
            Quaternion RandomRotation = Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), 0.0f);

            // Spawn a fish
            GameObject Fish = Instantiate(BP.Object, RandomPosition, RandomRotation, transform);

            // Set a override tint if there is one, otherwise set to properties tint
            Fish.transform.GetChild(0).GetComponent<Renderer>().material.color = FishTintOverride == Color.clear ? BP.FishTint : FishTintOverride;

            // Initialize it's properties
            Boid FishBoid = Fish.GetComponent<Boid>();
            FishBoid.BoidTarget = ObjectToTarget;
            FishBoid.BP = BP;

            // Add to list of spawned fish
            SpawnedBoids.Add(FishBoid);
        }
    }

    private void Update()
    {
        // Update only if there's boids on the scene
        if (SpawnedBoids != null)
        {
            // Get total boids and make a struct array
            int TotalBoids = SpawnedBoids.Count;
            BoidData[] BoidData = new BoidData[TotalBoids];

            // Set position and direction of each boid in boid data
            for (int i = 0; i < SpawnedBoids.Count; i++)
            {
                BoidData[i].Position = SpawnedBoids[i].transform.position;
                BoidData[i].Direction = SpawnedBoids[i].transform.forward;
            }

            // Make new computer buffer and set it's data
            ComputeBuffer BoidBuffer = new ComputeBuffer(TotalBoids, BoidSpawner.BoidData.Size);
            BoidBuffer.SetData(BoidData);

            BP.CS.SetBuffer(0, "Boids", BoidBuffer);
            BP.CS.SetInt("TotalBoids", SpawnedBoids.Count);
            BP.CS.SetFloat("ViewDistance", BP.ViewDistance);
            BP.CS.SetFloat("SeperationDistance", BP.SeperationDistance);

            // Split the total boids amongst the threads
            int ThreadGroups = Mathf.CeilToInt(TotalBoids / (float)BP.NumberOfThreads);

            // Excecute the compute shader
            BP.CS.Dispatch(0, ThreadGroups, 1, 1);

            // Get the calculated data from the shader
            BoidBuffer.GetData(BoidData);

            // Set the data to the actual boids
            for (int i = 0; i < SpawnedBoids.Count; i++)
            {
                SpawnedBoids[i].AlignmentForce = BoidData[i].AlignmentForce;
                SpawnedBoids[i].CohesionForce = BoidData[i].CohesionForce;
                SpawnedBoids[i].SeperationForce = BoidData[i].SeperationForce;
                SpawnedBoids[i].Neighbours = BoidData[i].Neighbours;

                SpawnedBoids[i].UpdateBoid();
            }

            // Release the buffer
            BoidBuffer.Release();
        }
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (ShowSpawnRadius == GizmoType.Always) { DrawGizmos(SpawnColour, SpawnRadius); }
    }

    void OnDrawGizmosSelected()
    {
        if (ShowSpawnRadius == GizmoType.SelectedOnly) { DrawGizmos(SpawnColour, SpawnRadius); }
    }

    void DrawGizmos(Color C, float F)
    {
        Gizmos.color = C;
        Gizmos.DrawSphere(transform.position, F);
    }
    #endregion
}
