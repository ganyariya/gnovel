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

        [SerializeField] private RectTransform _characterLayer = null;
        public RectTransform characterLayer => _characterLayer;

        /// <summary>
        /// CreateCharacter(guard1:name as Generic:prefabName) のようにすることで同じ prefab のキャラを複数用意できるようにする
        /// </summary>
        private const string CHARACTER_PREFAB_CAST_ID = " as ";

        private const string CHARACTER_NAME_ID = "<character-name-id>";
        private string characterRootPath => $"Characters/{CHARACTER_NAME_ID}";
        private string characterPrefabPath => $"{characterRootPath}/Character-[{CHARACTER_NAME_ID}]";
        private string FormatCharacterPath(string path, string characterName) => path.Replace(CHARACTER_NAME_ID, characterName);

        /// <summary>
        /// DialogueSystemController のシングルトンを介して
        /// Unity 上で設定されたキャラクタの設定を取得する
        /// </summary>
        private CharacterConfigSO characterConfigSO => DialogueSystemController.instance.dialogSystemConfig.characterConfigSO;

        private void Awake()
        {
            instance = this;
        }

        public CharacterConfig GetCharacterConfig(string characterName)
        {
            return characterConfigSO.FetchTargetCharacterConfig(characterName);
        }

        public Character GetCharacter(string characterName, bool create = false)
        {
            if (characters.ContainsKey(characterName.ToLower())) return characters[characterName.ToLower()];
            if (create) return CreateCharacter(characterName);

            return null;
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

            string[] splitNames = characterName.Split(CHARACTER_PREFAB_CAST_ID, System.StringSplitOptions.RemoveEmptyEntries);
            string name = splitNames[0];
            string prefabName = splitNames.Length > 1 ? splitNames[1] : splitNames[0];

            info.name = name;
            info.prefabName = prefabName;

            // prefab 名をつかって config, prefab を取得する
            info.config = characterConfigSO.FetchTargetCharacterConfig(prefabName);
            info.prefab = FetchCharacterPrefab(prefabName);

            return info;
        }

        private GameObject FetchCharacterPrefab(string characterName)
        {
            string path = FormatCharacterPath(characterPrefabPath, characterName);
            return Resources.Load<GameObject>(path);
        }

        private Character CreateAppropriateCharacterFromInfo(CharacterInfo info)
        {
            switch (info.config.characterType)
            {
                case Character.CharacterType.Text:
                    return new TextCharacter(info.name, info.config);
                case Character.CharacterType.Sprite or Character.CharacterType.SpriteSheet:
                    return new SpriteCharacter(info.name, info.config, info.prefab);
                case Character.CharacterType.Live2D:
                    return new Live2DCharacter(info.name, info.config, info.prefab);
                case Character.CharacterType.Model3D:
                    return new Model3DCharacter(info.name, info.config, info.prefab);
                default:
                    Debug.LogError("Character type not recognized.");
                    throw new Exception();
            }
        }

        private class CharacterInfo
        {
            /// <summary>
            /// Dialogue ファイル上で一意に認識できるキャラ名
            /// </summary>
            public string name;

            /// <summary>

            /// 表示したい画像 prefab の名前
            /// </summary>
            public string prefabName;
            public CharacterConfig config;
            public GameObject prefab;
        }
    }
}
