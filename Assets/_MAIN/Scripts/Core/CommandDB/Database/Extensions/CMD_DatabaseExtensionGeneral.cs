using System;
using System.Collections;
using Core.DisplayDialogue;
using UnityEngine;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionGeneral : CMD_DatabaseExtensionBase
    {
        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("wait", new Func<string, IEnumerator>(Wait));
            commandDatabase.AddCommand("showDBox", new Func<IEnumerator>(ShowDialogBox));
            commandDatabase.AddCommand("hideDBox", new Func<IEnumerator>(HideDialogBox));
        }

        private static IEnumerator Wait(string data)
        {
            if (float.TryParse(data, out float time))
            {
                yield return new WaitForSeconds(time);
            }
        }

        private static IEnumerator ShowDialogBox()
        {
            yield return DialogueSystemController.instance.dialogueContainer.Show();
        }

        private static IEnumerator HideDialogBox()
        {
            yield return DialogueSystemController.instance.dialogueContainer.Hide();
        }
    }
}