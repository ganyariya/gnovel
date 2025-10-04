using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CanvasGroupController
    {
        private const float DEFAULT_FADE_SPEED = 1f;

        private MonoBehaviour owner;
        private CanvasGroup canvasGroup;

        public CanvasGroupController(MonoBehaviour owner, CanvasGroup canvasGroup)
        {
            this.owner = owner;
            this.canvasGroup = canvasGroup;
            this.showingCoroutine = this.hidingCoroutine = null;
        }

        private Coroutine showingCoroutine;
        private Coroutine hidingCoroutine;

        private bool isShowing => showingCoroutine != null;
        private bool isHiding => hidingCoroutine != null;
        private bool isFading => isShowing || isHiding;
        public bool isVisible => isShowing || canvasGroup.alpha > 0f;

        public float Alpha
        {
            get => canvasGroup.alpha;
            set => canvasGroup.alpha = value;
        }

        public Coroutine Show(float speed = 1f, bool immediate = false)
        {
            if (isShowing) return null;
            if (isHiding)
            {
                owner.StopCoroutine(hidingCoroutine);
                hidingCoroutine = null;
            }

            return showingCoroutine = owner.StartCoroutine(Fading(1f, speed, immediate));
        }

        public Coroutine Hide(float speed = 1f, bool immediate = false)
        {
            if (isHiding) return null;
            if (isShowing)
            {
                owner.StopCoroutine(showingCoroutine);
                showingCoroutine = null;
            }

            return hidingCoroutine = owner.StartCoroutine(Fading(0f, speed, immediate));
        }

        private IEnumerator Fading(float targetAlpha, float speed = 1f, bool immediate = false)
        {
            var cg = canvasGroup;

            if (immediate) cg.alpha = targetAlpha;

            while (!Mathf.Approximately(cg.alpha, targetAlpha))
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * DEFAULT_FADE_SPEED * speed);
                yield return null;
            }

            showingCoroutine = hidingCoroutine = null;
        }

        /// <summary>
        /// https://docs.unity3d.com/ja/2018.4/Manual/class-CanvasGroup.html
        /// CanvasGroup の状態を変更することで UI 要素にアクセスできるようにする
        ///
        /// alpha = 0 としても blockRaycasts = true のままだと、裏側にある UI メニューなどにアクセスできない
        /// そのため InputPanel を非表示にするときは interactable と blockRaycasts を切る
        ///
        /// </summary>
        /// - interactable = 自分自身がクリック可能になるか？
        /// - blockRaycasts = 自分の「裏側」がクリック可能になるか？
        public void ChangeInteractionBehaviour(bool interactable)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
    }
}