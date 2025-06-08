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
        // name = thunder
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
}