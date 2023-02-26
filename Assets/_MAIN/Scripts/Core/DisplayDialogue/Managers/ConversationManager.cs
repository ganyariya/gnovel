using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// 会話を Unity 画面に出力する
    /// </summary>
    public class ConversationManager
    {
        public bool IsRunning => process != null;

        private readonly DialogueSystemController dialogueSystem;
        private readonly DisplayTextArchitect textArchitect;
        private Coroutine process;

        public ConversationManager(DialogueSystemController dialogueSystem, DisplayTextArchitect textArchitect)
        {
            this.dialogueSystem = dialogueSystem;
            this.textArchitect = textArchitect;
            this.process = null;
        }

        public void StartConversation(List<string> conversation)
        {
            StopConversation();

            // Coroutine 自体は monobehavior を持つ dialogueSystem に移譲する
            process = dialogueSystem.StartCoroutine(RunningConversation(conversation));
        }

        public void StopConversation()
        {
            if (process != null) dialogueSystem.StopCoroutine(process);
            process = null;
        }

        private IEnumerator RunningConversation(List<string> conversation)
        {
            foreach (var rawText in conversation)
            {
                if (string.IsNullOrWhiteSpace(rawText)) continue;
                DialogueLineData lineData = DialogueParser.Parse(rawText);

                if (lineData.HasDialogue) yield return RunningSingleDialogue(lineData);
                if (lineData.HasCommands) yield return RunningSingleCommands(lineData);

                yield return new WaitForSeconds(1);
            }
        }

        private IEnumerator RunningSingleDialogue(DialogueLineData lineData)
        {
            if (lineData.HasSpeaker) dialogueSystem.DisplaySpeakerName(lineData.speaker);
            else dialogueSystem.HideSpeakerName();

            textArchitect.Display(lineData.dialogue);

            while (textArchitect.IsDisplaying) yield return null;
        }

        private IEnumerator RunningSingleCommands(DialogueLineData lineData)
        {
            Debug.Log(lineData.commands);
            yield return null;
        }

    }
}
