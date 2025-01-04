using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "DialogueSystemConfigurationAsset", menuName = "DialogueSystem/DialogueSystemConfigurationAsset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public CharacterConfigSO characterConfigSO;

        public Color defaultTextColor;
        public TMP_FontAsset defaultFont;
    }
}
