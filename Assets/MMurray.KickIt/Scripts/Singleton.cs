using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMurray.GenericCode;
using MMurray.KickIt;
//using LoLSDK;
using SimpleJSON;
using System;
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

    JSONNode jsonString;  //used to parse text from dialogue JSON

    [Header("---Dialogue and UI text file---")]
    public TextAsset dialogueFile;      //language JSON from LoL re-purposed for standalone
    //public Dialogues dialogueText;

    //save state data
    /*public GameData gameData;
    public SaveState saveState;
    public bool saveStateFound;*/

    //TTS and music
    public bool /*ttsEnabled,*/ musicEnabled;

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
        //gameData = new GameData();
        //saveState = new SaveState();
        Application.runInBackground = false;

        PlayerPrefs.DeleteAll();    //to ensure the continue button doesn't show up
        //ttsEnabled = true;      //tts is on by default
        musicEnabled = true;

        //Set up dialogue JSON for extracting text
        jsonString = JSON.Parse(dialogueFile.text);  //must use var so that you can parse strings properly
        string boo = jsonString["dialogue"]["hq_endOfTutorialOne"].Value;
        Debug.Log(boo);

        DontDestroyOnLoad(instance);

        //call title manager
        ScreenFade sf = ScreenFade;
        sf.ChangeSceneFadeOut("Title");
    }

    //Use this to get dialogue from JSON
    public string GetText(string key)
    {
        if (jsonString == null)
        {
            jsonString = JSON.Parse(dialogueFile.text);
        }

        string text = jsonString["dialogue"][key].Value;
        return text ?? "--Missing--";
    }

    [Serializable]
    public class Dialogues
    {
        public DialogueText dialogue;
    }

    [Serializable]
    public class DialogueText
    {
        public string key;
        public string value;     //the value associated with the key
    }

}
