using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//contains features that persist across all scenes.
public class UniversalSettings : MonoBehaviour
{
    public bool ttsEnabled;          //false by default
    public bool musicEnabled;       //true by default
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTTSButtonClicked()
    {
        ttsEnabled = !ttsEnabled;
        Debug.Log("TTS Button clicked, setting is " + ttsEnabled);
    }

    public void OnMusicButtonClicked()
    {
        musicEnabled = !musicEnabled;
        Debug.Log("Music Button clicked, setting is " + musicEnabled);
    }
}
