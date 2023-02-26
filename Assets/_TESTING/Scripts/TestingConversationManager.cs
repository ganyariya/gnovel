using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.ScriptIO;
using UnityEngine;

public class TestingConversationManager : MonoBehaviour
{
    public string textAddressableName = "addressableTestScript.txt";

    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        var lines = TextReader.ReadAddressableTextFileSync(textAddressableName);

        DialogueSystemController.instance.Say(lines);
    }
}
