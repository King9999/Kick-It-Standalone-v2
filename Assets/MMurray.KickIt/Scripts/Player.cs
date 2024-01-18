using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using static UnityEngine.GraphicsBuffer;

/* The player can move, push blocks, and kick blocks. */
public class Player : MonoBehaviour
{
    public bool isKicking, isPushing, isStopped;    //isStopped prevents player from moving temporarily so a block's movement can't be interrupted after being pushed.
    public float moveSpeed;
    public Vector2 direction, facingDirection;  //facingDirection is used for when we want to know which way player is facing but not moving.
    public float pushDuration, currentPushTime;      //when the push duration value is exceeded, a block is pushed
    public float kickCooldown;
    public float stopDuration, currentStopTime; //prevents player from moving
    float currentTime, currentKickTime;
    public List<Sprite> playerSprites;  //index 0 contains the default sprite.
    public List<Sprite> fireSprites;
    public GameObject fireObject;       //rocket that appears when player moves
    SpriteRenderer sr, fireSr;                  //used to change sprite
    bool animateFireCoroutineOn;
    bool soundPlaying;                  //when player moves, a sound plays
    public Arrow arrow;                        //shows player's facing direction when near a targeted block.
    float inverter;
    float pushDurationMod;
    public List<Collider2D> colliders;

    // Start is called before the first frame update
    void Start()
    {
        /*currentTime = 0;
        pushDuration = 0.5f;
        stopDuration = 0.08f;
        currentPushTime = 0;
        currentKickTime = 0;
        kickCooldown = 0.5f;
        direction = Vector2.zero;
        facingDirection = Vector2.zero;
        isKicking = false;
        isPushing = false;
        isStopped = false;*/
        Initialize();
        sr = GetComponent<SpriteRenderer>();
        fireSr = fireObject.GetComponent<SpriteRenderer>();
        //playerBox = GetComponent<BoxCollider2D>();
        //Debug.Log(playerBox);
    }

    public void Initialize()
    {
        currentTime = 0;
        pushDuration = 0.5f;
        stopDuration = 0.08f;
        currentPushTime = 0;
        currentKickTime = 0;
        kickCooldown = 0.3f;
        direction = Vector2.zero;
        facingDirection = Vector2.zero;
        isKicking = false;
        isPushing = false;
        isStopped = false;
        soundPlaying = false;
        arrow.gameObject.SetActive(false);
        inverter = 1;
        pushDurationMod = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Singleton.instance.DialogueManager.dialogueOpen && Singleton.instance.GameManager.gameState != GameManager.GameState.MenuOpen)
        {
            //update kick cooldown
            if (isKicking && Time.time > currentKickTime + kickCooldown)
            {
                isKicking = false;
            }

            if (isStopped)
            {
                currentStopTime += Time.deltaTime;
                if (currentStopTime > stopDuration)
                {
                    isStopped = false;
                    currentStopTime = 0;
                }
            }

            //clear indicators if no block is currently being touched
            if (colliders.Count > 0)
            {
                bool blockFound = false;
                int i = 0;
                while (!blockFound && i < colliders.Count) 
                {
                    if (colliders[i].TryGetComponent(out FractionBlock b))
                    {
                        blockFound = true;
                    }
                    else
                    {
                        i++;
                    }
                }

                if (!blockFound)
                {
                    //hide indicators
                    List<FractionBlock> blockList = Singleton.instance.GameManager.blockList;
                    for (int j = 0; j < blockList.Count; j++)
                    {
                        blockList[j].ShowIndicatorSprite(null);
                    }
                }
            }
        }

        //update sprite and arrow position
        if (facingDirection.x < 0)
        {
            sr.flipX = true;
            inverter = 1;
        }
        else if (facingDirection.x > 0)
        {
            sr.flipX = false;
            inverter = -1;
        }

        //update arrow position
        arrow.transform.position = new Vector2(transform.position.x + (1f * inverter), transform.position.y);

