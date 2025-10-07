using System;
using System.Collections;
using Core.DisplayDialogue;
using Core.ScriptIO;
using UnityEngine;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionGeneral : CMD_DatabaseExtensionBase
    {
        private static string[] PARAMS_IMMEDIATE => new[] { "-i", "-immediate" };
        private static string[] PARAMS_SPEED => new[] { "-spd", "-speed" };
        private static string[] PARAMS_ENQUEUE => new[] { "-enq", "-enqueue" };
        private static string[] PARAMS_FILENAME => new[] { "-f", "-filename" };

        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("wait", new Func<string, IEnumerator>(Wait));
            commandDatabase.AddCommand("load", new Action<string[]>(LoadScript));

            commandDatabase.AddCommand("showDBox", new Func<string[], IEnumerator>(ShowDialogBox));
            commandDatabase.AddCommand("hideDBox", new Func<string[], IEnumerator>(HideDialogBox));

            commandDatabase.AddCommand("showUI", new Func<string[], IEnumerator>(ShowUI));
            commandDatabase.AddCommand("hideUI", new Func<string[], IEnumerator>(HideUI));
        }

        private static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float time))
            {
                yield return new WaitForSeconds(time);
            }
        }

        private static void LoadScript(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_FILENAME, out string fileName, string.Empty);
            parameterFetcher.TryGetValue(PARAMS_ENQUEUE, out bool enqueue, true);

            var filePath =
                UnityRuntimePathToolBox.ResolveHomeDirectoryPath(UnityRuntimePathToolBox.ResourcesDialoguePath,
                    fileName);
            var textAsset = Resources.Load<TextAsset>(filePath);
            if (textAsset == null)
            {
                Debug.LogWarning($"`LoadScript` cannot load text asset. fileName: {fileName}");
                return;
            }

            var lines = TextReader.ReadTextAsset(textAsset, true);
            var conversation = new Conversation(lines);
            
            // enqueue = true であればキューの末尾に新しいシナリオを追加する
            // false であれば今のシナリオを強制ストップして新しいシナリオを開始する
            if (enqueue) DialogueSystemController.instance.EnqueueConversation(conversation);
            else DialogueSystemController.instance.Say(conversation);
        }

        private static IEnumerator ShowDialogBox(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1);
            yield return DialogueSystemController.instance.dialogueContainer.Show(speed, immediate);
        }

        private static IEnumerator HideDialogBox(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1);
            yield return DialogueSystemController.instance.dialogueContainer.Hide(speed, immediate);
        }

        private static IEnumerator ShowUI(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1);

            yield return DialogueSystemController.instance.ShowUIAll(speed, immediate);
        }

        private static IEnumerator HideUI(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1);
            yield return DialogueSystemController.instance.HideUIAll(speed, immediate);
        }
    }
}