using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.GraphicPanel
{
    public class GraphicPanelManager : MonoBehaviour
    {
        public const float DEFAULT_TRANSITION_SPEED = 3f;

        public static GraphicPanelManager instance { get; private set; }

        /// <summary>
        /// ノベルゲームに出てくるすべての GraphicPanel
        /// 0 - BackGround
        /// 1 - Character
        /// ...
        /// </summary>
        [SerializeField] private GraphicPanel[] graphicPanels;

        public void Awake()
        {
            instance = this;
        }

        public GraphicPanel FetchPanel(string panelName)
        {
            panelName = panelName.ToLower();
            foreach (var panel in graphicPanels)
            {
                if (panel.IsTargetPanel(panelName)) return panel;
            }

            return null;
        }
    }
}