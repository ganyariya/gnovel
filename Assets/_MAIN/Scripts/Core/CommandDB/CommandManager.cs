using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
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
        private List<CommandProcess> activeCommandProcesses = new();
        private CommandProcess topCommandProcess => activeCommandProcesses.LastOrDefault();

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
        public CoroutineWrapper ExecuteCommand(string commandName, params string[] args)
        {
            Delegate command = commandDatabase.GetCommand(commandName);
            if (command == null) return null;

            return StartCommandProcess(command, commandName, args);
        }

        private CoroutineWrapper StartCommandProcess(Delegate command, string commandName, params string[] args)
        {
            var commandProcess = CommandProcess.Create(commandName, command, args);
            activeCommandProcesses.Add(commandProcess);

            // coroutine 生成の都合上 あとから commandProcess に coroutineWrapper を代入する
            Coroutine coroutine = StartCoroutine(RunningCommandProcess(commandProcess, args));
            CoroutineWrapper coroutineWrapper = new(this, coroutine);
            commandProcess.SetCoroutineWrapper(coroutineWrapper);

            return commandProcess.coroutineWrapper;
        }

        public void StopCurrentCommandProcess()
        {
            if (topCommandProcess != null) KillTargetCommandProcess(topCommandProcess);
        }

        public void KillTargetCommandProcess(CommandProcess targetCommandProcess)
        {
            activeCommandProcesses.Remove(targetCommandProcess);

            if (targetCommandProcess.ShouldStopCoroutine()) targetCommandProcess.StopCoroutine();

            targetCommandProcess.ExecuteTerminationEvent();
        }

        private IEnumerator RunningCommandProcess(CommandProcess commandProcess, string[] args)
        {
            var command = commandProcess.command;

            if (command is Action) command.DynamicInvoke();
            if (command is Action<string>) command.DynamicInvoke(args[0]);
            if (command is Action<string[]>) command.DynamicInvoke((object)args);

            if (command is Func<IEnumerator>) yield return ((Func<IEnumerator>)command)();
            if (command is Func<string, IEnumerator>) yield return ((Func<string, IEnumerator>)command)(args[0]);
            if (command is Func<string[], IEnumerator>) yield return ((Func<string[], IEnumerator>)command)(args);

            KillTargetCommandProcess(commandProcess);
        }
    }
}