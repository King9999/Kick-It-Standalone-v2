using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

//shows the current level to the player after a level loads.
public class LevelDisplayUI : MonoBehaviour
{
    public List<TextMeshProUGUI> text;
    public TextMeshProUGUI levelText;
    public List<TextMeshProUGUI> textGraveyard;
    public TextMeshProUGUI textPrefab;
    public string tutorialKey, roomKey;

    // Start is called before the first frame update
    void Awake()
    {
        float charPos = 60;
        float xOffset = 300;
        //find the level text key and populate the list. Must count the number of characters and instantiate object for each one.
        //the key changes depending on whether we're on a tutorial level or a normal level.
        string levelText = GetText(tutorialKey, Singleton.instance.gameData.level + 1);

        if (Singleton.instance.gameData.level > 1)
        {
            levelText = GetText(roomKey, Singleton.instance.gameData.level - 1);
        }

        for (int i = 0; i < levelText.Length; i++)
        {
            TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            textChar.text = levelText[i].ToString();

            //set character's position and rotation. Each character starts at a 90 degree angle on the Y axis
            textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + 50, 0);
            //textChar.transform.Rotate(0, 90, 0);
            text.Add(textChar);
        }

        //StartCoroutine(AnimateClearText());
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        Debug.Log("Disabling level display UI");
        StopCoroutine(AnimateLevelText());

        //set characters to 90 degrees
        foreach (TextMeshProUGUI character in text)
        {
            //character.transform.Rotate(0, 90, 0);
            Color charColor = character.color;
            character.color = new Color(charColor.r, charColor.g, charColor.b, 0);
        }
    }

    string GetText(string key, int level)
    {
        string value = SharedState.LanguageDefs?[key] + " " + level.ToString();
        return value ?? "--missing--";
    }

    public void ShowText()
    {
        //must check level first before running coroutine
        int level = Singleton.instance.GameManager.level;
        if (level <= 1)
        {
            SetLevelText(tutorialKey, level + 1);
            //levelText.text = "tutorial " + (level + 1);
            Debug.Log("Displaying " + levelText.text);
        }
        else
        {
            SetLevelText(roomKey, level - 1);
            //levelText.text = "room " + (level - 1);
            Debug.Log("Displaying " + levelText.text);
        }
        
        StartCoroutine(AnimateLevelText());
    }


    public void SetLevelText(string key, int level)
    {
        float charPos = 60;
        float xOffset = 300;
        //int level = Singleton.instance.GameManager.level <= 1 ? Singleton.instance.GameManager.level + 1 : Singleton.instance.GameManager.level - 1;
        string levelText = GetText(key, level);
        //levelText.text = GetText(key, level);

        for (int i = 0; i < text.Count; i++)
        {
            //TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
            
            if (i >= levelText.Length)
            {
                textGraveyard.Add(text[i]);
                text.Remove(text[i]);
                i--;
                continue;
            }

            text[i].text = levelText[i].ToString();

            //are there more chars that need to be added?
            if (i + 1 >= text.Count && i + 1 < levelText.Length)
            {
                //there's more text to add, so we must instantiate a new object
                TextMeshProUGUI textChar = Instantiate(textPrefab, transform);
                textChar.transform.position = new Vector3(transform.position.x + ((i + 1) * charPos) - xOffset, transform.position.y + 50, 0);
                text.Add(textChar);
            }

            //set character's position and rotation. Each character starts at a 90 degree angle on the Y axis
            //textChar.transform.position = new Vector3(transform.position.x + (i * charPos) - xOffset, transform.position.y + 50, 0);
            //textChar.transform.Rotate(0, 90, 0);
            //text.Add(textChar);
        }
    }

    IEnumerator AnimateLevelText()
    {
        /*IMPORTANT NOTES:
        --Must use Transform.Rotate
        --Must change *euler angles*, not rotation
        --Going below 0 degrees *does not* put degrees into negative, unlike what the inspector tells you! You can use Debug.Log to check the actual angle. */


        float rotateSpeed = -360; // -720;

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

        //change game state. If dialogue is to be displayed at the start, we do that here.
        Debug.Log("Entered AnimateText");
        Debug.Log("Game state is now: " + Singleton.instance.GameManager.gameState);
        yield return new WaitForSeconds(1);
        bool triggerFound = false;
        int j = 0;
        Debug.Log("passed WaitForSeconds in AnimateText");
        int level = Singleton.instance.GameManager.level;
        if (Singleton.instance.DialogueManager != null)
        {
            Debug.Log("Dialogue Manager is not null");
        }
        else
        {
            Debug.Log("Dialogue Manager is null!");
        }
        
        while (!triggerFound && j < Singleton.instance.DialogueManager.condTriggerList.Count)
        {
            if (Singleton.instance.DialogueManager.condTriggerList[j].level == level &&
                Singleton.instance.DialogueManager.condTriggerList[j].condition == DialogueManager.ConditionalTrigger.Condition.LevelStart)
            {
                triggerFound = true;
            }
            else
            {
                j++;
            }

        }

        if (triggerFound)
        {
            Singleton.instance.GameManager.dialogueIndex = j;
            Singleton.instance.GameManager.SetGameState(GameManager.GameState.DisplayStartOfLevelDialogue);
        }
        else
        {
            Singleton.instance.GameManager.gameState = GameManager.GameState.Normal;

            //enable sidebars if they were temp disabled
            UI ui = Singleton.instance.UI;

            if (ui.mainSidebar.sidebarOpen)
                ui.mainSidebar.gameObject.SetActive(true);
            if (ui.blockIconSidebar.sidebarOpen)
                ui.blockIconSidebar.gameObject.SetActive(true);

            Debug.Log("Game state is now: " + Singleton.instance.GameManager.gameState);
        }

        Debug.Log("Ending AnimateText");
        gameObject.SetActive(false);
    }
}
