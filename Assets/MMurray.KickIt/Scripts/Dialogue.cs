using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LoLSDK;
using UnityEngine.Video;
using UnityEditor;
using System.Runtime.InteropServices.WindowsRuntime;

/* This script handles any text that appears on screen via triggers. The dialogue has an ID so the game knows which one to play. */
public class Dialogue : MonoBehaviour
{
    public int dialogueId;            //a dialogue can have more than one ID that are tied to different triggers.
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    public Image dialogueImage;         //used mainly in tutorials
    public List<string> dialogueKeyList;        //the key from language JSON
    int currentKey;                         //iterator for the dialogueKey list.
    bool canAdvanceDialogue;                //this must be true before player can press Space to advnace text.
    public int displayIndex;            //the index of the dialogue to display the image/video.
    public float promptTimer;           //how much time must pass before player is prompted to go to next dialogue.

    [Header("---Video---")]
    public VideoPlayer vidPlayer;
    public RawImage vidRenderer;
    public string videoName;

    // Start is called before the first frame update
    void Start()
    {
        canAdvanceDialogue = false;
        currentKey = 0;
        dialogueImage.gameObject.SetActive(false);
        vidPlayer.gameObject.SetActive(false);
        vidRenderer.gameObject.SetActive(false);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void ShowDialogue(string key)
    {
        //show the text window, then the text
        //TODO: Animate the text window, then yield to animate text.
        gameObject.SetActive(true);

        //temporarily disable sidebars if they're open
        UI ui = Singleton.instance.UI;

        if (ui.mainSidebar.sidebarOpen)
            ui.mainSidebar.gameObject.SetActive(false);
        if(ui.blockIconSidebar.sidebarOpen)
            ui.blockIconSidebar.gameObject.SetActive(false);

        //temporarily disable hint dialogue
        HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
        GameManager gm = Singleton.instance.GameManager;
        if (hdm.hintDialogueOpen)
            hdm.dialogueList[gm.level].gameObject.SetActive(false);

        //show image if available
        if (dialogueImage.sprite != null && displayIndex == currentKey)
        {
            dialogueImage.gameObject.SetActive(true);
        }

        if (vidRenderer.texture != null && displayIndex == currentKey) 
        {
            vidPlayer.gameObject.SetActive(true);
            vidRenderer.gameObject.SetActive(true);

            //locate the clip. NOTE: Must play clip through the StreamingAssets folder, so must locate the path.
            if (!vidPlayer.isPlaying)
            {
                string vidPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
                vidPlayer.url = vidPath;
                Debug.Log("Playing video clip from path " + vidPlayer.url);
                vidPlayer.Play();
            }
        }
        Singleton.instance.DialogueManager.dialogueOpen = true;
        dialogueText.text = GetText(key);

       

        Debug.Log("Dialogue text key is " + key);
        Debug.Log("TTS State: " + Singleton.instance.ttsEnabled);

        if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText(key);
            Debug.Log("TTS is playing");
        }

        StartCoroutine(AnimateText(0.032f, dialogueText.text));
    }

    //Displays lesson details text one letter at a time. Should not run again once the text is fully displayed.
    IEnumerator AnimateText(float scrollSpeed, string textToAnimate)
    {
        List<string> copy = new List<string>();
        int i = 0;
        string p = "";
        promptText.text = "";
        while (i < textToAnimate.Length)
        {
            //if there's a color tag, the entire tag must be treated as one character so the entire tag is displayed at once.
            if (textToAnimate.Substring(i, 1).Equals("<"))
            {
                //keep incrementing i until we reach the end of tag
                string tag = "";
                do
                {
                    tag += textToAnimate.Substring(i, 1);
                    i++;
                }
                while (!textToAnimate.Substring(i, 1).Equals(">"));
                tag += textToAnimate.Substring(i++, 1); //adding the >
                copy.Add(tag);
            }
            else
            {
                p = textToAnimate.Substring(i, 1);
                copy.Add(p);
                //Debug.Log(textToAnimate.Substring(i, 1)); 
                i++;
            }
        }


        dialogueText.text = "";
        i = 0;
        while (i < copy.Count)
        {
            dialogueText.text += copy[i];
            i++;
            yield return new WaitForSeconds(scrollSpeed);
        }

        //show prompt to close dialogue box.
        yield return new WaitForSeconds(promptTimer);
        yield return ShowPrompt();
    }

