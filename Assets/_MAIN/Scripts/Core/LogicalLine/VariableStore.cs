using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.LogicalLine
{
    public class VariableStore
    {
        private const string DEFAULT_DATABASE = "default";
        private const char DATABASE_VARIABLE_SPLITTER = '.';

        public static VariableStore Instance { get; private set; }

        private readonly Dictionary<string, Database> _databases;
        private Database DefaultDatabase => _databases[DEFAULT_DATABASE];

        public VariableStore()
        {
            _databases = new Dictionary<string, Database> { [DEFAULT_DATABASE] = new(DEFAULT_DATABASE) };
            Instance ??= this;
        }

        private bool HasKey(string name) => _databases.ContainsKey(name);

        /// <returns>Database を作成したら true</returns>
        public bool CreateDatabase(string name)
        {
            if (HasKey(name)) return false;

            _databases[name] = new Database(name);
            return true;
        }

        public Database GetDatabase(string name)
        {
            if (name is "" or DEFAULT_DATABASE) return DefaultDatabase;

            CreateDatabase(name);
            return _databases[name];
        }

        /// <summary>
        /// money1 / intDB.money1
        /// `.` で区切られている場合は DB.variableName となる
        ///
        /// </summary>
        /// <returns>すでに変数が該当のデータベースに登録されているなら false; 登録できたら true</returns>
        public bool CreateVariable<T>(string name, T defaultValue, Func<T> getter = null, Action<T> setter = null)
        {
            var (databaseName, variableName) = VariableInfo.CreateVariableInfo(name).GetTuple();

            var database = GetDatabase(databaseName);
            if (database.HasKey(variableName)) return false;

            database.Register(variableName, new Variable<T>(defaultValue, getter, setter));
            return true;
        }

        public bool TryGetVariable<T>(string name, out T variable)
        {
            var (databaseName, variableName) = VariableInfo.CreateVariableInfo(name).GetTuple();
            var database = GetDatabase(databaseName);

            if (!database.HasKey(variableName))
            {
                variable = default;
                return false;
            }

            variable = database.GetVariableValue<T>(variableName);
            return true;
        }

        /// <summary>
        /// name(db.variableName) が存在しない場合は、 Set せずに false を返す
        /// name(db.variableName) が存在する場合のみ登録して true を返す
        /// </summary>
        public bool TrySetValue<T>(string name, T value)
        {
            var (databaseName, variableName) = VariableInfo.CreateVariableInfo(name).GetTuple();
            var database = GetDatabase(databaseName);

            if (!database.HasKey(variableName)) return false;
            database.SetVariableValue(variableName, value);
            return true;
        }

        public void Clear()
        {
            _databases.Clear();
        }

        public int DatabaseCount => _databases.Count;

        public void PrintAllDatabases()
        {
            foreach (var entry in _databases) Debug.Log($"Database: {entry.Key}");
        }

        public void PrintAllVariables()
        {
            foreach (var db in _databases.Values) PrintTargetDBVariables(db);
        }

        public static void PrintTargetDBVariables(Database database)
        {
            database.DebugPrint();
        }

        public class Database
        {
            public string Name { get; }
            private readonly Dictionary<string, AbstractVariable> _variables;

            public Database(string name)
            {
                Name = name;
                _variables = new Dictionary<string, AbstractVariable>();
            }

            public bool HasKey(string key) => _variables.ContainsKey(key);
            public void Register(string key, AbstractVariable v) => _variables[key] = v;
            private AbstractVariable GetVariable(string key) => _variables[key];
            public T GetVariableValue<T>(string key) => (T)GetVariable(key).Get();
            public void SetVariableValue<T>(string key, T value) => GetVariable(key).Set(value);

            public void DebugPrint()
            {
                StringBuilder builder = new();

                builder.AppendLine($"Database: <color=#F38544>{Name}</color>");
                foreach (var entry in _variables)
                {
                    builder.AppendLine($"\t[{entry.Key}: {entry.Value.Get()}]");
                }
                Debug.Log(builder.ToString());
            }
        }

        /// <summary>
        /// Variable[T] のようにしたいが Database の Dictionary として入れるために
        /// `object` で定義するしかない
        /// </summary>
        public abstract class AbstractVariable
        {
            public abstract object Get();
            public abstract void Set(object value);
        }

        public class Variable<T> : AbstractVariable
        {
            private T _value;
            private readonly Func<T> _getter;
            private readonly Action<T> _setter;

            public Variable(T value = default) : this(value, null, null)
            {
            }

            /// <summary>
            /// getter, setter を Custom することで LinkedVariable にできる
            /// 他のクラス・インスタンスと紐づけた getter/setter で他システムにアクセスする
            ///
            /// 指定しなければただの変数になる
            /// </summary>
            public Variable(T value, Func<T> getter, Action<T> setter)
            {
                _value = value;
                _getter = getter ?? (() => _value);
                _setter = setter ?? (v => _value = v);
            }

            public override object Get() => _getter();
            public override void Set(object value) => _setter((T)value);
        }

        private readonly struct VariableInfo
        {
            private readonly string _databaseName;
            private readonly string _variableName;

            private VariableInfo(string raw)
            {
                var parts = raw.Split(DATABASE_VARIABLE_SPLITTER);
                _databaseName = parts.Length > 1 ? parts[0] : DEFAULT_DATABASE;
                _variableName = parts[^1];
            }

            public static VariableInfo CreateVariableInfo(string name)
            {
                return new VariableInfo(name);
            }

            public (string, string) GetTuple()
            {
                return (_databaseName, _variableName);
            }
        }
    }
}