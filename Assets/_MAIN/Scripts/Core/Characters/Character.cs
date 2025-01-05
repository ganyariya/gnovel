using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using TMPro;
using UnityEngine;

namespace Core.Characters
{
    public abstract class Character
    {
        public DialogueSystemController dialogueSystem => DialogueSystemController.instance;

        public string name;
        public string displayName;
        public RectTransform root;
        public CharacterConfig config;

        public Character(string name, CharacterConfig config)
        {
            this.name = name;
            this.displayName = name;
            this.root = null;
            this.config = config;
        }

        /// <summary>
        /// DialogueSystemController.Say があれば会話テキストファイルを
        /// - パース
        /// - Unity 上で会話を表示
        /// が行える
        /// 
        /// しかしアウトゲームにおいて会話テキストファイル以外で会話を表示したい場合がある
        /// そのため Character.Say を用意する
        /// </summary>
        public Coroutine Say(string dialogue) => Say(new List<string> { dialogue });

        public Coroutine Say(List<string> dialogues)
        {
            dialogueSystem.DisplaySpeakerName(displayName);
            ApplyTextConfigOnScreen();
            return dialogueSystem.Say(dialogues);
        }

        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;
        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;

        public void ApplyTextConfigOnScreen() => dialogueSystem.ApplySpeakerConfigToDialogueContainer(config);
        public void ResetConfig() => config = CharacterManager.instance.GetCharacterConfig(name);

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