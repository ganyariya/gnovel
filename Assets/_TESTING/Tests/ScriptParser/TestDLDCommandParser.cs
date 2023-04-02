using System.Collections;
using System.Collections.Generic;
using Core.ScriptParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestDLDCommand
{

    class TestData
    {
        public string rawCommands;
        public DLD_CommandData expectedCommandData;

        public TestData(string rawCommands, DLD_CommandData expectedCommandData)
        {
            this.rawCommands = rawCommands;
            this.expectedCommandData = expectedCommandData;
        }
    }

    [Test]
    public void TestDLDCommandParser()
    {
        var testDatas = new List<TestData>() {
            new(
                "setMode(normal)",
                new(
                    new List<Command>{ new("setMode", new []{"normal"}, false) }
                )
            ),
            new(
                "[wait]setMode(normal)",
                new(
                    new List<Command>{ new("setMode", new []{"normal"}, true) }
                )
            ),
            new(
                "setMode(normal -v 10)",
                new(
                    new List<Command>{ new("setMode", new []{"normal", "-v", "10"}, false) }
                )
            ),
            new(
                "setMode(\"high power\" -v 10)",
                new(
                    new List<Command>{ new("setMode", new []{"high power", "-v", "10"}, false) }
                )
            ),
            new(
                "setCli(Elen 0:Angle1 1:High -s)",
                new(
                    new List<Command>{ new("setCli", new []{"Elen", "0:Angle1", "1:High", "-s"}, false) }
                )
            ),
            new(
                "setMode(normal),playMusic(\"Dog Land\" -p 10)",
                new(
                    new List<Command>{
                        new("setMode", new []{"normal"}, false),
                        new("playMusic", new []{"Dog Land", "-p", "10"}, false),
                    }
                )
            ),
        };

        foreach (var data in testDatas)
        {
            var commandData = new DLD_CommandData(data.rawCommands);
            DialogueParserChecker.CheckDLDCommandEquals(commandData, data.expectedCommandData);
        }
    }

}
