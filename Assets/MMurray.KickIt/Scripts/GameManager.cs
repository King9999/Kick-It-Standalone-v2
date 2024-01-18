using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;
using MMurray.KickIt;
using MMurray.GenericCode;

public class GameManager : MonoBehaviour
{
    [Header("---Room Setup---")]
    public TextAsset roomFile;      //contains all the levels in the game
    Rooms roomList;
    List<string[]> roomData;
    public List<FractionBlock> blockList;           //fraction blocks
    public List<GameObject> tileList;               //wall and floor tiles
    public List<GameObject> floorList;
    public List<GameObject> wallList;
    public List<GameObject> wallEdgeList;
    public GameObject roomContainer, blockContainer;
    public List<FractionBlock> blockGraveyard;      //destroyed blocks go here to prevent garbage collection
    public List<FractionBlock> blocksInView;        //all blocks that are in player's path
    public bool allBlocksViewed;                    //prevents duplicate blocks from being added to blocksInView
    public List<TextBubble> textBubbleList;
    public TextBubble bubblePrefab;
    public bool textBubblesOnScreen;
    public FractionBlock blockPrefab;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject wallEdgePrefab;       //used to decorate the corners of each room except the top wall
    public Player player;
    public Player playerPrefab;
    int rowCount, colCount;
    const float tileXOffset = 1.48f;
    const float tileYOffset = 2f;
    public DialogueManager dialogueManager;
    public HintDialogueManager hintDialogueManager;
    public int level;                       //this is an iterator for roomList
    public int MaxLevels { get;} = 10;             //2 tutorial levels plus 8 normal levels.
    public bool resetButtonPressed;         //used to prevent clicking more than once.

    [Header("---Spark---")]
    public Spark sparkPrefab;
    public List<Spark> sparks;         //these are generated when a block is kicked. 3 sparks total.
    public float sparkSpawnTime;            //how much time must pass before a spark is generated.
    public float currentTime;
    public int currentSpark;           //iterator

    [Header("---Target & Hint Cursor---")]
    public Target targetCursor;
    //public List<Target> hintCursors;
    //public List<FractionBlock> hintBlocks;  //the blocks that the hint cursors are resting on.

    //game states
    public enum GameState { Normal, ShowLevelNumber, OneBlockLeft, AllBlocksClear, GoToNextLevel, ShowRoomClearText, NoComparison, MenuOpen, ResetLevel, DisplayStartOfLevelDialogue, DisplayEndOfLevelDialogue, GameComplete }
    public GameState gameState, previousState;  //previousState is used by menu window to revert to previous state after menu is closed.
    public int dialogueIndex;                   //used for finding the correct dialouge to display at the start or end of level.

    //[Header("---Save State---")]
    //public GameData gameData;
    //[HideInInspector]public SaveState saveState;

    // Start is called before the first frame update
    void Start()
    {
        Singleton.instance.GameManager = this;      //master singleton captures 

        //TODO: CHECK FOR SAVE STATE HERE
        /*if (Singleton.instance.saveStateFound)
        {
            //Singleton.instance.saveState.LoadState(Singleton.instance.saveState.ReadState);
            level = Singleton.instance.gameData.level;  //gameData level should be populated from LoadState in Title Manager.
        }*/
        roomList = JsonUtility.FromJson<Rooms>(roomFile.text);
        Debug.Log("Number of levels (including tutorials): " + roomList.rooms.Length);

        //check data and draw the level to the screen.
        //creating a parent container for the board space objects
        roomContainer.name = "Room Info";
        blockContainer.name = "Block Info";

        //spark setup.
        for (int i = 0; i < 3; i++)
        {
            Spark spark = Instantiate(sparkPrefab, transform.parent);
            //spark.transform.position = transform.position;
            spark.name = "Spark " + (i + 1);
            spark.gameObject.SetActive(false);
            sparks.Add(spark);
        }

        //cursor setup
        /*targetCursor.ShowCursor(false);
        foreach(Target hintCursor in hintCursors)
        {
            hintCursor.ShowCursor(false);
        }*/

        //set up level.
        LoadLevel(level);
    }


