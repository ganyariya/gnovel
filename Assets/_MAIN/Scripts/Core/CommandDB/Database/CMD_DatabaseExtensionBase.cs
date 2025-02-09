using System.Collections;
using System.Collections.Generic;
using Core.CommandDB;
using UnityEngine;

namespace Core.CommandDB
{
    /// <summary>
    /// static method しか登録できない
    /// Extend を CommandManager から呼び出して CommandDatabase に登録する
    /// </summary>
    public abstract class CMD_DatabaseExtensionBase
    {
        public static void Extend(CommandDatabase commandDatabase) { }

        public static string GetCommandName() { return ""; }

        public static CommandParameterFetcher CreateFetcher(string[] parameters) => new(parameters);
    }
}
