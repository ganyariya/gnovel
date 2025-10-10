using System;
using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine
{
    public static class LogicalLineUtils
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
                public int StartIndex;
                public int EndIndex;
            }

            private const char ENCAPSULATION_START = '{';
            private const char ENCAPSULATION_END = '}';

            public static bool IsEncapsulationStart(string s) => s.Trim().StartsWith(ENCAPSULATION_START);
            public static bool IsEncapsulationEnd(string s) => s.Trim().StartsWith(ENCAPSULATION_END);

            /// <param name="useHeaderEncapsulation">
            /// true であれば choice "title" {} という Header(title) と {}(encapsulation) も含める
            /// false の場合は title 行と {} を取り除く
            ///   ネストされている choice {} は取り出したいので encapsulationDepth をチェックする
            /// </param>
            public static EncapsulationData Encapsulate(Conversation conversation, int startIndex,
                bool useHeaderEncapsulation = true)
            {
                var exceptionMsg =
                    $"Choice syntax is invalid. {conversation.Progress}/{conversation.Count} {conversation.CurrentLine}";

                var encapsulationDepth = 0;
                List<string> lines = new();

                for (var i = startIndex; i < conversation.Count; i++)
                {
                    var line = conversation.GetTargetLine(i);

                    if (
                        useHeaderEncapsulation // use であれば必ずすべての行を追加
                        || encapsulationDepth > 0 && !IsEncapsulationEnd(line) // ネストされている LogicalLine は lines に含める
                    ) lines.Add(line);


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
                            return new EncapsulationData { Lines = lines, StartIndex = startIndex, EndIndex = i };
                        }
                    }
                }

                throw new FormatException(exceptionMsg);
            }
        }
    }
}