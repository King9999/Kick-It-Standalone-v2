using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* Displays a larger version of the fraction whenever a player is adjacent to a fraction block. The bubble animates a bit. */
public class TextBubble : MonoBehaviour
{
    public TextMeshProUGUI fractionText;
    bool coroutineOn = false;
    float maxDistance;                //used by coroutine
    float moveDist;
    // Start is called before the first frame update
    void Start()
    {
        maxDistance = transform.position.y + 10;
        moveDist = 24;
    }

    // Update is called once per frame
    void Update()
    {
        if (!coroutineOn)
        {
            StartCoroutine(AnimateBubble());
        }
    }

    //will be called when this object is disabled.
    private void OnDisable()
    {
        StopCoroutine(AnimateBubble());
        coroutineOn = false;
    }



    public void Clamp(Vector3 position)
    {
        Vector3 imgPos = Camera.main.WorldToScreenPoint(position);
        transform.position = imgPos;
        //fractionText.transform.position = imgPos;
    }


    //bounces up and down slightly, or left and right if the player is facing vertically.
    IEnumerator AnimateBubble()
    {
        coroutineOn = true;

        //travel up
        //Player player = Singleton.instance.GameManager.player;
        //if (player.facingDirection.y == 0)
        //{
            Vector3 originalPos = transform.position;
            maxDistance = transform.position.y + 10;      //must always update the height since blocks can appear in different locations
            while (transform.position.y < maxDistance)
            {
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x, currentPos.y + moveDist * Time.deltaTime, 0);
                yield return null;
            }

            //travel down
            while (transform.position.y > originalPos.y)
            {
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x, currentPos.y - moveDist * Time.deltaTime, 0);
                yield return null;
            }
        //}
        /*else //travel side to side
        {
            Vector3 originalPos = transform.position;
            maxDistance = transform.position.x + 10;      //must always update the distance since blocks can appear in different locations
            while (transform.position.x < maxDistance)
            {
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x + moveDist * Time.deltaTime, currentPos.y, 0);
                yield return null;
            }

            //travel down
            while (transform.position.x > originalPos.x)
            {
                Vector3 currentPos = transform.position;
                transform.position = new Vector3(currentPos.x - moveDist * Time.deltaTime, currentPos.y, 0);
                yield return null;
            }
        }*/

        coroutineOn = false;
    }
}
