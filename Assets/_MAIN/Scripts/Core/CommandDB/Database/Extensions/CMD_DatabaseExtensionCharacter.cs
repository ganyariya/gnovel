using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Characters;
using UnityEngine;


namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionCharacter : CMD_DatabaseExtensionBase
    {
        private static string[] PARAMS_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string[] PARAMS_ENABLED => new string[] { "-e", "-enabled" };
        private static string[] PARAMS_SPEED => new string[] { "-spd", "-speed" };
        private static string[] PARAMS_SMOOTH => new string[] { "-sm", "-smooth" };
        private static string[] PARAMS_XPOS => new string[] { "-x" };
        private static string[] PARAMS_YPOS => new string[] { "-y" };

        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("createCharacter", new Action<string[]>(CreateCharacter));
            commandDatabase.AddCommand("moveCharacter", new Func<string[], IEnumerator>(MoveCharacter));
            commandDatabase.AddCommand("showCharacters", new Func<string[], IEnumerator>(ShowAll));
            commandDatabase.AddCommand("hideCharacters", new Func<string[], IEnumerator>(HideAll));
        }

        public static void CreateCharacter(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_ENABLED, out bool enabled, defaultValue: false);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, defaultValue: false);

            var characterName = data[0];
            var character = CharacterManager.instance.CreateCharacter(characterName);

            if (!enabled) return;

            if (immediate) character.isVisible = true;
            else character.Show();
        }

        public static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);

            if (character == null) yield break;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_XPOS, out float x, .0f);
            parameterFetcher.TryGetValue(PARAMS_YPOS, out float y, .0f);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 2.0f);
            parameterFetcher.TryGetValue(PARAMS_SMOOTH, out bool smooth, false);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);

            Vector2 position = new(x, y);

            if (immediate)
                character.SetScreenPosition(position);
            else
                yield return character.MoveToScreenPosition(position, speed, smooth);
        }

        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new();

            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, create: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0) yield break;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, defaultValue: false);

            foreach (Character character in characters)
            {
                if (immediate) character.isVisible = true;
                else character.Show();
            }

            if (!immediate)
            {
                // キャラが登場仕切るまで待つ
                while (characters.Any(x => x.isRevealing)) yield return null;
            }
        }

        public static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new();

            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, create: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0) yield break;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, defaultValue: false);

            foreach (Character character in characters)
            {
                if (immediate) character.isVisible = false;
                else character.Hide();
            }

            if (!immediate)
            {
                // キャラが登場仕切るまで待つ
                while (characters.Any(x => x.isHiding)) yield return null;
            }
        }
    }
}
