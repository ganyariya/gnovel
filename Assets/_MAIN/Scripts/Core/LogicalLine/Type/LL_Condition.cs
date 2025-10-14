using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;
using static Core.LogicalLine.LogicalLineUtils;

namespace Core.LogicalLine.Type
{
    public class LL_Condition : ILogicalLine
    {
        public string keyword { get; } = "if";
        private const string elseKeyword = "else";
        private const char PARENTHESIS_OPEN_IDENTIFIER = '(';
        private const char PARENTHESIS_CLOSE_IDENTIFIER = ')';


        public bool Match(DialogueLineData lineData)
        {
            return lineData.rawData.Trim().StartsWith(keyword);
        }

        public IEnumerator Execute(DialogueLineData lineData, DialogueSystemController dialogueSystemController)
        {
            var rawCondition = ExtractCondition(lineData.rawData.Trim());
            var condition = Conditions.EvaluateCondition(rawCondition);

            var currentConversation = dialogueSystemController.CurrentConversation;
            var currentProgress = dialogueSystemController.CurrentConversation.Progress;

            var ifData = Encapsulator.Encapsulate(currentConversation, currentProgress, false);
            var elseData = new Encapsulator.EncapsulationData();

            if (ifData.EndIndex + 1 < currentConversation.Count)
            {
                var nextLine = currentConversation.GetTargetLine(ifData.EndIndex + 1).Trim();
                if (nextLine.StartsWith(elseKeyword))
                {
                    elseData = Encapsulator.Encapsulate(currentConversation, ifData.EndIndex + 1, false);
                    ifData.EndIndex = elseData.EndIndex; // if 側が実行される場合も else 節が終わったところに飛ばす
                }
            }

            currentConversation.OverwriteProgress(ifData.EndIndex);
            var selectedData = condition ? ifData : elseData;
            if (selectedData.Lines.Count > 0) // else が設定されていない場合は Count = 0 で実行されない → else 考慮済み
            {
                var newConversation = new Conversation(selectedData.Lines);
                dialogueSystemController.InterruptEnqueueConversation(newConversation);
            }

            yield return null;
        }

        /// <summary>
        /// if (condition) から `condition` 部分を取り出す
        /// </summary>
        private string ExtractCondition(string line)
        {
            var startIndex = line.IndexOf(PARENTHESIS_OPEN_IDENTIFIER) + 1;
            var endIndex = line.IndexOf(PARENTHESIS_CLOSE_IDENTIFIER);
            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}