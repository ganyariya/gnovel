using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Characters
{
    public class CharacterSpriteLayer
    {
        private CharacterManager characterManager => CharacterManager.instance;

        public int layerIndex { get; private set; } = 0;

        /// <summary>
        /// キャラクタの画像 = renderer を設定する
        /// </summary>
        public Image renderer { get; private set; } = null;

        private CanvasGroup rendererCanvasGroup => renderer.GetComponent<CanvasGroup>();

        /// <summary>
        /// 過去の rendererList の alpha を少しずつ減らすことで
        /// 画像の fade out を実現する
        /// </summary>
        private List<CanvasGroup> oldRenderers = new List<CanvasGroup>();

        public CharacterSpriteLayer(Image defaultRenderer, int layerIndex = 0)
        {
            this.layerIndex = layerIndex;
            this.renderer = defaultRenderer;
        }
    }
}

