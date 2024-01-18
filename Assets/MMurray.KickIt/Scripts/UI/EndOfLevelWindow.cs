using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LoLSDK;
using UnityEngine.InputSystem;

//This menu appears after room completion.
public class EndOfLevelWindow : MonoBehaviour
{
    public Button resetRoomButton, nextRoomButton;
    public TextMeshProUGUI endOfLevelText, resetRoomButtonText, nextRoomButtonText;

    // Start is called before the first frame update
    void Start()
    {
        Singleton singleton = Singleton.instance;
        endOfLevelText.text = singleton.GetText("ui_endOfLevelMenuText");
        resetRoomButtonText.text = singleton.GetText("ui_retryRoomButtonText");
        nextRoomButtonText.text = singleton.GetText("ui_nextRoomButtonText");
        //gameObject.SetActive(false);
    }


    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    public void OpenWindow()
    {
        StartCoroutine(AnimateWindowOpen());
    }

    void CloseWindow()
    {
        StartCoroutine (AnimateWindowClose());
    }

    public void OnNextRoomButtonClicked()
    {
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        //stop TTS
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
        }*/

        GameManager gm = Singleton.instance.GameManager;
        gm.SetGameState(GameManager.GameState.GoToNextLevel);
        CloseWindow();
    }

    public void OnRetryRoomButtonClicked()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState == GameManager.GameState.ResetLevel /*|| gm.gameState == GameManager.GameState.AllBlocksClear*/)
            return;

        //close any common dialogue boxes
        List<Dialogue> commonDialogue = Singleton.instance.DialogueManager.commonDialogueList;
        foreach (Dialogue dialogue in commonDialogue)
        {
            if (dialogue.gameObject.activeSelf)
            {
                dialogue.ForceCloseDialogue();
            }
        }

        //close hint
        HintDialogueManager hdm = Singleton.instance.HintDialogueManager;
        if (hdm.hintDialogueOpen)
        {
            hdm.dialogueList[gm.level].HideDialogue();
        }

        //stop TTS
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
        }*/

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        gm.SetGameState(GameManager.GameState.ResetLevel);
        CloseWindow();
    }

    //window starts at 0 scale and increases until it reaches 1.
    IEnumerator AnimateWindowOpen()
    {
        //gameObject.SetActive(true);
        transform.localScale = new Vector3(0, 0, 1);
        float scaleSpeed = 4;

        while (transform.localScale.x < 1)
        {
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(currentScale.x + scaleSpeed * Time.deltaTime, currentScale.y + scaleSpeed * Time.deltaTime, 1);
            yield return null;
        }

        transform.localScale = Vector3.one;

        //play TTS
        /*if (Singleton.instance.ttsEnabled)
        {
            LOLSDK.Instance.SpeakText("ui_endOfLevelMenuText");
            Debug.Log("Reading end of level menu text");
        }*/
    }

    IEnumerator AnimateWindowClose()
    {
        float scaleSpeed = 4;

        while (transform.localScale.x > 0)
        {
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(currentScale.x - scaleSpeed * Time.deltaTime, currentScale.y - scaleSpeed * Time.deltaTime, 1);
            yield return null;
        }

        transform.localScale = new Vector3(0, 0, 1);

        //if the current game state is resetting level, we don't change game state here. The game state will change after level is loaded.
        //if (Singleton.instance.GameManager.gameState != GameManager.GameState.ResetLevel)
            //Singleton.instance.GameManager.gameState = GameManager.GameState.Normal;

        gameObject.SetActive(false);
    }
}
