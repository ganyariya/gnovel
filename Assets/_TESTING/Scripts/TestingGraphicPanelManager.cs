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
            CreateBackGroundLayer();
        }

        private void CreateBackGroundLayer()
        {
            GraphicPanelManager.instance.FetchPanel("background").CreateLayer(0);
        }
    }
}
