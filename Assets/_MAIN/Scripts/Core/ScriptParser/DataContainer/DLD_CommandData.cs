using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Core.ScriptParser
{
    public class Command
    {
        public string name;
        public string[] arguments;

        public Command(string name, string[] arguments)
        {
            this.name = name;
            this.arguments = arguments;
        }
    }

    public class DLD_CommandData
    {
        public List<Command> commands;

        private const char COMMANDS_SPLITTER_ID = ',';
        private const char ARGUMENTS_IDENTIFIER = '(';

        public bool HasCommands => commands.Count > 0;

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
                var args = ParseArguments(cmd.Substring(index + 1, cmd.Length - index - 2));
                var command = new Command(name, args);
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