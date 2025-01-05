using System;
using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using Core.ScriptableObjects;
using UnityEngine;

namespace Core.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        /// <summary>
        /// DialogueSystemController のシングルトンを介して
        /// Unity 上で設定されたキャラクタの設定を取得する
        /// </summary>
        private CharacterConfigSO characterConfigSO => DialogueSystemController.instance.dialogSystemConfig.characterConfigSO;

        private void Awake()
        {
            instance = this;
        }

        public Character CreateCharacter(string characterName)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning("Character with name " + characterName + " already exists.");
                return characters[characterName.ToLower()];
            }

            var characterInfo = GetCharacterInfo(characterName);
            var character = CreateAppropriateCharacterFromInfo(characterInfo);

            characters.Add(characterName.ToLower(), character);

            return character;
        }

        private CharacterInfo GetCharacterInfo(string characterName)
        {
            CharacterInfo info = new CharacterInfo();

            info.name = characterName;
            info.config = characterConfigSO.FetchTargetCharacterConfig(characterName);

            return info;
        }

        private Character CreateAppropriateCharacterFromInfo(CharacterInfo info)
        {
            switch (info.config.characterType)
            {
                case Character.CharacterType.Text:
                    return new TextCharacter(info.name);
                case Character.CharacterType.Sprite or Character.CharacterType.SpriteSheet:
                    return new SpriteCharacter(info.name);
                case Character.CharacterType.Live2D:
                    return new Live2DCharacter(info.name);
                case Character.CharacterType.Model3D:
                    return new Model3DCharacter(info.name);
                default:
                    Debug.LogError("Character type not recognized.");
                    throw new Exception();
            }
        }

        private class CharacterInfo
        {
            public string name;
            public CharacterConfig config;
        }
    }
}
