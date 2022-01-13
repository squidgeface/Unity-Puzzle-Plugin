using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPuzzlePieceScript : MonoBehaviour
{
    //Is activated bool
    bool IsActivated = false;

    // Update is called once per frame
    void Update()
    {
        //If its activated
        if (IsActivated)
        {
            //Rotate child object
            GetComponentInChildren<Transform>().rotation = Quaternion.Euler(new Vector3(0f, GetComponentInChildren<Transform>().rotation.eulerAngles.y + (100f * Time.deltaTime), 0f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //If player enters collision area
        if (other.gameObject.tag == "Player")
        {
            //set this puzzle piece to activated
            IsActivated = true;
        }
    }
    /// <summary>
    /// Sets activation bool public method
    /// </summary>
    /// <param name="_activate">bool</param>
    public void SetActivated(bool _activate)
    {
        IsActivated = _activate;
    }
    /// <summary>
    /// returns activation bool
    /// </summary>
    /// <returns>bool state of IsActivated</returns>
    public bool GetActivated()
    {
        return IsActivated;
    }
}
