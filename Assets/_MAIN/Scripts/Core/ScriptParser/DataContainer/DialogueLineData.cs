using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ScriptParser
{
    public class DialogueLineData
    {
        public string speaker;
        public string dialogue;
        public string commands;

        public bool HasSpeaker => speaker != string.Empty;
        public bool HasDialogue => dialogue != string.Empty;
        public bool HasCommands => commands != string.Empty;

        public DialogueLineData(string speaker, string dialogue, string commands)
        {
            this.speaker = speaker;
            this.dialogue = dialogue;
            this.commands = commands;
        }
    }
}
