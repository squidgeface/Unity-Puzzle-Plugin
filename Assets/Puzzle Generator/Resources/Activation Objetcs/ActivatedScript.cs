using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatedScript : MonoBehaviour
{
    //Serialized so you can see in inspector when it is activated
    //Is Active bool
    [SerializeField] bool IsActive = false;

    // Update is called once per frame
    void Update()
    {
        //If this object is activated
        if (IsActive)
        {
            ///////////////////////////////
            ////--Add Your Code Here--////
            /////////////////////////////
            ///Example Code:
            GetComponent<MyActivatorScript>().Activate();
            
        }
    }

    /// <summary>
    /// Set this object as activated
    /// </summary>
    /// <param name="_bool">Set if this object is activated</param>
    public void SetActive(bool _bool)
    {
        IsActive = _bool;
    }
}
