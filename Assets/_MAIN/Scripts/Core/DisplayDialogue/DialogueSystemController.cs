using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// Unity の会話システム全体を管理するコントローラ
    /// </summary>
    public class DialogueSystemController : MonoBehaviour
    {
        public static DialogueSystemController instance;

        /// <summary>
        /// Unity Inspector から設定され textArchitect に dialogueUGUI を渡す
        /// </summary>
        public DialogueContainer dialogueContainer = new();
        private ConversationManager conversationManager;
        private DisplayTextArchitect displayTextArchitect;

        public bool IsRunningConversation => conversationManager.IsRunning;
        [SerializeField] private DisplayMethod displayMethod;

        private void Awake()
        {
            // Singleton
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else DestroyImmediate(gameObject);
        }

        private void Initialize()
        {
            displayTextArchitect = new(dialogueContainer.dialogueText, null);
            conversationManager = new(this, displayTextArchitect);
            displayTextArchitect.CurrentDisplayMethod = displayMethod;
        }

        public void DisplaySpeakerName(string speaker = "") => dialogueContainer.nameContainer.Show(speaker);
        public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

        /// <summary>
        /// 画面に speaker & dialogue を出力する
        /// </summary>
        public void Say(string speaker, string dialogue)
        {
            Say(new List<string>() { $"{speaker} \"{dialogue}\"" });
        }

        /// <summary>
        /// ConversationManager に一連の会話を表示させる
        /// </summary>
        public void Say(List<string> conversation)
        {
            conversationManager.StartConversation(conversation);
        }
    }
}
