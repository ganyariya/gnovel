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
            StartCoroutine(TestSoundEffect());
        }

        IEnumerator TestSoundEffect()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya", true);
            yield return ganyariya.Show();
            yield return new WaitForSeconds(0.5f);

            AudioManager.instance.PlaySoundEffect("Audio/SFX/thunder_01");
            AudioManager.instance.PlayVoice("Audio/Voices/wakeup");
            ganyariya.CallTriggerAnimation("Hop");
            ganyariya.Say("\"Wao!\"");

            yield return new WaitForSeconds(2f);
            AudioManager.instance.StopSoundEffect("thunder_01");
            ganyariya.Say("\"Oh... stop thunder...\"");
        }
    }
}