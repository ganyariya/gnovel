using System;
using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine
{
    public static class Encapsulator
    {
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
        public class EncapsulationData
        {
            public List<string> Lines;
            public int EndIndex;
        }

        private const char ENCAPSULATION_START = '{';
        private const char ENCAPSULATION_END = '}';

        public static bool IsEncapsulationStart(string s) => s.Trim().StartsWith(ENCAPSULATION_START);
        public static bool IsEncapsulationEnd(string s) => s.Trim().StartsWith(ENCAPSULATION_END);

        public static EncapsulationData Encapsulate(Conversation conversation, DialogueLineData lineData)
        {
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
                        return new EncapsulationData { Lines = lines, EndIndex = i };
                    }
                }
            }

            throw new FormatException($"Choice syntax is invalid. {lineData.rawData}");
        }
    }
}