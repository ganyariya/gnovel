using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DialogueParserChecker
{
    public static void CheckDialogueLineEquals(DLD_DialogueData actualLine, DLD_DialogueData expectedLine)
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

    public static void CheckDLDSpeakerEquals(DLD_SpeakerData actualSpeakerData, DLD_SpeakerData expectedSpeakerData)
    {
        Assert.That(actualSpeakerData.name, Is.EqualTo(expectedSpeakerData.name));
        Assert.That(actualSpeakerData.castName, Is.EqualTo(expectedSpeakerData.castName));
        Assert.That(actualSpeakerData.castPosition.x, Is.EqualTo(expectedSpeakerData.castPosition.x));
        Assert.That(actualSpeakerData.castPosition.y, Is.EqualTo(expectedSpeakerData.castPosition.y));

        for (int i = 0; i < expectedSpeakerData.CastExpressions.Count; i++)
        {
            var a = actualSpeakerData.CastExpressions[i];
            var e = expectedSpeakerData.CastExpressions[i];
            Assert.That(a.layer, Is.EqualTo(e.layer));
            Assert.That(a.expression, Is.EqualTo(e.expression));
        }
    }

    public static void CheckDLDCommandEquals(DLD_CommandData actualCommandData, DLD_CommandData expectedCommandData)
    {
        Assert.That(actualCommandData.commands.Count, Is.EqualTo(expectedCommandData.commands.Count));

        for (int i = 0; i < expectedCommandData.commands.Count; i++)
        {
            var ac = actualCommandData.commands[i];
            var ec = expectedCommandData.commands[i];

            Assert.That(ac.name, Is.EqualTo(ec.name));
            Assert.That(ac.waitForCompletion, Is.EqualTo(ec.waitForCompletion));
            Assert.That(ac.arguments.Length, Is.EqualTo(ec.arguments.Length));

            for (int j = 0; j < ec.arguments.Length; j++)
            {
                Assert.That(ac.arguments[j], Is.EqualTo(ec.arguments[j]));
            }
        }
    }
}
