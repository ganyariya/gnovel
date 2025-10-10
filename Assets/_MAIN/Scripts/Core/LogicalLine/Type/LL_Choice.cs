using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.DisplayDialogue;
using Core.FeaturePanel;
using Core.ScriptParser;
using UnityEngine;
using static Core.LogicalLine.LogicalLineUtils;

namespace Core.LogicalLine.Type
{
    public class LL_Choice : ILogicalLine
    {
        public string keyword => "choice";
        private const char CHOICE_IDENTIFIER = '-';

        private static bool IsChoiceIdentifier(string s) => s.Trim().StartsWith(CHOICE_IDENTIFIER);

        public bool Match(DialogueLineData lineData)
        {
            return lineData.HasSpeaker && lineData.speakerData.name.ToLower() == keyword;
        }

        public IEnumerator Execute(DialogueLineData lineData, DialogueSystemController dialogueSystemController)
        {
            var conversation = dialogueSystemController.CurrentConversation;
            var encapsulationData = Encapsulator.Encapsulate(conversation, conversation.Progress, true);
            var choices = ParseChoices(encapsulationData);

            var panel = ChoicePanel.Instance;
            var title = lineData.dialogueData.rawData;
            var choiceTitles = choices.Select(x => x.Title).ToArray();
            panel.Show(title, choiceTitles);

            // 選択されるまで待つ
            while (panel.IsEnteringChoice) yield return null;

            // これまで実行していたシナリオについて、選択肢が終わったら先に移動させる
            // ConversationManager が 1 行進めるため、 endIndex のままでいい (+1 しなくていい)
            dialogueSystemController.ConversationManager.CurrentConversation.OverwriteProgress(encapsulationData
                .EndIndex);

            // 選択された先のシナリオを実行する
            var selectedChoice = choices[panel.LastDecision.AnswerIndex];
            var newConversation = new Conversation(selectedChoice.Lines);
            dialogueSystemController.InterruptEnqueueConversation(newConversation);
        }

        private static List<Choice> ParseChoices(Encapsulator.EncapsulationData rawData)
        {
            var encapsulationDepth = 0;
            List<Choice> choices = new();

            var f = new Func<Choice>(() => choices[^1]);
            var t = new Action<string>(s => f().Lines.Add(s));

            foreach (var line in rawData.Lines)
            {
                if (Encapsulator.IsEncapsulationStart(line))
                {
                    encapsulationDepth++;
                    // 2 階層目以降の Choice は選択肢としてあつかわず、そのまま生データ `line` として追加する
                    if (encapsulationDepth > 1) t(line);
                    continue;
                }

                if (Encapsulator.IsEncapsulationEnd(line))
                {
                    encapsulationDepth--;
                    if (encapsulationDepth > 0) t(line);
                    continue;
                }

                // 1 階層目の選択肢領域が始まった
                if (IsChoiceIdentifier(line) && encapsulationDepth == 1)
                {
                    choices.Add(new Choice { Title = line.Trim()[1..], Lines = new List<string>() });
                    continue;
                }

                if (choices.Count == 0) continue;

                t(line);
            }

            return choices;
        }

        /// <summary>
        /// - choiceA
        ///   ganyariya "hello"
        /// 
        /// 1 つの選択肢を表す
        /// </summary>
        private class Choice
        {
            public string Title;
            public List<string> Lines;
        }
    }
}