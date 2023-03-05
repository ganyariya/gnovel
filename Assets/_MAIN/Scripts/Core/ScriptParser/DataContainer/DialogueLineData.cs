using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ScriptParser
{
    public class DialogueLineData
    {

        public readonly string originalSpeaker;
        public readonly string originalDialogue;
        public readonly string originalCommands;

        public string speaker;
        public DialogueLine dialogueLine;
        public string commands;

        public bool HasSpeaker => speaker != string.Empty;
        public bool HasDialogue => dialogueLine.HasDialogue;
        public bool HasCommands => commands != string.Empty;

        public DialogueLineData(string speaker, string dialogue, string commands)
        {
            this.originalSpeaker = speaker;
            this.originalDialogue = dialogue;
            this.originalCommands = commands;

            this.speaker = speaker;
            this.dialogueLine = new DialogueLine(dialogue);
            this.commands = commands;
        }
    }
}
