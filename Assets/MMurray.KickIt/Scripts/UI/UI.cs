using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
//using LoLSDK;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    public Button undoButton, hintButton;
    Color undoButtonColorOn, undoButtonColorOff, hintButtonColorOn, hintButtonColorOff;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI menuButtonText, undoButtonText, noSpaceText, hintButtonText; //noSpaceText is shown when there's not enough space to kick a block
    public List<Result> resultList;    //displays resulting comparison when blocks are destroyed.
    public List<bool> resultActive;     //when false, the corresponding Result in resultList is disabled.
    public GameObject resultsContainer, textBubbleContainer;
    public Result resultPrefab;
    public MenuWindow menuWindow;
    public HelpWindow helpWindow;
    public EndOfLevelWindow endOfLevelWindow;
    public RoomClearUI roomClearUI;
    public LevelDisplayUI levelDisplayUI;
    public WinGameUI winGameUI;
    bool animateNoSpaceTextCoroutineOn;
    public bool hintButtonPressed;
    public Sidebar mainSidebar, blockIconSidebar;
    public Button mainSidebarButton, blockIconSidebarButton;
    Color buttonColorOn, buttonColorOff;            //used for sidebar buttons

    private void Awake()
    {
        Singleton.instance.UI = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Button button = undoButton.GetComponent<Button>();
        //button.onClick.AddListener(UndoButtonPressed);
        Singleton.instance.UI = this;
        undoButtonColorOn = undoButton.image.color;  //GetComponent<Color>();
        undoButtonColorOff = new Color(undoButtonColorOn.r * 0.2f, undoButtonColorOn.g * 0.2f, undoButtonColorOn.b * 0.2f);
        hintButtonColorOn = hintButton.image.color;
        hintButtonColorOff = new Color(hintButtonColorOn.r * 0.2f, hintButtonColorOn.g * 0.2f, hintButtonColorOn.b * 0.2f);

        //UI button text setup
        Singleton singleton = Singleton.instance;
        menuButtonText.text = singleton.GetText("ui_menuButtonText");
        undoButtonText.text = singleton.GetText("ui_undoButtonText");
        noSpaceText.text = singleton.GetText("ui_noSpaceText");
        hintButtonText.text = singleton.GetText("ui_hintButtonText");
        noSpaceText.gameObject.SetActive(false);
        animateNoSpaceTextCoroutineOn = false;

        menuWindow.gameObject.SetActive(false);
        helpWindow.gameObject.SetActive(false);
        endOfLevelWindow.gameObject.SetActive(false);

        //sidebar setup
        buttonColorOff = new Color(0.2f, 0.02f, 0.19f, 0.6f);
        buttonColorOn = new Color(0.1f, 0.77f, 0.01f, 0.6f);        //light green
        mainSidebar.gameObject.SetActive(false);
        blockIconSidebar.gameObject.SetActive(false);
        mainSidebarButton.gameObject.SetActive(false);
        blockIconSidebarButton.gameObject.SetActive(false);
        //levelDisplayUI.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //undo button state check
        if (Singleton.instance.Recorder.canUndo)
            undoButton.image.color = undoButtonColorOn;
        else
            undoButton.image.color = undoButtonColorOff;

        //hint button state check
        if (hintButtonPressed)
            hintButton.image.color = hintButtonColorOff;
        else
            hintButton.image.color = hintButtonColorOn;
    }

    /*This is used by the undo button. When pressed, the player's position is replaced with their previous location.
     * Any blocks that were moved or destroyed are restored to their previous state. */
    public void UndoButtonPressed()
    {
        if (Singleton.instance.GameManager.gameState != GameManager.GameState.Normal)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        GameManager gm = Singleton.instance.GameManager;
        Recorder rec = Singleton.instance.Recorder;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        if (Singleton.instance.Recorder.canUndo)
        {
            gm.player.transform.position = rec.playerLastPos;
            gm.player.facingDirection = rec.playerFacingDirection;

            //check for any previously destroyed blocks and restore them. This step is done before resetting any moved blocks
            //because that block may have been destroyed.
            while (rec.lastBlocksDestroyed.Count > 0)
            {
                int lastBlock = rec.lastBlocksDestroyed.Count - 1;
                FractionBlock block = rec.lastBlocksDestroyed[lastBlock];
                //restore these blocks and remove them from the list
                block.gameObject.SetActive(true);
                gm.blockList.Add(block);

                //was this block a hint block?
                HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
                foreach (Target cursor in hdm.hintCursorList)
                {
                    if (cursor.blockId == block.blockId)
                    {
                        //hdm.hintBlocks.Add(block);
                        cursor.isDestroyedByHintBlock = false;
                        cursor.ShowCursor(true);
                        cursor.OccupyBlock(block);
                    }
                }

                rec.lastBlocksDestroyed.Remove(block);
                gm.blockGraveyard.Remove(block);
            }

            //undo block movement
            bool blockFound = false;
            int i = 0;
            while(!blockFound && i < gm.blockList.Count)
            {
                FractionBlock targetBlock = gm.blockList[i];
                if (targetBlock.blockId == rec.lastBlockMoved.blockId)
                {
                    blockFound = true;
                    targetBlock.transform.position = rec.blockLastPos;

                    //reset target cursors
                    Target cursor = gm.targetCursor;
                    if (cursor.gameObject.activeSelf)
                    {
                        cursor.transform.position = targetBlock.transform.position;
                    }

                    //hint block?
                    foreach(Target hintCursor in Singleton.instance.HintDialogueManager.hintCursorList)
                    {
                        if (hintCursor.blockId == targetBlock.blockId)
                        {
                            hintCursor.ShowCursor(true);
                            hintCursor.OccupyBlock(targetBlock);
                        }
                    }
                }
                else
                {
                    i++;
                }
            }

            //Check if any of the previous hint cursors need to be restored
            /*HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
            
            if (hdm.hintBlocks.Count > 0)
            {
                for (i = 0; i < hdm.hintBlocks.Count; i++)
                {
                    hdm.hintCursorList[i].ShowCursor(true);
                    hdm.hintCursorList[i].OccupyBlock(hdm.hintBlocks[i]);
                }
            }*/

            //clear blocks viewed and hide text bubbles
            gm.allBlocksViewed = false;
            gm.blocksInView.Clear();
            gm.textBubblesOnScreen = false;
            foreach (TextBubble bubble in gm.textBubbleList)
            {
                bubble.gameObject.SetActive(false);
            }

            rec.canUndo = false;
            rec.lastBlockMoved = null;
            undoButton.image.color = undoButtonColorOff;
            Debug.Log("Last action undone!");
        }
    }

    /* Search the block list for two blocks that can be compared and highlight them. Hint button can be used twice per level */
    public void HintButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState != GameManager.GameState.Normal || hintButtonPressed)
            return;

        Debug.Log("Hint activated");
        hintButtonPressed = true;
        Singleton.instance.HintDialogueManager.GetHint(gm.level);

        /*SearchForComparison();
        if (gm.hintBlocks.Count > 0)
        {
            for (int i = 0; i < gm.hintBlocks.Count; i++) 
            {
                gm.hintCursors[i].ShowCursor(true);
                gm.hintCursors[i].OccupyBlock(gm.hintBlocks[i]);
            }
            
        }*/

        //search for a valid comparison and highlight
        /*int i = 0; int j = 0;
        bool comparisonFound = false;
        while(!comparisonFound && i < gm.blockList.Count)
        {
            j = i + 1;
            while(!comparisonFound && j < gm.blockList.Count) 
            {
                if (gm.blockList[i].numerator == gm.blockList[j].numerator || gm.blockList[i].denominator == gm.blockList[j].denominator)
                {
                    comparisonFound = true;
                }
                else
                {
                    j++;
                }
            }
            if (!comparisonFound)
                i++;
        }

        if (comparisonFound)
        {
            //highlight the blocks
            gm.hintCursors[0].ShowCursor(true);
            gm.hintCursors[0].OccupyBlock(gm.blockList[i]);
            //gm.hintCursors[0].gameObject.transform.position = gm.blockList[i].transform.position;
            gm.hintBlocks.Add(gm.blockList[i]);

            gm.hintCursors[1].ShowCursor(true);
            gm.hintCursors[1].OccupyBlock(gm.blockList[j]);
            //gm.hintCursors[1].gameObject.transform.position = gm.blockList[j].transform.position;
            gm.hintBlocks.Add(gm.blockList[j]);

        }*/
    }

    //restart a level from the beginning
    /*public void ResetButtonPressed()
    {
        if (Singleton.instance.GameManager.gameState == GameManager.GameState.ResetLevel || 
            Singleton.instance.GameManager.gameState == GameManager.GameState.AllBlocksClear)
            return;

        //Singleton.instance.GameManager.resetButtonPressed = true;
        Singleton.instance.GameManager.SetGameState(GameManager.GameState.ResetLevel);
    }*/

    //display a comparison result when 2 blocks are destroyed
    //blockPos is the position of the 2nd block (usually the static block)
    public void ShowResultText(string blockOneValue, string comparison, string blockTwoValue, Vector2 blockPos)
    {
        int i = 0;
        bool resultFound = false;


        //check for existing instances
        if (resultList.Count > 0)
        {
            while (!resultFound) /*&& i < resultList.Count)*/
            {
                if (i >= resultList.Count)
                {
                    resultList.Add(Instantiate(resultPrefab, resultsContainer.transform));
                    resultActive.Add(true);
                    resultFound = true;
                    resultList[i].fractionOne.text = blockOneValue;
                    resultList[i].fractionTwo.text = blockTwoValue;
                    resultList[i].comparisonOp.text = comparison;
                    resultList[i].startPos = resultList[i].Clamp(new Vector2(blockPos.x, blockPos.y));
                }
                else if (!resultActive[i])
                {
                    resultActive[i] = true;
                    resultFound = true;
                    resultList[i].gameObject.SetActive(true);   //when active, Update() triggers the coroutine
                    resultList[i].fractionOne.text = blockOneValue;
                    resultList[i].fractionTwo.text = blockTwoValue;
                    resultList[i].comparisonOp.text = comparison;
                    resultList[i].startPos = resultList[i].Clamp(new Vector2(blockPos.x, blockPos.y));
                }
                else
                {
                    i++;
                }
            }
        }
        else
        {
            resultList.Add(Instantiate(resultPrefab, resultsContainer.transform));
            resultActive.Add(true);
            resultList[i].fractionOne.text = blockOneValue;
            resultList[i].fractionTwo.text = blockTwoValue;
            resultList[i].comparisonOp.text = comparison;
            resultList[i].startPos = resultList[i].Clamp(new Vector2(blockPos.x, blockPos.y));
        }
    }

    public void HelpButtonPressed()
    {

    }

    public void MenuButtonPressed()
    {
        //open up a window containing additional buttons, including reset, help, TTS and music toggle, and return to title.
        //call coroutine to expand window
        if (menuWindow.gameObject.activeSelf || helpWindow.gameObject.activeSelf)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        //save the game's previous state before updating
  
        //Singleton.instance.GameManager.gameState = GameManager.GameState.MenuOpen;
        menuWindow.gameObject.SetActive(true);
        menuWindow.OpenWindow();
    }

    public void OnMainSidebarButtonPressed()
    {
        if (Singleton.instance.GameManager.gameState != GameManager.GameState.Normal)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        mainSidebar.sidebarOpen = !mainSidebar.sidebarOpen;
        mainSidebar.gameObject.SetActive(mainSidebar.sidebarOpen);

        //disabled block icon sidebar
        if (mainSidebar.sidebarOpen)
        {
            ColorBlock colorBlock = mainSidebarButton.colors;
            colorBlock.normalColor = buttonColorOn;
            colorBlock.highlightedColor = buttonColorOn;
            mainSidebarButton.colors = colorBlock;

            //TTS check
            /*if (Singleton.instance.ttsEnabled)
            {
                mainSidebar.ReadSidebar();
                Debug.Log("Reading Main Sidebar text");
            }*/

            if (blockIconSidebar.gameObject.activeSelf)
            {
                blockIconSidebar.sidebarOpen = false;
                blockIconSidebar.gameObject.SetActive(false);
                colorBlock = blockIconSidebarButton.colors;
                colorBlock.normalColor = buttonColorOff;
                colorBlock.highlightedColor = buttonColorOff;
                blockIconSidebarButton.colors = colorBlock;
            }
        }
        else //update button color
        {
            ColorBlock colorBlock = mainSidebarButton.colors;
            colorBlock.normalColor = buttonColorOff;
            colorBlock.highlightedColor = buttonColorOff;
            mainSidebarButton.colors = colorBlock;

            //turn off TTS in case it's currently playing
            /*if (Singleton.instance.ttsEnabled)
            {
                ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            }*/
        }
    }

    public void OnBlockIconSidebarButtonPressed()
    {
        if (Singleton.instance.GameManager.gameState != GameManager.GameState.Normal)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        blockIconSidebar.sidebarOpen = !blockIconSidebar.sidebarOpen;
        blockIconSidebar.gameObject.SetActive(blockIconSidebar.sidebarOpen);

        //disabled block icon sidebar
        if (blockIconSidebar.sidebarOpen)
        {
            ColorBlock colorBlock = blockIconSidebarButton.colors;
            colorBlock.normalColor = buttonColorOn;
            colorBlock.highlightedColor = buttonColorOn;
            blockIconSidebarButton.colors = colorBlock;

            //TTS check
            /*if (Singleton.instance.ttsEnabled)
            {
                blockIconSidebar.ReadSidebar();
                Debug.Log("Reading Block Icon Sidebar text");
            }*/

            if (mainSidebar.gameObject.activeSelf)
            {
                mainSidebar.sidebarOpen = false;
                mainSidebar.gameObject.SetActive(false);
                colorBlock = mainSidebarButton.colors;
                colorBlock.normalColor = buttonColorOff;
                colorBlock.highlightedColor = buttonColorOff;
                mainSidebarButton.colors = colorBlock;
            }
        }
        else //update button color
        {
            ColorBlock colorBlock = blockIconSidebarButton.colors;
            colorBlock.normalColor = buttonColorOff;
            colorBlock.highlightedColor = buttonColorOff;
            blockIconSidebarButton.colors = colorBlock;

            //turn off TTS in case it's currently playing
            /*if (Singleton.instance.ttsEnabled)
            {
                ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            }*/
        }
    }


    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void DisplayDialogue(string key)
    {
        dialogueText.text = GetText(key);
    }

    public void ShowNoSpaceUI()
    {
        if (!animateNoSpaceTextCoroutineOn)
        {
            StartCoroutine(AnimateNoSpaceText());
        }
    }

    //used by noSpaceText only
    public void Clamp(Vector3 position)
    {
        Vector3 newPos = Camera.main.WorldToScreenPoint(position);
        noSpaceText.transform.position = newPos;
    }

    //When hint button is pressed, find 2 blocks that can be compared. When first block is selected, prioritize a block with a smaller fraction.
    /*void SearchForComparison()
    {
        GameManager gm = Singleton.instance.GameManager;
        int i = 0; int j = 0;
        bool comparisonFound = false;
        while (!comparisonFound && i < gm.blockList.Count)
        {
            j = i + 1;
            while (!comparisonFound && j < gm.blockList.Count)
            {
                if (gm.blockList[i].numerator == gm.blockList[j].numerator) //|| gm.blockList[i].denominator == gm.blockList[j].denominator)
                {
                    comparisonFound = true;
                    
                    //make note of the blockList[j]'s denominator and look for a block with a larger denominator. Larger denom = smaller fraction.
                    FractionBlock smallestBlock = gm.blockList[j];
                    for (int k = j + 1;  k < gm.blockList.Count; k++)
                    {
                        if (smallestBlock.numerator == gm.blockList[k].numerator && gm.blockList[k].denominator > smallestBlock.denominator)
                        {
                            smallestBlock = gm.blockList[k];
                        }
                    }

                    gm.hintBlocks.Add(gm.blockList[i]);
                    gm.hintBlocks.Add(smallestBlock);
                }
                else if (gm.blockList[i].denominator == gm.blockList[j].denominator)
                {
                    comparisonFound = true;

                    //make note of the blockList[j]'s numerator and look for a block with a smaller numerator. Smaller numerator = smaller fraction.
                    FractionBlock smallestBlock = gm.blockList[j];
                    for (int k = j + 1; k < gm.blockList.Count; k++)
                    {
                        if (gm.blockList[k].numerator < smallestBlock.numerator && gm.blockList[k].denominator == smallestBlock.denominator)
                        {
                            smallestBlock = gm.blockList[k];
                        }
                    }

                    gm.hintBlocks.Add(gm.blockList[i]);
                    gm.hintBlocks.Add(smallestBlock);
                }
                else
                {
                    j++;
                }
            }
            if (!comparisonFound)
                i++;
        }

        //Once two blocks are found, they are added to hint block list.
        //return gm.hintBlocks;
    }*/

    IEnumerator AnimateNoSpaceText()
    {
        //display UI above player
        animateNoSpaceTextCoroutineOn = true;

        Player player = Singleton.instance.GameManager.player;
        noSpaceText.gameObject.SetActive(true);
        Clamp(new Vector3(player.transform.position.x, player.transform.position.y + 1.5f, 0));
        float finalPosY = noSpaceText.transform.position.y + 15;
        float moveSpeed = 20;

        //text travels upward a bit then disappears after a duration
        while(noSpaceText.transform.position.y < finalPosY)
        {
            float newPosY = noSpaceText.transform.position.y + moveSpeed * Time.deltaTime;
            noSpaceText.transform.position = new Vector3(noSpaceText.transform.position.x, newPosY, 0);
            yield return null;
        }

        yield return new WaitForSeconds(1);
        animateNoSpaceTextCoroutineOn = false;
        noSpaceText.gameObject.SetActive(false);
    }
}
