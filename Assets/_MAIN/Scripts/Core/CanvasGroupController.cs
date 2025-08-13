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

        public Coroutine Show()
        {
            if (isShowing) return null;
            if (isHiding)
            {
                owner.StopCoroutine(hidingCoroutine);
                hidingCoroutine = null;
            }

            return showingCoroutine = owner.StartCoroutine(Fading(1f));
        }

        public Coroutine Hide()
        {
            if (isHiding) return null;
            if (isShowing)
            {
                owner.StopCoroutine(showingCoroutine);
                showingCoroutine = null;
            }

            return hidingCoroutine = owner.StartCoroutine(Fading(0f));
        }

        private IEnumerator Fading(float targetAlpha)
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