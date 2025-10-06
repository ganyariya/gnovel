using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.DisplayDialogue;
using Core.FeaturePanel;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine.Type
{
    public class LL_Choice : ILogicalLine
    {
        public string keyword => "choice";
        private const char ENCAPSULATION_START = '{';
        private const char ENCAPSULATION_END = '}';
        private const char CHOICE_IDENTIFIER = '-';

        private static bool IsEncapsulationStart(string s) => s.Trim().StartsWith(ENCAPSULATION_START);
        private static bool IsEncapsulationEnd(string s) => s.Trim().StartsWith(ENCAPSULATION_END);
        private static bool IsChoiceIdentifier(string s) => s.Trim().StartsWith(CHOICE_IDENTIFIER);

        public bool Match(DialogueLineData lineData)
        {
            return lineData.HasSpeaker && lineData.speakerData.name.ToLower() == keyword;
        }

        public IEnumerator Execute(DialogueLineData lineData)
        {
            var choiceRawData = RipRawData(lineData);
            var choices = ParseChoices(choiceRawData);

            var panel = ChoicePanel.Instance;
            var title = lineData.dialogueData.rawData;
            var choiceTitles = choices.Select(x => x.title).ToArray();
            panel.Show(title, choiceTitles);

            // 選択されるまで待つ
            while (panel.IsEnteringChoice) yield return null;

            // これまで実行していたシナリオについて、選択肢が終わったら先に移動させる
            // ConversationManager が 1 行進めるため、 endIndex のままでいい (+1 しなくていい)
            DialogueSystemController.instance.ConversationManager.CurrentConversation.OverwriteProgress(choiceRawData
                .endIndex);

            // 選択された先のシナリオを実行する
            var selectedChoice = choices[panel.LastDecision.AnswerIndex];
            var newConversation = new Conversation(selectedChoice.lines);
            DialogueSystemController.instance.InterruptEnqueueConversation(newConversation);
        }

        private static ChoiceRawData RipRawData(DialogueLineData lineData)
        {
            var conversation = DialogueSystemController.instance.CurrentConversation;

            var encapsulationDepth = 0;
            List<string> lines = new();

            for (var i = conversation.Progress; i < conversation.Count; i++)
            {
                var line = conversation.GetTargetLine(i);
                lines.Add(line);

                // `choice` 構文が nest されることがある
                // そのため `{` の数を数えて level = 0 のときの領域を fetch する
                if (IsEncapsulationStart(line))
                {
                    encapsulationDepth++;
                    continue;
                }

                if (IsEncapsulationEnd(line))
                {
                    encapsulationDepth--;
                    if (encapsulationDepth == 0)
                    {
                        return new ChoiceRawData { lines = lines, endIndex = i };
                    }
                }
            }

            throw new FormatException($"Choice syntax is invalid. {lineData.rawData}");
        }

        private static List<Choice> ParseChoices(ChoiceRawData rawData)
        {
            var encapsulationDepth = 0;
            List<Choice> choices = new();
            
            var f = new Func<Choice>(() => choices[^1]);
            var t = new Action<string>(s => f().lines.Add(s));

            foreach (var line in rawData.lines)
            {
                if (IsEncapsulationStart(line))
                {
                    encapsulationDepth++;
                    if (encapsulationDepth > 1)
                    {
                        // 2 階層目以降の Choice は選択肢としてあつかわず、そのまま生データ `line` として追加する
                        t(line);
                        continue;
                    }
                }

                if (IsEncapsulationEnd(line))
                {
                    encapsulationDepth--;
                    if (encapsulationDepth > 0)
                    {
                        t(line);
                        continue;
                    }
                }

                // 1 階層目の選択肢領域が始まった
                if (IsChoiceIdentifier(line) && encapsulationDepth == 1)
                {
                    choices.Add(new Choice { title = line.Trim()[1..], lines = new List<string>() });
                    continue;
                }

                if (choices.Count == 0) continue;
                
                t(line);
            }

            return choices;
        }

        /// <summary>
        /// choice "choiceTitle"
        /// {
        ///   -choiceA
        ///     ganyariya "hello"
        ///   -choiceB
        ///     ganyariya "hoge"
        /// }
        ///
        /// 上記フォーマットの選択肢領域を表すデータ構造
        /// </summary>
        private class ChoiceRawData
        {
            public List<string> lines;

            /// <summary>
            /// Conversation 上における選択肢領域末尾 `}` の位置
            /// </summary>
            public int endIndex;
        }

        /// <summary>
        /// - choiceA
        ///   ganyariya "hello"
        /// 
        /// 1 つの選択肢を表す
        /// </summary>
        private class Choice
        {
            public string title;
            public List<string> lines;
        }
    }
}