using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* The player will step on these to make dialogue appear. */
public class DialogueTrigger : MonoBehaviour
{
    public int dialogueId;
    public bool triggerActivated;   //used by save state to prevent trigger from appearing again when game resumes.
    public bool conditional;        //if true, this trigger activates when certain events occur. 
    public enum Condition { None, LevelStart, LevelEnd }
    public Condition condition;

    public void GetCondition(Condition condition)
    {
        if (conditional == false) return;

        switch(condition)
        {
            case Condition.LevelStart:
                //find the matching id in the dialogue list
                break;

            case Condition.LevelEnd:
                //find the matching id in the dialogue list
                break;
        }
    }
}
