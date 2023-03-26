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
            commandDatabase.AddCommand("print", new Action(PrintDefaultMessage));
        }

        private static void PrintDefaultMessage()
        {
            Debug.Log("Printing a default message to Console");
        }
    }
}
