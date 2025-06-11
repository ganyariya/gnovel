using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

public class AudioTrack
{
    private const string TRACK_NAME_FORMAT = "Track-[{0}]";

    public string name { get; private set; }
    public GameObject root => source.gameObject;

    /// <summary>
    /// AudioTrack を保持する親 = Channel
    /// </summary>
    private AudioChannel channel;

    /// <summary>
    /// AudioTrack を作成したときに
    /// GameObject を生成したうえで AudioSource を動的に追加しこの変数に格納する
    /// </summary>
    private AudioSource source;

    public bool loop => source.loop;
    public bool IsPlaying => source.isPlaying;

    /// <summary>
    /// volumeCap の音量で流すことを目的とする
    /// </summary>
    public float volumeCap { get; private set; }

    public float volume
    {
        get => source.volume;
        set => source.volume = value;
    }

    public AudioTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, AudioChannel channel,
        AudioMixerGroup mixerGroup)
    {
        this.name = clip.name;
        this.volumeCap = volumeCap;
        this.channel = channel;

        source = CreateSource();
        source.clip = clip;
        source.loop = loop;
        source.volume = startingVolume;
        source.outputAudioMixerGroup = mixerGroup;
    }

    /// <summary>
    /// 副作用を持つことに注意する
    ///
    /// GameObject を新たに作成して AudioSource コンポーネントを追加し
    /// かつそのうえで Channel の子に設定する
    /// </summary>
    /// <returns></returns>
    private AudioSource CreateSource()
    {
        var go = new GameObject(string.Format(TRACK_NAME_FORMAT, name));
        go.transform.SetParent(channel.trackContainerTransform);
        var s = go.AddComponent<AudioSource>();
        return s;
    }

    public void Play()
    {
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }

    public void Destroy()
    {
        Object.Destroy(root);
    }
}