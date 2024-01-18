using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Data;
using UnityEditor;

//displays "Room Clear!" text when a level is complete. The text is animated. 
public class RoomClearUI : MonoBehaviour
{
    public List<TextMeshProUGUI> text;
    public TextMeshProUGUI textPrefab;
    bool coroutineOn;
    // Start is called before the first frame update
    void Start()
    {
        coroutineOn = false;
        float charPos = 60;
        float xOffset = 300;
        //find the clear text key and populate the list. Must count the number of characters and instantiate object for each one.
        string clearText = Singleton.instance.GetText("levelClearText");
        for (int i = 0; i < clearText.Length; i++)
        {
            TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            textChar.text = clearText[i].ToString();

            //set character's position and rotation. Each character starts at a 90 degree angle on the Y axis
            textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + 50, 0);
            //textChar.transform.Rotate(0, 90, 0);
            text.Add(textChar);
        }

        //StartCoroutine(AnimateClearText());
        gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        coroutineOn = false;
        StopCoroutine(AnimateClearText());

        //set characters to 90 degrees
        foreach(TextMeshProUGUI character in text)
        {
            //character.transform.Rotate(0, 90, 0);
            Color charColor = character.color;
            character.color = new Color(charColor.r, charColor.g, charColor.b, 0);
        }
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    public void ShowText()
    {
        //call coroutine here
        StartCoroutine(AnimateClearText());
    }

    IEnumerator AnimateClearText()
    {
        /*IMPORTANT NOTES:
        --Must use Transform.Rotate
        --Must change *euler angles*, not rotation
        --Going below 0 degrees *does not* put degrees into negative, unlike what the inspector tells you! You can use Debug.Log to check the actual angle. */


        //float rotateSpeed = -720;

        //I add a delay to let any comparison displays finish before showing the clear text. I don't want the clear text to obstruct
        //the comparison displays

        yield return new WaitForSeconds(2);

        for (int i = 0; i < text.Count; i++)
        {
            if (text[i].text == " ") continue;

            /*do
            {
                float vy = rotateSpeed * Time.deltaTime;
                text[i].transform.Rotate(0, vy, 0);
                //Debug.Log(text[0].transform.eulerAngles);
                yield return null;
            }
            while (text[i].transform.eulerAngles.y > 5);

            text[i].transform.eulerAngles = Vector3.zero;*/
            Color charColor = text[i].color;
            text[i].color = new Color(charColor.r, charColor.g, charColor.b, 1);
            yield return new WaitForSeconds(0.08f);
        }



        //change game state to all blocks clear
        yield return new WaitForSeconds(1);
        Singleton.instance.GameManager.SetGameState(GameManager.GameState.AllBlocksClear);

        gameObject.SetActive(false);
    }
}
