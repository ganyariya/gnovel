using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine
{
    public interface ILogicalLine
    {
        public string keyword { get; }

        bool Match(DialogueLineData lineData);
        IEnumerator Execute(DialogueLineData lineData, DialogueSystemController dialogueSystemController);
    }
}