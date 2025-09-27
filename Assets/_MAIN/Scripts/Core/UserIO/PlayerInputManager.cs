using System;
using System.Collections.Generic;
using Core.DisplayDialogue;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.UserIO
{
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput playerInput;
        private List<(InputAction action, Action<InputAction.CallbackContext> callback)> actions = new();

        public void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            BindActions();
        }

        private void BindActions()
        {
            actions.Add((playerInput.actions["Next"], OnNext));
        }

        private void OnEnable()
        {
            foreach (var tuple in actions)
                // action が完全に trigger されたら callback を実行する
                tuple.action.performed += tuple.callback;
        }

        private void OnDisable()
        {
            foreach (var tuple in actions)
                tuple.action.performed -= tuple.callback;
        }

        /// <summary>
        /// なにか入力があったら dialogueSystem のイベント発火を実行する
        /// </summary>
        private void OnNext(InputAction.CallbackContext context)
        {
            DialogueSystemController.instance.OnUserPromptNextEvent();
        }
    }
}