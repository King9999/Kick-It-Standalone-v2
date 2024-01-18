using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using LoLSDK;
using System;
using UnityEngine.Playables;
using static GameManager;

namespace MMurray.KickIt
{
    /* ROOM CLASSES FOR JSON
    This class pulls data from rooms.json */
    [Serializable]
    public class Room
    {
        public string roomNum;
        public Row[] rows;
        public BlockData[] blockData;
        public Trigger[] triggerLocations;
    }

    [Serializable]
    public class Rooms
    {
        public Room[] rooms;
    }

    [Serializable]
    public class Row
    {
        public string row;
    }

    [Serializable]
    public class BlockData
    {
        public int blockId;             //array index
        public int blockColor;          //0 = blue, 1 = red, 2 = green 
        public int blockRow, blockCol;  //location of block in the level
        public string blockValue;       //this will be converted to fractions
    }

    [Serializable]
    public class Trigger
    {
        public int triggerId;
        public int triggerRow, triggerCol;
    }

    //This class contains all data that will be written to file.
    [Serializable]
    public class GameData
    {
        //level data
        public int level;
        public int max_levels;      //includes tutorial levels
        public List<FractionBlockData> block_list;
        public List<FractionBlockData> block_graveyard;
        public List<FractionBlockData> hint_blocks;
        public bool hint_button_pressed;

        //player data
        public Vector2 player_pos;

        //recorder data
        public Vector2 player_last_pos, block_last_pos;     //blockLastPos is for the last block that was moved.
        public FractionBlockData last_block_moved;
        public List<FractionBlockData> last_blocks_destroyed;
        public bool can_undo;

        //trigger data
        public List<TriggerData> trigger_list;

        //universal data
        public bool music_enabled = true;
        public bool tts_enabled = true;
    }

    [Serializable]
    public class FractionBlockData
    {
        public int blockId;
        public int blockColor;
        public string blockValue;
        public float quotient;
        public Vector2 pos;
    }

    [Serializable]
    public class TriggerData
    {
        public int dialogueId;
        public bool triggerActivated;
    }

    //Used by player to save/resume a game in progress
    [Serializable]
    public class SaveState
    {
        string GetText(string key)
        {
            string value = SharedState.LanguageDefs?[key];
            return value ?? "--missing--";
        }

        public void WriteState(GameData data)
        {
            GameManager gm = Singleton.instance.GameManager;
            //HintDialogueManager hdm = Singleton.instance.HintDialogueManager;

            //save player's current position
            data.player_pos = gm.player.transform.position;

            //deep copy of block list
            data.block_list = new List<FractionBlockData>();
            data.block_graveyard = new List<FractionBlockData>();
            data.hint_blocks = new List<FractionBlockData>();

            foreach (FractionBlock block in gm.blockList)
            {
                FractionBlockData newBlock = new FractionBlockData();
                newBlock.blockId = block.blockId;
                newBlock.blockColor = block.blockColor;
                newBlock.blockValue = block.blockValue;
                newBlock.quotient = block.quotient;
                newBlock.pos = block.transform.position;
                data.block_list.Add(newBlock);

                //add this block to hint block list.
                /*if (hdm.hintBlocks.Contains(block))
                {
                    data.hint_blocks.Add(newBlock);
                }*/
            }

            foreach (FractionBlock block in gm.blockGraveyard)
            {
                FractionBlockData newBlock = new FractionBlockData();
                newBlock.blockId = block.blockId;
                newBlock.blockColor = block.blockColor;
                newBlock.blockValue = block.blockValue;
                newBlock.quotient = block.quotient;
                newBlock.pos = block.transform.position;
                data.block_graveyard.Add(newBlock);
            }

            //recorder data
            Recorder rec = Singleton.instance.Recorder;
            data.player_last_pos = rec.playerLastPos;
            data.block_last_pos = rec.blockLastPos;

            //last block moved data
            data.last_block_moved = new FractionBlockData();
            if (rec.lastBlockMoved != null)
            {
                data.last_block_moved.blockId = rec.lastBlockMoved.blockId;
                data.last_block_moved.blockColor = rec.lastBlockMoved.blockColor;
                data.last_block_moved.blockValue = rec.lastBlockMoved.blockValue;
                data.last_block_moved.quotient = rec.lastBlockMoved.quotient;
                data.last_block_moved.pos = rec.lastBlockMoved.transform.position;
            }
            else
            {
                data.last_block_moved = null;
            }

            data.last_blocks_destroyed = new List<FractionBlockData>();
            foreach (FractionBlock block in rec.lastBlocksDestroyed)
            {
                FractionBlockData newBlock = new FractionBlockData();
                newBlock.blockId = block.blockId;
                newBlock.blockColor = block.blockColor;
                newBlock.blockValue = block.blockValue;
                newBlock.quotient = block.quotient;
                newBlock.pos = block.transform.position;
                data.last_blocks_destroyed.Add(newBlock);
            }
            data.can_undo = rec.canUndo;

            //trigger data
            data.trigger_list = new List<TriggerData>();
            foreach(DialogueTrigger trigger in gm.dialogueManager.dialogueTrigger)
            {
                TriggerData trigData = new TriggerData();
                trigData.dialogueId = trigger.dialogueId;
                trigData.triggerActivated = trigger.triggerActivated;
                data.trigger_list.Add(trigData);
            }

            //hint data
            //data.hint_button_pressed = Singleton.instance.UI.hintButtonPressed;
        


            //submit current progress
            data.level = gm.level;
            //data.tts_enabled = Singleton.instance.ttsEnabled;
            data.music_enabled = Singleton.instance.musicEnabled;
            //LOLSDK.Instance.SubmitProgress(0, data.level + 1, gm.MaxLevels);    //add 1 to level because level starts at 0.
            //LOLSDK.Instance.SaveState(data);
        }

