using System;
using UnityEngine;
using PuzzleS;

namespace PuzzleS
{
    ///MovementPuzzle struct and Variables
    [Serializable]
    public struct Puzzle
    {
        ///Number of puzzle pieces
        public int NumPieces;
        ///Timer for puzzle completion (movement Only)
        public float TimeToComplete;
        ///Piece distance (Maximum for Movement, Minimum for collection)
        public float PieceDistance;
        ///Selected puzzle area
        public Vector3 PuzzleArea;
        ///Tag for gameobjects to atach puzzle pieces to (Movement Only)
        public string Tag;
        ///LayerMask for avoiding objects when placing (Collection only)
        public LayerMask ColliderObjects;
        ///Object to be activated by this puzzle
        public GameObject ActivatedObject;
    };
}

[CreateAssetMenu(fileName = "Puzzle Values", menuName="Scriptable Objects/Puzzle Values")]
public class ValuesObject : ScriptableObject
{
    public Puzzle ThisPuzzleValues;
}
