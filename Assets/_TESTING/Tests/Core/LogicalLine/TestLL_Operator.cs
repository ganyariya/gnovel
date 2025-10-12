using System.Collections;
using Core.DisplayDialogue;
using Core.LogicalLine;
using Core.LogicalLine.Type;
using Core.ScriptParser;
using NUnit.Framework;

namespace Tests.Core.LogicalLine
{
    public class TestLL_Operator
    {
        private LL_Operator _operatorLine;

        [SetUp]
        public void SetUp()
        {
            _operatorLine = new LL_Operator();
            VariableStore.Instance.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            VariableStore.Instance.Clear();
        }

        [TestCase("$value = 10", true)]
        [TestCase("$value += 10", true)]
        [TestCase("$value -= 10", true)]
        [TestCase("$value *= 10", true)]
        [TestCase("$value /= 10", true)]
        [TestCase("value = 10", false)] // No dollar sign
        [TestCase("$ value = 10", false)] // Space after dollar sign
        [TestCase("speaker \"hello\"", false)]
        public void Match_Returns_Correctly(string rawData, bool expected)
        {
            var lineData = new DialogueLineData(rawData, "", "", "");
            Assert.That(_operatorLine.Match(lineData), Is.EqualTo(expected));
        }

        [TestCase(new []{"$value = 10"}, "value", 10.0)]
        [TestCase(new []{"$value = 10 + 5"}, "value", 15.0)]
        [TestCase(new []{"$value = 10 - 5"}, "value", 5.0)]
        [TestCase(new []{"$value = 10 * 5"}, "value", 50.0)]
        [TestCase(new []{"$value = 10 / 5"}, "value", 2.0)]
        [TestCase(new []{"$x = 10", "$value = $x + 5"}, "value", 15.0)]
        [TestCase(new []{"$x = 10", "$y = 20", "$value = $x + $y"}, "value", 30.0)]
        [TestCase(new []{"$answerText = \"initial\"", "$answerText = \"$answerText is $answerText dayo!\""}, "answerText", "initial is initial dayo!")]
        public void Execute_Statement_Evaluation(string[] statements, string getVariable, object expected)
        {
            foreach (var statement in statements)
            {
                var lineData = new DialogueLineData(statement, "", "", "");
                var enumerator = _operatorLine.Execute(lineData, null);
                while (enumerator.MoveNext())
                {
                }
            }
            
            VariableStore.Instance.TryGetVariableValue(getVariable, out object value);
            Assert.That(value, Is.EqualTo(expected));
        }

        [Test]
        public void Execute_Assignment_CreatesAndSetsVariable()
        {
            var lineData = new DialogueLineData("$money = 100", "", "", "");
            var enumerator = _operatorLine.Execute(lineData, null);
            while (enumerator.MoveNext())
            {
            }

            VariableStore.Instance.TryGetVariableValue("money", out object value);
            Assert.That(value, Is.EqualTo(100.0));
        }

        [Test]
        public void Execute_AddAssign_UpdatesVariable()
        {
            VariableStore.Instance.CreateVariable("money", 100.0);
            var lineData = new DialogueLineData("$money += 50", "", "", "");
            var enumerator = _operatorLine.Execute(lineData, null);
            while (enumerator.MoveNext())
            {
            }

            VariableStore.Instance.TryGetVariableValue("money", out object value);
            Assert.That(value, Is.EqualTo(150.0));
        }

        [Test]
        public void Execute_SubtractAssign_UpdatesVariable()
        {
            VariableStore.Instance.CreateVariable("money", 100.0);
            var lineData = new DialogueLineData("$money -= 50", "", "", "");
            var enumerator = _operatorLine.Execute(lineData, null);
            while (enumerator.MoveNext())
            {
            }

            VariableStore.Instance.TryGetVariableValue("money", out object value);
            Assert.That(value, Is.EqualTo(50.0));
        }

        [Test]
        public void Execute_MultiplyAssign_UpdatesVariable()
        {
            VariableStore.Instance.CreateVariable("money", 10.0);
            var lineData = new DialogueLineData("$money *= 10", "", "", "");
            var enumerator = _operatorLine.Execute(lineData, null);
            while (enumerator.MoveNext())
            {
            }

            VariableStore.Instance.TryGetVariableValue("money", out object value);
            Assert.That(value, Is.EqualTo(100.0));
        }

        [Test]
        public void Execute_DivideAssign_UpdatesVariable()
        {
            VariableStore.Instance.CreateVariable("money", 100.0);
            var lineData = new DialogueLineData("$money /= 10", "", "", "");
            var enumerator = _operatorLine.Execute(lineData, null);
            while (enumerator.MoveNext())
            {
            }

            VariableStore.Instance.TryGetVariableValue("money", out object value);
            Assert.That(value, Is.EqualTo(10.0));
        }

        [Test]
        public void Execute_StringConcatenation()
        {
            VariableStore.Instance.CreateVariable("text", "hello");
            var lineData = new DialogueLineData("$text += \" world\"", "", "", "");
            var enumerator = _operatorLine.Execute(lineData, null);
            while (enumerator.MoveNext())
            {
            }

            VariableStore.Instance.TryGetVariableValue("text", out object value);
            Assert.That(value, Is.EqualTo("hello world"));
        }
    }
}