using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestScriptParser
{

    class TestData
    {
        public string rawLine;
        public string expectedSpeaker;
        public string expectedDialogue;
        public string expectedCommands;

        public TestData(string rawLine, string expectedSpeaker, string expectedDialogue, string expectedCommands)
        {
            this.rawLine = rawLine;
            this.expectedSpeaker = expectedSpeaker;
            this.expectedDialogue = expectedDialogue;
            this.expectedCommands = expectedCommands;
        }
    }

    [Test]
    public void TestDialogueParser()
    {
        var testDatas = new List<TestData> {
            new(
                "ganyariya \"こんにちは。\\\"人生\\\"を過ごしています。\" SetCli(\"10\", 20)",
                "ganyariya",
                "こんにちは。\"人生\"を過ごしています。",
                "SetCli(\"10\", 20)"
            ),
            new(
                "ganyariya",
                "ganyariya",
                "",
                ""
            ),
            new(
                "SetCli(\"10\", 20)",
                "",
                "",
                "SetCli(\"10\", 20)"
            ),
            new(
                "ganyariya \"edgeCase()\" cli2(\"10\")",
                "ganyariya",
                "edgeCase()",
                "cli2(\"10\")"
            ),
        };

        foreach (var data in testDatas)
        {
            var lineData = DialogueParser.Parse(data.rawLine);
            Assert.That(lineData.speaker, Is.EqualTo(data.expectedSpeaker));
            Assert.That(lineData.dialogue, Is.EqualTo(data.expectedDialogue));
            Assert.That(lineData.commands, Is.EqualTo(data.expectedCommands));
        }

        string rawLine = "ganyariya \"こんにちは。\\\"人生\\\"を過ごしています。\" SetCli(\"10\", 20)";
        DialogueParser.Parse(rawLine);
    }
}
