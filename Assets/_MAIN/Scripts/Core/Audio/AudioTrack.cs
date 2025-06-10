using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioTrack
{
    private const string TRACK_NAME_FORMAT = "Track-[{0}]";

    public string name { get; private set; }
    private AudioChannel channel;
    private AudioSource source;
    public bool loop => source.loop;
    public bool IsPlaying => source.isPlaying;
    public float volumeCap { get; private set; }

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
    /// AudioSource を設定した GameObject を新たに作成して
    /// Channel の子に設定する
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
}