using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Characters;
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
        private const char SUB_COMMAND_IDENTIFIER = '.';
        public const string DATABASE_CHARACTER_BASE = "character";
        public const string DATABASE_CHARACTER_SPRITE = "character_sprite";
        public const string DATABASE_CHARACTER_LIVE2D = "character_live2d";

        public static CommandManager instance { get; private set; }
        private CommandDatabase commandDatabase;
        /// <summary>
        /// ganyariya.Move()
        /// camera.Capture()
        /// のように X.verb のような syntax でコマンドを登録する
        /// </summary>
        private Dictionary<string, CommandDatabase> subCommandDatabases = new();
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
            if (commandName.Contains(SUB_COMMAND_IDENTIFIER)) return ExecuteSubCommand(commandName, args);

            Delegate command = commandDatabase.GetCommand(commandName);
            if (command == null) return null;

            return StartCommandProcess(command, commandName, args);
        }

        private CoroutineWrapper ExecuteSubCommand(string originalCommandName, string[] args)
        {
            string[] parts = originalCommandName.Split(SUB_COMMAND_IDENTIFIER); // super.ganyariya.Move
            string databaseName = string.Join(SUB_COMMAND_IDENTIFIER, parts.Take(parts.Length - 1)); // super.ganyariya
            string subCommandName = parts.Last(); // Move

            // システムに対してのサブコマンドとして解釈して実行する
            if (subCommandDatabases.ContainsKey(databaseName))
            {
                Delegate command = subCommandDatabases[databaseName].GetCommand(subCommandName);
                if (command == null)
                {
                    Debug.LogError($"No command called '{subCommandName}' was found in subDatabase '{databaseName}'");
                    return null;
                }
                return StartCommandProcess(command, originalCommandName, args);
            }

            // キャラに対してのサブコマンドであると解釈して実行する
            string characterName = databaseName;
            if (!CharacterManager.instance.HasCharacter(characterName))
            {
                Debug.LogError($"No sub database called '{databaseName} exists!' Command '{subCommandName}' could not be run.");
                return null;
            }

            return ExecuteCharacterSubCommand(subCommandName, characterName, args);
        }

        private CoroutineWrapper ExecuteCharacterSubCommand(string commandName, string characterName, string[] args)
        {
            string[] convertToCharacterArgs(string[] args)
            {
                List<string> copy = new(args);
                // ganyariya.Move(-x 10 -y 0) を CMD_DatabaseExtensionCharacter の moveCharacter で扱えるように
                // moveCharacter(ganyariya -x 10 -y 0) のように解釈させる
                copy.Insert(0, characterName);
                return copy.ToArray();
            }
            args = convertToCharacterArgs(args);

            Delegate command;

            if (subCommandDatabases.TryGetValue(DATABASE_CHARACTER_BASE, out CommandDatabase db))
            {
                if (db.HasCommand(commandName))
                {
                    command = db.GetCommand(commandName);
                    return StartCommandProcess(command, commandName, args);
                }
            }

            CharacterConfig config = CharacterManager.instance.GetCharacterConfig(characterName);
            switch (config.characterType)
            {
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    subCommandDatabases.TryGetValue(DATABASE_CHARACTER_SPRITE, out db);
                    break;
                case Character.CharacterType.Live2D:
                    subCommandDatabases.TryGetValue(DATABASE_CHARACTER_SPRITE, out db);
                    break;
            }

            command = db?.GetCommand(commandName) ?? null;
            if (command != null)
            {
                return StartCommandProcess(command, commandName, args);
            }

            Debug.LogError($"Command Manager was unable to execute command '{commandName}' on character {characterName}");
            return null;
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
            if (!targetCommandProcess.ShouldStopCoroutine()) return;

            activeCommandProcesses.Remove(targetCommandProcess);
            targetCommandProcess.StopCoroutine();
            // 仕上げとして 終了時の実行したい処理を実行する
            targetCommandProcess.ExecuteTerminationEvent();
        }

        private IEnumerator RunningCommandProcess(CommandProcess commandProcess, string[] args)
        {
            var command = commandProcess.command;

            if (
                command is Action
                || command is Action<string>
                || command is Action<string[]>
            )
            {
                if (command is Action) command.DynamicInvoke();
                if (command is Action<string>) command.DynamicInvoke(args[0]);
                if (command is Action<string[]>) command.DynamicInvoke((object)args);

                /*
                StartCommandProcess において coroutineWrapper をあとから CommandProcess に設定している
                そのため、 yield return null をせずに, そのまま KillTargetCommandProcess を実行してしまうと
                coroutineWrapper.isDone がずっと false になりゲームが進行不能になってしまう
                そのため一回 yield return null を実行して、 StartCommandProcess に coroutineWrapper を設定してもらう
                */
                yield return null;
            }


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

        public CommandDatabase CreateSubDatabase(string name)
        {
            name = name.ToLower();

            if (subCommandDatabases.TryGetValue(name, out CommandDatabase db))
            {
                Debug.LogWarning($"A sub database by the name of {name} already exists!");
                return db;
            }

            db = new CommandDatabase();
            subCommandDatabases.Add(name, db);
            return db;
        }
    }
}