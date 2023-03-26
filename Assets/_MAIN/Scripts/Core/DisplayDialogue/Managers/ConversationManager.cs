using System;
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

                // 生 string をパースして DialogueLineData に変換する
                DialogueLineData lineData = DialogueParser.Parse(rawText);

                if (lineData.HasDialogue) yield return RunningSingleDialogue(lineData);
                if (lineData.HasCommands) yield return RunningSingleCommands(lineData);
            }
        }

        /// <summary>
        /// ある 1 つの DialogueLineData.dialogueLine をもとに 画面にテキストを出力する
        /// 内部で dialogueLine.Segments を呼び出す
        /// </summary>
        private IEnumerator RunningSingleDialogue(DialogueLineData lineData)
        {
            if (lineData.HasSpeaker) dialogueSystem.DisplaySpeakerName(lineData.speakerData.DisplayName);

            foreach (var segment in lineData.dialogueData.segments)
            {
                yield return RunningSingleDLDDialogueSegment(segment);
            }

            yield return WaitForUserAdvance();
        }

        private IEnumerator RunningSingleDLDDialogueSegment(DLD_DialogueSegment segment)
        {
            yield return WaitForDialogueSegmentTriggered(segment);
            yield return DisplayingSingleDialogueText(segment.dialogue, segment.IsAppendText);
        }

        private IEnumerator WaitForDialogueSegmentTriggered(DLD_DialogueSegment segment)
        {
            switch (segment.startSignal)
            {
                case StartSignal.C:
                case StartSignal.A:
                    yield return WaitForUserAdvance();
                    break;
                case StartSignal.WA:
                case StartSignal.WC:
                    yield return new WaitForSeconds(segment.signalDelay);
                    break;
                default:
                    break;
            }
            yield return null;
        }

        /// <summary>
        /// 画面に非同期に 1 行の生stringテキストを表示する
        /// 表示中に userPrompt されたら加速させる
        /// </summary>
        private IEnumerator DisplayingSingleDialogueText(string dialogueText, bool append = true)
        {
            // TMProGUI が dialogue の表示を開始する（非同期で文字が画面に出力され始める）
            if (append) textArchitect.AppendDisplay(dialogueText);
            else textArchitect.Display(dialogueText);

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
        }

        private IEnumerator RunningSingleCommands(DialogueLineData lineData)
        {
            Debug.Log(lineData.commandData);
            yield return null;
        }

        private IEnumerator WaitForUserAdvance()
        {
            while (!userPromptNext) yield return null;
            userPromptNext = false;
        }
    }
}
