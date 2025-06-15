using System;
using System.Collections;
using System.Collections.Generic;
using Core.Audio;
using Core.ScriptIO;
using UnityEngine;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionAudio : CMD_DatabaseExtensionBase
    {
        private static string[] PARAM_SFX_NAME = new string[] { "-n", "-sfx", "-voice" };
        private static string[] PARAM_BGM_NAME = new string[] { "-bgm", "-b" };
        private static string[] PARAM_AMBIENCE_NAME = new string[] { "-ambience" };

        private static string[] PARAM_VOLUME = new string[] { "-volume", "-v" };
        private static string[] PARAM_PITCH = new string[] { "-pitch" };
        private static string[] PARAM_LOOP = new string[] { "-loop", "-l" };
        private static string[] PARAM_CHANNEL = new string[] { "-channel", "-c" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-immediate", "-i" };
        private static string[] PARAM_START_VOLUME = new string[] { "-startVolume", "-sv" };

        private static int AMBIENCE_CHANNEL = 0;
        private static int BGM_CHANNEL = 1;

        public new static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("playsfx", new Action<string[]>(PlaySFX));
            commandDatabase.AddCommand("stopsfx", new Action<string>(StopSFX));
            commandDatabase.AddCommand("playvoice", new Action<string[]>(PlayVoice));
            commandDatabase.AddCommand("stopVoice", new Action<string>(StopSFX));

            commandDatabase.AddCommand("playbgm", new Action<string[]>(PlayBGM));
            commandDatabase.AddCommand("playambience", new Action<string[]>(PlayAmbience));

            commandDatabase.AddCommand("stopbgm", new Action<string>(StopBGM));
            commandDatabase.AddCommand("stopambience", new Action<string>(StopAmbience));
            commandDatabase.AddCommand("stoptrack", new Action<string>(StopTrack));
        }

        private static void PlaySFX(string[] data)
        {
            var fetcher = CreateFetcher(data);
            fetcher.TryGetValue(PARAM_SFX_NAME, out var sfxName, "");
            fetcher.TryGetValue(PARAM_VOLUME, out var volume, 1.0f);
            fetcher.TryGetValue(PARAM_PITCH, out var pitch, 1.0f);
            fetcher.TryGetValue(PARAM_LOOP, out var loop, false);

            var audioClip =
                UnityRuntimePathToolBox.ResolveHomeDirectoryPath(UnityRuntimePathToolBox.ResourcesSfxPath, sfxName);
            if (audioClip == null)
            {
                Debug.LogWarning($"SFX {sfxName} not found");
                return;
            }

            AudioManager.instance.PlaySoundEffect(audioClip, volume: volume, pitch: pitch, loop: loop);
        }

        private static void StopSFX(string data)
        {
            AudioManager.instance.StopSoundEffect(data);
        }

        private static void PlayVoice(string[] data)
        {
            var fetcher = CreateFetcher(data);
            fetcher.TryGetValue(PARAM_SFX_NAME, out var sfxName, "");
            fetcher.TryGetValue(PARAM_VOLUME, out var volume, 1.0f);
            fetcher.TryGetValue(PARAM_PITCH, out var pitch, 1.0f);
            fetcher.TryGetValue(PARAM_LOOP, out var loop, false);

            var audioClip =
                UnityRuntimePathToolBox.ResolveHomeDirectoryPath(UnityRuntimePathToolBox.ResourcesVoicePath, sfxName);
            if (audioClip == null)
            {
                Debug.LogWarning($"SFX {sfxName} not found");
                return;
            }

            AudioManager.instance.PlayVoice(audioClip, volume: volume, pitch: pitch, loop: loop);
        }

        private static void PlayBGM(string[] data)
        {
            var fetcher = CreateFetcher(data);
            fetcher.TryGetValue(PARAM_BGM_NAME, out var bgmName, "");
            bgmName = UnityRuntimePathToolBox.ResolveHomeDirectoryPath(UnityRuntimePathToolBox.ResourcesBgmPath,
                bgmName);
            fetcher.TryGetValue(PARAM_CHANNEL, out var channel, BGM_CHANNEL);

            PlayTrack(bgmName, channel, fetcher);
        }

        private static void PlayAmbience(string[] data)
        {
            var fetcher = CreateFetcher(data);
            fetcher.TryGetValue(PARAM_AMBIENCE_NAME, out var ambienceName, "");
            ambienceName = UnityRuntimePathToolBox.ResolveHomeDirectoryPath(
                UnityRuntimePathToolBox.ResourcesAmbiencePath,
                ambienceName);

            fetcher.TryGetValue(PARAM_CHANNEL, out var channel, AMBIENCE_CHANNEL);

            PlayTrack(ambienceName, channel, fetcher);
        }

        private static void PlayTrack(string filePath, int channel, CommandParameterFetcher fetcher)
        {
            fetcher.TryGetValue(PARAM_VOLUME, out var volumeCap, 1.0f);
            fetcher.TryGetValue(PARAM_START_VOLUME, out var startVolume, 1.0f);
            fetcher.TryGetValue(PARAM_PITCH, out var pitch, 1.0f);
            fetcher.TryGetValue(PARAM_LOOP, out var loop, false);
            fetcher.TryGetValue(PARAM_IMMEDIATE, out var immediate, false);

            AudioManager.instance.PlayTrack(filePath, channel, loop, startVolume, pitch, volumeCap);
        }

        private static void StopBGM(string data)
        {
            if (data == string.Empty) StopTrack(BGM_CHANNEL.ToString());
            else StopTrack(data);
        }

        private static void StopAmbience(string data)
        {
            if (data == string.Empty) StopTrack(AMBIENCE_CHANNEL.ToString());
            else StopTrack(data);
        }

        private static void StopTrack(string data)
        {
            if (int.TryParse(data, out int channel))
            {
                AudioManager.instance.StopTrack(channel);
            }
            else
            {
                AudioManager.instance.StopTrack(data);
            }
        }
    }
}