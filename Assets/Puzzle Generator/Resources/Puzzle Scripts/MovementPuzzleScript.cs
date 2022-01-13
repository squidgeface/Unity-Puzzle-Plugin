using UnityEngine;
using PuzzleS;
public class MovementPuzzleScript : MonoBehaviour
{
    //Puzzle Pieces array for this puzzle
    GameObject[] ThisPuzzlePieces;
    //Timer for this puzzle
    float PuzzleTime = 0;
    //This puzzle values
    Puzzle ThisPuzzleValues;
    //Game Object to be activated by puzzle
    GameObject ActivatedObject;

    //Timer
    float timer;
    //Bool for timer
    bool TimerStarted = false;

    void Start()
    {
        //Set this puzzle pieces array to number of children minus 1
        ThisPuzzlePieces = new GameObject[transform.childCount-1];
        //Set puzzle piece array to include all the puzzle pieces but not the activator
        for (int i = 1; i < transform.childCount; i++)
        {
            ThisPuzzlePieces[i-1] = transform.GetChild(i).gameObject;
        }
        //Set this puzzle values from attached puzzle values script
        ThisPuzzleValues = GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues;
        //Set this puzzle time for timer
        PuzzleTime = ThisPuzzleValues.TimeToComplete;
        //Set this puzzle activated object
        ActivatedObject = ThisPuzzleValues.ActivatedObject;
    }

    // Update is called once per frame
    void Update()
    {
        //Temp int of number of pieces activated
        int piecesActive = 0;
        //For all puzzle pieces
        for (int i = 0; i < ThisPuzzlePieces.Length; i++)
        {
            //If this piece is activated
            if (ThisPuzzlePieces[i].GetComponent<MovementPuzzlePieceScript>().GetActivated())
            {
                //Itterate puzzle pieces activated to match number of pieces activated
                piecesActive++;
                //Start timer
                TimerStarted = true;
            }
        }

        //If all pieces active in time
        if (piecesActive == ThisPuzzlePieces.Length)
        {
            //Set activated object script to true
            ActivatedObject.GetComponent<ActivatedScript>().SetActive(true);
            //Stop timer
            TimerStarted = false;
        }

        //If puzzle timer goes above puzzle time
        if (timer >= PuzzleTime)
        {
            //Show console message in debug mode
#if DEBUG
            Debug.Log("You ran out of time!");
#endif
            //Stop Timer
            TimerStarted = false;
            //Reset Timer
            timer = 0;
            //Reset Pieces to inactive
            for (int i = 0; i < ThisPuzzlePieces.Length; i++)
            {
                ThisPuzzlePieces[i].GetComponent<MovementPuzzlePieceScript>().SetActivated(false);
            }
        }

        //Increment timer when running
        if (TimerStarted)
        {
            timer += Time.deltaTime;
        }
    }


}
