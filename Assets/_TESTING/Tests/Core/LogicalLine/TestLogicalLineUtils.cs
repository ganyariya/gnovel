using System.Collections.Generic;
using Core.LogicalLine;
using NUnit.Framework;
using static Core.LogicalLine.LogicalLineUtils;

namespace Tests.Core.LogicalLine
{
    public class TestExpressions
    {
        [SetUp]
        public void SetUp()
        {
            // Make sure each test starts with a clean slate
            VariableStore.Instance.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            VariableStore.Instance.Clear();
        }

        [Test]
        public void EvaluateRhsExpression_SimpleAddition()
        {
            var parts = new[] { "1", "+", "2" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo(3.0));
        }

        [Test]
        public void EvaluateRhsExpression_OperatorPrecedence()
        {
            var parts = new[] { "1", "+", "2", "*", "3" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo(7.0));
        }

        [Test]
        public void EvaluateRhsExpression_ComplexExpression()
        {
            var parts = new[] { "10", "/", "2", "-", "3", "*", "2", "+", "1" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo(0.0));
        }

        [Test]
        public void EvaluateRhsExpression_WithVariable()
        {
            VariableStore.Instance.CreateVariable("money", 100);
            var parts = new[] { "$money", "+", "50" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo(150.0));
        }

        [Test]
        public void EvaluateRhsExpression_StringValue()
        {
            var parts = new[] { "\"hello world\"" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo("hello world"));
        }

        [Test]
        public void EvaluateRhsExpression_BoolValue()
        {
            var parts = new[] { "true" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateRhsExpression_NegatedBoolValue()
        {
            var parts = new[] { "!true" };
            var result = Expressions.EvaluateRhsExpression(parts);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void EvaluateRhsExpression_ThrowsOnUndefinedVariable()
        {
            var parts = new[] { "$undefined", "+", "10" };
            Assert.Throws<System.ArgumentException>(() => Expressions.EvaluateRhsExpression(parts));
        }

        [Test]
        public void EvaluateRhsExpression_DivisionByZero_ThrowsException()
        {
            var parts = new[] { "10", "/", "0" };
            Assert.Throws<System.DivideByZeroException>(() => Expressions.EvaluateRhsExpression(parts));
        }
    }
}