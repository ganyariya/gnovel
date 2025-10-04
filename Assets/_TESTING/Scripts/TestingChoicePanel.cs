using System;
using System.Collections;
using System.Collections.Generic;
using Core.FeaturePanel;
using UnityEngine;

namespace Testing
{
    public class TestingChoicePanel : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(Running());
        }

        private IEnumerator Running()
        {
            var choicePanel = ChoicePanel.Instance;
            
            string[] choices = {"Choice 1", "Choice 2", "Choice 3"};
            choicePanel.Show("Question", choices);
            while (choicePanel.IsEnteringChoice) yield return null;
        } 
    }
}