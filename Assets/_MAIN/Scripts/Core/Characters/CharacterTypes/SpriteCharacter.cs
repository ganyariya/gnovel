using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Characters
{
    public class SpriteCharacter : Character
    {
        private const string RENDERER_LIST_ROOT_NAME = "Renderers";
        private const string DEFAULT_SPRITE_SHEET_NAME = "Default";
        private const char SPRITE_SHEET_SPRITE_DELIMITER = '-';

        /// <summary>
        /// SpriteCharacter Root に設定されている CanvasGroup
        /// alpha 値を変更して画像の表示・非表示を制御するために使用される
        /// </summary>
        private CanvasGroup rootCanvasGroup => rootRectTransform.GetComponent<CanvasGroup>();

        /// <summary>
        /// SpriteCharacter がそれぞれ、自身を描画するサブ spriteLayer の list を持つ
        /// </summary>
        public List<CharacterSpriteLayer> spriteLayers { get; private set; } = new List<CharacterSpriteLayer>();

        private string imageAssetDirectory = "";

        public override bool isVisible
        {
            get { return isRevealing || rootCanvasGroup.alpha == 1; }
            set { rootCanvasGroup.alpha = value ? 1.0f : 0.0f; }
        }

        public SpriteCharacter(string name, CharacterConfig config, GameObject prefab, string rootCharacterFolder) : base(name, config, prefab, rootCharacterFolder)
        {
            // キャラクタ作成時は非表示にする
            rootCanvasGroup.alpha = INITIAL_ENABLE ? 1.0f : 0.0f;
            this.imageAssetDirectory = rootCharacterFolder + "/Images";

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
                Image rendererImage = child.GetComponentInChildren<Image>();

                if (rendererImage == null) continue;

                CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                spriteLayers.Add(layer);
                child.name = $"Layer:{i}";
            }
        }

        public void SetSprite(Sprite sprite, int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= spriteLayers.Count) return;

            spriteLayers[layerIndex].SetSprite(sprite);
        }

        public Sprite FetchSpriteFromResources(string spriteName)
        {
            if (
                config.characterType != CharacterType.SpriteSheet
                && config.characterType != CharacterType.Sprite
            ) return null;

            if (config.characterType == CharacterType.Sprite)
            {
                return Resources.Load<Sprite>($"{imageAssetDirectory}/{spriteName}");
            }

            // SpriteSheet

            string[] data = spriteName.Split(SPRITE_SHEET_SPRITE_DELIMITER);
            Sprite[] sprites;

            // {textureName}-{spriteName} で指定された場合
            // textureName Texture2D から複数の Sprites を取得して spriteName に一致するものを返す
            if (data.Length == 2)
            {
                string textureName = data[0];
                spriteName = data[1];
                sprites = Resources.LoadAll<Sprite>($"{imageAssetDirectory}/{textureName}");
            }
            else
            {
                // Default Texture2D から spriteName に一致するものを返す
                sprites = Resources.LoadAll<Sprite>($"{imageAssetDirectory}/{DEFAULT_SPRITE_SHEET_NAME}");
            }

            if (sprites.Length == 0) Debug.LogWarning($"No default sprite found in {imageAssetDirectory}, {spriteName}");
            return Array.Find(sprites, sprite => sprite.name == spriteName);
        }

        public Coroutine ExecuteTransitionSprite(Sprite sprite, int layerIndex = 0, float speed = 1)
        {
            if (layerIndex < 0 || layerIndex >= spriteLayers.Count) return null;
            return spriteLayers[layerIndex].ExecuteTransitionSprite(sprite, speed);
        }

        protected override IEnumerator ShowingOrHiding(bool isShow, float speedMultiplier = 1f)
        {
            float targetAlpha = isShow ? 1.0f : 0.0f;
            CanvasGroup cg = rootCanvasGroup;

            // 目的の alpha になるまで Coroutine で alpha を変更する
            while (cg.alpha != targetAlpha)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, 1.0f * Time.deltaTime * speedMultiplier);
                yield return null;
            }

            revealingCoroutine = null;
            hidingCoroutine = null;
        }

        public override void SetColor(Color color)
        {
            // Entity に目指すべき色を保持する
            base.SetColor(color);

            // 実際に表示されるべき色（ハイライト考慮）を用意する
            color = displayColor;

            // 実際の色変更は各レイヤーごとに行う
            foreach (var layer in spriteLayers)
            {
                // Coroutine で実行中の色遷移処理を停止する
                layer.StopChangingColor();

                layer.SetColor(color);
            }
        }

        protected override IEnumerator ChangingColor(Color color, float speed = 1)
        {
            var coroutines = new List<Coroutine>();

            foreach (var layer in spriteLayers) coroutines.Add(layer.ExecuteChangingColor(displayColor, speed));
            foreach (var c in coroutines) yield return c;

            changingColorCoroutine = null;
        }

        protected override IEnumerator Highlighting(bool highlighted, float speed = 1f, bool immediate = false)
        {
            Color targetColor = displayColor;

            var coroutines = new List<Coroutine>();

            foreach (var layer in spriteLayers)
            {
                if (immediate) layer.SetColor(targetColor);
                else coroutines.Add(layer.ExecuteChangingColor(targetColor, speed));
            }
            foreach (var c in coroutines) yield return c;

            yield return null; // null を一旦返して親元のコルーチンを代入させる。その後 null を入れる。
            highlightingCoroutine = null;
        }

        public override IEnumerator FlippingToDirection(bool facingLeft, float speed = 1f, bool immediate = false)
        {
            var coroutines = new List<Coroutine>();
            foreach (var layer in spriteLayers)
            {
                if (facingLeft) coroutines.Add(layer.FlipToLeft(speed, immediate));
                else coroutines.Add(layer.FlipToRight(speed, immediate));
            }
            foreach (var c in coroutines) yield return c;
        }

        public override void CastingExpression(int layer, string expression)
        {
            var sprite = FetchSpriteFromResources(expression);

            if (sprite == null)
            {
                Debug.LogWarning($"No sprite found in {imageAssetDirectory}, {expression}");
                return;
            }

            ExecuteTransitionSprite(sprite, layer);
        }
    }
}