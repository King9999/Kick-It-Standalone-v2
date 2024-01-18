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

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    void UpdateLanguage()
    {
        backButtonText.text = GetText("ui_backButtonText");
        mainCreditText.text = GetText("credit_main");
        gameDesignText.text = GetText("credit_gameDesign");
        programmerText.text = GetText("credit_program");
        artText.text = GetText("credit_art");
        uiText.text = GetText("credit_ui");
        musicText.text = GetText("credit_music");
        soundText.text = GetText("credit_sfx");
    }

    public void OnBackButtonClicked()
    {
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        Singleton.instance.ScreenFade.ChangeSceneFadeOut("Title");
    }
}
