using System;
using System.Collections;
using System.Collections.Generic;
using MMurray.GenericCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LoLSDK;
using MMurray.KickIt;

public class TitleManager : MonoBehaviour
{
    float timer = 0;
    float duration = 1;

    public Button continueButton;
    public TextMeshProUGUI startButtonText, continueButtonText, creditsButtonText;
    public TextMeshProUGUI version;
    public string versionNumber;

    // Start is called before the first frame update
    void Start()
    {
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.FadeIn();

        //button text setup
        startButtonText.text = GetText("ui_startButtonText");
        continueButtonText.text = GetText("ui_continueButtonText");
        creditsButtonText.text = GetText("ui_creditsButtonText");
        version.text = versionNumber;

        //check for a save state
        LOLSDK.Instance.LoadState<GameData>(state =>
        {
            if (state != null)
            {
                Singleton.instance.saveStateFound = true;
                Singleton.instance.gameData.level = state.data.level;       //will need this for when game manager loads the level.

                //saved universal settings are applied here
                //state.data.tts_enabled = true;
                Singleton.instance.ttsEnabled = state.data.tts_enabled;
                Singleton.instance.musicEnabled = state.data.music_enabled;
                continueButton.gameObject.SetActive(true);
            }
            else
            {
                //hide continue button
                continueButton.gameObject.SetActive(false);
            }
           
        });
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void StartButtonClicked()
    {
        ScreenFade sf = Singleton.instance.ScreenFade;

        if (sf.coroutineOn)
            return;

        //the save state is deleted when starting a new game.
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        PlayerPrefs.DeleteAll();
        Singleton.instance.musicEnabled = true;
        Singleton.instance.ttsEnabled = true;
        Singleton.instance.saveStateFound = false;
        sf.ChangeSceneFadeOut("Game");
    }

    public void ContinueButtonPressed()
    {
        //need some kind of bool to load a game state
        ScreenFade sf = Singleton.instance.ScreenFade;
        if (sf.coroutineOn)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        sf.ChangeSceneFadeOut("Game");
    }

    public void CreditsButtonPressed()
    {
        //go to credits scene
        ScreenFade sf = Singleton.instance.ScreenFade;

        if (sf.coroutineOn)
            return;

        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        sf.ChangeSceneFadeOut("Credits");
    }
}