    // Update is called once per frame
    void Update()
    {
        //level ends when all blocks are destroyed

        if (blockList.Count <= 0 && gameState == GameState.Normal) //gameState != GameState.AllBlocksClear && gameState != GameState.DisplayEndOfLevelDialogue &&
            //gameState != GameState.ResetLevel)
        {
            //show "clear" text and move to next level after a short duration. Use a coroutine for this.
            /*Coroutine should do the following:
                * -Show "Clear" grahpic
                * -Save progression as dictated by LoL (tutorials don't count)
                * -fade screen to black
                * -load next level
                -fade to normal after level is loaded. */

            //SetGameState(GameState.AllBlocksClear);
            //close hint
            HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
            if (hdm.hintDialogueOpen)
            {
                hdm.dialogueList[level].HideDialogue();
                hdm.Clear();
            }
            SetGameState(GameState.ShowRoomClearText);
        }
        

        if (blockList.Count == 1 && gameState != GameState.OneBlockLeft && gameState != GameState.MenuOpen)
        {
            //Player messed up and can't complete the level. Show a hint to clear level and restart level.
            //close hint
            HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
            if (hdm.hintDialogueOpen)
            {
                hdm.dialogueList[level].HideDialogue();
                hdm.Clear();
            }
            SetGameState(GameState.OneBlockLeft);
        }

        //if the player is next to any fraction blocks, display text bubbles in their field of view.
        if (blocksInView.Count > 0)
        {
            if (!textBubblesOnScreen)
            {
                textBubblesOnScreen = true;
                float originalTextPosY = 30;
                float modifiedTextPosY = 16;        //applied when text bubble is rotated and placed on a block's left or right side.
                float bubbleXSidePos = 1;           //alternates between 1 and -1. Determines which side of the block the bubble appears on
                float originalFontSize = 36;
                for (int i = 0; i < blocksInView.Count; i++)
                {
                    //TODO: Must get the player's facing direction and adjust the bubble's position. If the Y facing direction is not zero, then
                    //the bubbles are positioned to the block's left or right side, possibly alternating.

                    if (player.facingDirection.y == 0)
                    {
                        //only instantiate if there are more blocks than bubbles
                        if (i >= textBubbleList.Count)
                        {
                            TextBubble bubble = Instantiate(bubblePrefab, Singleton.instance.UI.textBubbleContainer.transform);
                            bubble.fractionText.text = blocksInView[i].fractionValueUI.text;

                            //if a block's Y position is 9 or higher, the text bubble will be cut off. In this case, the bubble will appear below the block. We
                            //do this by inverting the Y scale to -1, and the text mesh Y scale to -1(because the text mesh is a child of the bubble, and would also be flipped).
                            if (blocksInView[i].transform.position.y >= 3)  //Why 3?
                            {
                                //invert bubble.
                                bubble.transform.localScale = new Vector3(1, -1, 1);
                                bubble.fractionText.transform.localScale = new Vector3(1, -1, 1);
                                bubble.transform.localRotation = Quaternion.identity;               //in case the bubble was rotated before.
                                bubble.Clamp(new Vector3(blocksInView[i].transform.position.x, blocksInView[i].transform.position.y - 3.5f, 0));
                                Vector3 textPos = bubble.transform.position;
                                bubble.fractionText.fontSize = originalFontSize;
                                bubble.fractionText.transform.localRotation = Quaternion.identity;
                                bubble.fractionText.transform.position = new Vector3(textPos.x, textPos.y, textPos.z); //the text's position is the same as the flipped bubble's pos in order to remain within the bubble
                            }
                            else
                            {
                                bubble.transform.localScale = Vector3.one;
                                bubble.fractionText.transform.localScale = Vector3.one;
                                bubble.transform.localRotation = Quaternion.identity;
                                bubble.Clamp(new Vector3(blocksInView[i].transform.position.x, blocksInView[i].transform.position.y + 3.5f, 0));
                                Vector3 textPos = bubble.transform.position;
                                bubble.fractionText.fontSize = originalFontSize;
                                bubble.fractionText.transform.localRotation = Quaternion.identity;
                                bubble.fractionText.transform.position = new Vector3(textPos.x, textPos.y + originalTextPosY, textPos.z); //30 is added to Y so that the text stays within the bubble after its scale is reset to 1.
                            }
                            //bubble.transform.position = blocksInView[i].transform.position;
                            textBubbleList.Add(bubble);
                        }
                        else
                        {
                            textBubbleList[i].gameObject.SetActive(true);
                            if (blocksInView[i].transform.position.y >= 3)
                            {
                                //invert bubble.
                                textBubbleList[i].transform.localScale = new Vector3(1, -1, 1);
                                textBubbleList[i].fractionText.transform.localScale = new Vector3(1, -1, 1);
                                textBubbleList[i].transform.localRotation = Quaternion.identity;
                                textBubbleList[i].Clamp(new Vector3(blocksInView[i].transform.position.x, blocksInView[i].transform.position.y - 3.5f, 0));
                                Vector3 textPos = textBubbleList[i].transform.position;
                                textBubbleList[i].fractionText.fontSize = originalFontSize;
                                textBubbleList[i].fractionText.transform.localRotation = Quaternion.identity;
                                textBubbleList[i].fractionText.transform.position = new Vector3(textPos.x, textPos.y, textPos.z);
                            }
                            else
                            {
                                textBubbleList[i].transform.localScale = Vector3.one;
                                textBubbleList[i].fractionText.transform.localScale = Vector3.one;
                                textBubbleList[i].transform.localRotation = Quaternion.identity;
                                textBubbleList[i].Clamp(new Vector3(blocksInView[i].transform.position.x, blocksInView[i].transform.position.y + 3.5f, 0));
                                Vector3 textPos = textBubbleList[i].transform.position;
                                textBubbleList[i].fractionText.fontSize = originalFontSize;
                                textBubbleList[i].fractionText.transform.localRotation = Quaternion.identity;
                                textBubbleList[i].fractionText.transform.position = new Vector3(textPos.x, textPos.y + originalTextPosY, textPos.z);
                            }
                            //textBubbleList[i].Clamp(new Vector3(blocksInView[i].transform.position.x, blocksInView[i].transform.position.y + 3.5f, 0));
                            textBubbleList[i].fractionText.text = blocksInView[i].fractionValueUI.text;
                        }
                    }
                    else //need to position bubbles to a block's left or right side.
                    {
                        //only instantiate if there are more blocks than bubbles
                        if (i >= textBubbleList.Count)
                        {
                            TextBubble bubble = Instantiate(bubblePrefab, Singleton.instance.UI.textBubbleContainer.transform);
                            bubble.fractionText.text = blocksInView[i].fractionValueUI.text;

                            //position the bubble to the left or right side. text is adjusted accordingly
                            
                            bubble.transform.localScale = Vector3.one;
                            bubble.fractionText.transform.localScale = new Vector3(1.8f, 1.2f, 1);
                            bubble.fractionText.fontSize = originalFontSize - 12;
                            bubble.Clamp(new Vector3(blocksInView[i].transform.position.x + 3.5f * bubbleXSidePos, blocksInView[i].transform.position.y, 0));

                            //rotate depending on which side bubble is on
                            float rotation = bubbleXSidePos > 0 ? -90 : 90;
                            bubble.transform.localRotation = Quaternion.identity;   //this resets rotation to 0
                            bubble.transform.Rotate(0, 0, rotation);

                            //update text
                            Vector3 textPos = bubble.transform.position;
                            rotation = bubbleXSidePos > 0 ? 90 : -90;       //text rotation is always the inverse of whatever the bubble's rotation is
                            bubble.fractionText.transform.localRotation = Quaternion.identity;
                            bubble.fractionText.transform.Rotate(0, 0, rotation);
                            bubble.fractionText.transform.position = new Vector3(textPos.x + modifiedTextPosY * bubbleXSidePos, textPos.y, textPos.z);  //the x is modified due to the rotation. It's a bit confusing

                            bubbleXSidePos *= -1;
                            textBubbleList.Add(bubble);
                        }
                        else
                        {
                            textBubbleList[i].gameObject.SetActive(true);
                            
                            textBubbleList[i].transform.localScale = Vector3.one;
                            textBubbleList[i].fractionText.transform.localScale = new Vector3(1.8f, 1.2f, 1);
                            textBubbleList[i].Clamp(new Vector3(blocksInView[i].transform.position.x + 3.5f * bubbleXSidePos, blocksInView[i].transform.position.y, 0));

                            //rotate depending on which side bubble is on
                            float rotation = bubbleXSidePos > 0 ? -90 : 90;
                            textBubbleList[i].transform.localRotation = Quaternion.identity;
                            textBubbleList[i].transform.Rotate(0, 0, rotation);

                            Vector3 textPos = textBubbleList[i].transform.position;
                            rotation = bubbleXSidePos > 0 ? 90 : -90;       //text rotation is always the inverse of whatever the bubble's rotation is
                            textBubbleList[i].fractionText.transform.localRotation = Quaternion.identity;
                            textBubbleList[i].fractionText.transform.Rotate(0, 0, rotation);
                            textBubbleList[i].fractionText.fontSize = originalFontSize - 12;
                            textBubbleList[i].fractionText.transform.position = new Vector3(textPos.x + modifiedTextPosY * bubbleXSidePos, textPos.y, textPos.z);
                            
                            textBubbleList[i].fractionText.text = blocksInView[i].fractionValueUI.text;
                            bubbleXSidePos *= -1;
                        }
                    }
                }
            }
        }
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    public void LoadLevel(int level)
    {
        //set up level.
        roomData = new List<string[]>();
        char[] delimiters = { ',', ' ' };

        for (int i = 0; i < roomList.rooms[level].rows.Length; i++)
        {
            string p = roomList.rooms[level].rows[i].row;
            //take data from the room file and remove commas and spaces 
            roomData.Add(p.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries));
            //Debug.Log(roomData[roomData.Count - 1]);
        }

        rowCount = roomList.rooms[level].rows.Length;       //must change 0 to whichever level must be loaded.
        colCount = roomData[0].Length;                      //the index is 0 to keep things simple and to prevent errors. The column count will always be the same for each row.

        //disable all floor and wall objects, and enable when necessary
        foreach (GameObject floor in floorList)
        {
            floor.SetActive(false);
        }

        foreach(GameObject wall in wallList)
        {
            wall.SetActive(false);
        }

        foreach(GameObject wallEdge in wallEdgeList)
        {
            wallEdge.SetActive(false);
        }

        //disable sparks
        StopSparks();

        int currentBlock = 0;         //tracks which block we're getting data from.
        int wallIterator = 0, wallEdgeIterator = 0, floorIterator = 0;  //used to enable wall/floor objects if they exist in their respective lists.
        bool triggerFound;      //
        int j, k;              //these will all be used later.
        int id;                //
        bool idFound;          //  
        for (int row = 0; row < roomData.Count; row++)
        {
            int col = 0;
            foreach (string space in roomData[row])
            {

                switch (space)
                {
                    case "0":   //wall edge
                        /*the wall that's drawn depends on which row or col we're on. Need a different wall tile
                         * for the rightmost, leftmost, top and bottom walls. Any walls in the middle of the room use the top wall. */
                        
                        //check for existing wall objects
                        if (wallEdgeList.Count > 0)
                        {
                            if (wallEdgeIterator >= wallEdgeList.Count)
                            {
                                //we have more objects to add to the level
                                wallEdgeList.Add(Instantiate(wallEdgePrefab, roomContainer.transform));
                                wallEdgeList[wallEdgeList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                                wallEdgeIterator++;
                            }
                            else
                            {
                                wallEdgeList[wallEdgeIterator].SetActive(true);
                                wallEdgeList[wallEdgeIterator].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                                wallEdgeIterator++;
                            }
                        }
                        else
                        {
                            wallEdgeList.Add(Instantiate(wallEdgePrefab, roomContainer.transform));
                            wallEdgeList[wallEdgeList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                            wallEdgeIterator++;
                        }
                       
                        break;

                    case "x":       //wall
                        if (wallList.Count > 0)
                        {
                            if (wallIterator >= wallList.Count)
                            {
                                //we have more objects to add to the level
                                wallList.Add(Instantiate(wallPrefab, roomContainer.transform));
                                wallList[wallList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                                wallIterator++;
                            }
                            else
                            {
                                wallList[wallIterator].SetActive(true);
                                wallList[wallIterator].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                                wallIterator++;
                            }
                        }
                        else
                        {
                            wallList.Add(Instantiate(wallPrefab, roomContainer.transform));
                            wallList[wallList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                            wallIterator++;
                        }
                        break;

                    case "1":   //floor tile
                        AddFloorTile(floorIterator, row, col, roomContainer);
                        floorIterator++;
           
                        break;

                    case "B":
                        //Check block graveyard first before instantiating new block
                        if (blockGraveyard.Count > 0)
                        {
                            //take existing block and update it if necessary.
                            FractionBlock newBlock = blockGraveyard[0];
                            newBlock.gameObject.SetActive(true);
                            blockGraveyard.Remove(blockGraveyard[0]);

                            //must get the row and col value from the JSON
                            int newCol = roomList.rooms[level].blockData[currentBlock].blockCol;
                            int newRow = roomList.rooms[level].blockData[currentBlock].blockRow;

                            newBlock.transform.position = new Vector3((newCol - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - newRow) * tileYOffset - 1, 0);
                            newBlock.blockId = roomList.rooms[level].blockData[currentBlock].blockId;
                            newBlock.blockColor = roomList.rooms[level].blockData[currentBlock].blockColor;

                            //check if block is red
                            if (newBlock.blockColor > 0)
                            {
                                newBlock.isRedBlock = true;
                                newBlock.GetComponent<SpriteRenderer>().sprite = newBlock.blockSprite[1];
                            }
                            else
                            {
                                newBlock.isRedBlock = false;
                                newBlock.GetComponent<SpriteRenderer>().sprite = newBlock.blockSprite[0];
                            }

                            //set the block's value
                            newBlock.blockValue = roomList.rooms[level].blockData[currentBlock].blockValue;
                            newBlock.GetNumeratorDenominator();
                            /*string[] nums = newBlock.blockValue.Split('/');
                            float numerator = float.Parse(nums[0]);
                            float denominator = float.Parse(nums[1]);
                            newBlock.quotient = numerator / denominator;

                            newBlock.fractionValueUI.text = nums[0] + "\n-\n" + nums[1];*/
                            currentBlock++;
                            blockList.Add(newBlock);
                        }
                        else
                        {
                            FractionBlock newBlock = Instantiate(blockPrefab, blockContainer.transform);

                            //get block info from JSON and attach to block.
                            int newCol = roomList.rooms[level].blockData[currentBlock].blockCol;
                            int newRow = roomList.rooms[level].blockData[currentBlock].blockRow;
                            newBlock.transform.position = new Vector3((newCol - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - newRow) * tileYOffset - 1, 0);
                            newBlock.blockId = roomList.rooms[level].blockData[currentBlock].blockId;
                            newBlock.blockColor = roomList.rooms[level].blockData[currentBlock].blockColor;
                            //check if block is red
                            if (newBlock.blockColor > 0)
                            {
                                newBlock.isRedBlock = true;
                                newBlock.GetComponent<SpriteRenderer>().sprite = newBlock.blockSprite[1];
                            }

                            //for the fraction, we must split the blockValue string, then convert to float. The first value will always be the numerator,
                            //and the second value will be the denominator. We do the conversion in order to make comparisons.
                            newBlock.blockValue = roomList.rooms[level].blockData[currentBlock].blockValue;
                            newBlock.GetNumeratorDenominator();
                            /*string[] nums = newBlock.blockValue.Split('/');
                            float numerator = float.Parse(nums[0]);
                            float denominator = float.Parse(nums[1]);
                            newBlock.quotient = numerator / denominator;

                            newBlock.fractionValueUI.text = nums[0] + "\n-\n" + nums[1];*/
                            currentBlock++;
                            blockList.Add(newBlock);
                        }

                        //add floor tile under the block
                        AddFloorTile(floorIterator, row, col, roomContainer);
                        floorIterator++;
                        break;

                    case "P":
                        if (player == null)
                        {
                            player = Instantiate(playerPrefab);
                        }

                        player.Initialize();
                        player.transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);

                        //check for a trigger at player's location
                        triggerFound = false;
                        j = 0;
                        while (!triggerFound && j < roomList.rooms[level].triggerLocations.Length)
                        {
                            if (roomList.rooms[level].triggerLocations[j].triggerCol == col && roomList.rooms[level].triggerLocations[j].triggerRow == row)
                            {
                                triggerFound = true;

                                //find the trigger in dialouge manager by id
                                id = roomList.rooms[level].triggerLocations[j].triggerId;
                                idFound = false;
                                k = 0;
                                while (!idFound && j < dialogueManager.dialogueTrigger.Count)
                                {
                                    if (id == dialogueManager.dialogueTrigger[k].dialogueId)
                                    {
                                        idFound = true;
                                        dialogueManager.dialogueTrigger[k].gameObject.SetActive(true);
                                        dialogueManager.dialogueTrigger[k].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                                    }
                                    else
                                    {
                                        k++;
                                    }
                                }
                            }
                            else
                            {
                                j++;
                            }
                        }

                        //add floor tile under the player
                        AddFloorTile(floorIterator, row, col, roomContainer);
                        floorIterator++;

                        //floorList.Add(Instantiate(floorPrefab, roomContainer.transform));
                        //floorList[floorList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset, ((rowCount / 2) - row) * tileYOffset, 0);
                        break;

                    case "T":
                        //place a trigger. These are invisible.
                        triggerFound = false;
                        j = 0;
                        while (!triggerFound && j < roomList.rooms[level].triggerLocations.Length)
                        {
                            if (roomList.rooms[level].triggerLocations[j].triggerCol == col && roomList.rooms[level].triggerLocations[j].triggerRow == row)
                            {
                                triggerFound = true;

                                //find the trigger in dialouge manager by id
                                id = roomList.rooms[level].triggerLocations[j].triggerId;
                                idFound = false;
                                k = 0;
                                while (!idFound && j < dialogueManager.dialogueTrigger.Count)
                                {
                                    if (id == dialogueManager.dialogueTrigger[k].dialogueId)
                                    {
                                        idFound = true;
                                        dialogueManager.dialogueTrigger[k].gameObject.SetActive(true);
                                        dialogueManager.dialogueTrigger[k].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                                    }
                                    else
                                    {
                                        k++;
                                    }
                                }
                            }
                            else
                            {
                                j++;
                            }
                        }
                        //add floor tile under the trigger
                        AddFloorTile(floorIterator, row, col, roomContainer);
                        floorIterator++;

                        //floorList.Add(Instantiate(floorPrefab, roomContainer.transform));
                        //floorList[floorList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset, ((rowCount / 2) - row) * tileYOffset, 0);
                        break;

                }

            
                col++;
            }
        }

        //check for a save state and update the level layout
        /*if (Singleton.instance.saveStateFound)
        {
            Singleton.instance.saveState.LoadState(Singleton.instance.saveState.ReadState);
        }*/
        //else
        //{
            //graveyard is cleared to prevent any blocks from previous levels interfering with blocks in current level.
            for (int i = 0; i < blockGraveyard.Count; i++)
            {
                Destroy(blockGraveyard[i].gameObject);
                blockGraveyard.Remove(blockGraveyard[i]);
                i--;
            }

            //blockGraveyard.Clear(); 
            //Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
            SetGameState(GameState.ShowLevelNumber);
            Singleton.instance.Recorder.ClearRecord();          //prevents player from undoing a non-existent action when starting new level
            Singleton.instance.UI.hintButtonPressed = false;    //allow player to get a hint again
            Singleton.instance.HintDialogueManager.Clear();
            /*foreach (Target cursor in hdm.hintCursorList)
            {
                cursor.ShowCursor(false);
            }
            hdm.cursorListIndex = 0;*/


            //play music
            if (Singleton.instance.musicEnabled && !Singleton.instance.AudioManager.musicMain.isPlaying)
            {
                //Singleton.instance.AudioManager.musicMain.volume = Singleton.instance.AudioManager.soundVolume;
                Singleton.instance.AudioManager.musicMain.Play();
            }
        //}

        //enable sidebar buttons if past the tutorial levels
        UI ui = Singleton.instance.UI;
        if (level > 1 && !ui.mainSidebarButton.gameObject.activeSelf)
        {
            ui.mainSidebarButton.gameObject.SetActive(true);
            ui.blockIconSidebarButton.gameObject.SetActive(true);
        }

    }

    void AddFloorTile(int index, int row, int col, GameObject parent)
    {
        if (floorList.Count > 0)
        {
            if (index >= floorList.Count)
            {
                floorList.Add(Instantiate(floorPrefab, parent.transform));
                floorList[floorList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                //index++;
            }
            else
            {
                floorList[index].SetActive(true);
                floorList[index].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
                //index++;
            }
        }
        else
        {
            floorList.Add(Instantiate(floorPrefab, parent.transform));
            floorList[floorList.Count - 1].transform.position = new Vector3((col - colCount / 2) * tileXOffset + 1, ((rowCount / 2) - row) * tileYOffset - 1, 0);
            //index++;
        }

    }

    public void StopSparks()
    {
        //disable sparks
        foreach (Spark spark in sparks)
        {
            spark.gameObject.SetActive(false);
        }
        currentSpark = 0;
    }

    public void SetGameState(GameState gameState)
    {
        //int dialogueIndex = 0;
        UI ui = Singleton.instance.UI;

        switch (gameState)
        {
            case GameState.OneBlockLeft:
                //display a message
                this.gameState = GameState.OneBlockLeft;
                string key = dialogueManager.commonDialogueList[0].dialogueKeyList[0];
                //dialogueManager.dialogueOpen = true;
                dialogueManager.commonDialogueList[0].ShowDialogue(key);
                break;

            case GameState.ShowLevelNumber:
                //show current level number
                this.gameState = GameState.ShowLevelNumber;
                if (Singleton.instance.UI.levelDisplayUI != null)
                {
                    Debug.Log("Level Display UI is not null");
                }
                Singleton.instance.UI.levelDisplayUI.gameObject.SetActive(true);
                Debug.Log("Level display UI state: " + Singleton.instance.UI.levelDisplayUI.gameObject.activeSelf);
                Singleton.instance.UI.levelDisplayUI.ShowText();
                break;

            case GameState.AllBlocksClear:
                this.gameState = GameState.AllBlocksClear;
                //check for conditional trigger to activate dialogue
                bool triggerFound = false;
                int i = 0;
                while (!triggerFound && i < dialogueManager.condTriggerList.Count)
                {
                    if (dialogueManager.condTriggerList[i].level == level &&
                        dialogueManager.condTriggerList[i].condition == DialogueManager.ConditionalTrigger.Condition.LevelEnd)
                    {
                        triggerFound = true;
                    }
                    else
                    {
                        i++;
                    }

                }


                if (triggerFound)
                {
                    dialogueIndex = i;
                    goto case GameState.DisplayEndOfLevelDialogue;
                }
                else
                {
                    
                    ui.endOfLevelWindow.gameObject.SetActive(true);
                    ui.endOfLevelWindow.OpenWindow();
                    //goto case GameState.GoToNextLevel;
                }
                break;

            case GameState.ShowRoomClearText:
                //show "Room Clear" animation
                this.gameState = GameState.ShowRoomClearText;

                //temp disable sidebars
                //UI ui = Singleton.instance.UI;

                if (ui.mainSidebar.sidebarOpen)
                    ui.mainSidebar.gameObject.SetActive(false);
                if (ui.blockIconSidebar.sidebarOpen)
                    ui.blockIconSidebar.gameObject.SetActive(false);

                ui.roomClearUI.gameObject.SetActive(true);
                ui.roomClearUI.ShowText();
                break;

            case GameState.ResetLevel:
                //fade out screen so level can load
                
                //if (this.gameState == GameState.MenuOpen)
                //{
                    this.gameState = GameState.ResetLevel;
                    //all blocks must go into the graveyard
                    for (int p = 0; p < blockList.Count; p++)
                    {
                        if (!blockGraveyard.Contains(blockList[p]))
                        {
                            blockGraveyard.Add(blockList[p]);
                        }
                        //blockList.Remove(blockList[p]);
                        //p--;
                    }
                    blockList.Clear();

                    Singleton.instance.ScreenFade.LoadLevelFadeOut(level);
                //}
                //load level. Triggers that already executed are disabled.
                //fade in
                break;

            case GameState.DisplayStartOfLevelDialogue:
                this.gameState = GameState.DisplayStartOfLevelDialogue;
                //display dialogue
                bool dialogueFound = false;
                int j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    if (dialogueManager.condTriggerList[dialogueIndex].dialogueId == dialogueManager.dialogueList[j].dialogueId)
                    {
                        dialogueFound = true;
                        string dKey = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        //dialogueManager.dialogueOpen = true;
                        dialogueManager.dialogueList[j].ShowDialogue(dKey);
                    }
                    else
                    {
                        j++;
                    }
                }

                break;

            case GameState.DisplayEndOfLevelDialogue:
                
                this.gameState = GameState.DisplayEndOfLevelDialogue;
                //display dialogue
                dialogueFound = false;
                j = 0;
                while (!dialogueFound && j < dialogueManager.dialogueList.Count)
                {
                    if (dialogueManager.condTriggerList[dialogueIndex].dialogueId == dialogueManager.dialogueList[j].dialogueId)
                    {
                        dialogueFound = true;
                        string dKey = dialogueManager.dialogueList[j].dialogueKeyList[0];
                        //dialogueManager.dialogueOpen = true;
                        dialogueManager.dialogueList[j].ShowDialogue(dKey);
                    }
                    else
                    {
                        j++;
                    }
                }
                
                break;

            case GameState.NoComparison:    //blocks cannot interact
                //display a message
                this.gameState = GameState.NoComparison;
                key = dialogueManager.commonDialogueList[1].dialogueKeyList[0];
                dialogueManager.commonDialogueList[1].ShowDialogue(key);
                break;

            case GameState.GoToNextLevel:
                //set up next level
                if (level + 1 < MaxLevels)
                {
                    //advance to next level and save progress
                    Singleton.instance.ScreenFade.LoadLevelFadeOut(++level);
                }
                else
                {
                    //game complete
                    goto case GameState.GameComplete;
                }
                break;

            case GameState.GameComplete:
                //display win text
                Singleton.instance.UI.winGameUI.gameObject.SetActive(true);
                Singleton.instance.UI.winGameUI.DisplayWin();
                break;
        }
    }

    public void ScreenShake()
    {
        StartCoroutine(ShakeCamera());
    }

    //set the camera position to a random value temporarily, then set back to normal
    IEnumerator ShakeCamera()
    {
        float shakeValue = 0.3f;
        float xValue = Random.Range(-shakeValue, shakeValue);
        float yValue = Random.Range(-shakeValue, shakeValue);
        Vector3 originalPos = Camera.main.transform.position;

        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + xValue, Camera.main.transform.position.y + yValue, Camera.main.transform.position.z);
        yield return new WaitForSeconds(0.04f);

        Camera.main.transform.position = originalPos;
    }
}
