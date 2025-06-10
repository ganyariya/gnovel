using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using UnityEngine;

namespace Testing
{
    public class TestingAudioManager : MonoBehaviour
    {
        void Start()
        {
            // StartCoroutine(TestAudioChannel());
            StartCoroutine(TestSoundEffect());
        }

        IEnumerator TestAudioChannel()
        {
            AudioManager.instance.PlayTrack("Audio/SFX/thunder_01");
            yield return null;
        }

        IEnumerator TestSoundEffect()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya", true);
            yield return ganyariya.Show();
            AudioManager.instance.PlayTrack("Audio/Music/Calm", startingVolume: 0.1f);
            ganyariya.Say("\"Oh, bgm is good!!\"");
            yield return new WaitForSeconds(2f);

            AudioManager.instance.PlaySoundEffect("Audio/SFX/thunder_01");
            AudioManager.instance.PlayVoice("Audio/Voices/wakeup");
            ganyariya.CallTriggerAnimation("Hop");
            ganyariya.Say("\"Wao! what's!?\"");
            yield return new WaitForSeconds(2f);

            AudioManager.instance.StopSoundEffect("thunder_01");
            ganyariya.Say("\"Oh... stop thunder...\"");
        }
    }
}