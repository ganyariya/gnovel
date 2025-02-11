using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Characters;
using Extensions;
using UnityEngine;
using UnityEngine.Events;


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
        private static string[] PARAMS_PRIORITY => new string[] { "-priority" };
        private static string[] PARAMS_COLOR => new string[] { "-color" };

        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("createCharacter", new Func<string[], IEnumerator>(CreateCharacter));
            commandDatabase.AddCommand("moveCharacter", new Func<string[], IEnumerator>(MoveCharacter));
            commandDatabase.AddCommand("showCharacters", new Func<string[], IEnumerator>(ShowAll));
            commandDatabase.AddCommand("hideCharacters", new Func<string[], IEnumerator>(HideAll));
            commandDatabase.AddCommand("setCharacterPriority", new Action<string[]>(SetPriority));
            commandDatabase.AddCommand("sortCharacters", new Action<string[]>(SortCharacters));
            commandDatabase.AddCommand("setCharacterColor", new Func<string[], IEnumerator>(SetCharacterColor));

            // register character baseDatabase
            CommandDatabase baseDatabase = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTER_BASE);
            baseDatabase.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));
            baseDatabase.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            baseDatabase.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
            baseDatabase.AddCommand("setPriority", new Action<string[]>(SetPriority));
            baseDatabase.AddCommand("setColor", new Func<string[], IEnumerator>(SetCharacterColor));
            baseDatabase.AddCommand("highlight", new Func<string[], IEnumerator>(HighlightCharacter));
            baseDatabase.AddCommand("unHighlight", new Func<string[], IEnumerator>(UnHighlightCharacter));
        }

        /// <summary>
        /// キャラ 1 体を登場させる
        /// createCharacter(ganyariya -enabled true -immediate true)
        /// - enabled
        /// - immediate
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static IEnumerator CreateCharacter(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_ENABLED, out bool enabled, defaultValue: false);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, defaultValue: false);

            var characterName = data[0];
            var character = CharacterManager.instance.CreateCharacter(characterName);

            if (!enabled) yield break;

            if (immediate) character.isVisible = true;
            else yield return character.Show();
        }

        /// <summary>
        /// キャラ 1 体を移動できる
        /// moveCharacter(ganyariya -x 1.0 -y 0.5 -speed 0.5 -smooth true)
        /// </summary>
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
            {
                character.SetScreenPosition(position);
            }
            else
            {
                CommandManager.instance.RegisterTerminationEventToCurrentCommandProcess(() => { character.SetScreenPosition(position); });
                yield return character.MoveToScreenPosition(position, speed, smooth);
            }
        }

        /// <summary>
        /// 複数のキャラを表示できる
        /// showCharacters(ganyariya raelin -immediate true)
        /// </summary>
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
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, defaultValue: 1f);

            foreach (Character character in characters)
            {
                if (immediate) character.isVisible = true;
                else character.Show(speed);
            }

            if (!immediate)
            {
                CommandManager.instance.RegisterTerminationEventToCurrentCommandProcess(() =>
                {
                    foreach (var character in characters) character.isVisible = true;
                });
                // キャラが登場仕切るまで待つ
                while (characters.Any(x => x.isRevealing)) yield return null;
            }
        }

        /// <summary>
        /// 複数のキャラを隠せる
        /// hideCharacters(ganyariya raelin -immediate true)
        /// </summary>
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
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, defaultValue: 1f);

            foreach (Character character in characters)
            {
                if (immediate) character.isVisible = false;
                else character.Hide(speed);
            }

            if (!immediate)
            {
                CommandManager.instance.RegisterTerminationEventToCurrentCommandProcess(() =>
                {
                    foreach (var character in characters) character.isVisible = false;
                });
                // キャラが登場仕切るまで待つ
                while (characters.Any(x => x.isHiding)) yield return null;
            }
        }

        public static void SetPriority(string[] data)
        {
            if (data.Length < 3) return;

            string characterName = data[0];

            Character character = CharacterManager.instance.GetCharacter(characterName);
            if (character == null) return;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_PRIORITY, out int priority, 0);

            character.SetPriority(priority);
        }

        public static void SortCharacters(string[] data)
        {
            CharacterManager.instance.SortCharacters(data);
        }

        private static IEnumerator SetCharacterColor(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);
            if (character == null) yield break;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_COLOR, out string colorText, "");
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1f);

            var color = ColorExtension.CreateColorFromString(colorText);

            void immediateAction()
            {
                character.SetColor(color);
            }

            if (immediate)
            {
                immediateAction();
                yield break;
            }

            CommandManager.instance.RegisterTerminationEventToCurrentCommandProcess(immediateAction);
            yield return character.ExecuteChangingColor(color, speed);
        }

        private static IEnumerator HighlightCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);
            if (character == null) yield break;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1f);

            CommandManager.instance.RegisterTerminationEventToCurrentCommandProcess(() => { character.ExecuteHighlighting(speed, immediate: true); });
            yield return character.ExecuteHighlighting(speed, immediate);
        }

        private static IEnumerator UnHighlightCharacter(string[] data)
        {
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);
            if (character == null) yield break;

            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1f);

            CommandManager.instance.RegisterTerminationEventToCurrentCommandProcess(() => { character.ExecuteUnHighlighting(speed, immediate: true); });
            yield return character.ExecuteUnHighlighting(speed, immediate);
        }
    }
}
