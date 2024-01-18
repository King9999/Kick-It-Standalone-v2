using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This UI appears when player is next to a block and facing it. It helps the player to identify which block they're going to kick, and if they're close enough.
public class Target : MonoBehaviour
{
    bool coroutineOn;
    Vector3 originalScale;
    public int blockId;                //the ID of the block that the cursor is occupying. Mainly for hint cursors.
    public bool isDestroyedByHintBlock;  //used when undoing an action that results in a hint block being reset.
    // Start is called before the first frame update
    void Awake()
    {
        coroutineOn = false;
        originalScale = transform.localScale;
        blockId = -1;
    }

    private void OnDisable()
    {
        coroutineOn = false;
        StopCoroutine(AnimateTarget());
        //transform.localScale = new Vector3(1.5f, 1.5f, 1);
        //blockId = -1;
    }

    private void OnEnable()
    {
        originalScale = new Vector3(1.5f, 1.5f, 1);
        transform.localScale = originalScale;
        blockId = -1;
        Debug.Log("Target scale is " + transform.localScale);
    }

    // Update is called once per frame
    void Update()
    {
        if (!coroutineOn)
        {
            StartCoroutine(AnimateTarget());
        }

        
    }

    /*private void FixedUpdate()
    {
        //update target's position. This is mainly for hint cursors.
        GameManager gm = Singleton.instance.GameManager;
        for (int i = 0; i < gm.hintBlocks.Count; i++)
        {
            if (blockId == gm.hintBlocks[i].blockId)
            {
                transform.position = gm.hintBlocks[i].transform.position;
            }
        }
    }*/

    public void ShowCursor(bool toggle)
    {
        gameObject.SetActive(toggle);
    }

    public void OccupyBlock(FractionBlock block)
    {
        transform.position = block.transform.position;
        blockId = block.blockId;
    }

    //increases in scale to a point, then reverts to normal.
    IEnumerator AnimateTarget()
    {
        coroutineOn = true;

        transform.localScale = originalScale;
        float maxScale = originalScale.x + 0.5f;
        //float rate = 4f;

        while(transform.localScale.x < maxScale) 
        {
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(currentScale.x + Time.deltaTime, currentScale.y + Time.deltaTime, currentScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
        coroutineOn = false;
    }
}
