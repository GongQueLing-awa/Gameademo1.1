using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    [Header("事件监听")]
    public PlayAudioEventSO BGMAudioEvent;
    public PlayAudioEventSO FxAudioEvent;

    [Header("奇妙小组件")]
    public AudioSource BGMSource;
    public AudioSource FXSource;
    public AudioMixer mixer;

    // 用 Awake 订阅（早于所有 OnEnable）：
    // 保证场景里 playOnEnable 的 AudioDefination 抛事件时，这里已经在监听
    private void Awake()
    {
        // 先减再加：关闭域重载时 SO 上可能残留上次的委托，避免重复订阅
        if (BGMAudioEvent != null)
        {
            BGMAudioEvent.OnEventRaised -= PlayBGM;
            BGMAudioEvent.OnEventRaised += PlayBGM;
        }

        if (FxAudioEvent != null)
        {
            FxAudioEvent.OnEventRaised -= PlayFX;
            FxAudioEvent.OnEventRaised += PlayFX;
        }
    }

    // SO 是资产，不会随场景销毁：不退订就会一直握着已销毁的 AudioManager，形成僵尸订阅
    private void OnDestroy()
    {
        if (BGMAudioEvent != null)
        {
            BGMAudioEvent.OnEventRaised -= PlayBGM;
        }

        if (FxAudioEvent != null)
        {
            FxAudioEvent.OnEventRaised -= PlayFX;
        }
    }

    // 播放 BGM：同一首正在播时不打断，换曲时切 clip 并保持循环
    private void PlayBGM(AudioClip audioClip)
    {
        if (audioClip == null || BGMSource == null)
        {
            return;
        }

        if (BGMSource.clip == audioClip && BGMSource.isPlaying)
        {
            return;
        }

        BGMSource.clip = audioClip;
        BGMSource.loop = true;
        BGMSource.Play();
    }

    // 播放 FX：一次性叠加，不打断其他音效
    private void PlayFX(AudioClip audioClip)
    {
        if (audioClip == null || FXSource == null)
        {
            return;
        }

        FXSource.PlayOneShot(audioClip);
    }
}
