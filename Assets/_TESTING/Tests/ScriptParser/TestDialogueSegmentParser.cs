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
        public DLD_DialogueData expectedDialogueLine;

        public TestData(string rawDialogue, DLD_DialogueData dialogueLine)
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
                new(new List<DLD_DialogueSegment>{
                    new DLD_DialogueSegment("hello, world", StartSignal.NONE, 0)
                })
            ),
            new(
                "hello, world{a}Second",
                new(new List<DLD_DialogueSegment>{
                    new DLD_DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DLD_DialogueSegment("Second", StartSignal.A, 0)
                })
            ),
            new(
                "hello, world{a}Second{c}Third",
                new(new List<DLD_DialogueSegment>{
                    new DLD_DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DLD_DialogueSegment("Second", StartSignal.A, 0),
                    new DLD_DialogueSegment("Third", StartSignal.C, 0)
                })
            ),
            new(
                "hello, world{a}Second{wa 3}Third",
                new(new List<DLD_DialogueSegment>{
                    new DLD_DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DLD_DialogueSegment("Second", StartSignal.A, 0),
                    new DLD_DialogueSegment("Third", StartSignal.WA, 3)
                })
            ),
            new(
                "hello, world{a}Second{wc 3.2}Third",
                new(new List<DLD_DialogueSegment>{
                    new DLD_DialogueSegment("hello, world", StartSignal.NONE, 0),
                    new DLD_DialogueSegment("Second", StartSignal.A, 0),
                    new DLD_DialogueSegment("Third", StartSignal.WC, 3.2f)
                })
            ),
        };

        foreach (var data in testDatas)
        {
            var dialogueLine = new DLD_DialogueData(data.rawDialogue);

            DialogueParserChecker.CheckDialogueLineEquals(
                dialogueLine, data.expectedDialogueLine
            );
        }
    }
}
