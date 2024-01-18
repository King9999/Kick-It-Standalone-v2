using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MMurray.GenericCode;
using MMurray.KickIt;
using UnityEditor;
using LoLSDK;

public class MenuWindow : MonoBehaviour
{
    public TextMeshProUGUI resetButtonText;
    public TextMeshProUGUI helpButtonText;
    public TextMeshProUGUI exitButtonText;
    public TextMeshProUGUI closeMenuButtonText;
    public TextMeshProUGUI ttsButtonText, musicButtonText;      //either On or Off
    public TextMeshProUGUI ttsText, musicText;                  //text resides next to their respective buttons
    Color onColor, offColor;                                    //for the tts and music buttons.
    public HelpWindow helpWindow;
    // Start is called before the first frame update
    void Start()
    {
        //set up button text
        resetButtonText.text = GetText("button_resetRoom");
        helpButtonText.text = GetText("button_help");
        exitButtonText.text = GetText("button_exit");
        closeMenuButtonText.text = GetText("button_closeMenu");
        //ttsButtonText.text = GetText("button_ttsOn");
        //musicButtonText.text = GetText("button_musicOn");
        ttsText.text = GetText("text_tts");
        musicText.text = GetText("text_music");
        onColor = new Color(0.1f, 0.9f, 0.3f);
        offColor = new Color(0.9f, 0.2f, 0.2f);

        //check tts and music states
        UpdateTTSState();
        UpdateMusicState();
        /*if (Singleton.instance.ttsEnabled)
        {
            //ON
            ttsButtonText.text = GetText("button_ttsOn");
            ttsButtonText.color = onColor;
        }
        else
        {
            ttsButtonText.text = GetText("button_ttsOff");
            ttsButtonText.color = offColor;
        }

        if (Singleton.instance.musicEnabled)
        {
            musicButtonText.text = GetText("button_musicOn");
            musicButtonText.color = onColor;
        }
        else
        {
            musicButtonText.text = GetText("button_musicOff");
            musicButtonText.color = offColor;
        }*/

        helpWindow.gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        UpdateTTSState();
        UpdateMusicState();
    }

    public void ResetButtonPressed()
    {
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState == GameManager.GameState.ResetLevel || gm.gameState == GameManager.GameState.AllBlocksClear)
            return;

        //close any common dialogue boxes
        List<Dialogue> commonDialogue = Singleton.instance.DialogueManager.commonDialogueList;
        foreach(Dialogue dialogue in commonDialogue)
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
        /*if (Singleton.instance.DialogueManager.commonDialogueList[0].gameObject.activeSelf)
        {
            Singleton.instance.DialogueManager.commonDialogueList[0].ForceCloseDialogue();
        }*/


        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        Singleton.instance.GameManager.SetGameState(GameManager.GameState.ResetLevel);
        CloseWindow();
    }

    public void HelpButtonPressed()
    {
        //shift menu window to the side and show a larger window
        helpWindow.gameObject.SetActive(true);
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        gameObject.SetActive(false);
    }

    public void ExitButtonPressed()
    {
        //save game state and go back to title scene
        //Singleton.instance.gameData = new GameData();
        CloseWindow();
        if (Singleton.instance.AudioManager.musicMain.isPlaying)
        {
            Singleton.instance.AudioManager.musicMain.Stop();
        }

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        //turn off TTS in case it's currently playing
        if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
        }

        Singleton.instance.saveState.WriteState(Singleton.instance.gameData);
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.ChangeSceneFadeOut("Title");
    }

    public void TTSButtonPressed()
    {
        Singleton.instance.ttsEnabled = !Singleton.instance.ttsEnabled;
        Debug.Log("TTS Button Pressed. TTS State: " + Singleton.instance.ttsEnabled);
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        /*if (Singleton.instance.ttsEnabled)
        {
            ttsButtonText.text = GetText("button_ttsOn");
            ttsButtonText.color = onColor;
        }
        else
        {
            ttsButtonText.text = GetText("button_ttsOff");
            ttsButtonText.color = offColor;
        }*/
        UpdateTTSState();
    }

    public void MusicButtonPressed()
    {
        Singleton.instance.musicEnabled = !Singleton.instance.musicEnabled;
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        /*if (Singleton.instance.musicEnabled)
        {
            musicButtonText.text = GetText("button_musicOn");
            musicButtonText.color = onColor;
        }
        else
        {
            musicButtonText.text = GetText("button_musicOff");
            musicButtonText.color = offColor;
        }*/
        UpdateMusicState();
    }

    public void UpdateMusicState()
    {
        if (Singleton.instance.musicEnabled)
        {
            musicButtonText.text = GetText("button_musicOn");
            musicButtonText.color = onColor;

            //play music
            AudioManager audio = Singleton.instance.AudioManager;
            if (!audio.musicMain.isPlaying)
            {
                audio.musicMain.Play();
            }
        }
        else
        {
            musicButtonText.text = GetText("button_musicOff");
            musicButtonText.color = offColor;
            AudioManager audio = Singleton.instance.AudioManager;
            audio.musicMain.Stop();
        }
    }

    public void UpdateTTSState()
    {
        if (Singleton.instance.ttsEnabled)
        {
            ttsButtonText.text = GetText("button_ttsOn");
            ttsButtonText.color = onColor;
        }
        else
        {
            ttsButtonText.text = GetText("button_ttsOff");
            ttsButtonText.color = offColor;

            //if TTS is running, turn it off
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
        }
    }


    public void OpenWindow()
    {
        //temporarily disable sidebars if they're open
        UI ui = Singleton.instance.UI;

        if (ui.mainSidebar.sidebarOpen)
            ui.mainSidebar.gameObject.SetActive(false);
        if (ui.blockIconSidebar.sidebarOpen)
            ui.blockIconSidebar.gameObject.SetActive(false);

        //update state. Need to keep previous state so we can go back to it when menu closes
        GameManager gm = Singleton.instance.GameManager;
        gm.previousState = gm.gameState;
        gm.gameState = GameManager.GameState.MenuOpen;

        StartCoroutine(AnimateWindowOpen());
    }

    public void CloseWindow()
    {
        //enable sidebars if they were temp disabled
        UI ui = Singleton.instance.UI;

        if (ui.mainSidebar.sidebarOpen)
            ui.mainSidebar.gameObject.SetActive(true);
        if (ui.blockIconSidebar.sidebarOpen)
            ui.blockIconSidebar.gameObject.SetActive(true);


        StartCoroutine(AnimateWindowClose());
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
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
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
        GameManager gm = Singleton.instance.GameManager;
        if (gm.gameState != GameManager.GameState.ResetLevel)
        {
            //revert back to previous game state.
            gm.gameState = gm.previousState;
            //gm.gameState = GameManager.GameState.Normal;
        }

        gameObject.SetActive(false);
    }
}
