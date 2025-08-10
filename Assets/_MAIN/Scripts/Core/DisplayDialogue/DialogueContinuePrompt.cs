using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core.DisplayDialogue
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class DialogueContinuePrompt : MonoBehaviour
    {
        private RectTransform rootTransform;

        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI tmpro;

        private bool IsShowing => animator.gameObject.activeSelf;

        private void Start()
        {
            rootTransform = GetComponent<RectTransform>();
            rootTransform.transform.SetParent(tmpro.transform);
        }

        public void Show()
        {
            if (tmpro.text == string.Empty)
            {
                if (IsShowing) Hide();
                return;
            }

            // 文字ごとのジオメトリ情報を取得する
            // 動画だと ForceMeshUpdate をしているが、自分の環境だと文字がすべて消えてしまう
            // そのため無効化する
            // tmpro.ForceMeshUpdate();

            animator.gameObject.SetActive(true);

            TMP_CharacterInfo finalCharacter = tmpro.textInfo.characterInfo[tmpro.textInfo.characterCount - 1];
            Vector3 targetPos = new Vector3(finalCharacter.bottomRight.x + finalCharacter.pointSize,
                finalCharacter.bottomRight.y, 0);

            // 親からの相対座標で横にずらす
            rootTransform.localPosition = targetPos;
        }

        public void Hide()
        {
            animator.gameObject.SetActive(false);
        }
    }
}