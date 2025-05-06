using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Core.GraphicPanel
{
    /// <summary>
    /// 2-BackGround
    ///  - Layer: 0 (Image)
    ///  - Layer: 1 (Movie)
    ///  - Layer: 2 (Image)
    /// のように各パネルに複数のレイヤを配置する
    /// このクラスは各レイヤそれぞれを担当する
    /// </summary>
    public class GraphicLayer
    {
        private const string LAYER_NAME_FORMAT = "Layer: {0}";

        public int depth { get; private set; } = 0;
        public Transform transform { get; private set; } = null;

        public GraphicObject currentGraphicObject;
        private List<GraphicObject> oldGraphicObjects = new();

        public string LayerName => string.Format(LAYER_NAME_FORMAT, depth);

        public GraphicLayer(int depth)
        {
            this.depth = depth;
        }

        public bool IsTargetLayer(int targetDepth) => this.depth == targetDepth;

        public void SetTransform(Transform transform)
        {
            this.transform = transform;
        }

        public Coroutine SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null,
            bool immediate = false)
        {
            Texture texture = Resources.Load<Texture>(filePath);

            if (texture == null)
            {
                Debug.LogError($"Could not load texture from '{filePath}'");
                return null;
            }

            return SetTexture(texture, transitionSpeed, blendingTexture, filePath, immediate);
        }

        /// <summary>
        /// 新しい GraphicObject を生成し 指定した texture を設定する (副作用を持つ)
        /// </summary>
        public Coroutine SetTexture(Texture texture, float transitionSpeed = 1f, Texture blendingTexture = null,
            string filePath = "", bool immediate = false)
        {
            return CreateGraphic(texture, transitionSpeed, filePath, blendingTexture: blendingTexture,
                immediate: immediate);
        }

        public Coroutine SetVideo(string filePath, float transitionSpeed = 1f, bool useAudio = true,
            Texture blendingTexture = null, bool immediate = false)
        {
            VideoClip clip = Resources.Load<VideoClip>(filePath);

            if (clip == null)
            {
                Debug.LogError($"Could not load clip from '{filePath}'");
                return null;
            }

            return SetVideo(clip, transitionSpeed, useAudio, blendingTexture, filePath, immediate);
        }

        public Coroutine SetVideo(VideoClip clip, float transitionSpeed = 1f, bool useAudio = true,
            Texture blendingTexture = null, string filePath = "", bool immediate = false)
        {
            return CreateGraphic(clip, transitionSpeed, filePath, useAudio, blendingTexture, immediate);
        }

        /// <summary>
        /// GraphicObject を生成して blendingTexture で FadeIn 表示させる
        /// </summary>
        private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudio = true,
            Texture blendingTexture = null, bool immediate = false) where T : Object
        {
            GraphicObject graphicObject = null;

            if (graphicData is Texture)
                graphicObject = new GraphicObject(this, filePath, graphicData as Texture, immediate);
            if (graphicData is VideoClip)
                graphicObject = new GraphicObject(this, filePath, graphicData as VideoClip, useAudio, immediate);

            if (currentGraphicObject != null) oldGraphicObjects.Add(currentGraphicObject);

            currentGraphicObject = graphicObject;
            if (!immediate) return currentGraphicObject?.FadeIn(transitionSpeed, blendingTexture);

            DestroyOldGraphics();
            return null;
        }

        public void DestroyOldGraphics()
        {
            foreach (var g in oldGraphicObjects)
            {
                Object.Destroy(g.renderer.gameObject);
            }

            oldGraphicObjects.Clear();
        }

        public void Clear()
        {
            currentGraphicObject?.FadeOut();

            foreach (var g in oldGraphicObjects)
            {
                g.FadeOut();
            }
        }
    }
}