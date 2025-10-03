using System.Collections;
using System.Collections.Generic;
using Core.FeaturePanel;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine.Type
{
    public class LL_Input : ILogicalLine
    {
        public string keyword => "input";

        public bool Match(DialogueLineData lineData)
        {
            return lineData.HasSpeaker && lineData.speakerData.name.ToLower() == keyword;
        }

        public IEnumerator Execute(DialogueLineData lineData)
        {
            // singleton から取得する
            var inputPanel = InputPanel.Instance;
            inputPanel.Show(lineData.dialogueData.rawData);

            while (inputPanel.IsEnteringInput) yield return null;
        }
    }
}