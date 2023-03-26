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

        public DLD_SpeakerData speaker;
        public DLD_DialogueLine dialogueLine;
        public DLD_CommandData commands;

        public bool HasSpeaker => speaker.HasSpeaker;
        public bool HasDialogue => dialogueLine.HasDialogue;
        public bool HasCommands => commands.HasCommands;

        public DialogueLineData(string speaker, string dialogue, string commands)
        {
            this.originalSpeaker = speaker;
            this.originalDialogue = dialogue;
            this.originalCommands = commands;

            this.speaker = new DLD_SpeakerData(speaker);
            this.dialogueLine = new DLD_DialogueLine(dialogue);
            this.commands = new DLD_CommandData(commands);
        }
    }
}
