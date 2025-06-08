using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup voiceMixerGroup;

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
    }
}