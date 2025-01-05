using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Characters
{
    public abstract class Character
    {
        public string name;
        public RectTransform root;

        public Character(string name)
        {
            this.name = name;
            this.root = null;
        }

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D,
        }
    }
}