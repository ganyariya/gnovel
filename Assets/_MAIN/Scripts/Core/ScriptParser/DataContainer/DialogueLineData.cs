using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ScriptParser
{
    /// <summary>
    /// Script テキスト 1 行を構文解析して得られたデータ
    /// </summary>
    public class DialogueLineData
    {

        public readonly string originalSpeaker;
        public readonly string originalDialogue;
        public readonly string originalCommands;

        public DLD_SpeakerData speakerData;
        public DLD_DialogueData dialogueData;
        public DLD_CommandData commandData;

        public bool HasSpeaker => speakerData != null;
        public bool HasDialogue => dialogueData != null;
        public bool HasCommands => commandData != null;

        public DialogueLineData(string speaker, string dialogue, string commands)
        {
            this.originalSpeaker = speaker;
            this.originalDialogue = dialogue;
            this.originalCommands = commands;

            this.speakerData = string.IsNullOrWhiteSpace(speaker) ? null : new DLD_SpeakerData(speaker);
            this.dialogueData = string.IsNullOrWhiteSpace(dialogue) ? null : new DLD_DialogueData(dialogue);
            this.commandData = string.IsNullOrWhiteSpace(commands) ? null : new DLD_CommandData(commands);

            // this.speakerData = new DLD_SpeakerData(speaker);
            // this.dialogueData = new DLD_DialogueData(dialogue);
            // this.commandData = new DLD_CommandData(commands);
        }
    }
}
