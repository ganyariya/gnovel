using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestDLDSpeakerDataParser
{
    class TestData
    {
        public string rawSpeaker;
        public DLD_SpeakerData expectedSpeakerData;

        public TestData(string rawSpeaker, DLD_SpeakerData expectedSpeakerData)
        {
            this.rawSpeaker = rawSpeaker;
            this.expectedSpeakerData = expectedSpeakerData;
        }
    }

    [Test]
    public void TestDLDSpeakerDataParserSimplePasses()
    {
        var testDatas = new List<TestData>
        {
            new(
                "ganyariya",
                new(
                    "ganyariya",
                    "",
                    Vector2.zero,
                    new List<(int layer, string expression)>()
                )
            ),
            new(
                "ganyariya as ???",
                new(
                    "ganyariya",
                    "???",
                    Vector2.zero,
                    new List<(int layer, string expression)>()
                )
            ),
            new(
                "ganyariya as ??? at 1:0.8",
                new(
                    "ganyariya",
                    "???",
                    new Vector2(1, 0.8f),
                    new List<(int layer, string expression)>()
                )
            ),
            new(
                "ganyariya [0:Happy,1:Sad] as ??? at 1:0.8",
                new(
                    "ganyariya",
                    "???",
                    new Vector2(1, 0.8f),
                    new List<(int layer, string expression)>{(0, "Happy"), (1, "Sad"),}
                )
            ),
        };

        foreach (var data in testDatas)
        {
            var speakerData = new DLD_SpeakerData(data.rawSpeaker);
            DialogueParserChecker.CheckDLDSpakerEquals(speakerData, data.expectedSpeakerData);
        }
    }
}
