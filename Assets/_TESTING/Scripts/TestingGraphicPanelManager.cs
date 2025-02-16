using System.Collections;
using System.Collections.Generic;
using Core.GraphicPanel;
using UnityEngine;

namespace Testing
{
    public class TestingGraphicPanelManager : MonoBehaviour
    {
        void Start()
        {
            // StartCoroutine(CreateBackGroundLayer());
            StartCoroutine(PlayVideo());
        }

        private IEnumerator PlayVideo()
        {
            GraphicPanel backGroundPanel = GraphicPanelManager.instance.FetchPanel("background");
            GraphicLayer layer = backGroundPanel.CreateLayer(0);

            yield return new WaitForSeconds(3);

            Texture blendTexture = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");

            layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", 0.01f, true, blendTexture);
        }

        private IEnumerator CreateBackGroundLayer()
        {
            GraphicPanel backGroundPanel = GraphicPanelManager.instance.FetchPanel("background");
            GraphicLayer layer = backGroundPanel.CreateLayer(0);

            yield return new WaitForSeconds(3);


            layer.SetTexture("Graphics/BG Images/01_1", 0.1f);
        }
    }
}
