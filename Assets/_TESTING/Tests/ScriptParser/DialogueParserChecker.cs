using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DialogueParserChecker
{
    public static void CheckDialogueLineEquals(DLD_DialogueLine actualLine, DLD_DialogueLine expectedLine)
    {
        for (int i = 0; i < actualLine.segments.Count; i++)
        {
            CheckDialogueSegmentEquals(actualLine.segments[i], expectedLine.segments[i]);
        }
    }

    public static void CheckDialogueSegmentEquals(DLD_DialogueSegment actualSegment, DLD_DialogueSegment expectedSegment)
    {
        Assert.That(actualSegment.dialogue, Is.EqualTo(expectedSegment.dialogue));
        Assert.That(actualSegment.startSignal, Is.EqualTo(expectedSegment.startSignal));
        Assert.That(actualSegment.signalDelay, Is.EqualTo(expectedSegment.signalDelay));
    }
}
