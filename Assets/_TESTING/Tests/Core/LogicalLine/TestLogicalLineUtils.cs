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

    public class TestConditions
    {
        [SetUp]
        public void SetUp()
        {
            VariableStore.Instance.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            VariableStore.Instance.Clear();
        }

        [Test]
        public void EvaluateCondition_TrueLiteral()
        {
            var result = Conditions.EvaluateCondition("true");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_FalseLiteral()
        {
            var result = Conditions.EvaluateCondition("false");
            Assert.That(result, Is.False);
        }

        [Test]
        public void EvaluateCondition_VariableIsTrue()
        {
            VariableStore.Instance.CreateVariable("catliked", true);
            var result = Conditions.EvaluateCondition("$catliked");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_NumberComparison_IsTrue()
        {
            VariableStore.Instance.CreateVariable("x", 50);
            var result = Conditions.EvaluateCondition("$x > 40");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_NumberComparison_IsFalse()
        {
            VariableStore.Instance.CreateVariable("x", 30);
            var result = Conditions.EvaluateCondition("$x > 40");
            Assert.That(result, Is.False);
        }

        [Test]
        public void EvaluateCondition_VariableComparison_IsTrue()
        {
            VariableStore.Instance.CreateVariable("x", 50);
            VariableStore.Instance.CreateVariable("y", 30);
            var result = Conditions.EvaluateCondition("$y < $x");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_StringComparison_IsTrue()
        {
            VariableStore.Instance.CreateVariable("name", "ganyariya");
            var result = Conditions.EvaluateCondition("$name == \"ganyariya\"");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_StringComparison_IsFalse()
        {
            VariableStore.Instance.CreateVariable("name", "hoge");
            var result = Conditions.EvaluateCondition("$name == \"ganyariya\"");
            Assert.That(result, Is.False);
        }

        [Test]
        public void EvaluateCondition_IntGreaterThanOrEqual_IsTrue()
        {
            var result = Conditions.EvaluateCondition("10 >= 10");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_IntLessThanOrEqual_IsTrue()
        {
            var result = Conditions.EvaluateCondition("10 <= 10");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_FloatGreaterThanOrEqual_IsTrue()
        {
            var result = Conditions.EvaluateCondition("10.5 >= 10.5");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_FloatLessThanOrEqual_IsTrue()
        {
            var result = Conditions.EvaluateCondition("10.5 <= 10.5");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_BoolAnd_IsTrue()
        {
            var result = Conditions.EvaluateCondition("true && true");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_BoolAnd_IsFalse()
        {
            var result = Conditions.EvaluateCondition("true && false");
            Assert.That(result, Is.False);
        }

        [Test]
        public void EvaluateCondition_BoolOr_IsTrue()
        {
            var result = Conditions.EvaluateCondition("true || false");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_BoolOr_IsFalse()
        {
            var result = Conditions.EvaluateCondition("false || false");
            Assert.That(result, Is.False);
        }

        [Test]
        public void EvaluateCondition_StringNotEqual_IsTrue()
        {
            var result = Conditions.EvaluateCondition("\"hello\" != \"world\"");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_IntNotEqual_IsTrue()
        {
            var result = Conditions.EvaluateCondition("10 != 20");
            Assert.That(result, Is.True);
        }

        [Test]
        public void EvaluateCondition_VariableComparison_IsFalse()
        {
            VariableStore.Instance.CreateVariable("a", 10);
            VariableStore.Instance.CreateVariable("b", 20);
            var result = Conditions.EvaluateCondition("$a == $b");
            Assert.That(result, Is.False);
        }

        [Test]
        public void EvaluateCondition_ThrowsOnMultipleOperators()
        {
            VariableStore.Instance.CreateVariable("a", 10);
            VariableStore.Instance.CreateVariable("b", 10);
            VariableStore.Instance.CreateVariable("c", true);
            Assert.Throws<System.InvalidOperationException>(() => Conditions.EvaluateCondition("$a == $b && $c"));
        }
    }
}