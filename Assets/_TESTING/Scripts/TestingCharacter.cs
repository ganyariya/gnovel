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
            // StartCoroutine(CharacterShowHideMoveTest());
            StartCoroutine(SpriteChangeTest());
        }

        IEnumerator SpriteChangeTest()
        {
            SpriteCharacter guard = CharacterManager.instance.CreateCharacter("guard1 as Generic") as SpriteCharacter;
            SpriteCharacter ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;

            yield return guard.Show();

            Sprite monkSprite = guard.FetchSpriteFromResources("Default-Monk");
            guard.SetSprite(monkSprite, 0);

            yield return new WaitForSeconds(1.0f);

            yield return guard.MoveToScreenPosition(Vector2.zero, 3, false);

            yield return ganyariya.Show();
        }

        IEnumerator CharacterShowHideMoveTest()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;
            var raelin = CharacterManager.instance.CreateCharacter("raelin") as SpriteCharacter;
            var femaleStudent2 = CharacterManager.instance.CreateCharacter("Female Student 2") as SpriteCharacter;

            yield return new WaitForSeconds(1.0f);

            yield return ganyariya.Show();
            yield return raelin.Show();
            yield return femaleStudent2.Show();

            yield return new WaitForSeconds(1.0f);

            var fullBodySprite = femaleStudent2.FetchSpriteFromResources("female student 2 - upset");
            femaleStudent2.SetSprite(fullBodySprite, 0);

            ganyariya.SetScreenPosition(Vector2.zero);
            yield return new WaitForSeconds(1.0f);

            yield return ganyariya.MoveToScreenPosition(new Vector2(0.5f, 0.5f), 1.0f);
            yield return new WaitForSeconds(1.0f);

            yield return ganyariya.MoveToScreenPosition(Vector2.one, 3.0f, true);
            yield return new WaitForSeconds(1.0f);

            yield return new WaitForSeconds(1.0f);

            yield return ganyariya.Say("\"Hello, I'm ganyariya!\"");

            yield return ganyariya.Hide();

            var guard1 = CharacterManager.instance.CreateCharacter("guard1 as Generic");
            var guard2 = CharacterManager.instance.CreateCharacter("guard2 as Generic");

            yield return guard1.Show();
            yield return guard2.Show();

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