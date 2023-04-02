using System.Collections;
using System.Collections.Generic;
using Core.CommandDB;
using Core.DisplayDialogue;
using Core.ScriptIO;
using UnityEngine;

public class TestingConversationManager : MonoBehaviour
{
    public string textAddressableName = "addressableTestScript.txt";

    void Start()
    {
        StartCoroutine(Running());
        StartConversation();
    }

    IEnumerator Running()
    {
        yield return CommandManager.instance.ExecuteCommand("print");
        yield return CommandManager.instance.ExecuteCommand("print_mp", "Line1", "Line2");

        yield return CommandManager.instance.ExecuteCommand("print_lambda");
        yield return CommandManager.instance.ExecuteCommand("print_lambda_mp", "Line1", "Line2");

        yield return CommandManager.instance.ExecuteCommand("print_process");
        yield return CommandManager.instance.ExecuteCommand("print_process_1p", "4");
        yield return CommandManager.instance.ExecuteCommand("print_process_mp", "Line1", "Line2");
    }

    void StartConversation()
    {
        var lines = TextReader.ReadAddressableTextFileSync(textAddressableName);

        DialogueSystemController.instance.Say(lines);
    }
}
