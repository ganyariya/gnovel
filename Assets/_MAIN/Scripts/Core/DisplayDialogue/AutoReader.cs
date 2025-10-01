using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core.DisplayDialogue
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_PER_SECOND = 10;
        private const float READ_TIME_PADDING = 0.25f;
        private const float LOWER_BOUND_READ_TIME = 0.5f;
        private const float UPPER_BOUND_READ_TIME = 10.0f;
        private const string AUTO_MODE_TEXT = "AutoMode";
        private const string SKIP_MODE_TEXT = "SkipMode";

        private ConversationManager conversationManager;
        private DisplayTextArchitect textArchitect => conversationManager.textArchitect;

        [SerializeField] private TextMeshProUGUI statusText;

        public bool skip { get; set; } = false;
        public float speed { get; private set; } = 1f;

        private Coroutine runningCoroutine;
        public bool IsRunning => runningCoroutine != null;

        // DialogueSystemController から初期化する
        // TODO: 相互参照になっていてうーんという感じ
        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;

            statusText.text = string.Empty;
        }

        public void Enable(string modeText)
        {
            if (IsRunning) return;
            runningCoroutine = StartCoroutine(AutoRead());
            statusText.text = modeText;
        }

        public void Disable()
        {
            if (!IsRunning) return;
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
            statusText.text = string.Empty;
        }

        public void ToggleAuto()
        {
            // https://youtu.be/QIm0dH8fOxE?list=PLGSox0FgA5B58Ki4t4VqAPDycEpmkBd0i&t=1283
            // 上記と異なりシンプルな Toggle にする
            if (IsRunning) Disable();
            else Enable(AUTO_MODE_TEXT);
        }

        public void ToggleSkip()
        {
            skip = !skip;
            if (skip) Enable(SKIP_MODE_TEXT);
            else Disable();
        }

        public IEnumerator AutoRead()
        {
            // 実行する会話スクリプト (List<string>) がないとき
            if (!conversationManager.IsRunning)
            {
                Disable();
                yield break;
            }

            // 1 文がすべて表示されているなら AutoMode 開始ボタンにあわせて 次の文章を始める 
            if (!textArchitect.IsDisplaying && textArchitect.CurrentText != string.Empty)
                DialogueSystemController.instance.OnSystemPromptNextEvent();

            while (conversationManager.IsRunning)
            {
                if (!skip)
                {
                    // 文字が表示されはじめるまで待機する
                    while (!textArchitect.IsDisplaying && !conversationManager.IsWaitingSegmentSignal)
                        yield return null;

                    var startedTime = Time.time;
                    // 文字表示をおこなっている間は待機する
                    while (textArchitect.IsDisplaying || conversationManager.IsWaitingSegmentSignal) yield return null;
                    var elapsedTime = Time.time - startedTime;

                    var timeToRead = Mathf.Clamp(
                        (float)textArchitect.TmProText.textInfo.characterCount / DEFAULT_CHARACTERS_PER_SECOND,
                        LOWER_BOUND_READ_TIME,
                        UPPER_BOUND_READ_TIME
                    );
                    timeToRead = Mathf.Clamp(
                        timeToRead - elapsedTime, // 文字表示中にかかった秒数は削っておく
                        LOWER_BOUND_READ_TIME,
                        UPPER_BOUND_READ_TIME
                    );
                    timeToRead = timeToRead / speed + READ_TIME_PADDING;

                    yield return new WaitForSeconds(timeToRead);
                }
                else
                {
                    textArchitect.ForceComplete();
                    yield return new WaitForSeconds(0.02f);
                }

                // 1 文の表示がおわり skip/auto 時間待機も終わったので、次に進める
                DialogueSystemController.instance.OnSystemPromptNextEvent();
            }

            Disable();
        }
    }
}