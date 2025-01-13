using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Characters
{
    public class Live2DCharacter : Character
    {
        public Live2DCharacter(string name, CharacterConfig config, GameObject prefab, string rootCharacterFolder) : base(name, config, prefab, rootCharacterFolder)
        {
        }
    }
}
