using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.CommandDB
{
    /// <summary>
    /// Command を登録する Database
    /// commandName に対応した Delegate が登録される
    /// 
    /// 登録自体は以下のように行う
    /// - CommandManager で Assembly を取得し ExtensionBase を継承したものをすべて取得する
    /// - 各 Extension
    /// </summary>
    public class CommandDatabase
    {
        private readonly Dictionary<string, Delegate> commandDB = new Dictionary<string, Delegate>();

        public bool HasCommand(string commandName) => commandDB.ContainsKey(commandName.ToLower());

        public void AddCommand(string commandName, Delegate command)
        {
            commandName = commandName.ToLower(); // 大文字と小文字を区別しない

            if (!HasCommand(commandName))
            {
                commandDB.Add(commandName, command);
            }
            else
            {
                Debug.Log($"Already registered command {commandName}");
            }
        }

        public Delegate GetCommand(string commandName)
        {
            commandName = commandName.ToLower();

            if (!HasCommand(commandName)) Debug.Log($"Already registered command {commandName}");
            return commandDB[commandName] ?? null;
        }
    }
}