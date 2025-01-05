using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using Core.DisplayDialogue;
using UnityEngine;

namespace Testing
{
    public class TestingCharacter : MonoBehaviour
    {
        void Start()
        {
            // Character ganyariya = CharacterManager.instance.CreateCharacter("ganyariya");
            // Character notFound = CharacterManager.instance.CreateCharacter("notFound");

            StartCoroutine(CharacterSpeakTest());
        }

        IEnumerator CharacterSpeakTest()
        {
            Character ganyariya = CharacterManager.instance.CreateCharacter("ganyariya");
            Character you = CharacterManager.instance.CreateCharacter("you");

            var dialogues = new List<string> {
                "\"こんにちは！{a}私は ganyariya です。\"",
                // `""` で囲まないと会話と認識されないことに注意する
                "\"あなたの名前をおしえて\"",
            };

            yield return ganyariya.Say(dialogues);

            var yourDialogues = new List<string> {
                "\"私の名前は you です。\"",
                "\"この世界における架空的存在です\"",
            };
            yield return you.Say(yourDialogues);

            Debug.Log("CharacterSpeakTest: End");
        }
    }
}