using System;
using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using TMPro;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// Screen に表示する"名前"に関するコンテナ
    /// DialogueContainer に含まれる
    /// </summary>
    [Serializable]
    public class NameContainer
    {
        /// <summary>
        /// Name 自体を表示するか、しないかを設定するための gameObject
        /// </summary>
        public GameObject rootGameObject;

        /// <summary>
        /// TextMeshProUGUI コンポーネントが設定されている gameObject を
        /// Inspector 上で紐づける
        /// </summary>
        public TextMeshProUGUI nameText;

        public void Show(string name = "")
        {
            rootGameObject.SetActive(true);

            if (!string.IsNullOrWhiteSpace(name))
            {
                nameText.text = name;
            }
        }

        public void Hide()
        {
            rootGameObject.SetActive(false);
        }

        public void ApplyCharacterConfig(CharacterConfig characterConfig)
        {
            /*
            なぜか色を設定すると画面上から name が消えてしまう
            そのためコメントアウトして、別途原因を調査したい
            https://www.youtube.com/watch?v=3LfpzaFqNsc&list=PLGSox0FgA5B58Ki4t4VqAPDycEpmkBd0i&index=17
            */
            // nameText.color = characterConfig.nameColor;
            nameText.font = characterConfig.nameFont;
        }
    }
}