using System;
using NUnit.Framework;
using Core.LogicalLine;

namespace Tests.Core.LogicalLine
{
    public class TestVariableStore
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void CreateDatabase_BehavesAsExpected()
        {
            var store = new VariableStore();
            Assert.That(store.DatabaseCount, Is.EqualTo(1));

            Assert.That(store.CreateDatabase("db1"), Is.True);
            Assert.That(store.DatabaseCount, Is.EqualTo(2));

            Assert.That(store.CreateDatabase("db1"), Is.False);
            Assert.That(store.DatabaseCount, Is.EqualTo(2));
        }

        [Test]
        public void GetDatabase_DefaultAndAutoCreate()
        {
            var store = new VariableStore();

            var dEmpty = store.GetDatabase("");
            var dDefault = store.GetDatabase("default");
            Assert.That(ReferenceEquals(dEmpty, dDefault), Is.True,
                "Empty name and 'default' should return the same default DB instance");

            var dFoo = store.GetDatabase("foo");
            Assert.That(dFoo, Is.Not.Null);
            Assert.That(store.DatabaseCount, Is.EqualTo(2));

            var dFoo2 = store.GetDatabase("foo");
            Assert.That(ReferenceEquals(dFoo, dFoo2), Is.True);
        }

        [Test]
        public void CreateVariable_ThenGetAndSet()
        {
            var store = new VariableStore();

            Assert.That(store.TryGetVariable<int>("money", out var missingValue), Is.False);
            Assert.That(missingValue, Is.EqualTo(0));

            Assert.That(store.CreateVariable("money", 10), Is.True);
            Assert.That(store.TryGetVariable<int>("money", out var v), Is.True);
            Assert.That(v, Is.EqualTo(10));

            Assert.That(store.TrySetValue("money", 20), Is.True);
            Assert.That(store.TryGetVariable<int>("money", out var v2), Is.True);
            Assert.That(v2, Is.EqualTo(20));

            Assert.That(store.TrySetValue("unknown", 123), Is.False);
        }

        [Test]
        public void CreateVariable_Duplicate_ReturnsFalse()
        {
            var store = new VariableStore();
            Assert.That(store.CreateVariable("x", 1), Is.True);
            Assert.That(store.CreateVariable("x", 999), Is.False);

            Assert.That(store.TryGetVariable<int>("x", out var v), Is.True);
            Assert.That(v, Is.EqualTo(1));
        }

        [Test]
        public void DotNotation_TargetsExpectedDatabase()
        {
            var store = new VariableStore();

            Assert.That(store.CreateVariable("db1.var1", 1), Is.True);
            Assert.That(store.CreateVariable("var1", 2), Is.True);

            Assert.That(store.TryGetVariable<int>("db1.var1", out var vDb1), Is.True);
            Assert.That(store.TryGetVariable<int>("var1", out var vDefault), Is.True);
            Assert.That(vDb1, Is.EqualTo(1));
            Assert.That(vDefault, Is.EqualTo(2));

            Assert.That(store.TrySetValue("db1.var1", 10), Is.True);
            Assert.That(store.TryGetVariable<int>("db1.var1", out var vDb1b), Is.True);
            Assert.That(store.TryGetVariable<int>("var1", out var vDefaultb), Is.True);
            Assert.That(vDb1b, Is.EqualTo(10));
            Assert.That(vDefaultb, Is.EqualTo(2));
        }

        [Test]
        public void Variable_WithCustomGetterSetter_LinksExternalValue()
        {
            var store = new VariableStore();
            var external = 5;

            // Link the variable to external via custom getter/setter
            Assert.That(store.CreateVariable("link", 0, () => external, v => external = v), Is.True);

            // Get should reflect external
            Assert.That(store.TryGetVariable<int>("link", out var v), Is.True);
            Assert.That(v, Is.EqualTo(5));

            // Set should write back to external
            Assert.That(store.TrySetValue("link", 7), Is.True);
            Assert.That(external, Is.EqualTo(7));

            // Getting again should see the updated external
            Assert.That(store.TryGetVariable<int>("link", out var v2), Is.True);
            Assert.That(v2, Is.EqualTo(7));
        }

        [Test]
        public void Clear_RemovesAllDatabases()
        {
            var store = new VariableStore();
            Assert.That(store.DatabaseCount, Is.EqualTo(1));

            store.CreateDatabase("a");
            store.CreateDatabase("b");
            Assert.That(store.DatabaseCount, Is.EqualTo(3));

            store.Clear();
            Assert.That(store.DatabaseCount, Is.EqualTo(0));
        }
    }
}