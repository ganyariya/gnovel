using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.ScriptParser
{
    /// <summary>
    /// W は WaitSeconds
    /// A は Append
    /// C は Clear
    /// </summary>
    public enum StartSignal { NONE, C, A, WA, WC };

    public class DLD_DialogueSegment
    {
        /// <summary>
        /// すでに DialogueParser によって スクリプトファイルがパースされており
        /// Segment としてこのクラスで受け取り、そのまま画面に displayArchitect で出力する
        /// </summary>
        public string dialogue;
        public StartSignal startSignal;
        public float signalDelay;

        public bool IsAppendText => startSignal == StartSignal.A || startSignal == StartSignal.WA;

        public DLD_DialogueSegment() { }
        public DLD_DialogueSegment(string dialogue, StartSignal startSignal, float signalDelay)
        {
            this.dialogue = dialogue;
            this.startSignal = startSignal;
            this.signalDelay = signalDelay;
        }
    }

    /// <summary>
    /// DialogueLineData の dialogue をさらに segment に分割したもの
    /// Wait
    /// </summary>
    public class DLD_DialogueData
    {
        public List<DLD_DialogueSegment> segments;
        private readonly static Regex segmentIdentifierPattern = new Regex(@"\{[ca]\}|\{w[ca]\s+\d*\.?\d*\}");

        public DLD_DialogueData(string rawDialogue)
        {
            segments = SplitSegments(rawDialogue);
        }

        public DLD_DialogueData(List<DLD_DialogueSegment> segments)
        {
            this.segments = segments;
        }

        public bool HasDialogue => segments.Count > 0;

        public static List<DLD_DialogueSegment> SplitSegments(string rawDialogue)
        {
            var segments = new List<DLD_DialogueSegment>();
            MatchCollection matches = segmentIdentifierPattern.Matches(rawDialogue);
            int lastIndex = 0;

            // Find First Segment
            var segment = new DLD_DialogueSegment();
            segment.dialogue = matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index);
            segment.startSignal = StartSignal.NONE;
            segment.signalDelay = 0;
            segments.Add(segment);

            if (matches.Count == 0) return segments;
            else lastIndex = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                segment = new DLD_DialogueSegment();

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

