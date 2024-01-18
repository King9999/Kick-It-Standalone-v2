using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MMurray.GenericCode;

public class Credits : MonoBehaviour
{
    public TextMeshProUGUI backButtonText;

    [Header("---Credits---")]
    public TextMeshProUGUI mainCreditText;
    public TextMeshProUGUI gameDesignText;
    public TextMeshProUGUI programmerText;
    public TextMeshProUGUI artText;
    public TextMeshProUGUI uiText;
    public TextMeshProUGUI musicText;
    public TextMeshProUGUI soundText;
    // Start is called before the first frame update
    void Start()
    {
        UpdateLanguage();
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    void UpdateLanguage()
    {
        Singleton singleton = Singleton.instance;
        backButtonText.text = singleton.GetText("ui_backButtonText");
        mainCreditText.text = singleton.GetText("credit_main");
        gameDesignText.text = singleton.GetText("credit_gameDesign");
        programmerText.text = singleton.GetText("credit_program");
        artText.text = singleton.GetText("credit_art");
        uiText.text = singleton.GetText("credit_ui");
        musicText.text = singleton.GetText("credit_music");
        soundText.text = singleton.GetText("credit_sfx");
    }

    public void OnBackButtonClicked()
    {
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        Singleton.instance.ScreenFade.ChangeSceneFadeOut("Title");
    }
}
