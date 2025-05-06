using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.GraphicPanel
{
    /// <summary>
    /// Unity において
    /// 1-Background
    /// 2-Characters
    /// 3-Overlay
    /// のように各 Panel Object を用意する
    /// このときの X-YYYY が GraphicPanel
    /// 
    /// GraphicPanel は複数の GraphicLayer で構成される
    /// </summary>
    [System.Serializable]
    public class GraphicPanel
    {
        [SerializeField] private string panelName;

        /// <summary>
        /// GraphicPanel に紐づける GameObject (Transform)
        /// 2-Characters GameObject
        /// </summary>
        [SerializeField] private GameObject rootPanel;

        public List<GraphicLayer> layers { get; private set; } = new();

        public bool IsTargetPanel(string targetPanelName)
        {
            return panelName.ToLower() == targetPanelName.ToLower();
        }

        public GraphicLayer GetLayer(int layerDepth, bool create = false)
        {
            foreach (var layer in layers)
            {
                if (layer.IsTargetLayer(layerDepth)) return layer;
            }

            if (create) return CreateLayer(layerDepth);

            return null;
        }

        /// <summary>
        /// 指定された depth の GraphicLayer を生成して
        /// GraphicPanel の children に加える（副作用）
        /// </summary>
        public GraphicLayer CreateLayer(int layerDepth)
        {
            GraphicLayer layer = new(layerDepth);

            GameObject layerObject = new(layer.LayerName);
            RectTransform rect = layerObject.AddComponent<RectTransform>();
            layerObject.AddComponent<CanvasGroup>();
            layerObject.transform.SetParent(rootPanel.transform, false);
            layer.SetTransform(layerObject.transform);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;

            int index = layers.FindIndex(l => l.depth > layerDepth);
            if (index == -1) layers.Add(layer);
            else layers.Insert(index, layer);

            foreach (var l in layers) l.transform.SetSiblingIndex(l.depth);

            return layer;
        }

        public void Clear(float transitionSpeed = 1f, Texture blendTexture = null, bool immediate = false)
        {
            foreach (var layer in layers) layer.Clear(transitionSpeed, blendTexture, immediate);
        }
    }
}