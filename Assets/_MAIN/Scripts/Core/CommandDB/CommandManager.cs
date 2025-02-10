using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.ScriptParser;
using Extensions;
using UnityEngine;
using UnityEngine.Events;

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
                MethodInfo extendMethod = extension.GetMethod(CMD_DatabaseExtensionBase.EXTEND_FUNCTION_NAME);
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

        public void StopAllCommandProcesses()
        {
            // そのまま消すと `InvalidOperationException: Collection was modified` になるため 一旦 list にする
            var list = activeCommandProcesses.ToList();
            foreach (var c in list) KillTargetCommandProcess(c);
            activeCommandProcesses.Clear();
        }

        /// <summary>
        /// 指定した targetCommandProcess について
        /// - コルーチン処理を停止させる
        /// - コマンド終了時の終了イベント（キャラを強引に移動させる、など）
        /// をおこなう
        /// </summary>
        public void KillTargetCommandProcess(CommandProcess targetCommandProcess)
        {
            activeCommandProcesses.Remove(targetCommandProcess);

            if (targetCommandProcess.ShouldStopCoroutine()) targetCommandProcess.StopCoroutine();

            // 仕上げとして 終了時の実行したい処理を実行する
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


        /// <summary>
        /// CommandProcess に 終了時に実行したい action を登録する
        /// 
        /// this.StartCommandProcess ですでにコマンドは activeCommandProcesses に登録済みのため問題ない
        /// </summary>
        public void RegisterTerminationEventToCurrentCommandProcess(UnityAction action)
        {
            CommandProcess process = topCommandProcess;
            if (process == null) return;

            process.RegisterTerminationAction(action);
        }
    }
}