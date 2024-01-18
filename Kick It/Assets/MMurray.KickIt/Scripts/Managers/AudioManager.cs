using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//manages all music and SFX
public class AudioManager : MonoBehaviour
{
    public float soundVolume;

    [Header("---Audio Clips---")]
    public AudioClip audioBlockMoving;      //block is kicked
    public AudioClip audioBlockPushed;      
    public AudioClip audioBlockHitObject;   //block is not destroyed
    public AudioClip click;             //used when a button is pressed
    public AudioClip audioPlayerKick;
    [Header("---Audio Sources---")]
    public AudioSource soundSource;             //for general sounds
    public AudioSource musicMain;
    public AudioSource musicGameCompleted;
    public AudioSource audioBlockDestroyed;     //this is an audiosource so the sound isn't interrupted by anything else.
    public AudioSource audioPlayerMoving;



    // Start is called before the first frame update
    void Start()
    {
        //soundVolume = 0.4f;
        audioPlayerMoving.volume = soundVolume;
    }



    //used in scenarios where feedback UI is displayed back to back.
    public void PlayDelayedSound(AudioClip clip, float delayDuration, float volume = 1)
    {
        StartCoroutine(PlaySoundAfterDelay(clip, delayDuration, volume));
    }

    IEnumerator PlaySoundAfterDelay(AudioClip clip, float delayDuration, float volume = 1)
    {
        yield return new WaitForSeconds(delayDuration);
        soundSource.PlayOneShot(clip, volume);
    }
}
