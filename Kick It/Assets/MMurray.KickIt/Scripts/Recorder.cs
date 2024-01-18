using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This script's only purpose is to record the player's last position when they pushed a block, as well as the position of the block that was pushed.
 * If the player undoes their last action, the player's current position is replaced with the recorded position. Same goes for the last block pushed.
 Must also record all blocks that were destroyed in case they need to be restored. */
public class Recorder : MonoBehaviour
{
    public Vector2 playerLastPos, blockLastPos, playerFacingDirection;     //blockLastPos is for the last block that was moved.
    public FractionBlock lastBlockMoved;
    public List<FractionBlock> lastBlocksDestroyed; 
    public bool canUndo;
    //public Button undoButton;

    public void ClearRecord()
    {
        lastBlockMoved = null;
        lastBlocksDestroyed.Clear();
        canUndo = false;
    }
    
}
