using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KelpMover : MonoBehaviour
{
    public float Radii = 1;
    public float Force = 1;
    public LayerMask Kelps;

    public Rigidbody RB;

    private void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get all the kelp objects that are within the push radius 
        Collider[] col = Physics.OverlapSphere(transform.position, Radii, Kelps);

        // Scale the force based on the size of the velocity 
        float scale = RB.velocity.magnitude * Force;

        // Loop through each kelp collider
        foreach (Collider hit in col)
        {
            // Get the delta/distance from the kelps position
            Vector3 delta = hit.transform.position - transform.position;
            float dist = (Radii - delta.magnitude) / Radii; // Normalised to be inbetween 0, 1

            // By setting Y to 0 it means that you can't push the kelp into the ground
            delta.y = 0; 

            // Add the force to the kelp object
            Rigidbody kelpRB = hit.GetComponent<Rigidbody>();
            kelpRB.AddForce(delta.normalized * scale * dist, ForceMode.Force);
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, Radii);
    }

}
