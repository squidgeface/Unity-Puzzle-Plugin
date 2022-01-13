using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PuzzleS;


public class PuzzleGenerator_Editor : EditorWindow
{
    ///Enum for stage of puzzle generation
    enum Generation_Stage
    {
        ///Selection stage
        Select,
        ///Edit stage
        Edit
    };
    ///Puzzle Type enum used for spawning selected puzzle
    enum Puzzle_Type
    {
        PleaseSelect,
        Movement,
        Collection
    };

    ////////////////////////
    ////// Variables ///////
    ////////////////////////
   
    ///Gameobject to instantiate
    [SerializeField] GameObject PuzzleGameObject;
    ///Terrain object for height
    Terrain TerrainObject;
    ///Temporary gameobject to store activator object
    GameObject TempActivatedObject;
    ///Temp gameobject for current puzzle
    GameObject TempPuzzleObject;
    ///Game Object for Pillars for collection puzzle
    GameObject[] Pillars;
    ///float for movement puzzle piece placement offset
    float PuzzlePieceOffset = 5;

    ///Struct for use in this class
    Puzzle PuzzleVariables;
    ///Enum Initialisation
    Puzzle_Type PuzzleSelection = Puzzle_Type.PleaseSelect;
    Generation_Stage PuzzleGenStage = Generation_Stage.Select;

    ///Editor Window initialisation
    [MenuItem("Window/PuzzleGenerator")]
    static void Init()
    {
        ///Create and show a puzzle generator window
        PuzzleGenerator_Editor newGenerator = (PuzzleGenerator_Editor)EditorWindow.GetWindow(typeof(PuzzleGenerator_Editor));
        newGenerator.titleContent.text = "Puzzle Generator Editor";
        newGenerator.Show();
    }

