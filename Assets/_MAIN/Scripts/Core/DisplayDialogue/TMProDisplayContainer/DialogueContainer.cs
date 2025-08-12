using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core.Characters;
using Core.ScriptableObjects;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// DialogueSystem によって利用される
    /// - DisplayTextArchitect に tmProUGUI が渡される
    /// </summary>
    [System.Serializable]
    public class DialogueContainer
    {
        private const float DEFAULT_FADE_SPEED = 1f;

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

        private CanvasGroup canvasGroup => rootGameObject.GetComponent<CanvasGroup>();

        private Coroutine showingCoroutine;
        private Coroutine hidingCoroutine;

        private bool isShowing => showingCoroutine != null;
        private bool isHiding => hidingCoroutine != null;
        private bool isFading => isShowing || isHiding;
        public bool isVisible => isShowing || canvasGroup.alpha > 0f;

        public void ApplyCharacterConfig(CharacterConfig characterConfig,
            DialogueSystemConfigurationSO dialogueSystemConfig)
        {
            nameContainer.ApplyCharacterConfig(characterConfig, dialogueSystemConfig);
            dialogueText.color = characterConfig.dialogueColor;
            dialogueText.font = characterConfig.dialogueFont;
            dialogueText.fontSize = characterConfig.dialogueFontSize * dialogueSystemConfig.dialogueFontScale;
        }

        public Coroutine Show()
        {
            if (isShowing) return null;
            if (isHiding)
            {
                DialogueSystemController.instance.StopCoroutine(hidingCoroutine);
                hidingCoroutine = null;
            }

            return showingCoroutine = DialogueSystemController.instance.StartCoroutine(Fading(1f));
        }

        public Coroutine Hide()
        {
            if (isHiding) return null;
            if (isShowing)
            {
                DialogueSystemController.instance.StopCoroutine(showingCoroutine);
                showingCoroutine = null;
            }

            return hidingCoroutine = DialogueSystemController.instance.StartCoroutine(Fading(0f));
        }

        public IEnumerator Fading(float targetAlpha)
        {
            var cg = canvasGroup;

            while (!Mathf.Approximately(cg.alpha, targetAlpha))
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * DEFAULT_FADE_SPEED);
                yield return null;
            }

            showingCoroutine = hidingCoroutine = null;
        }
    }
}