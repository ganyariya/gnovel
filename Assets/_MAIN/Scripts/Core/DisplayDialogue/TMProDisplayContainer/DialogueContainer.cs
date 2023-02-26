using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// DialogueSystem によって利用される
    /// - DisplayTextArchitect に tmProUGUI が渡される
    /// </summary>
    [System.Serializable]
    public class DialogueContainer
    {
        /// <summary>
        /// Layers.4Dialogue
        /// 表示・非表示などに使える
        /// </summary>
        public GameObject rootGameObject;

        public NameContainer nameContainer;

        /// <summary>
        /// ダイアログ内容
        /// TextMeshProUGUI をコンポーネントにもつ gameObject が紐付けられる
        /// </summary>
        public TextMeshProUGUI dialogueText;
    }
}
