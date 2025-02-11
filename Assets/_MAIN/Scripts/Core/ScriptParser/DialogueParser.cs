using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.ScriptParser
{
    public class DialogueParser
    {
        /// <summary>
        /// コマンドのパターンを見つける
        /// [\w\[\].]+ = ([a-zA-Z0-9_-] or `[` or `]` or `.`) が 1 文字以上
        /// \( = コマンドは `(` で始まるためそれを検知するため)
        /// 
        /// . を含むのは ganyariya.move() のパターンを検知するため
        /// </summary>
        /// 
        /// private static readonly Regex CommandsPattern = new(@"[\w\[\].]+[^\s]\(", RegexOptions.Compiled);
        /// removed: [^\s] = 謎　スペース以外を許容している
        /// 元々は [^\s] もついていたが用途が思い出せないため取り外した
        private static readonly Regex CommandsPattern = new(@"[\w\[\].]+\(", RegexOptions.Compiled);

        public static DialogueLineData Parse(string rawLine)
        {
            var (speaker, dialogue, commands) = SplitRawLine(rawLine);
            Debug.Log($"{rawLine} {speaker} {dialogue} {commands}");
            return new DialogueLineData(speaker, dialogue, commands);
        }

        /// <summary>
        /// 1 行の raw text を構文解析して
        /// - speaker
        /// - dialogue (会話)
        /// - command
        /// に分解する
        /// </summary>
        private static (string, string, string) SplitRawLine(string rawLine)
        {
            string speaker = "", dialogue = "", commands = "";

            /*
            Dialogue の位置を決定する
            */
            int dialogueStart = -1, dialogueEnd = -1;
            bool isEscaped = false;
            for (int i = 0; i < rawLine.Length; i++)
            {
                char c = rawLine[i];
                if (c == '\\') isEscaped = !isEscaped;
                else if (c == '"' && !isEscaped)
                {
                    if (dialogueStart == -1) dialogueStart = i;
                    else if (dialogueEnd == -1) dialogueEnd = i;
                    else break;
                }
                else isEscaped = false;
            }

            /*
            CommandPatterns があるか調べる
            */
            Match rawMatch = CommandsPattern.Match(rawLine);
            Match rightMatch = CommandsPattern.Match(rawLine.Substring(dialogueEnd + 1));
            int commandRawStart = rawMatch.Success ? rawMatch.Index : -1;
            int commandRightStart = rightMatch.Success ? rightMatch.Index : -1;
            // Debug.Log($"{rawLine} {dialogueStart} {dialogueEnd} {commandRawStart} {commandRightStart}");

            // コマンドが確実にある
            if (commandRightStart != -1)
            {
                // ダイアログ + コマンド
                if (dialogueEnd != -1)
                {
                    speaker = rawLine.Substring(0, dialogueStart).Trim();
                    dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                    commands = rawLine.Substring(dialogueEnd + 1).Trim();
                }
                // speakerがあるかも? + "のないコマンドのみ
                else
                {
                    speaker = rawLine.Substring(0, commandRightStart).Trim();
                    commands = rawLine.Substring(commandRightStart).Trim();
                }
            }
            // speakerがあるかも? + ("のあるコマンドのみ or ()を持つダイアログのみ)
            else if (commandRawStart != -1)
            {
                if (commandRawStart < dialogueStart)
                {
                    speaker = rawLine.Substring(0, commandRawStart).Trim();
                    commands = rawLine.Substring(commandRawStart).Trim();
                }
                else
                {
                    speaker = rawLine.Substring(0, dialogueStart).Trim();
                    dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                }
            }
            // ダイアログのみ
            else
            {
                if (dialogueEnd == -1) speaker = rawLine.Trim();
                else
                {
                    speaker = rawLine.Substring(0, dialogueStart).Trim();
                    dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                }
            }

            return (speaker, dialogue, commands);
        }
    }
}
