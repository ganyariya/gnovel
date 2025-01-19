using System;
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
            // StartCoroutine(SpriteChangeTest());
            // StartCoroutine(Chap81TutorialTest());
            // StartCoroutine(ColorTest());
            // StartCoroutine(PriorityTest());
            StartCoroutine(AnimateTest());
        }

        IEnumerator AnimateTest()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;
            yield return ganyariya.Show();

            yield return new WaitForSeconds(1);

            ganyariya.CallTriggerAnimation("Hop");

            yield return new WaitForSeconds(1);

            ganyariya.CallStateAnimation("Shiver", true);

            yield return new WaitForSeconds(5);

            ganyariya.CallStateAnimation("Shiver", false);
        }

        IEnumerator PriorityTest()
        {
            var guard = CharacterManager.instance.CreateCharacter("guard as Generic");
            var guardRed = CharacterManager.instance.CreateCharacter("Guard Red as Generic");
            var raelin = CharacterManager.instance.CreateCharacter("raelin") as SpriteCharacter;
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;

            guardRed.SetColor(Color.red);

            guard.Show();
            guardRed.Show();
            ganyariya.Show();
            raelin.Show();

            raelin.SetScreenPosition(new Vector2(0.3f, 0));
            ganyariya.SetScreenPosition(new Vector2(0.45f, 0));
            guard.SetScreenPosition(new Vector2(0.6f, 0));
            guardRed.SetScreenPosition(new Vector2(0.75f, 0));

            yield return new WaitForSeconds(3);

            guardRed.SetPriority(1000);
            ganyariya.SetPriority(15);
            raelin.SetPriority(8);
            guard.SetPriority(30);

            yield return new WaitForSeconds(3);

            CharacterManager.instance.SortCharacters(new string[] { "Raelin", "ganyariya" });

            yield return new WaitForSeconds(3);

            CharacterManager.instance.SortCharacters();

            yield return new WaitForSeconds(3);
            CharacterManager.instance.SortCharacters(new string[] { "Raelin", "Guard Red", "Guard", "ganyariya" });
        }

        IEnumerator ColorTest()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;
            yield return ganyariya.Show();

            yield return ganyariya.ExecuteHighlighting();
            yield return new WaitForSeconds(1);

            yield return ganyariya.ExecuteUnHighlighting();
            yield return new WaitForSeconds(1);

            yield return ganyariya.ExecuteChangingColor(Color.red, 1);
            yield return new WaitForSeconds(1);

            yield return ganyariya.ExecuteHighlighting();
            yield return new WaitForSeconds(1);

            yield return ganyariya.ExecuteChangingColor(Color.white, 1);
            yield return new WaitForSeconds(1);

            yield return ganyariya.Flip();
            yield return new WaitForSeconds(1);

            yield return ganyariya.Flip();
            yield return new WaitForSeconds(1);

            yield return ganyariya.FlipToLeft(1);
            yield return new WaitForSeconds(1);

            yield return ganyariya.FlipToLeft(1);
            yield return new WaitForSeconds(1);
        }

        IEnumerator Chap81TutorialTest()
        {
            Func<float, WaitForSeconds> wait = (float seconds) => new WaitForSeconds(seconds);

            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;
            var raelin = CharacterManager.instance.CreateCharacter("raelin") as SpriteCharacter;
            ganyariya.isVisible = false;
            raelin.isVisible = false;

            yield return wait(1.0f);
            yield return ganyariya.Show();

            yield return ganyariya.Say("\"ここ 1 週間の進捗を振り返るよ〜\"");
            yield return ganyariya.Say("\"まずはじめに、Sprite Character が自由に表示できるようになったよ。僕自身がそうだね\"");

            ganyariya.MoveToScreenPosition(Vector2.one, 1.0f, true);
            yield return ganyariya.Say("\"続いて、画面上の好きな位置に移動できるようになったよ。\"");

            ganyariya.MoveToScreenPosition(Vector2.zero, 2.0f, false);
            yield return ganyariya.Say("\"どこにでも動けて嬉しいね\"");

            yield return ganyariya.Say("\"次の機能は自分では説明できないから他の人にでてきてもらうね\"");

            yield return raelin.Show();
            yield return wait(1.0f);

            var bodySprite = raelin.FetchSpriteFromResources("B2");
            var faceSprite = raelin.FetchSpriteFromResources("B_Normal");

            raelin.ExecuteTransitionSprite(bodySprite, 0, 1.0f);
            raelin.ExecuteTransitionSprite(faceSprite, 1, 1.0f);
            yield return ganyariya.Say("\"好きな CharacterLayer の Sprite を変更できるようになったよ\"");

            faceSprite = raelin.FetchSpriteFromResources("B_SoSmile");
            raelin.ExecuteTransitionSprite(faceSprite, 1, 1.0f);
            yield return raelin.Say("\"これで表情も変えられるようになったよ。うれしいね\"");

            bodySprite = raelin.FetchSpriteFromResources("A1");
            faceSprite = raelin.FetchSpriteFromResources("A_Normal");
            raelin.ExecuteTransitionSprite(bodySprite, 0, 1.0f);
            raelin.ExecuteTransitionSprite(faceSprite, 1, 1.0f);
            yield return ganyariya.Say("\"今週は以上だよ、来週もがんばるぞ〜\"");

            yield return null;
        }

        IEnumerator SpriteChangeTest()
        {
            SpriteCharacter guard = CharacterManager.instance.CreateCharacter("guard1 as Generic") as SpriteCharacter;
            SpriteCharacter ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;

            yield return new WaitForSeconds(1.0f);

            yield return guard.Show();

            yield return new WaitForSeconds(1.0f);

            Sprite monkSprite = guard.FetchSpriteFromResources("Default-Monk");
            yield return guard.ExecuteTransitionSprite(monkSprite, 0, 1.0f);

            yield return new WaitForSeconds(1.0f);

            yield return guard.MoveToScreenPosition(Vector2.zero, 3, false);

            yield return ganyariya.Show();
            yield return new WaitForSeconds(1.0f);
            yield return ganyariya.MoveToScreenPosition(Vector2.one, 3, false);

            SpriteCharacter raelin = CharacterManager.instance.CreateCharacter("raelin") as SpriteCharacter;
            yield return raelin.Show();

            yield return new WaitForSeconds(1.0f);

            Sprite bodySprite = raelin.FetchSpriteFromResources("B2");
            Sprite faceSprite = raelin.FetchSpriteFromResources("B_Blush");

            raelin.ExecuteTransitionSprite(bodySprite, 0, 1.0f);
            yield return raelin.ExecuteTransitionSprite(faceSprite, 1, 1.0f);

            yield return new WaitForSeconds(1.0f);

            faceSprite = raelin.FetchSpriteFromResources("B_SoSmile");
            yield return raelin.ExecuteTransitionSprite(faceSprite, 1, 1.0f);

            yield return new WaitForSeconds(1.0f);
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