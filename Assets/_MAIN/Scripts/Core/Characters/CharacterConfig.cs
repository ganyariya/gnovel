using System.Collections;
using System.Collections.Generic;
using Core.DisplayDialogue;
using TMPro;
using UnityEngine;

namespace Core.Characters
{
    /// <summary>
    /// キャラクタの設定を保持する
    /// Serializable を設定しているので Unity で設定できる
    /// 
    /// ただし、 CharacterConfigSO = ScriptableObject を介してまとめて設定する
    /// </summary>
    [System.Serializable]
    public class CharacterConfig
    {
        public string name;
        public string alias;
        public Character.CharacterType characterType;

        public Color nameColor;
        public Color dialogueColor;

        public TMP_FontAsset nameFont;
        public TMP_FontAsset dialogueFont;

        /// <summary>
        /// http://halcyonsystemblog.jp/blog-entry-792.html 
        /// 
        /// ScriptableObject の値を参照するときに大本の値を変更しないようにするため
        /// </summary>
        public CharacterConfig Copy()
        {
            return new CharacterConfig
            {
                name = name,
                alias = alias,
                characterType = characterType,
                nameColor = new Color(nameColor.r, nameColor.g, nameColor.b, nameColor.a),
                dialogueColor = new Color(dialogueColor.r, dialogueColor.g, dialogueColor.b, dialogueColor.a),
                nameFont = nameFont,
                dialogueFont = dialogueFont
            };
        }

        private static Color defaultColor => DialogueSystemController.instance.dialogSystemConfig.defaultTextColor;
        private static TMP_FontAsset defaultFont => DialogueSystemController.instance.dialogSystemConfig.defaultFont;

        public static CharacterConfig Default
        {
            get
            {
                return new CharacterConfig
                {
                    name = "Default",
                    alias = "default",
                    characterType = Character.CharacterType.Text,
                    nameColor = defaultColor,
                    dialogueColor = defaultColor,
                    nameFont = defaultFont,
                    dialogueFont = defaultFont,
                };
            }
        }
    }
}