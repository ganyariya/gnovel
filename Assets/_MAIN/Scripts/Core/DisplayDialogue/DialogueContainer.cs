using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// キャラ名とダイアログの GameObject を管理するだけの Container
    /// Unity との接続部分である DialogueSystem によって利用される
    /// </summary>
    [System.Serializable]
    public class DialogueContainer
    {
        /// <summary>
        /// Layers.4Dialogue の RootContainer
        /// 表示・非表示などに使える
        /// </summary>
        public GameObject rootContainer;

        /// <summary>
        /// 名前
        /// </summary>
        public TextMeshProUGUI nameText;

        /// <summary>
        /// ダイアログ内容
        /// TextMeshProUGUI をコンポーネントにもつ gameObject が紐付けられる
        /// </summary>
        public TextMeshProUGUI dialogueText;
    }
}
