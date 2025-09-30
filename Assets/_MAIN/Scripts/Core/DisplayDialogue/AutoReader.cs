using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core.DisplayDialogue
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_PER_SECOND = 18;
        private const float READ_TIME_PADDING = 0.3f;
        private const float LOWER_BOUND_READ_TIME = 0.5f;
        private const float UPPER_BOUND_READ_TIME = 10.0f;

        private ConversationManager conversationManager;
        private DisplayTextArchitect textArchitect => conversationManager.textArchitect;

        public bool skip { get; set; }
        public float speed { get; private set; }

        private Coroutine runningCoroutine;
        private bool isRuning => runningCoroutine != null;

        // DialogueSystemController から初期化する
        // TODO: 相互参照になっていてうーんという感じ
        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;
        }

        public void Enable()
        {
            if (isRuning) return;
            StartCoroutine(AutoRead());
        }

        public void Disable()
        {
            if (!isRuning) return;
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
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
                DialogueSystemController.instance.OnUserPromptNextEvent();

            while (conversationManager.IsRunning)
            {
                if (!skip)
                {
                    // 文字が表示されはじめるまで待機する
                    while (!textArchitect.IsDisplaying) yield return null;

                    var startedTime = Time.time;
                    // 文字表示をおこなっている間は待機する
                    while (textArchitect.IsDisplaying) yield return null;
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
                DialogueSystemController.instance.OnUserPromptNextEvent();
            }

            Disable();
        }
    }
}