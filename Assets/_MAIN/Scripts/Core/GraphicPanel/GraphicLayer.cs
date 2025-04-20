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

        public void SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null)
        {
            Texture texture = Resources.Load<Texture>(filePath);

            if (texture == null)
            {
                Debug.LogError($"Could not load texture from '{filePath}'");
                return;
            }

            SetTexture(texture, transitionSpeed, blendingTexture, filePath);
        }

        /// <summary>
        /// 新しい GraphicObject を生成し 指定した texture を設定する (副作用を持つ)
        /// </summary>
        public void SetTexture(Texture texture, float transitionSpeed = 1f, Texture blendingTexture = null, string filePath = "")
        {
            CreateGraphic(texture, transitionSpeed, filePath, blendingTexture: blendingTexture);
        }

        public void SetVideo(string filePath, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null)
        {
            VideoClip clip = Resources.Load<VideoClip>(filePath);

            if (clip == null)
            {
                Debug.LogError($"Could not load clip from '{filePath}'");
                return;
            }

            SetVideo(clip, transitionSpeed, useAudio, blendingTexture, filePath);
        }

        public void SetVideo(VideoClip clip, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, string filePath = "")
        {
            CreateGraphic(clip, transitionSpeed, filePath, useAudio, blendingTexture);
        }

        /// <summary>
        /// GraphicObject を生成して blendingTexture で FadeIn 表示させる
        /// </summary>
        private void CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudio = true, Texture blendingTexture = null)
        {
            GraphicObject graphicObject = null;

            if (graphicData is Texture)
                graphicObject = new GraphicObject(this, filePath, graphicData as Texture);
            if (graphicData is VideoClip)
                graphicObject = new GraphicObject(this, filePath, graphicData as VideoClip, useAudio);
            
            if (currentGraphicObject != null) oldGraphicObjects.Add(currentGraphicObject);

            currentGraphicObject = graphicObject;
            currentGraphicObject?.FadeIn(transitionSpeed, blendingTexture);
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