    [ExecuteInEditMode]
    private void OnGUI()
    {
        ///Name the puzzle objects in the scene
        RenamePuzzleObjects();

        ///Check if active puzzle gameobject selected
        if (Selection.activeObject != null)
        {
            if (Selection.activeObject.GetType() == typeof(GameObject))
            {
                GameObject tempObj = (GameObject)Selection.activeObject;

                if (tempObj.GetComponent<PuzzleValues>() != null)
                {
                    PuzzleGenStage = Generation_Stage.Edit;
                }

            }
        }

        ///Swithc based on puzzle generation stage
        switch (PuzzleGenStage)
        {
            ///Selecting which puzzle to generate (no puzzle selected)
            case Generation_Stage.Select:
                {
                    ///Select which type of puzzle to generate
                    PuzzleSelection = (Puzzle_Type)EditorGUILayout.EnumPopup("Puzzle Type", PuzzleSelection);
                    //If a puzzle type has been chosen
                    if (PuzzleSelection != Puzzle_Type.PleaseSelect)
                    {
                        ///Generate puzzle button
                        if (GUILayout.Button("Create New Puzzle"))
                        {
                            ///Find the Puzzle scene parent
                            GameObject PuzzleHolder = GameObject.Find("ScenePuzzles");

                            ///If it doesnt already exist
                            if (PuzzleHolder == null)
                            {
                                ///Create parent empty gameobject
                                PuzzleHolder = new GameObject("ScenePuzzles");
                            }


                            ///Only generate if a type of puzzle has been selected
                            if (PuzzleSelection != Puzzle_Type.PleaseSelect)
                            {

                                ///Change puzzle generation stage to edit
                                PuzzleGenStage = Generation_Stage.Edit;


                                ///Change selected objects puzzle type

                                switch (PuzzleSelection)
                                {
                                    case Puzzle_Type.Movement:
                                        ///If it doesnt exist, Create Parent empty gameobject
                                        GameObject MovementHolder = GameObject.Find("Movement Puzzles");
                                        if (MovementHolder == null)
                                        {
                                            MovementHolder = new GameObject("Movement Puzzles");
                                            ///Set as child of puzzle holder
                                            MovementHolder.transform.parent = PuzzleHolder.transform;
                                        }

                                        ///Instantiate new gameobject in scene
                                        TempPuzzleObject = Instantiate(PuzzleGameObject, MovementHolder.transform) as GameObject;
                                        ///Make gameobject the active object in the onspector
                                        Selection.activeObject = TempPuzzleObject;
                                        ///Set puzzle type
                                        TempPuzzleObject.GetComponent<PuzzleValues>().PuzzleType = PuzzleValues.Puzzle_Type.Movement;
                                        ///Instantiate a new scriptable object for this puzzle
                                        TempPuzzleObject.GetComponent<PuzzleValues>().ThisPuzzleValues = ScriptableObject.CreateInstance<ValuesObject>();


                                        break;

                                    case Puzzle_Type.Collection:
                                        ///If it doesnt exist, Create Parent empty gameobject
                                        GameObject CollectionHolder = GameObject.Find("Collection Puzzles");
                                        if (CollectionHolder == null)
                                        {
                                            CollectionHolder = new GameObject("Collection Puzzles");
                                            ///Set as child of puzzle holder
                                            CollectionHolder.transform.parent = PuzzleHolder.transform;
                                        }

                                        ///Instantiate new gameobject in scene
                                        TempPuzzleObject = Instantiate(PuzzleGameObject, CollectionHolder.transform) as GameObject;
                                        ///Make gameobject the active object in the onspector
                                        Selection.activeObject = TempPuzzleObject;
                                        ///Set puzzle type
                                        TempPuzzleObject.GetComponent<PuzzleValues>().PuzzleType = PuzzleValues.Puzzle_Type.Collection;
                                        ///Instantiate a new scriptable object for this puzzle
                                        TempPuzzleObject.GetComponent<PuzzleValues>().ThisPuzzleValues = ScriptableObject.CreateInstance<ValuesObject>();

                                        break;
                                    default:
                                        break;
                                }
                            }
                            ///Set puzzle default area size
                            TempPuzzleObject.GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues.PuzzleArea = new Vector3(100, 30, 100);
                        }
                    }
                    break;
                }
                ///Edit stage of puzzle creation (a puzzle is selected)
            case Generation_Stage.Edit:
                {
                    ///If nothing is selected
                    if (Selection.activeObject != null)
                    {
                        ///Instantiate temp gameobject
                        GameObject tempGO = null;

                        ///check if selected object is a gameobject in the scene
                        ///Required otherwise non scene objects selected will be cause an invalid cast
                        if (Selection.activeObject.GetType() == typeof(GameObject))
                        {
                            ///Set temp object as the selected gameobject
                            tempGO = (GameObject)Selection.activeObject;
                        }
                        ///If the temp object has successfully been set
                        if (tempGO) {
                            ///if temp gameobject has a component of buzzle values (is a puzzle object)
                            if (tempGO.GetComponent<PuzzleValues>() != null)
                            {
                                ///Get puzzle values from currently selected puzzle
                                PuzzleVariables = tempGO.GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues;
                                ///Switch UI based on selected puzzle
                                switch (tempGO.GetComponent<PuzzleValues>().PuzzleType)
                                {
         ///////////////////////////////
        /// Movement Puzzle editing ///
       ///////////////////////////////
                                    case PuzzleValues.Puzzle_Type.Movement:
                                        {
                                            ///Editor window Inputs
                                            ///Editor Label header
                                            EditorGUILayout.LabelField("Editing " + tempGO.name.ToString(), EditorStyles.centeredGreyMiniLabel);
                                            ///Puzzle area adjustment
                                            PuzzleVariables.PuzzleArea = EditorGUILayout.Vector3Field(new GUIContent("Puzzle Area Size", "Choose a size for the puzzle area"), PuzzleVariables.PuzzleArea);
                                            ///Set the maximum distance between objects based on the total area / 4
                                            PuzzleVariables.PieceDistance = (PuzzleVariables.PuzzleArea.x + PuzzleVariables.PuzzleArea.y + PuzzleVariables.PuzzleArea.z) / 4f;
                                            ///Set Puzzle Values area for gizmo
                                            tempGO.GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues.PuzzleArea = PuzzleVariables.PuzzleArea;
                                            ///Select the number of puzzle pieces
                                            PuzzleVariables.NumPieces = EditorGUILayout.IntSlider(new GUIContent("Puzzle Pieces", "Select the Number of Puzzle Pieces"), PuzzleVariables.NumPieces, 2, 10);
                                            ///Set the time required to complete the puzzle
                                            PuzzleVariables.TimeToComplete = EditorGUILayout.IntField(new GUIContent("Puzzle Time", "Total time to complete puzzle (seconds)"), (int)PuzzleVariables.TimeToComplete);
                                            ///Dont let the Time to complete go lower than 3 * the number of pieces
                                            if (PuzzleVariables.TimeToComplete < PuzzleVariables.NumPieces * 3)
                                            {
                                                PuzzleVariables.TimeToComplete = PuzzleVariables.NumPieces * 3;
                                            }
                                            ///Tag input for puzzle piece placement
                                            PuzzleVariables.Tag = EditorGUILayout.TextField(new GUIContent("Placement Tag", "Tag your gameobjects you want the puzzle pieces to spawn near with this"), PuzzleVariables.Tag);
                                            ///Puzzle Piece placement offset
                                            PuzzlePieceOffset = EditorGUILayout.FloatField(new GUIContent("Placement Offset", "Offset the puzzle pieces by this amount to move them closer to the centre"), PuzzlePieceOffset);
                                            ///Select the prefab for the object to be activated by this puzzle
                                            TempActivatedObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Activated Object", "Select the Object to activate on puzzle completion"), TempActivatedObject, typeof(GameObject), false);

                                            ///Only show generate button when the puzzle piece gameobject is available
                                            if (TempActivatedObject != null && PuzzleVariables.Tag != null)
                                            {
                                                ///Generate puzzle button pressed
                                                if (GUILayout.Button("Generate Puzzle"))
                                                {
                                                    ///If puzzle script not already added
                                                    if(!tempGO.GetComponent<MovementPuzzleScript>())
                                                    {
                                                        ///Add movement puzzle script
                                                        tempGO.AddComponent<MovementPuzzleScript>();
                                                    }
                                                    ///Find all the gameobjects in the puzle area and put them in a colider array
                                                    Collider[] hit = Physics.OverlapBox(tempGO.transform.position + new Vector3(0, 15, 0), PuzzleVariables.PuzzleArea / 2f, Quaternion.identity);
                                                    ///Create a list of gameobjects
                                                    var TaggedGameobjectList = new List<GameObject>();
                                                    ///for each gameobject in the collider
                                                    foreach (Collider item in hit)
                                                    {
                                                        ///If the gameobject is tagged with user tag
                                                        if (item.gameObject.tag == PuzzleVariables.Tag)
                                                        {
                                                            ///Add to list
                                                            TaggedGameobjectList.Add(item.gameObject);
                                                        }
                                                    }
                                                    ///Convert list to gameobject array
                                                    GameObject[] TaggedGameobjects = TaggedGameobjectList.ToArray();

                                                    ///Delete all current child puzzle gameobjects
                                                    var children = new List<GameObject>();
                                                    foreach (Transform child in tempGO.transform) children.Add(child.gameObject);
                                                    children.ForEach(child => DestroyImmediate(child));
                                                    children.Clear();

                                                    ///Create a list to store set puzzle pieces
                                                    var PuzzlePieceList = new List<GameObject>();

                                                    ///If there are at least 2 Gameobjects in the array
                                                    if (TaggedGameobjects.Length > 1)
                                                    {
                                                        //Ray cast hit for finding terrain
                                                        RaycastHit TerrainHit;
                                                        ///Instatiate set Activator object at the origin of the puzzle object above the ground
                                                        PuzzleVariables.ActivatedObject = Instantiate(TempActivatedObject, tempGO.transform.position + new Vector3(0, PuzzleVariables.PuzzleArea.y/2, 0), Quaternion.identity, tempGO.transform) as GameObject;
                                                        ///Add activator script to this object
                                                        PuzzleVariables.ActivatedObject.AddComponent<ActivatedScript>();
                                                        ///Raycast down from this objects position
                                                        if (Physics.Raycast(PuzzleVariables.ActivatedObject.transform.position, Vector3.down, out TerrainHit))
                                                        {
                                                            //If terrain object exists
                                                            if (TerrainHit.collider.gameObject.GetComponent<Terrain>())
                                                            {
                                                                ///Move Activator onto terrain
                                                                PuzzleVariables.ActivatedObject.transform.position = new Vector3(PuzzleVariables.ActivatedObject.transform.position.x, TerrainHit.point.y, PuzzleVariables.ActivatedObject.transform.position.z);
                                                            }
                                                        }
                                                        ///If there are less gameobjects in the array than pieces to put down
                                                        ///Put all pieces down on available gameobjects (No need to do random)
                                                        if (TaggedGameobjects.Length <= PuzzleVariables.NumPieces)
                                                        {
                                                            
                                                            ///Temp counter for instantiating puzzle pieces in order
                                                            int count = 0;
                                                            ///Set total number of pieces to total number of gameobjects
                                                            PuzzleVariables.NumPieces = TaggedGameobjects.Length;
                                                            ///Gameobject list for instantiating the puzzle pieces
                                                            
                                                            foreach (GameObject GO in TaggedGameobjects)
                                                            {
                                                                ///Get vector of this gameobject to the center
                                                                Vector3 centerVector = tempGO.transform.position - GO.transform.position;
                                                                ///Find a position to put the puzzle piece on the vector
                                                                Vector3 piecePosition = GO.transform.position + (centerVector.normalized * PuzzlePieceOffset);
                                                                ///Put a puzzle piece on one of the found locations
                                                                PuzzlePieceList.Add(Instantiate((GameObject)Resources.Load("Puzzle Pieces/MovementPuzzlePiece"), piecePosition, Quaternion.identity, tempGO.transform));
                                                                ///Give the puzzle piece a name
                                                                PuzzlePieceList[count].name = "Puzzle Piece " + (count + 1).ToString();
                                                                ///iterate counter
                                                                count++;
                                                            }

                                                        }
                                                        ///Else use a random method for placement
                                                        else
                                                        {
                                                            ///Error handler for infinite loops
                                                            int ErrorHandler = 0;
                                                            ///new list of gameobjects
                                                            var TempRandomList = new List<GameObject>();
                                                            ///Random gameobject
                                                            GameObject tempRandomGameobject;
                                                            ///Temp counter for instantiating puzzle pieces in order
                                                            int count = 0;
                                                            
                                                            ///while count is less than or equal to the number of pieces
                                                            while (count < PuzzleVariables.NumPieces)
                                                            {
                                                                ///If error handler is high, break from the loop
                                                                if (ErrorHandler > TaggedGameobjects.Length)
                                                                {
                                                                    break;
                                                                }
                                                                ///Get random gameobject from array
                                                                tempRandomGameobject = TaggedGameobjects[UnityEngine.Random.Range(0, TaggedGameobjects.Length)];
                                                                ///Bool for checking if this object has already been used
                                                                bool IsInList = false;
                                                                ///check if gameobject has been used already
                                                                for (int i = 0; i < TempRandomList.Count; i++)
                                                                {
                                                                    if (tempRandomGameobject == TempRandomList[i])
                                                                    {
                                                                        ///Set this object is in the list
                                                                        IsInList = true;
                                                                    }
                                                                }
                                                                ///If gameobject is not in the list 
                                                                if (!IsInList)
                                                                {
                                                                    ///If first gameobject placed
                                                                    if (TempRandomList.Count == 0)
                                                                    {
                                                                        ///Put random gameobject found in the list
                                                                        TempRandomList.Add(tempRandomGameobject);
                                                                        ///Get vector of this gameobject to the center
                                                                        Vector3 centerVector = tempGO.transform.position - tempRandomGameobject.transform.position;
                                                                        ///Find a position to put the puzzle piece on the vector
                                                                        Vector3 piecePosition = tempRandomGameobject.transform.position + (centerVector.normalized * PuzzlePieceOffset);
                                                                        ///Put a puzzle piece on one of the found locations
                                                                        PuzzlePieceList.Add(Instantiate((GameObject)Resources.Load("Puzzle Pieces/MovementPuzzlePiece"), piecePosition, Quaternion.identity, tempGO.transform));
                                                                        ///Give the puzzle piece a name
                                                                        PuzzlePieceList[count].name = "Puzzle Piece " + (count + 1).ToString();
                                                                        ///iterate counter
                                                                        count++;
                                                                    }
                                                                    ///If Gameobject is too far away from the previously placed random gameobject
                                                                    else if (Vector3.Distance(tempRandomGameobject.transform.position, TempRandomList[count - 1].transform.position) < PuzzleVariables.PieceDistance)
                                                                    {
                                                                        ///Put random gameobject found in the list
                                                                        TempRandomList.Add(tempRandomGameobject);
                                                                        ///Get vector of this gameobject to the center
                                                                        Vector3 centerVector = tempGO.transform.position - tempRandomGameobject.transform.position;
                                                                        ///Find a position to put the puzzle piece on the vector
                                                                        Vector3 piecePosition = tempRandomGameobject.transform.position + (centerVector.normalized * PuzzlePieceOffset);
                                                                        ///Put a puzzle piece on one of the found locations
                                                                        PuzzlePieceList.Add(Instantiate((GameObject)Resources.Load("Puzzle Pieces/MovementPuzzlePiece"), piecePosition, Quaternion.identity, tempGO.transform));
                                                                        ///Give the puzzle piece a name
                                                                        PuzzlePieceList[count].name = "Puzzle Piece " + (count + 1).ToString();
                                                                        ///iterate counter
                                                                        count++;
                                                                    }
                                                                    else
                                                                    {
                                                                        ///Iterate error handler 
                                                                        ErrorHandler++;
                                                                    }
                                                                }
                                                            }
                                                        }  
                                                    }
                                                    
                                                }
                                            }

                                            break;
                                        }
         /////////////////////////////////
        /// Collection Puzzle editing ///
       /////////////////////////////////                   
                                    case PuzzleValues.Puzzle_Type.Collection:
                                        {

                                            ///Editor Label header
                                            EditorGUILayout.LabelField("Editing " + tempGO.name.ToString(), EditorStyles.centeredGreyMiniLabel);


                                            ///Puzzle area adjustment
                                            PuzzleVariables.PuzzleArea = EditorGUILayout.Vector3Field(new GUIContent("Puzzle Area Size", "Choose a size for the puzzle area"), PuzzleVariables.PuzzleArea);
                                            ///Set Puzzle Values area for gizmo
                                            tempGO.GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues.PuzzleArea = PuzzleVariables.PuzzleArea;

                                            ///Select the number of puzzle pieces
                                            PuzzleVariables.NumPieces = EditorGUILayout.IntSlider(new GUIContent("Puzzle Pieces", "Select the Number of Puzzle Pieces"), PuzzleVariables.NumPieces, 2, 10);
                                            ///Tag input for puzzle piece placement
                                            PuzzleVariables.Tag = EditorGUILayout.TextField(new GUIContent("Collision Tag", "Tag your gameobjects you want the puzzle pieces to spawn near with this"), PuzzleVariables.Tag);

                                            ///Select the prefab for the object to be activated by this puzzle
                                            TempActivatedObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Activated Object", "Select the Object to activate on puzzle completion"), TempActivatedObject, typeof(GameObject), false);
                                       
                                            ///Only show generate button when the puzzle piece gameobject is available
                                            if (TempActivatedObject != null)
                                            {
                                                
                                                ///If Generate puzzle button pressed
                                                if (GUILayout.Button("Generate Puzzle"))
                                                {
                                                    //If no script attached
                                                    if(!tempGO.GetComponent<CollectionPuzzleScript>())
                                                    {
                                                        ///Add collection puzzle script 
                                                        tempGO.AddComponent<CollectionPuzzleScript>();
                                                    }
                                                    //Raycast for terrain
                                                    RaycastHit TerrainHit;
                                                    ///Initialise pillars
                                                    Pillars = new GameObject[PuzzleVariables.NumPieces];

                                                    ///Delete all current child puzzle gameobjects
                                                    var children = new List<GameObject>();
                                                    foreach (Transform child in tempGO.transform) children.Add(child.gameObject);
                                                    children.ForEach(child => DestroyImmediate(child));

                                                    ///Set Activator object
                                                    PuzzleVariables.ActivatedObject = Instantiate(TempActivatedObject, new Vector3(tempGO.transform.position.x, PuzzleVariables.PuzzleArea.y/2, tempGO.transform.position.z), Quaternion.identity, tempGO.transform) as GameObject;
                                                    ///Add activator script to this object
                                                    PuzzleVariables.ActivatedObject.AddComponent<ActivatedScript>();
                                                    ///Raycast down from this objects position
                                                    if (Physics.Raycast(PuzzleVariables.ActivatedObject.transform.position, Vector3.down, out TerrainHit))
                                                    {
                                                        //If terrain object exists
                                                        if (TerrainHit.collider.gameObject.GetComponent<Terrain>())
                                                        {
                                                            ///Move Activator onto terrain
                                                            PuzzleVariables.ActivatedObject.transform.position = new Vector3(PuzzleVariables.ActivatedObject.transform.position.x, TerrainHit.point.y, PuzzleVariables.ActivatedObject.transform.position.z);
                                                        }
                                                    }

                                                    ///Instantiate all the puzzle pieces on the ground
                                                    for (int i = 0; i < PuzzleVariables.NumPieces; i++)
                                                    {
                                                        
                                                        ///Convert number of pieces to angles on a circle
                                                        float angle = ((Mathf.PI * 2) / PuzzleVariables.NumPieces) * (i + 1);
                                                        //Get an x and z position for this piece in a circle
                                                        float x = Mathf.Sin(angle) * (PuzzleVariables.NumPieces/2 + 1);
                                                        float z = Mathf.Cos(angle) * (PuzzleVariables.NumPieces/2 + 1); 
                                                        ///Spawn a pillars around Activation object
                                                        Pillars[i] = Instantiate((GameObject)Resources.Load("prefabs/Collection Puzzle/Pillar"), PuzzleVariables.ActivatedObject.transform.position + new Vector3(x, 0, z), Quaternion.identity, PuzzleVariables.ActivatedObject.transform);
                                                        
                                                        ///Int used for error handling in while loop
                                                        int ErrorHandler = 0;
                                                        ///Get origin of the puzzle object (center)
                                                        Vector3 Origin = tempGO.transform.position;
                                                        ///Get ground position with offset + puzzle gameobject position offset by the puzzle area y /2 over area to raycast down to find terrain
                                                        Vector3 piecePosition = Origin + new Vector3(UnityEngine.Random.Range(-PuzzleVariables.PuzzleArea.x/2, PuzzleVariables.PuzzleArea.x/2), PuzzleVariables.PuzzleArea.y/2, UnityEngine.Random.Range(-PuzzleVariables.PuzzleArea.z / 2, PuzzleVariables.PuzzleArea.z/2));
                                                        ///Raycast down from this piece position
                                                        if(Physics.Raycast(piecePosition, Vector3.down, out TerrainHit))
                                                        {
                                                            //If terrain object exists
                                                            if (TerrainHit.collider.gameObject.GetComponent<Terrain>())
                                                            {
                                                                piecePosition.y = TerrainHit.point.y;
                                                            }
                                                        }
                                                        

                                                        ///Check for cliff edge and nearby gameobjects
                                                        while (!CheckGround(piecePosition))
                                                        {
                                                            ///Keep on attempting to put the piece down for 1000 times until breaking
                                                            ///This avoids infinite loops if the user puts in bad puzzle generation values
                                                            if (ErrorHandler < 1000)
                                                            {
                                                                ///Get ground position with offset
                                                                piecePosition = Origin + new Vector3(UnityEngine.Random.Range(-PuzzleVariables.PuzzleArea.x / 2, PuzzleVariables.PuzzleArea.x / 2), PuzzleVariables.PuzzleArea.y/2, UnityEngine.Random.Range(-PuzzleVariables.PuzzleArea.z / 2, PuzzleVariables.PuzzleArea.z / 2));
                                                                ///Raycast down from this piece position
                                                                if (Physics.Raycast(piecePosition, Vector3.down, out TerrainHit))
                                                                {
                                                                    ///If terrain object exists
                                                                    if (TerrainHit.collider.gameObject.GetComponent<Terrain>())
                                                                    {
                                                                        piecePosition.y = TerrainHit.point.y;
                                                                    }
                                                                }
                                                                
                                                                ///Iterate error handler
                                                                ErrorHandler++;
                                                            }
                                                            else
                                                            {
                                                                ///Break loop when 1000 errors hit
                                                                break;
                                                            }
                                                        }
                                                   
                                                        ///Instantiate the puzzle piece
                                                        GameObject tempPuzzlePiece = Instantiate((GameObject)Resources.Load("Puzzle Pieces/CollectionPuzzlePiece"), piecePosition, Quaternion.identity, tempGO.transform) as GameObject;
                                                        ///Give the puzzle piece a name
                                                        tempPuzzlePiece.name = "Puzzle Piece " + (i + 1).ToString();
                                                    }  
                                                }
                                            }

                                            break;
                                        }
                                    
                                    default:
                                        break;
                                }
                                ///Update currently selected puzzle values (can put this in a function)
                                tempGO.GetComponent<PuzzleValues>().ThisPuzzleValues.ThisPuzzleValues = PuzzleVariables;
                            }
                            else
                            {
                                ///If UI action is repaint (This stops errors)
                                if (Event.current.type == EventType.Repaint)
                                {
                                    ///Set puzzle generation stage to Select
                                    PuzzleGenStage = Generation_Stage.Select;
                                    ///Set puzzle selection enum to default
                                    PuzzleSelection = Puzzle_Type.PleaseSelect;
                                }
                            }
                        }
                        

                    }
                    ///Else nothing is selected
                    else
                    {
                        ///If UI action is repaint (This stops errors)
                        if (Event.current.type == EventType.Repaint)
                        {
                            ///Set puzzle generation stage to Select
                            PuzzleGenStage = Generation_Stage.Select;
                            ///Set puzzle selection enum to default
                            PuzzleSelection = Puzzle_Type.PleaseSelect;
                        }
                    }

                    break;
                }

            default:
                break;
        }


    }

