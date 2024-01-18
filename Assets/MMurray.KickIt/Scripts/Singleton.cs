using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.GenericCode;
using MMurray.KickIt;
using LoLSDK;
//using UnityEngine.Android;

/* This script is used to call all other singletons in the game. Having them all in one place should make singletons easier to manage. */
public class Singleton : MonoBehaviour
{
    public static Singleton instance {get; private set;}
    public GameManager GameManager {get; set;}
    public Recorder Recorder { get; private set; }
    public DialogueManager DialogueManager { get; set; }
    public HintDialogueManager HintDialogueManager { get; set; }
    public ScreenFade ScreenFade {get; private set;}
    public UI UI { get; set; }
    public AudioManager AudioManager { get; private set; }
    //public TitleManager TitleManager {get; private set;}

    //save state data
    public GameData gameData;
    public SaveState saveState;
    public bool saveStateFound;

    //TTS and music
    public bool ttsEnabled, musicEnabled;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        ScreenFade = GetComponentInChildren<ScreenFade>();
        Recorder = GetComponentInChildren<Recorder>();
        AudioManager = GetComponentInChildren<AudioManager>();
        gameData = new GameData();
        saveState = new SaveState();
        Application.runInBackground = false;

        //PlayerPrefs.DeleteAll();
        ttsEnabled = true;      //tts is on by default
        musicEnabled = true;

        DontDestroyOnLoad(instance);
    }

}
