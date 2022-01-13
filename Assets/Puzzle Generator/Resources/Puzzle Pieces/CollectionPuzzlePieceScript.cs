using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionPuzzlePieceScript : MonoBehaviour
{
    //Activation bool
    bool ActivateMe = false;
    //Follower gameobject
    [SerializeField] GameObject Follower;
    //Animation handler
    [SerializeField] Animator GrowAni;

    private void OnTriggerStay(Collider other)
    {
        //If player is in the collision area of this puzzle piece
        if (other.gameObject.tag == "Player")
        {
            //If the player is singing and this puzzle piece is not active
            if (Input.GetKeyDown(KeyCode.E) && !ActivateMe)
            {
                //Activate this puzzle piece
                ActivateMe = true;
                //Instantiate follower
                GameObject Orb = Instantiate(Follower, this.transform.position, Quaternion.identity);
                //set orb as active
                Orb.GetComponent<Pet>().SetActive(true);
                //Set grow animation
                GrowAni.SetTrigger("Grow");
            }
        }
    }
}