    /// <summary>
    /// Renames all the puzzle game objects in the scene to Puzzle + number in order
    /// </summary>
    private void RenamePuzzleObjects()
    {
        ///Get all gameobjects in the scene with puzzle tag
        GameObject[] allGOs = GameObject.FindGameObjectsWithTag("Puzzle");
        ///Counter for naming
        int MCounter = 1;
        int CCounter = 1;
        ///For all gameobjects in array
        foreach (GameObject tmpGameObject in allGOs)
        {
            ///If movement puzzle type
            if (tmpGameObject.GetComponent<PuzzleValues>().PuzzleType == PuzzleValues.Puzzle_Type.Movement)
            {
                ///Rename as movement puzzle + MCounter
                tmpGameObject.name = "Movement puzzle " + MCounter.ToString();
                ///Iterate MCounter
                MCounter++;
            }
            ///If Collection puzzle type
            else if (tmpGameObject.GetComponent<PuzzleValues>().PuzzleType == PuzzleValues.Puzzle_Type.Collection)
            {
                ///Rename as collection puzzle + COunter
                tmpGameObject.name = "Collection puzzle " + CCounter.ToString();
                ///Iterate CCounter
                CCounter++;
            }
        }
    }

    /// <summary>
    /// Checks for ground underneath, to determine some info about it, including the slope angle.
    /// </summary>
    /// <param name="origin">Location of puzzle piece to start checking from</param>
    bool CheckGround(Vector3 origin)
    {
        ///Maximum distance to check
        float MaximumDistance = 5;
        /// Initial ground slope angle to 0
        float GroundSlopeAngle = 0;
        /// Radius to search
        float Radius = 1f;
        /// Max distance for raycast
        float MaxDist = 1f;
        /// Out hit point for our cast
        RaycastHit hit;
        /// SPHERECAST down to check for ground
        if (Physics.SphereCast(origin, Radius, Vector3.down, out hit, MaxDist))
        {
            
            /// A hit normal is at a 90 degree angle from the surface that is collided with (at the point of collision).
            /// e.g. On a flat surface, both vectors are facing straight up, so the angle is 0.
            GroundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            //If it is not 0, there is a slope (not a flat surface)
            if (GroundSlopeAngle != 0)
            {
                //Return false
                return false;
            }
        }

        ///Repeat with raycasts with offset to check around origin
        RaycastHit SlopeHit1;
        ///Offset 1 for search
        Vector3 Offset1 = new Vector3(-2f, 0f, 2f);
        ///Offset 2 for search
        Vector3 Offset2 = new Vector3(-2f, 0f, -2f);

        /// FIRST RAYCAST down with offset  to check for ground
        if (Physics.Raycast(origin + Offset1, Vector3.down, out SlopeHit1, MaxDist))
        {
            /// Get angle of slope on hit normal
            float angleOne = Vector3.Angle(SlopeHit1.normal, Vector3.up);
            //Check for zero angle and return if not zero
            if (angleOne != 0)
            {
                return false;
            }
        }
        ///If nothing hit (empty space) return false
        else
        {
            return false;
        }
        /// SECOND RAYCAST
        if (Physics.Raycast(origin + Offset2, Vector3.down, out SlopeHit1, MaxDist))
        {
            /// Get angle of slope of the hit point.
            float angleTwo = Vector3.Angle(SlopeHit1.normal, Vector3.up);
            //Check for zero angle and return if not zero
            if (angleTwo != 0)
            {
                return false;
            }
        }
        //If nothing hit (empty space) return false
        else
        {
            return false;
        }
        /// THIRD RAYCAST
        if (Physics.Raycast(origin - Offset1, Vector3.down, out SlopeHit1, MaxDist))
        {
            /// Get angle of slope on hit normal
            float angleThree = Vector3.Angle(SlopeHit1.normal, Vector3.up);
            ///Check for zero angle and return if not zero
            if (angleThree != 0)
            {
                return false;
            }
        }
        ///If nothing hit (empty space) return false
        else
        {
            return false;
        }

        /// FOURTH RAYCAST
        if (Physics.Raycast(origin - Offset2, Vector3.down, out SlopeHit1, MaxDist))
        {
            /// Get angle of slope of the hit point.
            float anglefour = Vector3.Angle(SlopeHit1.normal, Vector3.up);
            //Check for zero angle and return if not zero
            if (anglefour != 0)
            {
                return false;
            }
        }
        ///If nothing hit (empty space) return false
        else
        {
            return false;
        }


        ///Racast in other directions to check for walls at the max distance
        ///Check right
        if (Physics.Raycast(origin, Vector3.right, out SlopeHit1, MaximumDistance))
        {
            ///If there is a collided gameobject
            if (SlopeHit1.collider.gameObject)
            {
                //Return false
                return false;
            }
        }
        ///Check forward
        if (Physics.Raycast(origin, Vector3.forward, out SlopeHit1, MaximumDistance))
        {
            ///If there is a collided gameobject
            if (SlopeHit1.collider.gameObject)
            {
                //Return false
                return false;
            }
        }
        ///Check left
        if (Physics.Raycast(origin, Vector3.left, out SlopeHit1, MaximumDistance))
        {
            ///If there is a collided gameobject
            if (SlopeHit1.collider.gameObject)
            {
                //Return false
                return false;
            }
        }
        ///Check backward
        if (Physics.Raycast(origin, Vector3.back, out SlopeHit1, MaximumDistance))
        {
            ///If there is a collided gameobject
            if (SlopeHit1.collider.gameObject)
            {
                ///Return false
                return false;
            }
        }

        ///Box cast around this object slightly above the floor
        Collider[] BoxCheck = Physics.OverlapBox(origin + new Vector3(0, 1.5f, 0), new Vector3(MaximumDistance, 1, MaximumDistance));

        foreach (var GO in BoxCheck)
        {
            ///If collision is with the terrain, a rock or another puzzle piece
            if (GO.gameObject.GetComponent<Terrain>() || GO.gameObject.tag == PuzzleVariables.Tag || GO.GetComponent<CollectionPuzzlePieceScript>() || GO.GetComponent<ActivatedScript>())
            {
                ///Return false
                return false;
            }
        }

        ///Return Ground slope angle
        return (GroundSlopeAngle == 0);
    }


    /// <summary>
    /// Show gizmos based on selected object and puzzle area
    /// </summary>
    /// <param name="puzzle">Object has a puzzle values script</param>
    /// <param name="gizmoType">unused but necessary</param>
    [DrawGizmo(GizmoType.Selected)]
    static void DrawGizmos(PuzzleValues puzzle, GizmoType gizmoType) 
    { 
        ///Size based on puzzle values of object
        Vector3 size = puzzle.ThisPuzzleValues.ThisPuzzleValues.PuzzleArea;
        ///Switch colour based on puzzle type
        switch (puzzle.PuzzleType)
        {
            case PuzzleValues.Puzzle_Type.Movement:
                Gizmos.color = Color.red;
                break;
            case PuzzleValues.Puzzle_Type.Collection:
                Gizmos.color = Color.yellow;
                break;
            default:
                break;
        }
        ///Draw wire cube based on puzzle values and position with offset
        Gizmos.DrawWireCube(puzzle.transform.position + new Vector3(0,15,0), size); 
    }


}
