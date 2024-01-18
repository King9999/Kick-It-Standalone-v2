using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintDialogueManager : MonoBehaviour
{
    public List<HintDialogue> dialogueList;
    public bool hintDialogueOpen;
    public List<Target> hintCursorList;
    public List<FractionBlock> hintBlocks;  //the blocks that the hint cursors are resting on.
    public Target hintCursorPrefab;
    public int cursorListIndex;                 //iterator for hintCursorList
    public GameObject hintCursorContainer;

    // Start is called before the first frame update
    void Awake()
    {
        Singleton.instance.HintDialogueManager = this;

        foreach (HintDialogue dialogue in dialogueList)
        {
            dialogue.gameObject.SetActive(false);
        }
    }


    public void GetHint(int level)
    {
        HintDialogue dialogue = dialogueList[level];
        hintDialogueOpen = true;
        dialogue.ShowDialogue(dialogue.dialogueKey);
    }

    //used when resetting level
    public void Clear()
    {
        foreach (Target cursor in hintCursorList)
        {
            cursor.isDestroyedByHintBlock = false;
            cursor.ShowCursor(false);
        }

        hintBlocks.Clear();
        cursorListIndex = 0;
    }
}
