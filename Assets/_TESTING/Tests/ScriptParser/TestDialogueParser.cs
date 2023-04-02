using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestDialogueParser
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
    public void TestDialogueParse()
    {
        var testDatas = new List<TestData> {
            new(
                "ganyariya \"こんにちは。\\\"人生\\\"を過ごしています。\" SetCli(\"10\" 20)",
                "ganyariya",
                "こんにちは。\"人生\"を過ごしています。",
                "SetCli(\"10\" 20)"
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
                "SetCli(\"10\" 20)",
                "",
                "",
                "SetCli(\"10\" 20)"
            ),
            new(
                "ganyariya \"youkoso()\" SetCli(\"10\" 20)",
                "ganyariya",
                "youkoso()",
                "SetCli(\"10\" 20)"
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
            new(
                "hide",
                "hide",
                "",
                ""
            ),
            new(
                "\"並行してBackend(go/gcp)もがんばるぞ\"",
                "",
                "並行してBackend(go/gcp)もがんばるぞ",
                ""
            ),
            new(
                "speaker \"hello\"",
                "speaker",
                "hello",
                ""
            ),
            new(
                "speaker \"hello()\"",
                "speaker",
                "hello()",
                ""
            ),
            new(
                "speaker \"hello()\" cli()",
                "speaker",
                "hello()",
                "cli()"
            ),
            new(
                "speaker \"hello()\" cli(\"a\")",
                "speaker",
                "hello()",
                "cli(\"a\")"
            ),
            new(
                "speaker cli(\"a\")",
                "speaker",
                "",
                "cli(\"a\")"
            ),
            new(
                "speaker cli(a)",
                "speaker",
                "",
                "cli(a)"
            ),
        };

        foreach (var data in testDatas)
        {
            var lineData = DialogueParser.Parse(data.rawLine);
            Assert.That(lineData.originalSpeaker, Is.EqualTo(data.expectedSpeaker));
            Assert.That(lineData.originalDialogue, Is.EqualTo(data.expectedDialogue));
            Assert.That(lineData.originalCommands, Is.EqualTo(data.expectedCommands));
        }
    }
}
