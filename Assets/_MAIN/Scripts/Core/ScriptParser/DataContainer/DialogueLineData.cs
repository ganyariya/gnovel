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

        public bool HasSpeaker => speakerData.HasSpeaker;
        public bool HasDialogue => dialogueData.HasDialogue;
        public bool HasCommands => commandData.HasCommands;

        public DialogueLineData(string speaker, string dialogue, string commands)
        {
            this.originalSpeaker = speaker;
            this.originalDialogue = dialogue;
            this.originalCommands = commands;

            this.speakerData = new DLD_SpeakerData(speaker);
            this.dialogueData = new DLD_DialogueData(dialogue);
            this.commandData = new DLD_CommandData(commands);
        }
    }
}
