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
            StartCoroutine(CreateBackGroundLayer());
        }

        private IEnumerator CreateBackGroundLayer()
        {
            GraphicPanel backGroundPanel = GraphicPanelManager.instance.FetchPanel("background");
            GraphicLayer layer = backGroundPanel.CreateLayer(0);

            yield return new WaitForSeconds(3);


            layer.SetTexture("Graphics/BG Images/01_1");
        }
    }
}
