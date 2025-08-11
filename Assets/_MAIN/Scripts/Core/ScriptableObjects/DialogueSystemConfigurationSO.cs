using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DialogueSystemConfigurationAsset",
        menuName = "DialogueSystem/DialogueSystemConfigurationAsset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        private const float DEFAULT_FONTSIZE_DIALOGUE = 40;
        private const float DEFAULT_FONTSIZE_NAME = 40;

        public CharacterConfigSO characterConfigSO;

        public Color defaultTextColor;
        public TMP_FontAsset defaultFont;

        public float dialogueFontScale = 1f;
        public float defaultDialogueFontSize = DEFAULT_FONTSIZE_DIALOGUE;
        public float defaultNameFontSize = DEFAULT_FONTSIZE_NAME;
    }
}