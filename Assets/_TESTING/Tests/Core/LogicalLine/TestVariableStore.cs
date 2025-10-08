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
            Assert.That(store.DatabaseCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateGetSet_String()
        {
            var store = new VariableStore();
            Assert.That(store.CreateVariable("s", "hello"), Is.True);
            Assert.That(store.TryGetVariable<string>("s", out var v1), Is.True);
            Assert.That(v1, Is.EqualTo("hello"));
            Assert.That(store.TrySetValue("s", "world"), Is.True);
            Assert.That(store.TryGetVariable<string>("s", out var v2), Is.True);
            Assert.That(v2, Is.EqualTo("world"));
        }

        [Test]
        public void CreateGetSet_Bool()
        {
            var store = new VariableStore();
            Assert.That(store.CreateVariable("flag", false), Is.True);
            Assert.That(store.TryGetVariable<bool>("flag", out var b1), Is.True);
            Assert.That(b1, Is.False);
            Assert.That(store.TrySetValue("flag", true), Is.True);
            Assert.That(store.TryGetVariable<bool>("flag", out var b2), Is.True);
            Assert.That(b2, Is.True);
        }

        [Test]
        public void ExternalVariable_Int_Capture_ReflectsChanges()
        {
            var store = new VariableStore();

            // external = 10 をキャプチャする
            var external = 10;
            Assert.That(store.CreateVariable("extInt", 0, () => external, v => external = v), Is.True);
            Assert.That(store.TryGetVariable<int>("extInt", out var g1), Is.True);
            Assert.That(g1, Is.EqualTo(10));

            // 20 に書き換える
            external = 20;
            Assert.That(store.TryGetVariable<int>("extInt", out var g2), Is.True);
            // () => external という関数のため、実行時の値である `20` が入る
            Assert.That(g2, Is.EqualTo(20));

            // store 経由で exInt を 30 にする
            Assert.That(store.TrySetValue("extInt", 30), Is.True);
            // このとき、 external 変数自体も書き換えられている
            Assert.That(external, Is.EqualTo(30));
            Assert.That(store.TryGetVariable<int>("extInt", out var g3), Is.True);
            Assert.That(g3, Is.EqualTo(30));
        }

        private class Box
        {
            public int V;
        }

        [Test]
        public void ExternalVariable_ObjectField_Capture_ReflectsChanges()
        {
            var store = new VariableStore();
            var box = new Box { V = 1 };
            Assert.That(store.CreateVariable("boxV", 0, () => box.V, v => box.V = v), Is.True);
            Assert.That(store.TryGetVariable<int>("boxV", out var v1), Is.True);
            Assert.That(v1, Is.EqualTo(1));
            Assert.That(store.TrySetValue("boxV", 5), Is.True);
            Assert.That(box.V, Is.EqualTo(5));
            box.V = 9;
            Assert.That(store.TryGetVariable<int>("boxV", out var v2), Is.True);
            Assert.That(v2, Is.EqualTo(9));
        }

        [Test]
        public void ExternalVariable_Bool_Capture_ReflectsChanges()
        {
            var store = new VariableStore();
            var flag2 = false;
            Assert.That(store.CreateVariable("extFlag", false, () => flag2, v => flag2 = v), Is.True);
            Assert.That(store.TryGetVariable<bool>("extFlag", out var bb1), Is.True);
            Assert.That(bb1, Is.False);
            Assert.That(store.TrySetValue("extFlag", true), Is.True);
            Assert.That(flag2, Is.True);
            flag2 = false;
            Assert.That(store.TryGetVariable<bool>("extFlag", out var bb2), Is.True);
            Assert.That(bb2, Is.False);
        }

        [Test]
        public void DeleteVariable_RemovesFromDefaultAndPreventsGetSet()
        {
            var store = new VariableStore();
            Assert.That(store.CreateVariable("x", 10), Is.True);
            Assert.That(store.TryGetVariable<int>("x", out var before), Is.True);
            Assert.That(before, Is.EqualTo(10));

            store.DeleteVariable("x");

            Assert.That(store.TryGetVariable<int>("x", out var after), Is.False);
            Assert.That(after, Is.EqualTo(0));
            Assert.That(store.TrySetValue("x", 99), Is.False);

            Assert.That(store.CreateVariable("x", 1), Is.True);
            Assert.That(store.TryGetVariable<int>("x", out var recreated), Is.True);
            Assert.That(recreated, Is.EqualTo(1));
        }

        [Test]
        public void DeleteVariable_WithDatabasePrefix_Isolated()
        {
            var store = new VariableStore();
            Assert.That(store.CreateVariable("db1.a", 100), Is.True);
            Assert.That(store.CreateVariable("a", 200), Is.True);

            store.DeleteVariable("db1.a");

            Assert.That(store.TryGetVariable<int>("db1.a", out var v1), Is.False);
            Assert.That(store.TryGetVariable<int>("a", out var v2), Is.True);
            Assert.That(v2, Is.EqualTo(200));
        }

        [Test]
        public void DeleteDatabase_RemovesDatabaseAndVariables()
        {
            var store = new VariableStore();
            Assert.That(store.DatabaseCount, Is.EqualTo(1));
            Assert.That(store.CreateVariable("db2.v", 5), Is.True);
            Assert.That(store.DatabaseCount, Is.EqualTo(2));

            store.DeleteDatabase("db2");

            Assert.That(store.DatabaseCount, Is.EqualTo(1));

            Assert.That(store.TryGetVariable<int>("db2.v", out var missing), Is.False);
            Assert.That(missing, Is.EqualTo(0));
            Assert.That(store.DatabaseCount, Is.EqualTo(2));
        }

        [Test]
        public void DeleteDatabase_NonExisting_NoThrowOrChange()
        {
            var store = new VariableStore();
            Assert.That(store.DatabaseCount, Is.EqualTo(1));
            store.DeleteDatabase("nope");
            Assert.That(store.DatabaseCount, Is.EqualTo(1));
        }
    }
}