        //This code is executed after the level is loaded and all objects are instantiated.
        public void ReadState(GameData data)
        {
            //set up level
            GameManager gm = Singleton.instance.GameManager;

            //update player's current position
            gm.player.transform.position = data.player_pos;

            //recorder data
            Recorder rec = Singleton.instance.Recorder;
            rec.playerLastPos = data.player_last_pos;
            rec.blockLastPos = data.block_last_pos;
            rec.canUndo = data.can_undo;

            //check if a last block moved exists
            int i = 0;
            bool blockFound = false;
            if (data.last_block_moved != null)
            {
                
                while (!blockFound && i < gm.blockList.Count)
                {
                    if (gm.blockList[i].blockValue == data.last_block_moved.blockValue && gm.blockList[i].blockId == data.last_block_moved.blockId)
                    {
                        blockFound = true;
                        rec.lastBlockMoved = gm.blockList[i];
                        rec.lastBlockMoved.blockId = data.last_block_moved.blockId;
                        rec.lastBlockMoved.blockColor = data.last_block_moved.blockColor;
                        rec.lastBlockMoved.blockValue = data.last_block_moved.blockValue;
                        rec.lastBlockMoved.quotient = data.last_block_moved.quotient;
                        rec.lastBlockMoved.transform.position = data.last_block_moved.pos;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
           

            //check the block list for blocks that were destroyed
            rec.lastBlocksDestroyed = new List<FractionBlock>();
            foreach (FractionBlockData block in data.last_blocks_destroyed)
            {
                blockFound = false;
                for (int j = 0; j < gm.blockList.Count; j++)
                {
                    if (!blockFound && block.blockId == gm.blockList[j].blockId)
                    {
                        //capture this block. the data is copied to the block.
                        blockFound = true;
                        gm.blockList[j].transform.position = block.pos;
                        rec.lastBlocksDestroyed.Add(gm.blockList[j]);
                    }
                }
                
            }

            //update block list. Send any blocks that were destroyed to the graveyard.
            i = 0;
            while (i < gm.blockList.Count)
            {
                blockFound = false;
                for (int j = 0; j < data.block_graveyard.Count; j++)
                {
                    if (!blockFound && gm.blockList[i].blockId == data.block_graveyard[j].blockId)
                    {
                        blockFound = true;
                        gm.blockList[i].transform.position = data.block_graveyard[j].pos;
                        gm.blockGraveyard.Add(gm.blockList[i]);
                        gm.blockList[i].gameObject.SetActive(false);
                        gm.blockList.Remove(gm.blockList[i]);     
                        //i--;
                        //data.block_graveyard.Remove(data.block_graveyard[j]);
                        //j--;
                    }

                }

                if (!blockFound)    //if a block was found, we don't increment i so no block is skipped in the list.
                {
                    //get the saved block list position and update the active block list data.
                    //gm.blockList[i].transform.position = data.block_list[i].pos;
                    i++;
                }
                //i++;
            }

            //check remaining blocks and get their saved positions
            i = 0;
            while(i < gm.blockList.Count)
            {
                blockFound = false;
                int j = 0;
                while (!blockFound && j < data.block_list.Count)
                {
                    if (gm.blockList[i].blockId == data.block_list[j].blockId)
                    {
                        blockFound = true;
                        gm.blockList[i].transform.position = data.block_list[j].pos;
                    }
                    else
                    {
                        j++;
                    }
                }
                i++;
            }
           

            //trigger setup
            for (i = 0; i < data.trigger_list.Count; i++)
            {
                //if trigger was activated, the trigger is disabled.
                gm.dialogueManager.dialogueTrigger[i].triggerActivated = data.trigger_list[i].triggerActivated;
                if (gm.dialogueManager.dialogueTrigger[i].triggerActivated == true)
                {
                    gm.dialogueManager.dialogueTrigger[i].gameObject.SetActive(false);
                }
            }

            //hint data
            /*Singleton.instance.UI.hintButtonPressed = data.hint_button_pressed;
            HintDialogueManager hdm = Singleton.instance.HintDialogueManager;

            hdm.hintBlocks = new List<FractionBlock>();
            for (int j = 0; j < data.hint_blocks.Count; j++)
            {
                blockFound = false;
                int k = 0;
                while(!blockFound && k < gm.blockList.Count)
                {
                    if (data.hint_blocks[j].blockId == gm.blockList[k].blockId)
                    {
                        blockFound = true;
                        hdm.hintBlocks.Add(gm.blockList[k]);
                        hdm.hintCursorList[j].ShowCursor(true);
                        hdm.hintCursorList[j].OccupyBlock(gm.blockList[k]);
                    }
                    else
                    {
                        k++;
                    }
                }
       
            }*/
            

            //reached the end
            //Singleton.instance.saveStateFound = false;  //this is to prevent loading a save state for the next level.
            gm.gameState = GameManager.GameState.Normal;
            //play music
            if (Singleton.instance.musicEnabled && !Singleton.instance.AudioManager.musicMain.isPlaying)
            {
                //Singleton.instance.AudioManager.musicMain.volume = Singleton.instance.AudioManager.soundVolume;
                Singleton.instance.AudioManager.musicMain.Play();
            }
        }

        //This LoL code is used to check for an existing save state when the game runs.
        /*public void LoadState(Action<GameData> callback)
        {
            LOLSDK.Instance.LoadState<GameData>(state =>
            {
                if (state != null)
                {
                    callback(state.data);
                }

            });
        }*/

    }
}


