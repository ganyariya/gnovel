using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Characters
{
    public class SpriteCharacter : Character
    {
        private CanvasGroup canvasGroup => root.GetComponent<CanvasGroup>();

        public SpriteCharacter(string name, CharacterConfig config, GameObject prefab) : base(name, config, prefab)
        {
            // キャラクタ作成時は非表示にする
            canvasGroup.alpha = 0;
        }

        protected override IEnumerator ShowingOrHiding(bool isShow)
        {
            float targetAlpha = isShow ? 1.0f : 0.0f;
            CanvasGroup cg = canvasGroup;

            // 目的の alpha になるまで Coroutine で alpha を変更する
            while (cg.alpha != targetAlpha)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, 1.0f * Time.deltaTime);
                yield return null;
            }

            revealingCoroutine = null;
            hidingCoroutine = null;
        }
    }
}