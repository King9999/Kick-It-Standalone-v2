using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using LoLSDK;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//displays tutorials. Each tutorial has video clips
public class HelpWindow : MonoBehaviour
{
    public MenuWindow menuWindow;
    public TutorialWindow tutorialWindow;
    public TextMeshProUGUI helpButtonOneText;       //how to move
    public TextMeshProUGUI helpButtonTwoText;       //push
    public TextMeshProUGUI helpButtonThreeText;     //kick
    public TextMeshProUGUI helpButtonFourText;      //blocks
    public TextMeshProUGUI helpButtonFiveText;      //what's a fraction
    public TextMeshProUGUI helpButtonSixText;       //block icons
    public TextMeshProUGUI backButtonText;
    float fontSize = 24;

    //[Header("---Video Clips---")]
    //public VideoPlayer videoPlayer;
    //public List<VideoClip> videoClips;

    // Start is called before the first frame update
    void Start()
    {
        helpButtonOneText.text = GetText("button_howToMove");
        helpButtonTwoText.text = GetText("button_pushBlock");
        helpButtonThreeText.text = GetText("button_kick");
        helpButtonFourText.text = GetText("button_fractionBlock");
        helpButtonFiveText.text = GetText("button_numeratorDenominator");
        helpButtonSixText.text = GetText("button_blockIcons");
        backButtonText.text = GetText("button_back");
        tutorialWindow.gameObject.SetActive(false);
    }

    string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }

    public void HelpButtonOneClicked()
    {
        //open a secondary window and show clip
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.HideAllAssets();
        //tutorialWindow.exampleImage.gameObject.SetActive(false);
        //tutorialWindow.vidPlayer
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
            

        //videoPlayer.clip = videoClips[0];
        tutorialWindow.tutorialText.text = GetText("tutorial_howToMove");
        tutorialWindow.tutorialText.fontSize = fontSize;
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText(); //cancel any previous sound
            LOLSDK.Instance.SpeakText("tutorial_howToMove");
        }*/

    }

    public void HelpButtonTwoClicked()
    {
        //open a secondary window and show clip
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.HideAllAssets();
        //tutorialWindow.exampleImage.gameObject.SetActive(false);
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        //videoPlayer.clip = videoClips[1];
        tutorialWindow.tutorialText.text = GetText("tutorial_pushBlock");
        tutorialWindow.tutorialText.fontSize = fontSize;
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            LOLSDK.Instance.SpeakText("tutorial_pushBlock");
        }*/
    }

    public void HelpButtonThreeClicked()
    {
        //open a secondary window and show clip
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.HideAllAssets();
        //tutorialWindow.exampleImage.gameObject.SetActive(false);
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        //videoPlayer.clip = videoClips[2];
        tutorialWindow.tutorialText.text = GetText("tutorial_kick");
        tutorialWindow.tutorialText.fontSize = fontSize;
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            LOLSDK.Instance.SpeakText("tutorial_kick");
        }*/
    }

    public void HelpButtonFourClicked()
    {
        //open a secondary window and show clip
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.HideAllAssets();
        //tutorialWindow.exampleImage.gameObject.SetActive(false);
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        //videoPlayer.clip = videoClips[3];
        tutorialWindow.tutorialText.text = GetText("tutorial_fractionBlock");
        tutorialWindow.tutorialText.fontSize = fontSize;
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            LOLSDK.Instance.SpeakText("tutorial_fractionBlock");
        }*/
    }

    public void HelpButtonFiveClicked()
    {
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.HideAllAssets();
        tutorialWindow.ShowFractionImage(true); //only time we see this image
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        tutorialWindow.tutorialText.text = GetText("tutorial_numeratorDenominator");
        tutorialWindow.tutorialText.fontSize = fontSize - 4;
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            LOLSDK.Instance.SpeakText("tutorial_numeratorDenominator");
        }*/
    }

    public void HelpButtonSixClicked()
    {
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.ShowVideoClip(true);
        tutorialWindow.ShowBlockIconImage(true);
        tutorialWindow.ShowFractionImage(false); 
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        tutorialWindow.tutorialText.text = GetText("tutorial_blockIcons");
        tutorialWindow.tutorialText.fontSize = fontSize - 4;
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
            LOLSDK.Instance.SpeakText("tutorial_blockIcons");
        }*/
    }

    public void BackButtonClicked()
    {
        //close help and tutorial window
        menuWindow.gameObject.SetActive(true);
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        //cancel TTS if it's currently active
        /*if (Singleton.instance.ttsEnabled)
        {
            ((ILOLSDK_EXTENSION)LOLSDK.Instance.PostMessage).CancelSpeakText();
        }*/
        tutorialWindow.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
