using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public string LayerName => string.Format(LAYER_NAME_FORMAT, depth);

        public GraphicLayer(int depth)
        {
            this.depth = depth;
        }

        public bool IsTargetLayer(int targetDepth)
        {
            return this.depth == targetDepth;
        }

        public void SetTransform(Transform transform)
        {
            this.transform = transform;
        }
    }
}