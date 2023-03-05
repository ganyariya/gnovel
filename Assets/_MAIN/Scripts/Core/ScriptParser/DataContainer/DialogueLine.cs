using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.ScriptParser
{
    public enum StartSignal { NONE, C, A, WA, WC };

    public class DialogueSegment
    {
        public string dialogue;
        public StartSignal startSignal;
        public float signalDelay;

        public bool IsAppendText => startSignal == StartSignal.A || startSignal == StartSignal.WA;

        public DialogueSegment() { }
        public DialogueSegment(string dialogue, StartSignal startSignal, float signalDelay)
        {
            this.dialogue = dialogue;
            this.startSignal = startSignal;
            this.signalDelay = signalDelay;
        }
    }

    public class DialogueLine
    {
        public List<DialogueSegment> segments;
        private readonly static Regex segmentIdentifierPattern = new Regex(@"\{[ca]\}|\{w[ca]\s+\d*\.?\d*\}");

        public DialogueLine(string rawDialogue)
        {
            segments = SplitSegments(rawDialogue);
        }

        public DialogueLine(List<DialogueSegment> segments)
        {
            this.segments = segments;
        }

        public bool HasDialogue => segments.Count > 0;

        public static List<DialogueSegment> SplitSegments(string rawDialogue)
        {
            var segments = new List<DialogueSegment>();
            MatchCollection matches = segmentIdentifierPattern.Matches(rawDialogue);
            int lastIndex = 0;

            // Find First Segment
            var segment = new DialogueSegment();
            segment.dialogue = matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index);
            segment.startSignal = StartSignal.NONE;
            segment.signalDelay = 0;
            segments.Add(segment);

            if (matches.Count == 0) return segments;
            else lastIndex = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                segment = new DialogueSegment();

                string signalMatch = match.Value;
                signalMatch = signalMatch.Substring(1, match.Length - 2);
                string[] signalSplit = signalMatch.Split(' ');
                segment.startSignal = (StartSignal)Enum.Parse(typeof(StartSignal), signalSplit[0].ToUpper());

                if (signalSplit.Length > 1) float.TryParse(signalSplit[1], out segment.signalDelay);

                int nextIndex = i + 1 < matches.Count ? matches[i + 1].Index : rawDialogue.Length;
                segment.dialogue = rawDialogue.Substring(lastIndex + match.Length, nextIndex - (lastIndex + match.Length));
                lastIndex = nextIndex;

                segments.Add(segment);
            }

            return segments;
        }
    }
}

