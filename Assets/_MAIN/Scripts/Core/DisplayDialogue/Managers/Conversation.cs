using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// 1 つの会話の流れ Chunk を表す Data class
    /// ConversationQueue を利用して「会話シナリオ X を途中で停止させて、別の会話シナリオ Y を割り込み実行する」を実現する
    /// </summary>
    public class Conversation
    {
        private readonly List<string> _lines;
        public int Progress { get; private set; }
        public int Count => _lines.Count;

        /// <summary>
        /// Save/Load 機能に対応しやすくするために progress をコンストラクタで受け取れるようにする
        /// </summary>
        public Conversation(List<string> lines, int progress = 0)
        {
            _lines = lines;
            Progress = progress;
        }

        public void Proceed() => Progress++;
        public string CurrentLine => _lines[Progress];
        public bool HasReachedEnd => Progress >= Count;
    }
}