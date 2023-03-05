using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestDialogueSegmentParser
{

    class TestData
    {
        public string rawDialogue;
        public DialogueLine expectedDialogueLine;

        public TestData(string rawDialogue, DialogueLine dialogueLine)
        {
            this.rawDialogue = rawDialogue;
            this.expectedDialogueLine = dialogueLine;
        }
    }

    [Test]
    public void TestDialogueSegmentParserSimplePasses()
    {
        var testDatas = new List<TestData>
        {
            new(
                "hello, world",
                new(new List<DialogueSegment>{
                    new DialogueSegment("hello, world", StartSignal.NONE, 0)
                })
            ),
            new(
                "hello, world{a}Second",
                new(new List<DialogueSegment>{
                    new DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DialogueSegment("Second", StartSignal.A, 0)
                })
            ),
            new(
                "hello, world{a}Second{c}Third",
                new(new List<DialogueSegment>{
                    new DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DialogueSegment("Second", StartSignal.A, 0),
                    new DialogueSegment("Third", StartSignal.C, 0)
                })
            ),
            new(
                "hello, world{a}Second{wa 3}Third",
                new(new List<DialogueSegment>{
                    new DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DialogueSegment("Second", StartSignal.A, 0),
                    new DialogueSegment("Third", StartSignal.WA, 3)
                })
            ),
            new(
                "hello, world{a}Second{wc 3.2}Third",
                new(new List<DialogueSegment>{
                    new DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DialogueSegment("Second", StartSignal.A, 0),
                    new DialogueSegment("Third", StartSignal.WC, 3.2f)
                })
            ),
        };

        foreach (var data in testDatas)
        {
            var dialogueLine = new DialogueLine(data.rawDialogue);

            DialogueParserChecker.CheckDialogueLineEquals(
                dialogueLine, data.expectedDialogueLine
            );
        }
    }
}
