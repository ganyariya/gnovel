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
        private static string[] PARAM_VOLUME = new string[] { "-volume", "-v" };
        private static string[] PARAM_PITCH = new string[] { "-pitch" };
        private static string[] PARAM_LOOP = new string[] { "-loop", "-l" };

        public new static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("playsfx", new Action<string[]>(PlaySFX));
            commandDatabase.AddCommand("stopsfx", new Action<string>(StopSFX));
            commandDatabase.AddCommand("playvoice", new Action<string[]>(PlayVoice));
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
    }
}