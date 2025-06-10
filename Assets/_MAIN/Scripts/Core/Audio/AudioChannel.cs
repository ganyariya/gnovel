using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioChannel : MonoBehaviour
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel-[{0}]";

    public int channelIndex { get; private set; }

    public Transform trackContainerTransform { get; private set; } = null;

    private List<AudioTrack> tracks = new();

    public AudioChannel(int channelIndex, Transform parent)
    {
        this.channelIndex = channelIndex;
        trackContainerTransform = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channelIndex)).transform;
        trackContainerTransform.SetParent(parent);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, string filePath,
        AudioMixerGroup mixerGroup)
    {
        if (TryGetTrack(clip.name, out var foundedTrack))
        {
            if (!foundedTrack.IsPlaying) foundedTrack.Play();
            return foundedTrack;
        }

        var track = new AudioTrack(clip, loop, startingVolume, volumeCap, this, mixerGroup);
        track.Play();
        tracks.Add(track); // 動画でやってなかったけどやる
        return track;
    }

    /// <summary>
    /// すでに Play 中の Track があるならそれを返す 
    /// </summary>
    public bool TryGetTrack(string trackName, out AudioTrack result)
    {
        trackName = trackName.ToLower();

        foreach (var track in tracks.Where(track => track.name.ToLower() == trackName))
        {
            result = track;
            return true;
        }

        result = null;
        return false;
    }
}