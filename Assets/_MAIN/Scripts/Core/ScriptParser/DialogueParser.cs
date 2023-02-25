using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.ScriptParser
{
    public class DialogueParser
    {
        private static readonly Regex CommandsPattern = new Regex("\\w+[^\\s]\\(", RegexOptions.Compiled);

        public static DialogueLineData Parse(string rawLine)
        {
            var (speaker, dialogue, commands) = SplitRawLine(rawLine);
            return new DialogueLineData(speaker, dialogue, commands);
        }

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
            Match match = CommandsPattern.Match(rawLine);
            int commandStart = match.Success ? match.Index : -1;

            // ダイアログ("") がある + コマンドがない
            // ダイアログ("") がある + コマンドもある
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1) commands = rawLine.Substring(commandStart).Trim();
            }
            // コマンドしかない
            else if (commandStart != -1 && dialogueStart > commandStart)
            {
                commands = rawLine;
            }
            // Speaker しかない
            else
            {
                speaker = rawLine;
            }

            return (speaker, dialogue, commands);
        }
    }
}
