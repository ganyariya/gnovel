using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Core.CommandDB
{
    /// <summary>
    /// CommandDatabase のインスタンスを内部に格納している
    /// 外部からコマンドを実行できるようにする (Execute)
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager instance { get; private set; }
        private CommandDatabase commandDatabase;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                commandDatabase = RegisterExtensions(new());
            }
            else DestroyImmediate(gameObject);
        }

        /// <summary>
        /// commandDatabase に ExtensionBase を継承している Command をすべて登録する
        /// </summary>
        CommandDatabase RegisterExtensions(CommandDatabase database)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] extensionTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CMD_DatabaseExtensionBase))).ToArray();

            foreach (var extension in extensionTypes)
            {
                MethodInfo extendMethod = extension.GetMethod("Extend");
                extendMethod.Invoke(null, new object[] { database });
            }
            return database;
        }

        /// <summary>
        /// 外部から Command を呼び出す
        /// </summary>
        public void ExecuteCommand(string commandName)
        {
            Delegate command = commandDatabase.GetCommand(commandName);
            command?.DynamicInvoke();
        }
    }
}