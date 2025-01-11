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
            StartCoroutine(CharacterShowHideTest());
        }

        IEnumerator CharacterShowHideTest()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya");
            // 表示できるけど見づらいので student 2 は非表示にする
            // var femaleStudent2 = CharacterManager.instance.CreateCharacter("Female Student 2");

            yield return new WaitForSeconds(2.0f);

            yield return ganyariya.Show();

            yield return new WaitForSeconds(2.0f);

            yield return ganyariya.Say("\"Hello, I'm ganyariya!\"");

            yield return null;
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

            ganyariya.SetDialogueColor(Color.red);
            yield return ganyariya.Say(dialogues);

            ganyariya.ResetConfig();
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