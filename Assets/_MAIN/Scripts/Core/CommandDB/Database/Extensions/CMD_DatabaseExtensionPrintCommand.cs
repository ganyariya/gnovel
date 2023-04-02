using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionPrintCommand : CMD_DatabaseExtensionBase
    {
        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand(GetCommandName(), new Action(PrintDefaultMessage));
            commandDatabase.AddCommand("print_1p", new Action<string>(PrintUserMessage));
            commandDatabase.AddCommand("print_mp", new Action<string[]>(PrintLines));

            commandDatabase.AddCommand("print_lambda", new Action(() => { Debug.Log("Lambda"); }));
            commandDatabase.AddCommand("print_lambda_1p", new Action<string>((string message) => { Debug.Log($"Lambda {message}"); }));
            commandDatabase.AddCommand("print_lambda_mp", new Action<string[]>((string[] message) => { Debug.Log($"Lambda {message}"); }));

            commandDatabase.AddCommand("print_process", new Func<IEnumerator>(SimpleProcess));
            commandDatabase.AddCommand("print_process_1p", new Func<string, IEnumerator>(SimpleProcessLine));
            commandDatabase.AddCommand("print_process_mp", new Func<string[], IEnumerator>(SimpleProcessLines));

            commandDatabase.AddCommand("move_character", new Func<string, IEnumerator>(MoveCharacter));
        }

        new public static string GetCommandName()
        {
            return "print";
        }

        private static void PrintDefaultMessage()
        {
            Debug.Log("Printing a default message to Console");
        }

        private static void PrintUserMessage(string message)
        {
            Debug.Log($"Printing {message}");
        }

        private static void PrintLines(string[] message)
        {
            Debug.Log($"Printing Lines {string.Join(',', message)}");
        }

        private static IEnumerator SimpleProcess()
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"Process Running... {i}");
                yield return new WaitForSeconds(1);
            }
        }

        private static IEnumerator SimpleProcessLine(string line)
        {
            if (int.TryParse(line, out var num))
            {
                for (int i = 0; i < num; i++)
                {
                    Debug.Log($"Process Running... {i}");
                    yield return new WaitForSeconds(1);
                }
            }
        }

        private static IEnumerator SimpleProcessLines(string[] lines)
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"Process Runnnng... {i}, {string.Join(',', lines)}");
                yield return new WaitForSeconds(1);
            }
        }

        private static IEnumerator MoveCharacter(string direction)
        {
            bool left = direction.ToLower() == "left";

            Transform characterTransform = GameObject.Find("Image").transform;
            const float moveSpeed = 15;

            float targetX = left ? -100 : 100;
            float currentX = characterTransform.position.x;

            while (Mathf.Abs(targetX - currentX) > 0.1)
            {
                currentX = Mathf.MoveTowards(currentX, targetX, moveSpeed * Time.deltaTime);
                characterTransform.position = new Vector3(currentX, characterTransform.position.y, characterTransform.position.z);
                yield return null;
            }

            yield return null;
        }
    }
}
