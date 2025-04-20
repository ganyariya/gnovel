using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using Core.GraphicPanel;
using UnityEngine;

namespace Testing
{
    public class TestingGraphicPanelManager : MonoBehaviour
    {
        void Start()
        {
            // StartCoroutine(CreateBackGroundLayer());
            // StartCoroutine(PlayVideo());
            // StartCoroutine(MultipleGraphic());

            StartCoroutine(MultiLayersOnPanel());
        }

        private IEnumerator MultiLayersOnPanel()
        {
            var panel = GraphicPanelManager.instance.FetchPanel("background");
            var layer0 = panel.GetLayer(0, true);
            var layer1 = panel.GetLayer(1, true);
            
            layer0.SetVideo("Graphics/BG Videos/Fantasy Landscape");
            layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

            yield return new WaitForSeconds(2);
            
            var cinematicPanel = GraphicPanelManager.instance.FetchPanel("Cinematic");
            var cinematicLayer = cinematicPanel.GetLayer(0, true);

            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya", true);
            yield return ganyariya.Say("\"Let's take a look at a picture on the layer.\"");
            
            cinematicLayer.SetTexture("Graphics/Gallery/1");

            yield return new WaitForSeconds(2);
            cinematicLayer.Clear();
            
            yield return new WaitForSeconds(2);
            panel.Clear();
        }

        private IEnumerator MultipleGraphic()
        {
            GraphicPanel backGroundPanel = GraphicPanelManager.instance.FetchPanel("background");
            GraphicLayer layer = backGroundPanel.CreateLayer(0);
            
            yield return new WaitForSeconds(1);
            
            var blendTexture = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
            layer.SetTexture("Graphics/BG Images/01_1", 1f, blendTexture);
            yield return new WaitForSeconds(3);
            
            layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", 1f, true, blendTexture);
            yield return new WaitForSeconds(1);

            layer.currentGraphicObject.FadeOut(1);

            yield return new WaitForSeconds(2);

        } 

        private IEnumerator PlayVideo()
        {
            GraphicPanel backGroundPanel = GraphicPanelManager.instance.FetchPanel("background");
            GraphicLayer layer = backGroundPanel.CreateLayer(0);

            yield return new WaitForSeconds(1);
            
            Texture blendTexture = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
            layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", 1f, true, blendTexture);
            
            yield return new WaitForSeconds(1);

            layer.currentGraphicObject.FadeOut(1f);
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
