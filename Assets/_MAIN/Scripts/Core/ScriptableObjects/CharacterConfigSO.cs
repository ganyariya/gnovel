using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using UnityEngine;

namespace Core.ScriptableObjects
{
    /// <summary>
    /// ゲーム内に出てくるキャラクタの情報を Asset として管理する
    /// 
    /// ScriptableObject を利用することでオブジェクトに紐づけず
    /// マスタデータをアセットとして独立して管理できる
    /// https://note.com/citronworld/n/n47965ddec2ec
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterConfigAsset", menuName = "DialogueSystem/CharacterConfigAsset")]
    public class CharacterConfigSO : ScriptableObject
    {
        /// <summary>
        /// Unity 上のインスペクタでマスタデータとして設定する
        /// そのため参照メソッドしかこのクラスは持たない
        /// </summary>
        public CharacterConfig[] characterConfigs;

        /// <summary>
        /// characterName に一致する CharacterConfig をマスタデータから取得する
        /// なければデフォルト値を返す
        /// </summary>
        public CharacterConfig FetchTargetCharacterConfig(string characterName)
        {
            characterName = characterName.ToLower();

            foreach (CharacterConfig config in characterConfigs)
            {
                if (config.name.ToLower() == characterName || config.alias.ToLower() == characterName) return config.Copy();
            }

            return CharacterConfig.Default;
        }
    }
}