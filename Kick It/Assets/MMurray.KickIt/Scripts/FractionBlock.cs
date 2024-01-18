using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.HID;

/* Fraction blocks can be manipulated by the player. They can be pushed or kicked, and they interact with other fraction blocks based on their value.
 * The following actions occur when two blocks clash:
   * If they have equal value, they are both destroyed
   * If one has a higher value, it destroys the other block, and keeps moving until it hits a wall or is destroyed.
   * If one has a lower value, it's destroyed by blocks with higher values.
   * If each block has a different colour, then nothing happens.
   * 
 */
public class FractionBlock : MonoBehaviour
{
    public List<Sprite> blockSprite;        //0 = blue, 1 = red
    public List<Sprite> indicators;         //indicates what happens when a block collides with another.\
    public GameObject indicatorObject;
    public int blockId;
    public int blockColor;                 //must be an int because LoL cannot save enums
    public string blockValue;   
    public float quotient;                 //used to compare blocks' values. Not seen by the player in game.
    public float numerator, denominator;
    public bool isKicked, isPushed, isRedBlock;       //if true, then block is currently moving. Red blocks can't be pushed.
    public float moveSpeed;
    public float maxSpeed { get; } = 14;
    public float pushSpeed { get; } = 4;
    public float pushDistanceX { get; } = 1.48f;     //how far a block travels when pushed. Moves ahead 1 unit.
    public float pushDistanceY { get; } = 2f;     //how far a block travels when pushed. Moves ahead 1 unit.

    public Vector2 direction, blockDestination;     //blockDestination is used for when a block is pushed.
    public enum BlockColor { Blue, Red, Green}
    public BlockColor color;
    public TextMeshProUGUI fractionValueUI;
    public Rigidbody2D blockBody;
    SpriteRenderer indicatorRenderer;
    bool soundPlaying;

    //indicator consts
    public const int SPRITE_CIRCLE = 0;
    public const int SPRITE_CROSS = 1;
    public const int SPRITE_WARNING = 2;
    public const int SPRITE_STOP = 3;

    // Start is called before the first frame update
    void Start()
    {
        blockBody = GetComponent<Rigidbody2D>();
        indicatorRenderer = indicatorObject.GetComponent<SpriteRenderer>();
        soundPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        Clamp(transform.position);

        if (isPushed || isKicked)
        {
            //any text bubbles are disabled while blocks are moving. The blocksInView is also cleared to prevent bubbles from showing up again.
            Singleton.instance.GameManager.allBlocksViewed = false;
            Singleton.instance.GameManager.blocksInView.Clear();

            //hide text bubbles
            Singleton.instance.GameManager.textBubblesOnScreen = false;
            foreach (TextBubble bubble in Singleton.instance.GameManager.textBubbleList)
            {
                bubble.gameObject.SetActive(false);
            }

            if (isPushed)
            {
                //play sound
                if (!soundPlaying)
                {
                    AudioManager audio = Singleton.instance.AudioManager;
                    //if (!audio.soundSource.isPlaying)
                    //{
                        //audio.soundSource.clip = audio.audioBlockPushed;
                        audio.soundSource.loop = false;
                        audio.soundSource.PlayOneShot(audio.audioBlockPushed);
                        soundPlaying = true;
                    //}
                }
            }

            if (isKicked)
            {
                //generate spark
                float currentTime = Singleton.instance.GameManager.currentTime;
                float sparkSpawnTime = Singleton.instance.GameManager.sparkSpawnTime;
                int currentSpark = Singleton.instance.GameManager.currentSpark;
                if (Time.time > currentTime + sparkSpawnTime)
                {
                    Spark spark = Singleton.instance.GameManager.sparks[currentSpark];
                    spark.gameObject.SetActive(true);
                    spark.GenerateSpark(new Vector2(transform.position.x, transform.position.y - 1), direction * -1);   //sparks always fly in opposite direction so I multiply by -1
                    Singleton.instance.GameManager.currentTime = Time.time;
                    Singleton.instance.GameManager.currentSpark++;
                    if (Singleton.instance.GameManager.currentSpark >= Singleton.instance.GameManager.sparks.Count)
                    {
                        Singleton.instance.GameManager.currentSpark = 0;
                    }
                }

                //play sound
                if (!soundPlaying)
                {
                    AudioManager audio = Singleton.instance.AudioManager;
                    if (!audio.soundSource.isPlaying)
                    {
                        audio.soundSource.clip = audio.audioBlockMoving;
                        audio.soundSource.loop = true;
                        audio.soundSource.volume = audio.soundVolume;

                        //if (!audio.soundSource.isPlaying)
                        audio.soundSource.Play();
                        soundPlaying = true;
                    }
                }

            }
        }
    }

