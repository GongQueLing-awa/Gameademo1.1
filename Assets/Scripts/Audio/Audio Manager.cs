using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("事件监听")]
    public PlayAudioEventSO BGMAudioEvent;
    public PlayAudioEventSO FxAudioEvent;


    public AudioSource BGMSource;
    public AudioSource FXSource;

    public void OnEnable()
    {
        //BGMAudioEvent.OnEventRaised += PlayBGM;
        FxAudioEvent.OnEventRaised += PlayFX;
    }

    public void OnDisable()
    {
        //BGMAudioEvent.OnEventRaised -= PlayBGM;
        FxAudioEvent.OnEventRaised -= PlayFX;
    }

    //private void PlayBGM(AudioClip arg0)
    //{
    //    throw new NotImplementedException();
    //}

    private void PlayFX(AudioClip audioPlay)
    {
        FXSource.PlayOneShot(audioPlay);
    }
   

    
}
