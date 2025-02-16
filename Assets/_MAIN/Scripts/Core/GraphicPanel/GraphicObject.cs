using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Core.GraphicPanel
{
    public class GraphicObject
    {
        private const string NAME_FORMAT = "Graphic - [{0}]";
        private const string TRANSITION_MATERIAL_PATH = "Materials/layerTransitionMaterial";
        private const string TRANSITION_MATERIAL_FIELD_COLOR = "_Color";
        private const string TRANSITION_MATERIAL_FIELD_MAIN_TEXTURE = "_MainTex";
        private const string TRANSITION_MATERIAL_FIELD_BLEND_TEXTURE = "_BlendTex";
        private const string TRANSITION_MATERIAL_FIELD_BLEND = "_Blend";
        private const string TRANSITION_MATERIAL_FIELD_ALPHA = "_Alpha";

        private RawImage renderer = null;
        private VideoPlayer videoPlayer = null;
        private AudioSource audio = null;

        private string graphicPath { get; set; }
        private string originalTextureName { get; set; }
        private string RendererName => string.Format(NAME_FORMAT, originalTextureName);

        private Coroutine fadingInCoroutine = null;
        private Coroutine fadingOutCoroutine = null;
        private bool isFadingIn => fadingInCoroutine != null;
        private bool isFadingOut => fadingOutCoroutine != null;

        public bool IsVideo => videoPlayer != null;

        private GraphicPanelManager manager => GraphicPanelManager.instance;

        public GraphicObject(GraphicLayer graphicLayer, string graphicPath, Texture texture)
        {
            this.graphicPath = graphicPath;

            // GraphicLayer に新しい object を追加して RawImage に表示したいテクスチャを設定する
            GameObject gameObject = new();
            gameObject.transform.SetParent(graphicLayer.transform);
            renderer = gameObject.AddComponent<RawImage>();
            renderer.texture = texture;

            originalTextureName = texture.name;
            renderer.name = RendererName;

            InitGraphic(texture);

        }

        private void InitGraphic(Texture texture)
        {
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = Vector3.one;

            RectTransform rect = renderer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;

            renderer.material = FetchTransitionMaterial();
            renderer.material.SetFloat(TRANSITION_MATERIAL_FIELD_BLEND, 0);
            renderer.material.SetFloat(TRANSITION_MATERIAL_FIELD_ALPHA, 0);
            renderer.material.SetTexture(TRANSITION_MATERIAL_FIELD_MAIN_TEXTURE, texture);
        }

        private Material FetchTransitionMaterial()
        {
            var material = Resources.Load<Material>(TRANSITION_MATERIAL_PATH);
            if (material == null) return material;

            // material をそのまま使ってしまうとどこかで変更されたときに
            // 全部のオブジェクトに影響する & Resources 側に影響が出てしまう
            return new Material(material);
        }

        public Coroutine FadeIn(float speed, Texture blend = null)
        {
            if (isFadingOut) manager.StopCoroutine(fadingOutCoroutine);
            if (isFadingIn) return fadingInCoroutine;
            return fadingInCoroutine = manager.StartCoroutine(Fading(true, 1, speed, blend));
        }
        public Coroutine FadeOut(float speed, Texture blend = null)
        {
            if (isFadingIn) manager.StopCoroutine(fadingInCoroutine);
            if (isFadingOut) return fadingOutCoroutine;
            return fadingOutCoroutine = manager.StartCoroutine(Fading(false, 0, speed, blend));
        }
        private IEnumerator Fading(bool fadeIn, float target, float speed, Texture blendTexture)
        {
            var material = renderer.material;

            bool shouldBlend = blendTexture != null;

            // blendTexture を使うのであれば alpha は最初から 1 でよい
            // そうでないならば fadeIn するのか fadeOut するのかで初期 alpha が決まる
            float alpha = shouldBlend ? 1 : (fadeIn ? 0 : 1);
            float blend = shouldBlend ? (fadeIn ? 0 : 1) : 1;

            material.SetTexture(TRANSITION_MATERIAL_FIELD_BLEND_TEXTURE, blendTexture);
            material.SetFloat(TRANSITION_MATERIAL_FIELD_ALPHA, alpha);
            material.SetFloat(TRANSITION_MATERIAL_FIELD_BLEND, blend);

            string opacityParameter = shouldBlend ? TRANSITION_MATERIAL_FIELD_BLEND : TRANSITION_MATERIAL_FIELD_ALPHA;

            while (material.GetFloat(opacityParameter) != target)
            {
                float opacity = Mathf.MoveTowards(material.GetFloat(opacityParameter), target, speed * Time.deltaTime);
                material.SetFloat(opacityParameter, opacity);
                yield return null;
            }

            fadingInCoroutine = fadingOutCoroutine = null;
        }
    }
}
