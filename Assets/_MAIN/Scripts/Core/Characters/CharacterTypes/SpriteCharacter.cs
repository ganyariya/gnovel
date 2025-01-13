using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Characters
{
    public class SpriteCharacter : Character
    {
        private const string RENDERER_LIST_ROOT_NAME = "Renderers";

        private CanvasGroup canvasGroup => rootRectTransform.GetComponent<CanvasGroup>();

        public List<CharacterSpriteLayer> spriteLayers { get; private set; } = new List<CharacterSpriteLayer>();

        public SpriteCharacter(string name, CharacterConfig config, GameObject prefab) : base(name, config, prefab)
        {
            // キャラクタ作成時は非表示にする
            canvasGroup.alpha = 0;
            CollectDefaultLayers();
        }

        /// <summary>
        /// prefab 生成時に animator 配下にある
        /// `Image` コンポーネントをすべて取得して CharacterSpriteLayer を生成して収集する
        /// </summary>
        private void CollectDefaultLayers()
        {
            Transform rendererRoot = animator.transform.Find(RENDERER_LIST_ROOT_NAME);

            if (rendererRoot == null) return;

            for (int i = 0; i < rendererRoot.childCount; i++)
            {
                Transform child = rendererRoot.GetChild(i);
                Image rendererImage = child.GetComponent<Image>();

                if (rendererImage == null) continue;

                CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                spriteLayers.Add(layer);
                child.name = $"Layer:{i}";
            }
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