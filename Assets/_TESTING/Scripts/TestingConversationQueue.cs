using System;
using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using UnityEngine;

namespace Testing
{
    public class TestingConversationQueue : MonoBehaviour
    {
        private int count = 0;
        private void Start()
        {
            StartCoroutine(Running());
        }

        private IEnumerator Running()
        {
            var lines = new List<string>
            {
                "\"This is 1st statement.\"",
                "\"This is 2nd statement.\"",
                "\"This is 3rd statement.\"",
            };
            yield return DialogueSystemController.instance.Say(lines);

            DialogueSystemController.instance.HideUIAll(1, false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // "\"text\"" のように囲わないと、 DialogueParser において hasDialogue = false になってしまい会話が表示されない
                // エラーはでないのに会話が表示されず、非常にわかりづらいバグとなるため注意する
                var lines = new List<string>
                {
                    $"\"{count}: Append 1st line\"",
                    $"\"{count}: Append 2nd line\"",
                    $"\"{count}: Append 3rd line\"",
                };
                DialogueSystemController.instance.EnqueueConversation(new Conversation(lines));
                count++;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                var lines = new List<string>
                {
                    $"\"{count}: Force 1st line\"",
                    $"\"{count}: Force 2nd line\"",
                    $"\"{count}: Force 3rd line\"",
                };
                DialogueSystemController.instance.InterruptEnqueueConversation(new Conversation(lines));
                count++;
            }
        }
    }
}