using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 1 つの Channel には複数の AudioTrack を設定できる
/// ただし、メインで再生するのは ActiveTrack 1 つのみ
/// それ以外のものは徐々に FadeOut されて volume = 0 になったら消す
///
/// よって Channel がメインで再生するのはたかだか 1 つであり、複数の場合は FadeIn/FadeOut して 1 曲に絞る
/// </summary>
public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel-[{0}]";
    private const float TRACK_TRANSITION_SPEED = 0.0001f;

    public int channelIndex { get; private set; }

    public Transform trackContainerTransform { get; private set; } = null;

    /// <summary>
    /// 
    /// </summary>
    private AudioTrack activeTrack { get; set; } = null;

    private List<AudioTrack> tracks = new();

    bool IsLevelingVolume => volumeLevelingCoroutine != null;
    private Coroutine volumeLevelingCoroutine = null;

    public AudioChannel(int channelIndex, Transform parent)
    {
        this.channelIndex = channelIndex;
        trackContainerTransform = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channelIndex)).transform;
        trackContainerTransform.SetParent(parent);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch,
        string filePath,
        AudioMixerGroup mixerGroup)
    {
        if (TryGetTrack(clip.name, out var foundedTrack))
        {
            if (!foundedTrack.IsPlaying) foundedTrack.Play();
            ActivateTrack(foundedTrack);
            return foundedTrack;
        }

        var track = new AudioTrack(clip, loop, startingVolume, volumeCap, pitch, this, mixerGroup);
        track.Play();
        ActivateTrack(track);

        return track;
    }

    public void StopTrack()
    {
        if (activeTrack == null) return;

        // activeTrack を null にすることで tracks をすべて fadeOut させる
        activeTrack = null;
        StartVolumeLeveling();
    }

    private void ActivateTrack(AudioTrack track)
    {
        if (!tracks.Contains(track)) tracks.Add(track);
        activeTrack = track;
        StartVolumeLeveling();
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

    private void StartVolumeLeveling()
    {
        if (!IsLevelingVolume)
        {
            volumeLevelingCoroutine = AudioManager.instance.StartCoroutine(VolumeLeveling());
        }
    }

    private IEnumerator VolumeLeveling()
    {
        while (true)
        {
            if (tracks.Count == 0) break;

            // activeTrack が何も設定されてないならすべて fadeOut させるべき
            // 2 つ以上あるなら 1 つになるまで activeTrack 以外をすべて fadeOut させるべき
            var shouldFadeoutInactiveTracks =
                activeTrack == null || tracks.Count > 1;
            // activeTrack が目標 cap に到達していないのであれば 調整するべき
            var shouldBalanceActiveTrack = activeTrack != null && activeTrack.volume != activeTrack.volumeCap;

            if (!shouldFadeoutInactiveTracks && !shouldBalanceActiveTrack) break;

            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                var track = tracks[i];
                var targetVolume = activeTrack == track ? track.volumeCap : 0;
                track.volume = Mathf.MoveTowards(track.volume, targetVolume, TRACK_TRANSITION_SPEED);

                if (track != activeTrack && track.volume == 0) DestroyTrack(track);
            }

            yield return null;
        }

        volumeLevelingCoroutine = null;
    }

    private void DestroyTrack(AudioTrack track)
    {
        if (tracks.Contains(track)) tracks.Remove(track);
        track.Destroy();
    }
}