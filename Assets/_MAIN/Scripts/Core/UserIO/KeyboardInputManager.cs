using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using UnityEngine;


namespace Core.UserIO
{
    public class KeyboardInputManager : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                PromptAdvance();
            }
        }

        /// <summary>
        /// なにか入力があったら dialogueSystem のイベント発火を実行する
        /// </summary>
        public void PromptAdvance()
        {
            DialogueSystemController.instance.OnUserPromptNextEvent();
        }
    }
}