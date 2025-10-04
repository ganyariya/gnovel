using System;
using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using Core.FeaturePanel;
using UnityEngine;

namespace Testing
{
    public class TestingChoicePanel : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(Running());
        }

        private IEnumerator Running()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;
            ganyariya.Show();
            yield return ganyariya.Say("\"Hello!, What's your favorite food?\"");

            var choicePanel = ChoicePanel.Instance;

            string[] choices =
            {
                "apple",
                "orange",
                "fish",
                "delicious delicious banana",
                "supersupersupersupersupersupersupersuperlong"
            };
            choicePanel.Show("What is your favorite food?", choices);
            while (choicePanel.IsEnteringChoice) yield return null;

            string answer = choicePanel.LastDecision.GetAnswer();
            yield return ganyariya.Say($"\"Oh! I like {answer} too!\"");
        }
    }
}