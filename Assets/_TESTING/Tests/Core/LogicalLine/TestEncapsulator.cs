using System;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.LogicalLine;
using NUnit.Framework;

namespace Tests.Core.LogicalLine
{
    public class TestEncapsulator
    {
        private Conversation MakeConversation(IEnumerable<string> lines)
        {
            return new Conversation(new List<string>(lines));
        }

        [Test]
        public void IsEncapsulationStartEnd_Works_With_Whitespace()
        {
            Assert.That(Encapsulator.IsEncapsulationStart("  {"), Is.True);
            Assert.That(Encapsulator.IsEncapsulationEnd("\t}\t"), Is.True);
            Assert.That(Encapsulator.IsEncapsulationStart("not brace"), Is.False);
            Assert.That(Encapsulator.IsEncapsulationEnd("not brace"), Is.False);
        }

        [Test]
        public void Encapsulate_WithHeader_SimpleBlock_Includes_Title_And_Braces()
        {
            var lines = new List<string>
            {
                "choice \"A\"",
                "{",
                "  -A",
                "    speaker \"hello\"",
                "}"
            };
            var conv = MakeConversation(lines);

            var data = Encapsulator.Encapsulate(conv, 0, true);

            Assert.That(data.StartIndex, Is.EqualTo(0));
            Assert.That(data.EndIndex, Is.EqualTo(4));
            // With header, every line from startIndex to EndIndex should be included
            Assert.That(data.Lines, Is.EqualTo(lines));
        }

        [Test]
        public void Encapsulate_WithHeader_NestedBlock_Returns_Full_Block()
        {
            var lines = new List<string>
            {
                "choice \"Which pet do you like?\"",
                "{",
                "  -Dog",
                "    \"So, you like dogs. {wa 1.0} I'm not a big fan because they bark.\"",
                "  -Cat",
                "    \"So, you like cats.\"",
                "    choice \"Why do you like cats?\"",
                "    {",
                "      -Cute",
                "        \"That's right, cats are very cute.\"",
                "      -Lie",
                "        \"That's mean. Do you actually dislike cats? {c} I thought we were the same...\"",
                "    }",
                "    \"cat question is end!\"",
                "  -Rabbit",
                "    \"Ah~ My heart goes *pyon pyon*~\"",
                "}"
            };
            var conv = MakeConversation(lines);

            var data = Encapsulator.Encapsulate(conv, 0, true);

            Assert.That(data.StartIndex, Is.EqualTo(0));
            Assert.That(data.EndIndex, Is.EqualTo(lines.Count - 1));
            Assert.That(data.Lines, Is.EqualTo(lines));
        }

        [Test]
        public void Encapsulate_WithoutHeader_Excludes_Outer_Braces()
        {
            var lines = new List<string>
            {
                "choice \"A\"",
                "{",
                "  -A",
                "    speaker \"hello\"",
                "}"
            };
            var conv = MakeConversation(lines);

            var data = Encapsulator.Encapsulate(conv, 1, false); // start at the opening brace

            Assert.That(data.StartIndex, Is.EqualTo(1));
            Assert.That(data.EndIndex, Is.EqualTo(4));
            // Without header, content excludes the outer braces lines
            CollectionAssert.AreEqual(new[]
            {
                "  -A",
                "    speaker \"hello\""
            }, data.Lines);
        }

        [Test]
        public void Encapsulate_Throws_When_No_Closing_Brace()
        {
            var lines = new List<string>
            {
                "{",
                "  -A",
                "    speaker \"hello\""
                // missing closing '}'
            };
            var conv = MakeConversation(lines);

            Assert.Throws<FormatException>(() => Encapsulator.Encapsulate(conv, 0, false));
        }
    }
}
