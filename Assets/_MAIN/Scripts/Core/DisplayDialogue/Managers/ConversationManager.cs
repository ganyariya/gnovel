using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// RawTestList をもとに DialogueLineData を生成し 非同期で textArchitect を使って画面に出力する
    /// </summary>
    public class ConversationManager
    {
        public bool IsRunning => process != null;

        private readonly DialogueSystemController dialogueSystem;
        private readonly DisplayTextArchitect textArchitect;
        private Coroutine process;
        private bool userPromptNext = false;

        public ConversationManager(DialogueSystemController dialogueSystem, DisplayTextArchitect textArchitect)
        {
            dialogueSystem.UserPromptNextEvent += UserPromptNextEventReceived; // イベントを subscribe する
            this.dialogueSystem = dialogueSystem;
            this.textArchitect = textArchitect;
            this.process = null;
        }

        /// <summary>
        /// DisplaySystemController からイベントが発火されたときの Subscribe 処理
        /// 次の処理に進む
        /// </summary>
        private void UserPromptNextEventReceived()
        {
            userPromptNext = true;
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
            }
        }

        private IEnumerator RunningSingleDialogue(DialogueLineData lineData)
        {
            if (lineData.HasSpeaker) dialogueSystem.DisplaySpeakerName(lineData.speaker);

            // TMProGUI が dialogue の表示を開始する（非同期で文字が画面に出力され始める）
            textArchitect.Display(lineData.dialogue);

            while (textArchitect.IsDisplaying)
            {
                if (userPromptNext)
                {
                    if (!textArchitect.HurryUp) textArchitect.HurryUp = true;
                    else textArchitect.ForceComplete();
                    userPromptNext = false;
                }
                yield return null;
            }

            yield return WaitForUserAdvance();
        }

        private IEnumerator RunningSingleCommands(DialogueLineData lineData)
        {
            Debug.Log(lineData.commands);
            yield return null;
        }

        private IEnumerator WaitForUserAdvance()
        {
            while (!userPromptNext) yield return null;
            userPromptNext = false;
        }
    }
}
