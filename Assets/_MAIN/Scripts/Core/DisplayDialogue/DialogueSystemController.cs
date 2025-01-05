using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.ScriptableObjects;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// Unity の会話システム全体を管理するコントローラ
    /// </summary>
    public class DialogueSystemController : MonoBehaviour
    {
        public static DialogueSystemController instance { get; private set; }

        /// <summary>
        /// Unity Inspector 上から会話システムの設定を行う（SerializeField に設定する）
        /// 
        /// この会話システムの設定が他クラスから
        /// systemController.instance.dialogSystemConfig として参照される
        /// 
        /// これによって MonoBehaviour でないクラスから会話システムの設定値を取得できる
        /// </summary>
        [SerializeField]
        private DialogueSystemConfigurationSO _dialogueSystemConfig;
        public DialogueSystemConfigurationSO dialogSystemConfig => _dialogueSystemConfig;

        /// <summary>
        /// Unity Inspector から設定され textArchitect に dialogueUGUI を渡す
        /// </summary>
        public DialogueContainer dialogueContainer = new();
        private ConversationManager conversationManager;
        private DisplayTextArchitect displayTextArchitect;
        [SerializeField] private DisplayMethod displayMethod;

        public bool IsRunningConversation => conversationManager.IsRunning;

        /// <summary>
        /// ユーザからの入力を受け付けたときに発火する Event Sender
        /// 他 Manager からの Subscribe を受け付けて、 Event を subscriber に対して Send する
        /// </summary>
        public event DialogueSystemEvent UserPromptNextEvent;
        public delegate void DialogueSystemEvent();

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

        public void DisplaySpeakerName(string speaker = "")
        {
            if (speaker.ToLower() == "hide" || speaker.ToLower() == "narrator") HideSpeakerName();
            else if (!string.IsNullOrWhiteSpace(speaker)) dialogueContainer.nameContainer.Show(speaker);
        }
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

        /// <summary>
        /// キー入力など 次に進める処理が行われたら Subscriber に対して イベントを send する
        /// </summary>
        public void OnUserPromptNextEvent()
        {
            UserPromptNextEvent?.Invoke();
        }
    }
}
