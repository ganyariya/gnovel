using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private const string SFX_PARENT_NAME = "SFX";
    private const string SFX_NAME_FORMAT = "SFX-[{0}]";

    public static AudioManager instance { get; private set; }

    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup voiceMixerGroup;

    private Dictionary<int, AudioChannel> audioChannels = new();

    private Transform sfxRoot;

    private void Awake()
    {
        if (instance == null)
        {
            // DontDestroyOnLoad は RootObject である必要がある
            transform.SetParent(null);
            // DontDestroyOnLoad(this) だと AudioManager コンポーネントのみが対象になってしまう
            // そのため AudioManager コンポーネントが attach されている gameObject を指定する
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        sfxRoot = new GameObject(SFX_PARENT_NAME).transform;
        sfxRoot.SetParent(gameObject.transform);
    }

    public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixerGroup = null, float volume = 1f,
        float pitch = 1f, bool loop = false)
    {
        // filePath: Audio/SFX/thunder
        // → name = thunder
        var clip = Resources.Load<AudioClip>(filePath);
        if (clip == null)
        {
            Debug.LogError($"Could not load audio file '{filePath}'");
            return null;
        }

        return PlaySoundEffect(clip, mixerGroup, volume, pitch, loop);
    }

    public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixerGroup = null, float volume = 1f,
        float pitch = 1f, bool loop = false)
    {
        var source = new GameObject(string.Format(SFX_NAME_FORMAT, clip.name)).AddComponent<AudioSource>();
        source.transform.SetParent(sfxRoot);

        source.clip = clip;
        if (mixerGroup == null) mixerGroup = sfxMixerGroup;

        // 該当の mixerGroup で AudioSource を再生する
        source.outputAudioMixerGroup = mixerGroup;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.spatialBlend = 0; // 2D

        source.Play();

        if (!loop) Destroy(source.gameObject, (clip.length / pitch) + 1);

        return source;
    }

    public AudioSource PlayVoice(string filePath, float volume = 1f, float pitch = 1f, bool loop = false) =>
        PlaySoundEffect(filePath, voiceMixerGroup, volume, pitch, loop);

    public AudioSource PlayVoice(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false) =>
        PlaySoundEffect(clip, voiceMixerGroup, volume, pitch, loop);

    public void StopSoundEffect(AudioClip clip) => StopSoundEffect(clip.name);

    public void StopSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();

        var sources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach (var source in sources)
        {
            if (source.clip.name.ToLower() != soundName) continue;
            Destroy(source.gameObject);
        }
    }

    /// <summary>
    /// AudioChannel/AudioTrack を存在しない場合は作成し、かつそのまま音声を再生する
    /// AudioManager GameObject の子としてこれらが登録されることに注意する
    /// </summary>
    public AudioTrack PlayTrack(string filePath, int channel = 0, bool loop = true, float startingVolume = 0f,
        float volumeCap = 1f)
    {
        var clip = Resources.Load<AudioClip>(filePath);
        if (clip == null)
        {
            Debug.LogError($"Could not load audio file '{filePath}'");
            return null;
        }

        return PlayTrack(clip, channel, loop, startingVolume, volumeCap, filePath);
    }

    private AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f,
        float volumeCap = 1f, string filePath = "")
    {
        var audioChannel = TryGetChannel(channel, true);
        var track = audioChannel.PlayTrack(clip, loop, startingVolume, volumeCap, filePath,
            AudioManager.instance.musicMixerGroup);
        return track;
    }

    /// <summary>
    /// channel がなかったら GameObject を作成する
    /// </summary>
    private AudioChannel TryGetChannel(int channel, bool createIfNotExist = false)
    {
        if (audioChannels.TryGetValue(channel, out var audioChannel)) return audioChannel;
        if (!createIfNotExist) return null;
        audioChannel = new AudioChannel(channel, transform);
        audioChannels.Add(channel, audioChannel);
        return audioChannel;
    }
}