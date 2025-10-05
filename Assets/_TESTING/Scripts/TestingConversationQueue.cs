using System;
using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using UnityEngine;

namespace Testing
{
    public class TestingConversationQueue : MonoBehaviour
    {
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
    }
}