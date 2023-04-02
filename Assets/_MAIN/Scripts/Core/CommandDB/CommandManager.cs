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
        private static Coroutine commandProcess = null;
        private static bool IsRunningCommandProcess = commandProcess != null;

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
        /// コマンドは Coroutine で実行される
        /// </summary>
        public Coroutine ExecuteCommand(string commandName, params string[] args)
        {
            Delegate command = commandDatabase.GetCommand(commandName);
            if (command == null) return null;

            return StartCommandProcess(command, commandName, args);
        }

        private Coroutine StartCommandProcess(Delegate command, string commandName, params string[] args)
        {
            StopCurrentCommandProcess();
            commandProcess = StartCoroutine(RunningCommandProcess(command, args));
            return commandProcess;
        }

        private void StopCurrentCommandProcess()
        {
            if (commandProcess != null) StopCoroutine(commandProcess);
            commandProcess = null;
        }

        private IEnumerator RunningCommandProcess(Delegate command, string[] args)
        {
            if (command is Action) command.DynamicInvoke();
            if (command is Action<string>) command.DynamicInvoke(args[0]);
            if (command is Action<string[]>) command.DynamicInvoke((object)args);

            if (command is Func<IEnumerator>) yield return ((Func<IEnumerator>)command)();
            if (command is Func<string, IEnumerator>) yield return ((Func<string, IEnumerator>)command)(args[0]);
            if (command is Func<string[], IEnumerator>) yield return ((Func<string[], IEnumerator>)command)(args);

            commandProcess = null; // Command DONE
        }
    }
}