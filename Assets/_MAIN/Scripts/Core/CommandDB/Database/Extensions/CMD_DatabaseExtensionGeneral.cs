using System;
using System.Collections;
using Core.DisplayDialogue;
using UnityEngine;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionGeneral : CMD_DatabaseExtensionBase
    {
        private static string[] PARAMS_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string[] PARAMS_SPEED => new string[] { "-spd", "-speed" };

        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("wait", new Func<string, IEnumerator>(Wait));
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

            yield return DialogueSystemController.instance.Show(speed, immediate);
        }

        private static IEnumerator HideUI(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAMS_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAMS_SPEED, out float speed, 1);
            yield return DialogueSystemController.instance.Hide(speed, immediate);
        }
    }
}