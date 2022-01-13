using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KelpAccelerate : MonoBehaviour
{
    [Header("Upward Motion")]
    public float UpForce = 500f;

    [Header("Idle Movement")]
    public float Amplitude;
    public float Frequency;
    public Vector3 Direction;

    private GameObject[] Kelps;


    // Start is called before the first frame update
    private void Start()
    {
        Kelps = GameObject.FindGameObjectsWithTag("Kelp");
        Direction = Direction.normalized;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Loop through all the kelp objects
        for (int i = 0; i < Kelps.Length; i++)
        {
            // Get the middle x, z position of the base segment of the kelp
            Vector3 pos = Kelps[i].transform.GetChild(0).position;
            Vector2 middle = new Vector2(pos.x, pos.z);

            int count = 39;

            Transform current = Kelps[i].transform.GetChild(0);
            int j = 0;

            // Loop through all the segments
            while (current.childCount > 0)
            {
                // Gets the rigidbody of the current kelp object
                Rigidbody kelpRB = current.GetComponent<Rigidbody>();

                // The idle current force to add to each kelp
                float heightValue = ((float)j / count) * 4.0f;
                Vector3 idleForce = Direction * Mathf.Sin(Time.time * Frequency + heightValue + Random.Range(-2.0f, 2.0f)) * Amplitude;

                // Add up and idle forces
                kelpRB.AddForce(new Vector3(0, UpForce, 0), ForceMode.Acceleration);
                kelpRB.AddForce(idleForce, ForceMode.Force);

                j++;
                current = current.GetChild(0);

                if (kelpRB.transform.childCount > 1)
                {
                    // Get directional data
                    Transform leafTransform = kelpRB.transform.GetChild(1);
                    Vector2 delta = new Vector2(kelpRB.velocity.x, kelpRB.velocity.z);
                    //Vector2 delta = new Vector2(kelpRB.position.x, kelpRB.position.z) - middle;

                    // Only do rotations if the distance is far enough away
                    if (delta.sqrMagnitude > 0.001f)
                    {
                        // Get the current vs target angle
                        float targetAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90;
                        float currentAngle = leafTransform.localEulerAngles.x;

                        // Lerp towards that angle
                        leafTransform.localEulerAngles = new Vector3(Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 0.2f), 0.0f, 0.0f);
                        //leafTransform.rotation = Quaternion.AngleAxis(target, kelpRB.transform.up);
                    }
                }
            }
        }
    }
}
