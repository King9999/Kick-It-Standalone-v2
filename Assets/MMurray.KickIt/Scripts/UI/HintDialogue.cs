using LoLSDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/* Displays hints unique to each level. Hints remain on screen until player meets objective. */
public class HintDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    //public Image dialogueImage;         //used mainly in tutorials
    public string dialogueKey;        //the key from language JSON
    int currentKey;                         //iterator for the dialogueKey list.
    //public int displayIndex;            //the index of the dialogue to display the image/video.
    public List<int> blockIdList;       //used to highlight blocks of interest.


    // Start is called before the first frame update
    void Start()
    {
        currentKey = 0;
        //dialogueImage.gameObject.SetActive(false);
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    public void ShowDialogue(string key)
    {
        //show the text window, then the text
        //TODO: Animate the text window, then yield to animate text.
        gameObject.SetActive(true);

        //show image if available
        /*if (dialogueImage.sprite != null && displayIndex == currentKey)
        {
            dialogueImage.gameObject.SetActive(true);
        }*/

        dialogueText.text = Singleton.instance.GetText(key);

        Debug.Log("Dialogue text key is " + key);
        //Debug.Log("TTS State: " + Singleton.instance.ttsEnabled);

        /*if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText(key);
            Debug.Log("TTS is playing");
        }*/

        //highlight any blocks
        HighlightBlocks(blockIdList);

        StartCoroutine(AnimateText(0.032f, dialogueText.text));
    }

    public void HideDialogue()
    {
        Singleton.instance.HintDialogueManager.hintDialogueOpen = false;
        gameObject.SetActive(false);
    }

    void HighlightBlocks(List<int> blockIdList)
    {
        if (blockIdList.Count <= 0)
            return;

        List<FractionBlock> blockList = Singleton.instance.GameManager.blockList;
        HintDialogueManager hdm = Singleton.instance.HintDialogueManager;

        for (int i = 0; i < blockIdList.Count; i++)
        {
            bool blockFound = false;
            int j = 0;
            while(!blockFound && j < blockList.Count)
            {
                if (blockIdList[i] == blockList[j].blockId)
                {
                    blockFound = true;
                    hdm.hintBlocks.Add(blockList[j]);
                    //apply hint cursor to the block
                    if (hdm.cursorListIndex >= hdm.hintCursorList.Count)
                    {
                        Target hintCursor = Instantiate(hdm.hintCursorPrefab, hdm.hintCursorContainer.transform);
                        hdm.hintCursorList.Add(hintCursor);
                    }
                    
                    hdm.hintCursorList[hdm.cursorListIndex].ShowCursor(true);
                    hdm.hintCursorList[hdm.cursorListIndex].OccupyBlock(blockList[j]);
                    hdm.cursorListIndex++;
                }
                else
                {
                    j++;
                }
            }
        }
    }

    //Displays lesson details text one letter at a time. Should not run again once the text is fully displayed.
    IEnumerator AnimateText(float scrollSpeed, string textToAnimate)
    {
        List<string> copy = new List<string>();
        int i = 0;
        string p = "";
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

    }


    //used when there's an interruption, such as room being reset
    public void ForceCloseDialogue()
    {
        Singleton.instance.DialogueManager.dialogueOpen = false;
        currentKey = 0;
        //dialogueImage.gameObject.SetActive(false);
        gameObject.SetActive(false);
        Debug.Log("Force Closing Window");
    }

}
