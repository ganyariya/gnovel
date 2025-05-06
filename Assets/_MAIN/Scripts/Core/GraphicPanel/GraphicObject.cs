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

        /// <summary>
        /// BlendTexture を設定できる 画面遷移 Material
        /// </summary>
        private const string TRANSITION_MATERIAL_PATH = "Materials/layerTransitionMaterial";

        private const string TRANSITION_MATERIAL_FIELD_COLOR = "_Color";
        private const string TRANSITION_MATERIAL_FIELD_MAIN_TEXTURE = "_MainTex";
        private const string TRANSITION_MATERIAL_FIELD_BLEND_TEXTURE = "_BlendTex";
        private const string TRANSITION_MATERIAL_FIELD_BLEND = "_Blend";
        private const string TRANSITION_MATERIAL_FIELD_ALPHA = "_Alpha";

        /// <summary>
        /// RawImage renderer を介して `動画 or 画像` を表示する
        /// - 画像の場合は material.texture に texture を
        /// - 動画の場合は material.texture に renderTexture (VideoPlayer が renderTexture に書き込む) を
        /// 設定する
        /// </summary>
        public RawImage renderer = null;

        private readonly VideoPlayer videoPlayer = null;
        private readonly AudioSource audio = null;

        private GraphicLayer layer;

        private string graphicPath { get; set; }
        private string originalGraphicName { get; set; }
        private string RendererName => string.Format(NAME_FORMAT, originalGraphicName);

        private Coroutine fadingInCoroutine = null;
        private Coroutine fadingOutCoroutine = null;
        private bool IsFadingIn => fadingInCoroutine != null;
        private bool IsFadingOut => fadingOutCoroutine != null;

        public bool IsVideo => videoPlayer != null;

        private GraphicPanelManager manager => GraphicPanelManager.instance;

        /// <summary>
        /// 新しい RawImage GameObject を生成して
        // Texture を RawImage に設定する
        /// </summary>
        public GraphicObject(GraphicLayer graphicLayer, string graphicPath, Texture texture, bool immediate)
        {
            this.graphicPath = graphicPath;
            this.layer = graphicLayer;

            // GraphicLayer に新しい object を追加して RawImage に表示したいテクスチャを設定する
            GameObject gameObject = new();
            // 1-Background
            //  Layer: 0
            //    Graphic - [ImageName]
            // のように 登録する
            gameObject.transform.SetParent(graphicLayer.transform);
            renderer = gameObject.AddComponent<RawImage>();
            renderer.texture = texture; // transition material を利用しているため, renderer.texture は設定してもしていなくても正常に表示される 

            originalGraphicName = texture.name;
            renderer.name = RendererName;

            InitGraphic(texture, immediate);
        }

        /// <summary>
        /// 新しい rawImage ゲームおジェクトを生成し
        /// かつ 新しい RenderTexture コンポーネントを Attach したうえで
        /// VideoClip の内容を RenderTexture に書き込むことで動画を表示する
        /// </summary>
        public GraphicObject(GraphicLayer graphicLayer, string graphicPath, VideoClip videoClip, bool useAudio,
            bool immediate)
        {
            this.graphicPath = graphicPath;
            this.layer = graphicLayer;

            // GraphicLayer に新しい object を追加して RawImage に表示したいテクスチャを設定する
            GameObject gameObject = new();
            gameObject.transform.SetParent(graphicLayer.transform);
            renderer = gameObject.AddComponent<RawImage>();

            originalGraphicName = videoClip.name;
            renderer.name = RendererName;

            // RenderTexture に VideoClip の内容を書き込んでいく
            RenderTexture renderTexture = new(Mathf.RoundToInt(videoClip.width), Mathf.RoundToInt(videoClip.height), 0);
            renderer.texture = renderTexture; // transition material を利用しているため, renderer.texture は設定してもしていなくても正常に表示される 
            InitGraphic(renderTexture, immediate);

            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            audio = gameObject.AddComponent<AudioSource>();

            videoPlayer.playOnAwake = true;
            videoPlayer.isLooping = true;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
            // renderTexture に書き込んでいく
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            audio.volume = immediate ? 1 : 0;
            if (!useAudio) audio.mute = true;
            videoPlayer.SetTargetAudioSource(0, audio); // audio に向かって音声を吐き出す

            videoPlayer.frame = 0;
            videoPlayer.Prepare();
            videoPlayer.Play();

            // なぜか音声が useAudio にかかわらず流れてしまうため enabled を調整することで適用する
            videoPlayer.enabled = false;
            videoPlayer.enabled = true;
        }

        /// <summary>
        /// RawImage renderer の位置を初期化する
        /// Renderer の material に texture を設定する
        /// </summary>
        private void InitGraphic(Texture texture, bool immediate)
        {
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = Vector3.one;

            RectTransform rect = renderer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;

            renderer.material = FetchTransitionMaterial();
            float startingOpacity = immediate ? 1 : 0;
            renderer.material.SetFloat(TRANSITION_MATERIAL_FIELD_BLEND, startingOpacity);
            renderer.material.SetFloat(TRANSITION_MATERIAL_FIELD_ALPHA, startingOpacity);
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

        public Coroutine FadeIn(float speed = 1f, Texture blend = null)
        {
            if (IsFadingOut) manager.StopCoroutine(fadingOutCoroutine);
            if (IsFadingIn) return fadingInCoroutine;
            return fadingInCoroutine = manager.StartCoroutine(Fading(true, 1, speed, blend));
        }

        public Coroutine FadeOut(float speed = 1f, Texture blend = null)
        {
            if (IsFadingIn) manager.StopCoroutine(fadingInCoroutine);
            if (IsFadingOut) return fadingOutCoroutine;
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
                if (IsVideo) audio.volume = opacity;
                yield return null;
            }

            fadingInCoroutine = fadingOutCoroutine = null;

            if (target == 0) Destroy();
            else DestroyBacksideGraphics(); // 2 枚目の GraphicObject を追加するときに 1 枚目を消す
        }

        private void Destroy()
        {
            if (layer.currentGraphicObject != null && layer.currentGraphicObject.renderer == renderer)
            {
                layer.currentGraphicObject = null;
            }

            Object.Destroy(renderer.gameObject);
        }

        private void DestroyBacksideGraphics()
        {
            layer.DestroyOldGraphics();
        }
    }
}