using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private GameObject rootGameObject;

        /// <summary>
        /// TextMeshProUGUI コンポーネントが設定されている gameObject を
        /// Inspector 上で紐づける
        /// </summary>
        [SerializeField] private TextMeshProUGUI nameText;

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
    }
}