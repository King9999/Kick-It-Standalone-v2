using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using LoLSDK;
using MMurray.GenericCode;
using UnityEngine.UI;

//displays text when the game is won. Also displays buttons for quitting game and returning to title.
public class WinGameUI : MonoBehaviour
{
    public TextMeshProUGUI winText;
    public TextMeshProUGUI exitButtonText, returnButtonText;

    private void Start()
    {
        Singleton singleton = Singleton.instance;
        winText.text = singleton.GetText("allLevelsClearText");
        exitButtonText.text = singleton.GetText("ui_exitButtonText");
        returnButtonText.text = singleton.GetText("ui_returnButtonText");
        gameObject.SetActive(false);
    }

    /*string GetText(string key)
    {
        string value = SharedState.LanguageDefs?[key];
        return value ?? "--missing--";
    }*/

    public void ExitButtonClicked()
    {
        //save progress and call LoL function to exit game.
        AudioManager audio = Singleton.instance.AudioManager;
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);

        GameManager gm = Singleton.instance.GameManager;
        //LOLSDK.Instance.SubmitProgress(0, gm.level + 1, gm.MaxLevels);
        //LOLSDK.Instance.CompleteGame();
    }

    public void ReturnButtonClicked()
    {
        //return to title screen
        AudioManager audio = Singleton.instance.AudioManager;
        audio.musicMain.Stop();
        audio.soundSource.PlayOneShot(audio.click, audio.soundVolume);
        ScreenFade sf = Singleton.instance.ScreenFade;
        sf.ChangeSceneFadeOut("Title");
    }

    public void DisplayWin()
    {
        StartCoroutine(AnimateWin());
    }

    /* The winner's name pops in from center position and increases in scale up to a certain point. A "bounce"(?) effect is applied after the
    scaling is done. Afterwards, particle effect plays. */
    IEnumerator AnimateWin()
    {
        //scale begins at 0, then increses to 1.5
        Vector3 originalScale = new Vector3(winText.transform.localScale.x, winText.transform.localScale.y, 0);
        Vector3 destinationScale = winText.transform.localScale * 1.4f;
        Vector3 secondaryScale = winText.transform.localScale * 1.2f;
        winText.transform.localScale = Vector3.zero;
        

        float scaleSpeed = 4;
        yield return ScaleWinLabel(scaleSpeed, destinationScale.x);
        yield return ScaleWinLabelSecondary(scaleSpeed, secondaryScale.x, destinationScale.x);

        //display button to exit game or return to title.
    }

    IEnumerator ScaleWinLabel(float scaleSpeed, float maxScale)
    {
        while (winText.transform.localScale.x < maxScale)
        {
            float vx = winText.transform.localScale.x + scaleSpeed * Time.deltaTime;
            float vy = winText.transform.localScale.y + scaleSpeed * Time.deltaTime;
            winText.transform.localScale = new Vector3(vx, vy, 0);
            yield return null;
        }
    }

    //bounce effect. Scales down, then back up briefly. 
    IEnumerator ScaleWinLabelSecondary(float scaleSpeed, float minScale, float maxScale)
    {
        //scale down
        while (winText.transform.localScale.x > minScale)
        {
            float vx = winText.transform.localScale.x - scaleSpeed * Time.deltaTime;
            float vy = winText.transform.localScale.y - scaleSpeed * Time.deltaTime;
            winText.transform.localScale = new Vector3(vx, vy, 0);
            yield return null;
        }

        //scale up
        while (winText.transform.localScale.x < maxScale)
        {
            float vx = winText.transform.localScale.x + scaleSpeed * Time.deltaTime;
            float vy = winText.transform.localScale.y + scaleSpeed * Time.deltaTime;
            winText.transform.localScale = new Vector3(vx, vy, 0);
            yield return null;
        }
    }
}
