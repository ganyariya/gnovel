using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core.ScriptParser
{
    /// <summary>
    /// Command を管理するクラス
    /// </summary>
    public class Command
    {
        /// <summary>
        /// コマンド名
        /// </summary>
        public string name;

        /// <summary>
        /// コマンドにわたす引数たち
        /// </summary>
        public string[] arguments;

        /// <summary>
        /// wait を実行するか
        /// </summary>
        public bool waitForCompletion;

        public Command(string name, string[] arguments, bool waitForCompletion)
        {
            this.name = name;
            this.arguments = arguments;
            this.waitForCompletion = waitForCompletion;
        }
    }

    public class DLD_CommandData
    {
        public List<Command> commands;

        private const char COMMANDS_SPLITTER_ID = ',';
        private const char ARGUMENTS_IDENTIFIER = '(';
        private const string WAIT_COMMAND_ID = "[wait]";

        public bool HasCommands => commands.Count > 0;

        /// <summary>
        /// 1 行の会話テキストダイアログには、複数のコマンドが設定されることがある
        /// "setMode(normal),playMusic(\"Dog Land\" -p 10)"
        /// 
        /// そのためこれらを それぞれ Command に変換して
        /// その Collection として DLD_CommandData がある
        /// </summary>
        public DLD_CommandData(string rawCommands)
        {
            Debug.Log(rawCommands);
            commands = ParseCommands(rawCommands);
        }

        public DLD_CommandData(List<Command> commands)
        {
            this.commands = commands;
        }

        static private List<Command> ParseCommands(string rawCommands)
        {
            var data = rawCommands.Split(COMMANDS_SPLITTER_ID, System.StringSplitOptions.RemoveEmptyEntries);
            var results = new List<Command>();

            foreach (string cmd in data)
            {
                int index = cmd.IndexOf(ARGUMENTS_IDENTIFIER);
                var name = cmd[..index].Trim();

                var wait = false;
                if (name.ToLower().StartsWith(WAIT_COMMAND_ID))
                {
                    wait = true;
                    name = name[WAIT_COMMAND_ID.Length..];
                }

                var args = ParseArguments(cmd.Substring(index + 1, cmd.Length - index - 2));
                var command = new Command(name, args, wait);
                results.Add(command);
            }
            return results;
        }

        /// <param name="rawArgs">playSong("Song Name" -v 1 -p 1)</param>
        static private string[] ParseArguments(string rawArgs)
        {
            var argList = new List<string>();
            var currentArg = new StringBuilder();

            bool quotes = false;
            for (int i = 0; i < rawArgs.Length; i++)
            {
                if (rawArgs[i] == '"')
                {
                    quotes = !quotes;
                    continue;
                }

                if (!quotes && rawArgs[i] == ' ')
                {
                    argList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }

                currentArg.Append(rawArgs[i]);
            }

            if (currentArg.Length > 0) argList.Add(currentArg.ToString());

            return argList.ToArray();
        }
    }
}