    IEnumerator ShowPrompt()
    {
        promptText.text = GetText("dialoguePrompt");
        canAdvanceDialogue = true;
        yield return null;
    }

    //used when there's an interruption, such as room being reset
    public void ForceCloseDialogue()
    {
        Singleton.instance.DialogueManager.dialogueOpen = false;
        currentKey = 0;
        dialogueImage.gameObject.SetActive(false);
        gameObject.SetActive(false);
        Debug.Log("Force Closing Window");
    }

    void OnAdvance(InputValue value)
    {
        Debug.Log("Pressed Space");

        if (Singleton.instance.ttsEnabled)
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText(); //stops TTS if still in the middle of speaking

        if (gameObject.activeSelf && canAdvanceDialogue)
        {
            
            canAdvanceDialogue = false;
            currentKey++;

            //check if there's more dialogue to display, otherwise we close the window.
            if (currentKey >= dialogueKeyList.Count)
            {
                Singleton.instance.DialogueManager.dialogueOpen = false;
                currentKey = 0;
                dialogueImage.gameObject.SetActive(false);
                gameObject.SetActive(false);
                Debug.Log("Closing Window");

                //check game state. If level has ended we go to next level
                GameManager gm = Singleton.instance.GameManager;
                /*if (gm.gameState == GameManager.GameState.OneBlockLeft || gm.gameState == GameManager.GameState.NoComparison)
                    return;

                if (gm.gameState == GameManager.GameState.DisplayEndOfLevelDialogue)
                {
                    gm.SetGameState(GameManager.GameState.GoToNextLevel);
                }
                else
                {
                    //the dialogue was displayed at start of level. We don't do anything special.
                    gm.gameState = GameManager.GameState.Normal;
                }*/

                //Show a dialogue box prompting player to replay room or go to next room. Replaying room offers chance for player
                //to get used to the gameplay instead of just going to the next level.
                if (gm.gameState == GameManager.GameState.OneBlockLeft || gm.gameState == GameManager.GameState.NoComparison)
                    return;     //player must select "reset room" from menu

                //if (gm.gameState == GameManager.GameState.OneBlockLeft || gm.gameState == GameManager.GameState.NoComparison ||
                if (gm.gameState == GameManager.GameState.DisplayEndOfLevelDialogue)
                {
                    if (gm.level + 1 >= gm.MaxLevels)
                    {
                        gm.SetGameState(GameManager.GameState.GoToNextLevel);   //game is done
                    }
                    else
                    {
                        //show window
                        UI ui = Singleton.instance.UI;
                        ui.endOfLevelWindow.gameObject.SetActive(true);
                        ui.endOfLevelWindow.OpenWindow();
                    }

                }
                else //level is in progress
                {
                    gm.gameState = GameManager.GameState.Normal;

                    //enable sidebars if they were temp disabled
                    UI ui = Singleton.instance.UI;

                    if (ui.mainSidebar.sidebarOpen)
                        ui.mainSidebar.gameObject.SetActive(true);
                    if (ui.blockIconSidebar.sidebarOpen)
                        ui.blockIconSidebar.gameObject.SetActive(true);

                    //restore hint dialogue
                    HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
                    //GameManager gm = Singleton.instance.GameManager;
                    if (hdm.hintDialogueOpen)
                        hdm.dialogueList[gm.level].gameObject.SetActive(true);

                }
            }
            else
            {
                //get the next dialogue Key
                ShowDialogue(dialogueKeyList[currentKey]);
            }
            
        }
    }
}
