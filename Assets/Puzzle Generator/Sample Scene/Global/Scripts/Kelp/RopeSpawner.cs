using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSpawner : MonoBehaviour
{
    public List<GameObject> LeafPrefabs;
    public bool SnapFirst;

    private void Start()
    {
        SpawnBetterRope();
    }

    // Ignores the last joint because che's sausage has a pointless joint at the end 
    // (if you fix will need to edit KelpAccelerate also as that also ignores the last one)
    public void SpawnBetterRope()
    {
        Rigidbody previous = null;
        Transform current = transform.GetChild(0);

        int i = 0;

        // Loop through all the children
        while (current.childCount > 0)
        {
            // Add joints
            CharacterJoint cj = current.gameObject.AddComponent<CharacterJoint>();
            Rigidbody rb = current.gameObject.GetComponent<Rigidbody>();
            SphereCollider sc = current.gameObject.AddComponent<SphereCollider>();

            // Set character joint defaults
            cj.twistLimitSpring = new SoftJointLimitSpring() { spring = 100 };
            cj.lowTwistLimit = new SoftJointLimit() { limit = -5.0f };
            cj.highTwistLimit = new SoftJointLimit() { limit = 5.0f };

            cj.swingLimitSpring = new SoftJointLimitSpring() { spring = 100 };
            cj.swing1Limit = new SoftJointLimit() { limit = 5.0f };
            cj.swing2Limit = new SoftJointLimit() { limit = 5.0f };

            // Set sphere collider defaults
            sc.radius = 0.5f;
            sc.isTrigger = true;

            // Set rigid body defaults
            rb.useGravity = false;
            rb.mass = 0.1f;
            rb.drag = 3.0f;

            // Should only be null for the first object
            if (previous != null)
            {
                cj.connectedBody = previous;

                if (i % 3 == 0)
                {
                    // Add leaf (in here so that it ignores the base joint)
                    GameObject leaf = Instantiate(LeafPrefabs[Random.Range(0, LeafPrefabs.Count)]);
                    leaf.transform.parent = current;
                    leaf.transform.localPosition = Vector3.zero;
                    leaf.transform.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                }
            }
            else if (SnapFirst)
            {
                // Freezes the first joint
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            // Increment down a child
            previous = rb;
            current = current.GetChild(0);
            ++i;
        }
    }
}
