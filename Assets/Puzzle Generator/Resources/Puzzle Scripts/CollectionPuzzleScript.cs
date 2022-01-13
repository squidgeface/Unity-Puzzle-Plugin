using PuzzleS;
using UnityEngine;

public class CollectionPuzzleScript : MonoBehaviour
{
    //This puzzle's puzzle values
    Puzzle ThisPuzzleValues;
    //this puzzle's activation object
    GameObject ActivatedObject;
    ///Pillars gameobject array
    GameObject[] Pillars;

    void Start()
    {
        ///Set the values object
        ThisPuzzleValues = GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues;
        ///Set the activated object
        ActivatedObject = ThisPuzzleValues.ActivatedObject;
        ///Set pillars size
        Pillars = new GameObject[ThisPuzzleValues.NumPieces];
        ///int counter for finding pillars
        int Counter = 0;
        ///Initialise Pillars gameobjects
        for (int i = 0; i < ActivatedObject.transform.childCount; i++)
        {
            //Check child objects for pillars
            if (ActivatedObject.transform.GetChild(i).gameObject.tag == "Pillar")
            {
                //Add pillar to array
                Pillars[Counter] = ActivatedObject.transform.GetChild(i).gameObject;
                Counter++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        int PillarsActive = 0;

        for (int i = 0; i < ThisPuzzleValues.NumPieces; i++)
        {
            if (Pillars[i].GetComponent<OrbPillarScript>().GetActivated())
            {
                PillarsActive++;
            }
        }

        //If all pieces active in time
        if (PillarsActive == ThisPuzzleValues.NumPieces)
        {
            ActivatedObject.GetComponent<ActivatedScript>().SetActive(true);
            Destroy(this);
        }
    }

}
