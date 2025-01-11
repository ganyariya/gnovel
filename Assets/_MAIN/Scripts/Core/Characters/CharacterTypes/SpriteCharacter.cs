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
            RevealOnInstantiation();
        }

        /// <summary>
        /// キャラクタを強制的にゆっくり表示する
        /// TODO: 将来的にはここをパラメータ化してもよい（作成時に alpha=0, alpha=1 などを切り替えられるようにする）
        /// </summary>
        private void RevealOnInstantiation()
        {
            canvasGroup.alpha = 0;
            Show();
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