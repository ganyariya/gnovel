using System;
using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using Core.LogicalLine;
using UnityEngine;

namespace Testing
{
    public class TestingInputPanel : MonoBehaviour
    {
        [SerializeField] private InputPanel _inputPanel;

        public void Start()
        {
            StartCoroutine(Running());
        }

        IEnumerator Running()
        {
            var ganyariya = CharacterManager.instance.CreateCharacter("ganyariya") as SpriteCharacter;
            yield return ganyariya.Say("\"Hello!, What's your name?\"");

            _inputPanel.Show("your name");
            while (_inputPanel.IsEnteringInput) yield return null;

            var name = _inputPanel.LastInputUserText;
            yield return ganyariya.Say($"\"Oh! Hi! {name}\"");
        }
    }
    
}

