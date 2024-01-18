using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UIElements;

//shows tutorial info and video clips.
public class TutorialWindow : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public RawImage exampleImage, blockIconImage;           //appears when the "What's a fraction?" button is clicked
    public RawImage vidRenderer;            //appaers when "Block icons" buttin is clicked
    public VideoPlayer vidPlayer;
    public string videoName;
    // Start is called before the first frame update
    void Start()
    {
        exampleImage.gameObject.SetActive(false);
        vidRenderer.gameObject.SetActive(false);
        vidPlayer.gameObject.SetActive(false);
    }

    public void HideAllAssets()
    {
        exampleImage.gameObject.SetActive(false);
        blockIconImage.gameObject.SetActive(false);
        vidRenderer.gameObject.SetActive(false);
        vidPlayer.gameObject.SetActive(false);
    }

    public void ShowVideoClip(bool toggle)
    {
        vidRenderer.gameObject.SetActive(toggle);
        vidPlayer.gameObject.SetActive(toggle);

        if (toggle == true)
        {
            //locate the clip. NOTE: Must play clip through the StreamingAssets folder, so must locate the path.
            string vidPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
            vidPlayer.url = vidPath;
            Debug.Log("Playing video clip from path " + vidPlayer.url);
            vidPlayer.Play();
        }
    }

    public void ShowFractionImage(bool toggle)
    {
        exampleImage.gameObject.SetActive(toggle);
    }

    public void ShowBlockIconImage(bool toggle)
    {
        blockIconImage.gameObject.SetActive(toggle);
    }


}
