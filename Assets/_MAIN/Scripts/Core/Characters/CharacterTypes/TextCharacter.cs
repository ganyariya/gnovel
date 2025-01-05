using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Characters
{
    public class TextCharacter : Character
    {
        public TextCharacter(string name, CharacterConfig config) : base(name, config)
        {
            Debug.Log("TextCharacter created name: " + name);
        }
    }
}
