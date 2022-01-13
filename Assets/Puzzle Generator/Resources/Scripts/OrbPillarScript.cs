using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbPillarScript : MonoBehaviour
{
    //If this pillar is active
    bool HasActivated = false;
    //Orb move target
    [SerializeField] GameObject OrbTarget;
    //Orb glow material
    [SerializeField] GameObject GlowMaterial1;
    [SerializeField] GameObject GlowMaterial2;
    [SerializeField] GameObject Fire;
    //Material Instance
    Material TempMaterial1;
    Material TempMaterial2;
    private void Start()
    {
        TempMaterial1 = GlowMaterial1.GetComponent<MeshRenderer>().material; 
        TempMaterial2 = GlowMaterial2.GetComponent<MeshRenderer>().material; 
    }

    private void OnTriggerStay(Collider other)
    {
        //If collided object is an orb
        if (other.gameObject.tag == "Player" && !HasActivated)
        {
            
            //Get all scene orbs
            GameObject[] ActiveOrbs = GameObject.FindGameObjectsWithTag("Orb");
            //Iterate through scene orbs
            for (int i = 0; i < ActiveOrbs.Length; i++)
            {
                //If one found that is still active
                if (ActiveOrbs[i].GetComponent<Pet>().GetActive())
                {
                    //set this pillar is activated
                    HasActivated = true;
                    //Set Fire to active
                    Fire.SetActive(true);
                    //Enable material Emissions
                    TempMaterial1.EnableKeyword("_EMISSION"); 
                    TempMaterial2.EnableKeyword("_EMISSION");
                    //Set that orb target to this pillar
                    ActiveOrbs[i].GetComponent<Pet>().SetTarget(OrbTarget);
                    //Set this orb to not active
                    ActiveOrbs[i].GetComponent<Pet>().SetActive(false);
                    break;
                }
            }
            
        }
    }
    /// <summary>
    /// Check status of activation for this object
    /// </summary>
    /// <returns>Has activated bool</returns>
    public bool GetActivated()
    {
        return HasActivated;
    }
}
