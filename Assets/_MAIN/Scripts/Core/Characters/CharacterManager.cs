using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.DisplayDialogue;
using Core.ScriptableObjects;
using UnityEngine;

namespace Core.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        [SerializeField] private RectTransform _spriteCharacterPanel = null;
        /// <summary>
        /// Panel RectTransform 配下に sprite character をそれぞれ生成して配置する
        /// </summary>
        public RectTransform spriteCharacterPanel => _spriteCharacterPanel;
        [SerializeField] private RectTransform _live2DCharacterPanel = null;
        public RectTransform live2DCharacterPanel => _live2DCharacterPanel;
        [SerializeField] private RectTransform _model3DCharacterPanel = null;
        public RectTransform model3DCharacterPanel => _model3DCharacterPanel;

        /// <summary>
        /// CreateCharacter(guard1:name as Generic:prefabName) のようにすることで同じ prefab のキャラを複数用意できるようにする
        /// </summary>
        private const string CHARACTER_PREFAB_CAST_ID = " as ";

        private const string CHARACTER_NAME_ID = "<character-name-id>";

        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}";
        public string characterPrefabNameFormat => $"Character-[{CHARACTER_NAME_ID}]";
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}";

        public string FormatCharacterPath(string path, string characterName) => path.Replace(CHARACTER_NAME_ID, characterName);

        /// <summary>
        /// DialogueSystemController のシングルトンを介して
        /// Unity 上で設定されたキャラクタの設定を取得する
        /// </summary>
        private CharacterConfigSO characterConfigSO => DialogueSystemController.instance.dialogSystemConfig.characterConfigSO;

        public List<Character> AllCharacters => characters.Values.ToList();

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

        public bool HasCharacter(string characterName) => characters.ContainsKey(characterName.ToLower());

        public Character CreateCharacter(string characterName, bool revealAfterCreation = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning("Character with name " + characterName + " already exists.");
                return characters[characterName.ToLower()];
            }

            var characterInfo = GetCharacterInfo(characterName);
            var character = CreateAppropriateCharacterFromInfo(characterInfo);

            characters.Add(characterInfo.name.ToLower(), character);

            if (revealAfterCreation) character.Show();

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
            info.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, prefabName);

            return info;
        }

        private GameObject FetchCharacterPrefab(string characterName)
        {
            string path = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(path);
        }

        private Character CreateAppropriateCharacterFromInfo(CharacterInfo info)
        {
            switch (info.config.characterType)
            {
                case Character.CharacterType.Text:
                    return new TextCharacter(info.name, info.config);
                case Character.CharacterType.Sprite or Character.CharacterType.SpriteSheet:
                    return new SpriteCharacter(info.name, info.config, info.prefab, info.rootCharacterFolder);
                case Character.CharacterType.Live2D:
                    return new Live2DCharacter(info.name, info.config, info.prefab, info.rootCharacterFolder);
                case Character.CharacterType.Model3D:
                    return new Model3DCharacter(info.name, info.config, info.prefab, info.rootCharacterFolder);
                default:
                    Debug.LogError("Character type not recognized.");
                    throw new Exception();
            }
        }

        /// <summary>
        /// priority にしたがって画面上のキャラクタの表示順をソートする
        /// priority が大きいほど画面の手前側のレイヤに表示される
        /// </summary>
        public void SortCharacters()
        {
            List<Character> activeCharacters =
                characters
                .Values
                .Where(c => c.rootRectTransform.gameObject.activeInHierarchy && c.isVisible)
                .ToList();
            List<Character> inactiveCharacters =
                characters
                .Values
                .Except(activeCharacters)
                .ToList();

            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));
            List<Character> sortedCharacters = activeCharacters.Concat(inactiveCharacters).ToList();

            SortCharacterIndicesOnHierarchy(sortedCharacters);
        }

        /// <summary>
        /// 指定されたキャラクタの表示優先度を上げる
        /// 
        /// 未指定のキャラクタの priority は変わらないが, 指定したキャラはそれらより大きくなるため注意
        /// </summary>
        public void SortCharacters(string[] characterNames)
        {
            List<Character> targetCharacters =
                characterNames
                .Select(x => GetCharacter(x))
                .Where(c => c != null).ToList();
            List<Character> remainCharacters =
                characters
                .Values
                .Except(targetCharacters)
                .OrderBy(c => c.priority)
                .ToList();

            int maxPriority = remainCharacters.Count > 0 ? remainCharacters.Max(c => c.priority) : 0;
            for (int i = 0; i < targetCharacters.Count; i++)
            {
                // 無限ループにならないように false
                targetCharacters[i].SetPriority(maxPriority + i + 1, autoSortCharactersOnUI: false);
            }

            List<Character> sortedCharacters = remainCharacters.Concat(targetCharacters).ToList();
            SortCharacterIndicesOnHierarchy(sortedCharacters);
        }

        private void SortCharacterIndicesOnHierarchy(List<Character> sortedCharacters)
        {
            for (int i = 0; i < sortedCharacters.Count; i++)
            {
                sortedCharacters[i].rootRectTransform.SetSiblingIndex(i);
                sortedCharacters[i].CallbackOnSort(i);
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

            public string rootCharacterFolder;

            public CharacterConfig config;
            public GameObject prefab;
        }
    }
}
