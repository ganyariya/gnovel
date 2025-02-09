using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Characters;


namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionCharacter : CMD_DatabaseExtensionBase
    {
        private static string[] IMMEDIATE_PARAMS => new string[] { "-i", "-immediate" };
        private static string[] ENABLE_PARAMS => new string[] { "-e", "-enabled" };

        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("createCharacter", new Action<string[]>(CreateCharacter));
            commandDatabase.AddCommand("showCharacters", new Func<string[], IEnumerator>(ShowAll));
            commandDatabase.AddCommand("hideCharacters", new Func<string[], IEnumerator>(HideAll));
        }

        public static void CreateCharacter(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(ENABLE_PARAMS, out bool enabled, defaultValue: false);
            parameterFetcher.TryGetValue(IMMEDIATE_PARAMS, out bool immediate, defaultValue: false);

            var characterName = data[0];
            var character = CharacterManager.instance.CreateCharacter(characterName);

            if (!enabled) return;

            if (immediate) character.isVisible = true;
            else character.Show();
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
            parameterFetcher.TryGetValue(IMMEDIATE_PARAMS, out bool immediate, defaultValue: false);

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
            parameterFetcher.TryGetValue(IMMEDIATE_PARAMS, out bool immediate, defaultValue: false);

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
