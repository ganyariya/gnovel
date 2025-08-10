using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core.DisplayDialogue
{
    public class DialogueContinuePrompt : MonoBehaviour
    {
        private RectTransform rootTransform;

        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI tmpro;

        public bool IsShowing => animator.gameObject.activeSelf;

        private void Start()
        {
            rootTransform = GetComponent<RectTransform>();
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
            rootTransform.transform.SetParent(tmpro.transform);

            TMP_CharacterInfo finalCharacter = tmpro.textInfo.characterInfo[tmpro.textInfo.characterCount - 1];
            Vector3 targetPos = finalCharacter.bottomRight;
            float characterWidth = finalCharacter.pointSize * 1.0f;
            targetPos = new Vector3(targetPos.x + characterWidth, targetPos.y, 0);

            // 親からの相対座標で横にずらす
            rootTransform.localPosition = targetPos;
        }

        public void Hide()
        {
            animator.gameObject.SetActive(false);
        }
    }
}