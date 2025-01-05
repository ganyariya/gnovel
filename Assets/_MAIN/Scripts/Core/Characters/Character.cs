using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using UnityEngine;

namespace Core.Characters
{
    public abstract class Character
    {
        public DialogueSystemController dialogueSystem => DialogueSystemController.instance;

        public string name;
        public string displayName;
        public RectTransform root;

        public Character(string name)
        {
            this.name = name;
            this.displayName = name;
            this.root = null;
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
            return dialogueSystem.Say(dialogues);
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