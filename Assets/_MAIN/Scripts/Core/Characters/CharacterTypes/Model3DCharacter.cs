using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Characters
{
    public class Model3DCharacter : Character
    {
        public Model3DCharacter(string name, CharacterConfig config, GameObject prefab, string rootCharacterFolder) : base(name, config, prefab, rootCharacterFolder)
        {
        }
    }
}