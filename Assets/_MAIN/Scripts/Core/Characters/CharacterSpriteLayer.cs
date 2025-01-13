using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Characters
{
    /// <summary>
    /// あるキャラ X を描画するために必要な Layer Y の情報を持つ
    /// 
    /// 1 つの Layer Y には 1 つの Image と Z 個の過去 Image の情報を持つ
    /// そうすることで過去の alpha を少しずつ減らすことで fade out を実現する
    /// </summary>
    public class CharacterSpriteLayer
    {
        private CharacterManager characterManager => CharacterManager.instance;
        private const float DEFAULT_TRANSITION_SPEED = 3f;
        private float transitionSpeedMultiplier = 1f;


        public int layerIndex { get; private set; } = 0;

        /// <summary>
        /// キャラクタの画像 = renderer を設定する
        /// </summary>
        public Image renderer { get; private set; } = null;

        /// <summary>
        /// 毎回 GetComponent するのは面倒なので renderer から取得する
        /// </summary>
        private CanvasGroup rendererCanvasGroup => renderer.GetComponent<CanvasGroup>();

        /// <summary>
        /// 過去の rendererList の alpha を少しずつ減らすことで
        /// 画像の fade out を実現する
        /// </summary>
        private List<CanvasGroup> oldRenderCanvasGroups = new List<CanvasGroup>();

        private Coroutine transitionLayerCoroutine;
        private Coroutine levelingAlphaCoroutine;
        public bool isTransitioningLayer => transitionLayerCoroutine != null;
        public bool isLevelingAlpha => levelingAlphaCoroutine != null;

        public CharacterSpriteLayer(Image defaultRenderer, int layerIndex = 0)
        {
            this.layerIndex = layerIndex;
            this.renderer = defaultRenderer;
        }

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        /// <summary>
        /// 指定された sprite に Transition で切り替える
        /// このとき speed で古い画像と新しい画像を fade in / fade out する
        /// </summary>
        public Coroutine ExecuteTransitionSprite(Sprite sprite, float speed = 1f)
        {
            if (sprite == renderer.sprite) return null;
            if (isTransitioningLayer) characterManager.StopCoroutine(transitionLayerCoroutine);

            transitionLayerCoroutine = characterManager.StartCoroutine(TransitioningSprite(sprite, speed));
            return transitionLayerCoroutine;
        }

        private IEnumerator TransitioningSprite(Sprite sprite, float speed)
        {
            // 副作用であることに注意
            transitionSpeedMultiplier = speed;

            Image newRenderer = CopyAndRegisterRenderer(renderer.transform.parent);
            newRenderer.sprite = sprite;

            yield return ExecuteLevelingAlpha();

            transitionLayerCoroutine = null;
        }

        /// <summary>
        /// renderer を複製して新しい renderer を作成して parent 配下に設定する
        /// 副作用を多く持つことに注意する
        /// </summary>
        private Image CopyAndRegisterRenderer(Transform parent)
        {
            // renderer を複製して新しい renderer を作成して parent 配下に設定する
            Image newRenderer = Object.Instantiate(renderer, parent);
            newRenderer.name = renderer.name;

            // 現在の rendererCanvasGroup を過去 list に登録する
            oldRenderCanvasGroups.Add(rendererCanvasGroup);

            // 新しい newRenderer を instance.renderer として設定する（副作用）
            renderer = newRenderer;
            renderer.gameObject.SetActive(true);
            rendererCanvasGroup.alpha = 0f;

            return newRenderer;
        }

        private Coroutine ExecuteLevelingAlpha()
        {
            if (isLevelingAlpha) return levelingAlphaCoroutine;
            levelingAlphaCoroutine = characterManager.StartCoroutine(LevelingAlpha());
            return levelingAlphaCoroutine;
        }

        private IEnumerator LevelingAlpha()
        {
            while (
                rendererCanvasGroup.alpha < 1
                || oldRenderCanvasGroups.Any(x => x.alpha > 0)
            )
            {
                float speed = DEFAULT_TRANSITION_SPEED * transitionSpeedMultiplier * Time.deltaTime;
                rendererCanvasGroup.alpha = Mathf.MoveTowards(rendererCanvasGroup.alpha, 1, speed);

                for (int i = oldRenderCanvasGroups.Count - 1; i >= 0; i--)
                {
                    var cg = oldRenderCanvasGroups[i];
                    cg.alpha = Mathf.MoveTowards(cg.alpha, 0, speed);

                    if (cg.alpha <= 0f)
                    {
                        oldRenderCanvasGroups.RemoveAt(i);
                        Object.Destroy(cg.gameObject);
                    }
                }

                yield return null;
            }

            levelingAlphaCoroutine = null;
        }
    }
}