    private void OnDisable()
    {
        if (blockBody == null)
        {
            blockBody = GetComponent<Rigidbody2D>();
        }

        if (indicatorRenderer == null) 
        {
            indicatorRenderer = GetComponent<SpriteRenderer>();
        }

        blockBody.bodyType = RigidbodyType2D.Static;    //ensures they are the correct body type when re-enabled.
        indicatorRenderer.sprite = null;
        moveSpeed = 0;
        isPushed = false;
        isKicked = false;
    }

    private void FixedUpdate()
    {
        if (!isRedBlock && isPushed)
        {
            transform.position = Vector3.MoveTowards(transform.position, blockDestination, moveSpeed * Time.deltaTime);

            //cast a ray to see if block hit a wall
            float distance = 1f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position /*+ new Vector3(direction.x, direction.y, 0)*/, direction, distance);
            Debug.DrawRay(transform.position /*+ new Vector3(direction.x, direction.y, 0)*/, direction * distance, Color.white, 2);

            if (transform.position == new Vector3(blockDestination.x, blockDestination.y, 0) || (hit.collider != null && !hit.collider.TryGetComponent(out FractionBlock block)))
            {
                isPushed = false;
                blockBody.bodyType = RigidbodyType2D.Static;
                moveSpeed = 0;
                Singleton.instance.Recorder.canUndo = true;     //when the action is done, player can now undo the action

                //restore hint cursors if possible
                /*GameManager gm = Singleton.instance.GameManager;
                HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
                if (hdm.hintBlocks.Count > 0)
                {
                    for (int i = 0; i < hdm.hintBlocks.Count; i++)
                    {
                        if (!hdm.hintCursorList[i].isDestroyedByHintBlock)
                        {
                            hdm.hintCursorList[i].ShowCursor(true);
                            hdm.hintCursorList[i].OccupyBlock(hdm.hintBlocks[i]);
                        }
                    }
                }*/

                if (soundPlaying)
                {
                    AudioManager audio = Singleton.instance.AudioManager;
                    audio.soundSource.Stop();
                    soundPlaying = false;
                }

            }
        }
        else  //block is either not moving or was kicked.
        {
            if (moveSpeed > 0)
            {
                transform.Translate(new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.deltaTime);

                //cast a ray to see if block hit a wall
                float distance = 0.005f;
                RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, distance);
                Debug.DrawRay(transform.position + new Vector3(direction.x, direction.y, 0), direction * distance, Color.white, 2);
                if (hit.collider != null && !hit.collider.TryGetComponent(out FractionBlock block))
                {
                    isKicked = false;
                    isPushed = false;
                    blockBody.bodyType = RigidbodyType2D.Static;
                    moveSpeed = 0;
                    Singleton.instance.GameManager.StopSparks();
                    Singleton.instance.Recorder.canUndo = true;

                    //push the block slightly towards the wall so there's no gap between block and wall. The block collision box cannot overlap with wall.
                    float xOffset = 0.21f;
                    float yOffset = 0.12f;
                    if (direction.x < 0)
                    {
                        //move block slightly in opposite direction
                        //transform.position = new Vector2(transform.position.x - xOffset, transform.position.y);
                        Debug.Log("Repositioned block closer to wall to the left");
                        direction = new Vector2(0, 1);
                        RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                        transform.position = new Vector2(wallHit.collider.transform.position.x, transform.position.y);
                    }
                    else if (direction.x > 0)
                    {
                        //transform.position = new Vector2(transform.position.x + xOffset, transform.position.y);
                        Debug.Log("Repositioned block closer to wall to the right");
                        direction = new Vector2(0, 1);
                        RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                        transform.position = new Vector2(wallHit.collider.transform.position.x, transform.position.y);
                    }
                    else if (direction.y > 0)
                    {
                        //transform.position = new Vector2(transform.position.x, transform.position.y - yOffset - 0.08f);
                        Debug.Log("Repositioned block away from the wall");

                        //cast a ray on the X axis to find a wall, and this block's Y pos matches the wall's
                        direction = new Vector2(-1, 0);
                        RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                        transform.position = new Vector2(transform.position.x, wallHit.collider.transform.position.y);

                    }
                    else if (direction.y < 0)
                    {
                        //transform.position = new Vector2(transform.position.x, transform.position.y + yOffset);
                        Debug.Log("Repositioned block away from the wall");
                        direction = new Vector2(-1, 0);
                        RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                        transform.position = new Vector2(transform.position.x, wallHit.collider.transform.position.y);
                    }

                    //shake the camera a bit
                    GameManager gm = Singleton.instance.GameManager;
                    gm.ScreenShake();

                    

                    //block hit something, play sound
                    AudioManager audio = Singleton.instance.AudioManager;
                    audio.soundSource.Stop();   //stop the looping sound
                    audio.soundSource.PlayOneShot(audio.audioBlockHitObject, audio.soundVolume);
                    soundPlaying = false;   //in case a looping sound was playing before.
                }
            }
            else
            {
                if (soundPlaying)
                {
                    AudioManager audio = Singleton.instance.AudioManager;
                    audio.soundSource.Stop();
                    soundPlaying = false;
                }

               
            }

           

        }

    }

    private void LateUpdate()
    {
        //restore hint cursors if possible
        HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
        if (!isPushed && !isKicked && hdm.hintBlocks.Count > 0)
        {
            for (int i = 0; i < hdm.hintBlocks.Count; i++)
            {
                if (!hdm.hintCursorList[i].isDestroyedByHintBlock)
                {
                    hdm.hintCursorList[i].ShowCursor(true);
                    hdm.hintCursorList[i].OccupyBlock(hdm.hintBlocks[i]);
                }
            }
        }
    }

    public void Clamp(Vector3 position)
    {
        Vector3 imgPos = Camera.main.WorldToScreenPoint(position);
        fractionValueUI.transform.position = imgPos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //check collision against other objects. If another block is hit, check its value
        if (collision.collider != null) 
        {
            if(collision.collider.TryGetComponent(out FractionBlock block)) //fraction block
            {
                if (isKicked)
                {
                    string fractionOne = FormatFraction(blockValue);
                    string fractionTwo = FormatFraction(block.blockValue);
                    GameManager gm = Singleton.instance.GameManager;    //will need this throughout the following code
                    HintDialogueManager hdm = Singleton.instance.HintDialogueManager;

                    //if both the denominator and numerators are different, nothing happens
                    if (numerator != block.numerator && denominator != block.denominator)
                    {
                        isKicked = false;
                        isPushed = false;
                        blockBody.bodyType = RigidbodyType2D.Static;
                        moveSpeed = 0;

                        //push the block out slightly so the collision doesn't overlap with the block it just hit.
                        //float xOffset = 0.21f;
                        //float yOffset = 0.12f;
                        float offset = 0.16f;
                        if (direction.x < 0)
                        {
                            //move block slightly in opposite direction
                            //transform.position = new Vector2(transform.position.x + offset, transform.position.y);
                            Debug.Log("Repositioned block to the right");
                            direction = new Vector2(0, 1);
                            RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                            transform.position = new Vector2(wallHit.collider.transform.position.x, transform.position.y);
                        }
                        else if (direction.x > 0)
                        {
                            //transform.position = new Vector2(transform.position.x - offset, transform.position.y);
                            Debug.Log("Repositioned block to the left");
                            direction = new Vector2(0, 1);
                            RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                            transform.position = new Vector2(wallHit.collider.transform.position.x, transform.position.y);
                        }
                        else if (direction.y < 0)
                        {
                            //transform.position = new Vector2(transform.position.x, transform.position.y + offset);
                            Debug.Log("Repositioned block upwards");
                            direction = new Vector2(-1, 0);
                            RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                            transform.position = new Vector2(transform.position.x, wallHit.collider.transform.position.y);
                        }
                        else if (direction.y > 0)
                        {
                            //transform.position = new Vector2(transform.position.x, transform.position.y - offset);
                            Debug.Log("Repositioned block downwards");
                            direction = new Vector2(-1, 0);
                            RaycastHit2D wallHit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y, 0), direction, 50);
                            transform.position = new Vector2(transform.position.x, wallHit.collider.transform.position.y);
                        }

                        //shake the camera a bit
                        gm.ScreenShake();
                        gm.StopSparks();

                        //play sound
                        AudioManager audio = Singleton.instance.AudioManager;
                        audio.soundSource.Stop();   //stop the looping sound
                        audio.soundSource.PlayOneShot(audio.audioBlockHitObject, audio.soundVolume);
                        soundPlaying = false;

                        //remove indicator on block
                        block.ShowIndicatorSprite(null);

                        //allow this action to be undone
                        Singleton.instance.Recorder.canUndo = true;

                        return;
                    }


                    //check block's value against the block that was kicked
                    if (quotient > block.quotient)
                    {
                        //kicked block destroys other block and keeps moving
                        gm.blockGraveyard.Add(block);
                        gm.blockList.Remove(block);

                        //play sound
                        Singleton.instance.AudioManager.audioBlockDestroyed.Play();

                        //record the destroyed blocks for undoing later.
                        //Singleton.instance.Recorder.lastBlocksDestroyed.Clear();    //eliminate any previous blocks first, they can't be restored
                        Singleton.instance.Recorder.lastBlocksDestroyed.Add(block);

                        //show result
                        //string fractionOne = FormatFraction(blockValue);
                        //string fractionTwo = FormatFraction(block.blockValue);
                        Singleton.instance.UI.ShowResultText(fractionOne, ">", fractionTwo, block.transform.position);

                        //if block was a hint block, remove the hint cursors and clear hint block list.
                        if (hdm.hintBlocks.Contains(block))
                        {
                            bool blockFound = false;
                            int i = 0;
                            while (!blockFound && i < hdm.hintCursorList.Count) 
                            {
                                if (hdm.hintCursorList[i].blockId == block.blockId) 
                                {
                                    blockFound = true;
                                    hdm.hintCursorList[i].isDestroyedByHintBlock = true; //used to restore the cursor when undo is clicked.
                                    hdm.hintCursorList[i].ShowCursor(false);
                                }
                                else
                                {
                                    i++;
                                }
                            }

                            //hdm.hintBlocks.Remove(block);                         
                        }
                        /*if (gm.hintBlocks.Contains(block))
                        {
                            foreach(Target cursor in gm.hintCursors)
                            {
                                cursor.ShowCursor(false);
                            }
                            gm.hintBlocks.Clear();
                        }*/

                        block.gameObject.SetActive(false);
                    }
                    else if (quotient < block.quotient)
                    {
                        //kicked block is destroyed
                        gm.blockGraveyard.Add(this);
                        gm.blockList.Remove(this);

                        //Singleton.instance.Recorder.lastBlocksDestroyed.Clear();    //eliminate any previous blocks first, they can't be restored
                        Singleton.instance.Recorder.lastBlocksDestroyed.Add(this);
                        Singleton.instance.Recorder.canUndo = true;

                        //show result
                        Singleton.instance.UI.ShowResultText(fractionOne, "<", fractionTwo, transform.position);

                        if (soundPlaying)
                        {
                            //stop the block moving sound
                            AudioManager audio = Singleton.instance.AudioManager;
                            audio.soundSource.Stop();
                            soundPlaying = false;

                            //play block destroyed sound
                            audio.audioBlockDestroyed.Play();
                        }

                        //if block was a hint block, remove the hint cursors and clear hint block list.
                        if (hdm.hintBlocks.Contains(this))
                        {
                            bool blockFound = false;
                            int i = 0;
                            while (!blockFound && i < hdm.hintCursorList.Count)
                            {
                                if (hdm.hintCursorList[i].blockId == blockId)
                                {
                                    blockFound = true;
                                    hdm.hintCursorList[i].isDestroyedByHintBlock = true;
                                    hdm.hintCursorList[i].ShowCursor(false);
                                }
                                else
                                {
                                    i++;
                                }
                            }

                            //hdm.hintBlocks.Remove(this);
                        }
                        /*if (gm.hintBlocks.Contains(this))
                        {
                            foreach (Target cursor in gm.hintCursors)
                            {
                                cursor.ShowCursor(false);
                            }
                            gm.hintBlocks.Clear();
                        }*/

                        //remove indicator on block
                        block.ShowIndicatorSprite(null);
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        //both blocks are destroyed
                        Debug.Log("Block " + blockId + " (value " + blockValue + ") collided with block " + block.blockId + " (value " + block.blockValue + ")");

                        //remove from game manager's block list and add to graveyard
                        gm.blockGraveyard.Add(block);
                        gm.blockGraveyard.Add(this);
                        gm.blockList.Remove(block);
                        gm.blockList.Remove(this);


                        //record the destroyed blocks for undoing later.
                        //Singleton.instance.Recorder.lastBlocksDestroyed.Clear();    //eliminate any previous blocks first, they can't be restored
                        Singleton.instance.Recorder.lastBlocksDestroyed.Add(block);
                        Singleton.instance.Recorder.lastBlocksDestroyed.Add(this);
                        Singleton.instance.Recorder.canUndo = true;

                        //show result
                        Singleton.instance.UI.ShowResultText(fractionOne, "=", fractionTwo, block.transform.position);

                        if (soundPlaying)
                        {
                            AudioManager audio = Singleton.instance.AudioManager;
                            audio.soundSource.Stop();
                            soundPlaying = false;

                            //play block destroyed sound
                            audio.audioBlockDestroyed.Play();
                        }

                        //if block was a hint block, remove the hint cursors and clear hint block list.
                        if (hdm.hintBlocks.Contains(block) || hdm.hintBlocks.Contains(this))
                        {
                            for (int i = 0; i < hdm.hintBlocks.Count; i++) 
                            {
                                if (hdm.hintBlocks[i].blockId == block.blockId)
                                {
                                    for (int j = 0; j < hdm.hintCursorList.Count; j++)
                                    {
                                        if (hdm.hintCursorList[j].blockId == block.blockId)
                                        {
                                            hdm.hintCursorList[j].isDestroyedByHintBlock = true;
                                            hdm.hintCursorList[j].ShowCursor(false);
                                        }
                                    }
                                    /*foreach(Target cursor in hdm.hintCursorList)
                                    {
                                        if (cursor.blockId == block.blockId)
                                        {
                                            
                                            cursor.ShowCursor(false);
                                        }
                                    }*/
                                    //hdm.hintBlocks.Remove(block);
                                    //i--;
                                }
                                else if (hdm.hintBlocks[i].blockId == blockId)
                                {
                                    for (int j = 0; j < hdm.hintCursorList.Count; j++)
                                    {
                                        if (hdm.hintCursorList[j].blockId == block.blockId)
                                        {
                                            hdm.hintCursorList[j].isDestroyedByHintBlock = true;
                                            hdm.hintCursorList[j].ShowCursor(false);
                                        }
                                    }
                                    /*foreach (Target cursor in hdm.hintCursorList)
                                    {
                                        if (cursor.blockId == blockId)
                                        {
                                            cursor.ShowCursor(false);
                                        }
                                    }*/
                                    //hdm.hintBlocks.Remove(this);
                                    //i--;
                                }
                            }
                        }
                        /*if (gm.hintBlocks.Contains(block) || gm.hintBlocks.Contains(this))
                        {
                            foreach (Target cursor in gm.hintCursors)
                            {
                                cursor.ShowCursor(false);
                            }
                            gm.hintBlocks.Clear();
                        }*/

                        block.gameObject.SetActive(false);
                        gameObject.SetActive(false);

                    }

                    //shake the camera a bit and allow action to be undone
                    gm.ScreenShake();
                    gm.StopSparks();

                    //restore hint cursors if possible
                    /*if (gm.hintBlocks.Count > 0)
                    {
                        for (int i = 0; i < gm.hintBlocks.Count; i++)
                        {
                            gm.hintCursors[i].ShowCursor(true);
                            gm.hintCursors[i].OccupyBlock(gm.hintBlocks[i]);
                        }
                    }*/

                    //After a block is destroyed, check if there are any valid comparisons. If not, then the game is over.
                    CheckForValidComparison();

                }
            }
            
        }
    }

    //converts fraction into a "proper" looking fraction for easier reading
    string FormatFraction(string fractionStr)
    {
        string[] nums = fractionStr.Split('/');
        return nums[0] + "\n-\n" + nums[1];
    }

    //this method also gets the quotient and updates fraction value UI
    public void GetNumeratorDenominator()
    {
        string[] nums = blockValue.Split('/');
        numerator = float.Parse(nums[0]);
        denominator = float.Parse(nums[1]);
        quotient = numerator / denominator;
        fractionValueUI.text = nums[0] + "\n-\n" + nums[1];
    }

    public void ShowIndicatorSprite(Sprite indicator)
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = indicatorObject.GetComponent<SpriteRenderer>();
        }

        indicatorRenderer.sprite = indicator;
    }

    //used to display correct indicator based on result of fraction comparison
    //the 'block' parameter is the block the player is going to kick.
    public Sprite CompareFractions(FractionBlock block)
    {
        if (numerator != block.numerator && denominator != block.denominator)
        {
            return indicators[SPRITE_STOP];
        }
        else if (quotient > block.quotient)
        {
            return indicators[SPRITE_CROSS];
        }
        else if (quotient < block.quotient)
        {
            return indicators[SPRITE_CIRCLE];
        }
        else
        {
            return indicators[SPRITE_WARNING];  //both blocks will be destroyed
        }
        
    }

    void CheckForValidComparison()
    {
        GameManager gm = Singleton.instance.GameManager;
        //List<FractionBlock> blockList = gm.blockList;
        if (gm.blockList.Count > 1)
        {
            FractionBlock b = gm.blockList[0];
            bool comparisonFound = false;
            int i = 1;
            while (!comparisonFound && i < gm.blockList.Count)
            {
                if ((b.numerator == gm.blockList[i].numerator) || (b.denominator == gm.blockList[i].denominator))
                {
                    comparisonFound = true;
                }
                else
                {
                    i++;
                }
            }

            if (!comparisonFound)
            {
                //game is over
                //close hint
                HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
                if (hdm.hintDialogueOpen)
                {
                    hdm.dialogueList[gm.level].HideDialogue();
                    hdm.Clear();
                }
                gm.SetGameState(GameManager.GameState.NoComparison);
            }

        }
    }
}
