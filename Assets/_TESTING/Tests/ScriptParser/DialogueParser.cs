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
                "ganyariya \"Hello, World!\" playMusic() drawImage(10 40)",
                "ganyariya",
                "Hello, World!",
                "playMusic() drawImage(10 40)"
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
                "ganyariya \"youkoso()\" SetCli(\"10\", 20)",
                "ganyariya",
                "youkoso()",
                "SetCli(\"10\", 20)"
            ),
            new(
                "cli()",
                "",
                "",
                "cli()"
            ),
            new(
                "fake cli()",
                "fake",
                "",
                "cli()"
            ),
            new(
                "fake cli()cli()",
                "fake",
                "",
                "cli()cli()"
            ),
            new(
                "fake cli()cli(\"\")",
                "fake",
                "",
                "cli()cli(\"\")"
            ),
            new(
                "fake \"hello\"",
                "fake",
                "hello",
                ""
            ),
            new(
                "fake \"hello()\" cli(\"a\")",
                "fake",
                "hello()",
                "cli(\"a\")"
            ),
            new(
                "",
                "",
                "",
                ""
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
