using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionPrintCommand : CMD_DatabaseExtensionBase
    {
        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand(GetCommandName(), new Action(PrintDefaultMessage));
        }

        new public static string GetCommandName()
        {
            return "print";
        }

        private static void PrintDefaultMessage()
        {
            Debug.Log("Printing a default message to Console");
        }
    }
}
