using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core.Characters;

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
        /// Layers.4 - Dialogue
        /// ダイアログレイヤ全体（名前・ダイアログを含む）表示・非表示などに使える
        /// </summary>
        public GameObject rootGameObject;

        /// <summary>
        /// Name を管理する
        /// </summary>
        public NameContainer nameContainer;

        /// <summary>
        /// ダイアログ内容
        /// TextMeshProUGUI をコンポーネントにもつ gameObject が紐付けられる
        /// </summary>
        public TextMeshProUGUI dialogueText;

        public void ApplyCharacterConfig(CharacterConfig characterConfig)
        {
            nameContainer.ApplyCharacterConfig(characterConfig);
            dialogueText.color = characterConfig.dialogueColor;
            dialogueText.font = characterConfig.dialogueFont;
        }
    }
}
