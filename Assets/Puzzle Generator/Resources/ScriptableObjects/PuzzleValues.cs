using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleValues : MonoBehaviour
{
   public enum Puzzle_Type
    {
        Movement,
        Collection
    };

    [HideInInspector] public Puzzle_Type PuzzleType;

    public ValuesObject ThisPuzzleValues;

}
