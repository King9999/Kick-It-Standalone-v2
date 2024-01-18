using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/* When 2 blocks clash, the resulting comparison is displayed to the player so they understand what happened. */
public class Result : MonoBehaviour
{
    public TextMeshProUGUI fractionOne, comparisonOp, fractionTwo;  //comparisonOp is the operator (<, >, =)
    bool coroutineOn;
    float maxHeight;                //used by coroutine
    float moveDist;
    public Vector2 startPos;    //the position the object starts at before it scrolls. This is usually going to be block 2's (fraction 2) position.
    // Start is called before the first frame update
    void Start()
    {
        coroutineOn = false;
        //maxHeight = transform.position.y + 10;
        moveDist = 24;
    }

    // Update is called once per frame
    void Update()
    {
        if (!coroutineOn)
        {
            StartCoroutine(AnimateText(startPos));
        }
    }

    private void OnDisable()
    {
        coroutineOn = false;
        StopCoroutine(AnimateText(startPos));
    }

    public Vector2 Clamp(Vector3 position)
    {
        Vector3 imgPos = Camera.main.WorldToScreenPoint(position);
        transform.position = imgPos;
        return transform.position;
    }

    //text scrolls up for a duration and then fades. startPos = fraction 2's block location.
    IEnumerator AnimateText(Vector2 startPos)
    {
        coroutineOn = true;
        //travel up
        transform.position = startPos;
        maxHeight = transform.position.y + 25;      //must always update the height since blocks can appear in different locations
        while (transform.position.y < maxHeight)
        {
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(currentPos.x, currentPos.y + moveDist * Time.deltaTime, 0);
            yield return null;
        }

        //fade out
        yield return new WaitForSeconds(1);

        coroutineOn = false;

        //find the bool that corresponds to this object.
        int i = 0;
        bool resultFound = false;
        while(!resultFound && i < Singleton.instance.UI.resultList.Count)
        {
            if (Singleton.instance.UI.resultList[i] == this)
            {
                resultFound = true;
                Singleton.instance.UI.resultActive[i] = false;
            }
            else
            {
                i++;
            }
        }
        gameObject.SetActive(false);
    }
}