        if (direction == Vector2.zero)
        {
            currentPushTime = 0;
        }
    }

    void FixedUpdate()
    {
        //update movement
        if (!isStopped && !Singleton.instance.DialogueManager.dialogueOpen && Singleton.instance.GameManager.gameState == GameManager.GameState.Normal)
            transform.Translate(new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.deltaTime);

        //show fire if player is moving
        if (direction.x != 0 || direction.y != 0)
        {
            fireObject.SetActive(true);

            //set fire's position based on player sprite's direction, taking into account whether sprite is flipped
            float fireOffset = 0.5f;
            if (sr.flipX == false)
            {
                fireSr.flipX = false;
                fireObject.transform.position = new Vector2(transform.position.x - fireOffset, transform.position.y - fireOffset);
            }
            else
            {
                fireSr.flipX = true;
                fireObject.transform.position = new Vector2(transform.position.x + fireOffset, transform.position.y - fireOffset);
            }

            if (!animateFireCoroutineOn)
            {
                StartCoroutine(AnimateFire());
            }

            //play sound
            if (!soundPlaying)
            {
                Singleton.instance.AudioManager.audioPlayerMoving.Play();
                soundPlaying = true;
            }
        }
        else
        {
            if (fireObject.activeSelf)
            {
                fireObject.SetActive(false);
                fireSr.sprite = fireSprites[0];
                StopCoroutine(AnimateFire());
                Singleton.instance.AudioManager.audioPlayerMoving.Stop();
                soundPlaying = false;
            }
        }
    }

    private void OnMove(InputValue value)
    {
        if (isStopped || Singleton.instance.DialogueManager.dialogueOpen || Singleton.instance.GameManager.gameState != GameManager.GameState.Normal)
        {
            direction = Vector2.zero;
            Singleton.instance.AudioManager.audioPlayerMoving.Stop();
            soundPlaying = false;
            return;
        }
        //if (!isStopped && !Singleton.instance.DialogueManager.dialogueOpen && Singleton.instance.GameManager.gameState != GameManager.GameState.MenuOpen)
        //{
        direction = value.Get<Vector2>();
        if (direction != Vector2.zero)
        {
            facingDirection = direction;
            isPushing = true;
        }
        else
        {
            isPushing = false;
        }
        //}

    }

    //player kicks. If in front of a block while doing this, the block will move. The kick action is active
    //for a small duration
    void OnKick(InputValue value)
    {
        if (Singleton.instance.DialogueManager.dialogueOpen || Singleton.instance.GameManager.gameState != GameManager.GameState.Normal)
            return;

        //if (!Singleton.instance.DialogueManager.dialogueOpen && Singleton.instance.GameManager.gameState != GameManager.GameState.MenuOpen)
        //{
            if (!isKicking)
            {
                isKicking = value.isPressed;
                currentKickTime = Time.time;

                //run animation coroutine
                StartCoroutine(AnimateKick());

                //play sound
                AudioManager audio = Singleton.instance.AudioManager;
                audio.soundSource.PlayOneShot(audio.audioPlayerKick, audio.soundVolume);

                //if player is facing a block, move the block. To find out if a block was kicked, we use raycasting.
                float distance = 0.125f;
                float yOffset = 1.5f;
                //float yOffset = facingDirection.y == -1 ? 1.5f : 1;
                RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(facingDirection.x, facingDirection.y * yOffset, 0), facingDirection, distance);
                Debug.DrawRay(transform.position + new Vector3(facingDirection.x, facingDirection.y * yOffset, 0), facingDirection * distance, Color.green, 5);

                if (hit.collider != null)
                {
                    Debug.Log("hit " + hit.collider.name);
                    if (hit.collider.TryGetComponent(out FractionBlock block))
                    {
                        //check if the block being kicked is adjacent to another block or a wall. There must be at least one empty space so the block gains momentum.
                        //for the ray cast, the facing direction Y value is multiplied so the ray doesn't hit the block being kicked.
                        RaycastHit2D objectHit = Physics2D.Raycast(block.transform.position + new Vector3(facingDirection.x, facingDirection.y * yOffset, 0), facingDirection, distance);
                        Debug.DrawRay(block.transform.position + new Vector3(facingDirection.x, facingDirection.y * yOffset, 0), facingDirection * distance, Color.yellow, 5);

                        if (objectHit.collider != null)
                        {
                            Singleton.instance.UI.ShowNoSpaceUI();
                            Debug.Log("Must be at least 1 space to kick this block!");
                            return;
                        }

                        if (block.isPushed)
                        {
                            Debug.Log("Cannot kick a block that's currently moving after a push");
                            return;
                        }

                        //record this action
                        Singleton.instance.Recorder.playerLastPos = transform.position;
                        Singleton.instance.Recorder.playerFacingDirection = facingDirection;
                        Singleton.instance.Recorder.lastBlockMoved = block;
                        Singleton.instance.Recorder.blockLastPos = block.transform.position;
                        Singleton.instance.Recorder.lastBlocksDestroyed.Clear();
                        Singleton.instance.Recorder.canUndo = false;        //this prevents the player from pressing undo while an action is being performed.

                        //disable the target and arrow
                        GameManager gm = Singleton.instance.GameManager;
                        HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
                        gm.targetCursor.ShowCursor(false);
                        arrow.gameObject.SetActive(false);

                        if (hdm.hintBlocks.Count > 0)
                        {
                            foreach (Target cursor in hdm.hintCursorList)
                            {
                                cursor.ShowCursor(false);
                            }
                        }

                        //move the block
                        block.moveSpeed = block.maxSpeed;
                        block.isKicked = true;
                        block.direction = facingDirection;
                        block.blockBody.bodyType = RigidbodyType2D.Dynamic;
                        Debug.Log("kicked block " + block.blockId + " with fraction " + block.blockValue);

                    }
                }
            }
        //}
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        //check the trigger ID and display the correct text.
        if (Singleton.instance.GameManager.gameState == GameManager.GameState.Normal)   //I do this check to prevent triggers from activating right away upon starting new level
        {
            if (collision.TryGetComponent(out DialogueTrigger trigger))
            {
                int i = 0;
                bool dialogueFound = false;
                while (!dialogueFound && i < Singleton.instance.DialogueManager.dialogueList.Count)
                {
                    if (trigger.dialogueId == Singleton.instance.DialogueManager.dialogueList[i].dialogueId)
                    {
                        dialogueFound = true;
                    }
                    else
                    {
                        i++;
                    }
                }

                //display dialogue
                if (dialogueFound)
                {
                    string key = Singleton.instance.DialogueManager.dialogueList[i].dialogueKeyList[0];
                    Singleton.instance.DialogueManager.dialogueList[i].ShowDialogue(key);
                    //Singleton.instance.DialogueManager.dialogueOpen = true;

                    //if player was moving, stop them
                    direction = Vector2.zero;
                }

                //disable trigger
                trigger.triggerActivated = true;
                trigger.gameObject.SetActive(false);
            }
            Debug.Log("touched trigger");
        }
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        //check collision if player is holding a direction to push the block.
        if (collision.collider != null && collision.gameObject.layer == 8 && direction != Vector2.zero)
        {
            currentTime = Time.time;
            Debug.Log("current time is " + currentTime);
        }
    }*/

    private void OnCollisionStay2D(Collision2D collision)
    {
        //Debug.Log("Player is colliding with " + collision.collider.name);

        //record all objects player is colliding with. This is done to reduce collision checks when pushing blocks.
        if (!colliders.Contains(collision.collider))
        {
            colliders.Add(collision.collider);
        }
        //check if player is colliding with a fraction block. The block is moved in the direction the player is pushing after a short duration.
        if (direction.x == 0 || direction.y == 0)   //if both x and y are not zero, that means player is facing a diagonal direction
        {
            
            float distance = 0.125f;
            float yOffset = facingDirection.y == -1 ? 1.5f : 1;       //applied when player is facing down. Prevents ray from intersecting with player
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(facingDirection.x * 0.6f, facingDirection.y /* * yOffset*/, 0), facingDirection, distance);
            Debug.DrawRay(transform.position + new Vector3(facingDirection.x * 0.6f, facingDirection.y /* * yOffset*/, 0), facingDirection * distance, Color.green);

            if (hit.collider != null && hit.collider.TryGetComponent(out FractionBlock block))
            {

                //show target cursor
                Target target = Singleton.instance.GameManager.targetCursor;
                if (!target.gameObject.activeSelf && !block.isPushed && !block.isKicked)
                {
                    target.ShowCursor(true);
                    target.OccupyBlock(block);
                    //target.transform.position = block.transform.position;
                }

                //show arrow
                arrow.gameObject.SetActive(true);

                //while the player is facing a block, get all other blocks in the direction player is facing.
                if (!Singleton.instance.GameManager.allBlocksViewed)
                {
                    RaycastHit2D[] allBlocksHit = Physics2D.RaycastAll(transform.position + new Vector3(facingDirection.x, facingDirection.y * yOffset, 0), facingDirection, 50f);
                    Debug.Log("Number of objects: " + allBlocksHit.Length);
                    int i = 0;
                    bool wallFound = false;
                    while (!wallFound && i < allBlocksHit.Length)
                    {
                        if (allBlocksHit[i].collider.TryGetComponent(out FractionBlock fractionBlock))
                        {
                            Singleton.instance.GameManager.blocksInView.Add(fractionBlock);
                            i++;
                        }
                        else //we found a wall
                        {
                            wallFound = true;
                        }

                    }

                    Singleton.instance.GameManager.allBlocksViewed = true;
                }

                //show the indicators that will show what will happen to the blocks if they collide with the block about to be kicked.
                //we ignore the first block because that's the one the player's going to kick
                List<FractionBlock> blocksInView = Singleton.instance.GameManager.blocksInView;
                for (int i = 1; i < blocksInView.Count; i++)  
                {
                    blocksInView[i].ShowIndicatorSprite(blocksInView[i].CompareFractions(blocksInView[0]));
                }

                //check if the block being pushed is adjacent to another block or a wall. If true, this block can't be pushed.
                //for the ray cast, the facing direction Y value is multiplied so the ray doesn't hit the block being pushed.
                RaycastHit2D objectHit = Physics2D.Raycast(block.transform.position + new Vector3(facingDirection.x, facingDirection.y * 1.5f, 0), facingDirection, distance);
                Debug.DrawRay(block.transform.position + new Vector3(facingDirection.x, facingDirection.y * 1.5f, 0), facingDirection * distance, Color.red);
                //Debug.Log("objectHit hit " + objectHit.collider.name);
                //BoxCollider2D blockCollider = block.GetComponent<BoxCollider2D>();

                if (objectHit.collider != null /*&& objectHit.collider != blockCollider*/)
                {
                    //can't move this block
                    Debug.Log("can't push this block");
                    return;
                }

                if (block.isRedBlock)
                {
                    Debug.Log("Block is red, can't push!");
                    return;     //can't push a red block.
                }

                //if (direction.x != 0 && direction.y != 0)
                //return;     //prevent pushing block diagonally.

                //start timer to push block
                if (isPushing)
                {
                    pushDurationMod = 1;
                    currentPushTime += Time.deltaTime;
                    Debug.Log("Push Time: " + currentPushTime);

                    //Check if player is also colliding with any other objects. If true, then increase the pushDuration.
                    if (colliders.Count > 1)
                    {
                        pushDurationMod = 3;
                    }
                    /*foreach (Collider2D collider in colliders)
                    {
                        if (collider.gameObject.layer == 6)
                        {
                            //Physics2D.IgnoreCollision(collider, collision.otherCollider, true);
                            pushDurationMod = 3;
                        }
                    }*/

                    Debug.Log("Push Duration Mod: " + pushDurationMod);
                }

                if (!block.isPushed && !block.isKicked && currentPushTime > pushDuration * pushDurationMod)
                {
                    //record this action. Any previous action before this new action occurs disappears.
                    Singleton.instance.Recorder.playerLastPos = transform.position;
                    Singleton.instance.Recorder.playerFacingDirection = facingDirection;
                    Singleton.instance.Recorder.lastBlockMoved = block;
                    Singleton.instance.Recorder.blockLastPos = block.transform.position;
                    Singleton.instance.Recorder.lastBlocksDestroyed.Clear();
                    Singleton.instance.Recorder.canUndo = false;        //can't undo while an action is in progress

                    //disable the target cursors and arrow
                    GameManager gm = Singleton.instance.GameManager;
                    HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
                    gm.targetCursor.ShowCursor(false);
                    arrow.gameObject.SetActive(false);

                    if (hdm.hintBlocks.Count > 0)
                    {
                        foreach(Target cursor in hdm.hintCursorList)
                        {
                            cursor.ShowCursor(false);
                        }
                    }

                    //hide indicators
                    for (int i = 0; i < blocksInView.Count; i++)
                    {
                        blocksInView[i].ShowIndicatorSprite(null);
                    }
                    //blocksInView.Clear();

                    //move the block. Player cannot move briefly while block is moving to prevent them from pushing the block while it's moving.
                    isStopped = true;
                    currentPushTime = 0;
                    block.moveSpeed = block.pushSpeed;
                    block.isPushed = true;
                    block.direction = facingDirection;
                    Vector3 blockPos = block.transform.position;
                    block.blockDestination = new Vector3(blockPos.x + (facingDirection.x * block.pushDistanceX), blockPos.y + (facingDirection.y * block.pushDistanceY), 0);
                    block.blockBody.bodyType = RigidbodyType2D.Dynamic;
                    Debug.Log("block " + block.blockId + " was pushed");
                }
            }
            else /*if (hit.collider != null && !hit.collider.TryGetComponent(out FractionBlock b))*/
            {
                currentPushTime = 0;
                //hide target cursor
                Target target = Singleton.instance.GameManager.targetCursor;
                if (target.gameObject.activeSelf)
                {
                    target.ShowCursor(false);
                }

                //hide indicators and bubbles
                /*List<FractionBlock> blockList = Singleton.instance.GameManager.blockList;
                for (int i = 0; i < blockList.Count; i++)
                {
                    blockList[i].ShowIndicatorSprite(null);
                }*/
                

                //hide text bubbles
                /*Singleton.instance.GameManager.textBubblesOnScreen = false;
                foreach (TextBubble bubble in Singleton.instance.GameManager.textBubbleList)
                {
                    bubble.gameObject.SetActive(false);
                }*/
                //blocksInView.Clear();

                Debug.Log("Colliding with a wall");
            }
        }
        else
        {
            currentPushTime = 0;
            Debug.Log("Can't push block diagonally!");
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {

        if (colliders.Contains(collision.collider))
        {
            /*if (collision.collider.gameObject.layer == 6)
            {
                Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, false);
            }*/
            colliders.Remove(collision.collider);
        }
        
        float yOffset = facingDirection.y == -1 ? 1.5f : 1;
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(facingDirection.x * 0.6f, facingDirection.y  /* * yOffset*/, 0), facingDirection, 0.125f);
        if (hit.collider == null || !hit.collider.TryGetComponent(out FractionBlock block)) //ray is hitting something other than a block
        {
            //hide target cursor
            Target target = Singleton.instance.GameManager.targetCursor;
            if (target.gameObject.activeSelf)
            {
                target.ShowCursor(false);
            }

            //hide arrow
            arrow.gameObject.SetActive(false);

            currentPushTime = 0;
            List<FractionBlock> blocksInView = Singleton.instance.GameManager.blocksInView;
            if (blocksInView.Count > 0)
            {
                Singleton.instance.GameManager.allBlocksViewed = false;

                //hide indicators
                for (int i = 0; i < blocksInView.Count; i++)
                {
                    blocksInView[i].ShowIndicatorSprite(null);
                }
                blocksInView.Clear();

                //hide text bubbles
                Singleton.instance.GameManager.textBubblesOnScreen = false;
                foreach(TextBubble bubble in Singleton.instance.GameManager.textBubbleList)
                {
                    bubble.gameObject.SetActive(false);
                }
                //Singleton.instance.GameManager.textBubbleList.Clear();
            }
        }
    }

    IEnumerator AnimateKick()
    {
        //change sprites
        float animationTime = 0.04f;
        sr.sprite = playerSprites[1];
        yield return new WaitForSeconds(animationTime);

        sr.sprite = playerSprites[2];       //actual kick
        yield return new WaitForSeconds(animationTime * 2);

        sr.sprite = playerSprites[1];
        yield return new WaitForSeconds(animationTime);

        sr.sprite = playerSprites[0];
        //isKicking = false;
    }

    IEnumerator AnimateFire()
    {
        animateFireCoroutineOn = true;
        float animationTime = 0.04f;
        Vector3 originalScale = fireObject.transform.localScale;
        float scaleReduction = 0.05f;
        yield return new WaitForSeconds(animationTime); //should be on fireSprites[0]

        fireSr.sprite = fireSprites[1];
        fireObject.transform.localScale = new Vector3(fireObject.transform.localScale.x - scaleReduction, 
            fireObject.transform.localScale.y - scaleReduction, 1);
        yield return new WaitForSeconds(animationTime);

        fireSr.sprite = fireSprites[2];
        fireObject.transform.localScale = new Vector3(fireObject.transform.localScale.x - scaleReduction,
            fireObject.transform.localScale.y - scaleReduction, 1);
        yield return new WaitForSeconds(animationTime);

        fireSr.sprite = fireSprites[0];
        fireObject.transform.localScale = originalScale;
        animateFireCoroutineOn = false;
    }
}
