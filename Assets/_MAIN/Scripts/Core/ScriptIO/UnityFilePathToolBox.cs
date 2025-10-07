using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Unity Application Class
https://docs.unity3d.com/ja/2020.3/ScriptReference/Application.html
アプリケーションの実行データを保持している

Application.dataPath
Unity が各 Platform でデータを保存・取得できるパス
*/

namespace Core.ScriptIO
{
    public class UnityRuntimePathToolBox
    {
        /// <summary>
        /// GameData が置かれる RootFolderPath
        /// Application.dataPath は unity editor の場合 Assets
        /// </summary>
        public static readonly string RootApplicationDataPath = $"{Application.dataPath}/GameData";

        private const string HOME_DIRECTORY_SYMBOL = "~/";

        private const string RESOURCES_GRAPHICS_PATH = "Graphics/";
        public static readonly string ResourcesBackgroundImagePath = $"{RESOURCES_GRAPHICS_PATH}BG Images/";
        public static readonly string ResourcesBackgroundVideoPath = $"{RESOURCES_GRAPHICS_PATH}BG Videos/";
        public static readonly string ResourcesBlendTexturePath = $"{RESOURCES_GRAPHICS_PATH}Transition Effects/";

        private const string RESOURCES_AUDIO_PATH = "Audio/";
        public static readonly string ResourcesSfxPath = $"{RESOURCES_AUDIO_PATH}SFX/";
        public static readonly string ResourcesVoicePath = $"{RESOURCES_AUDIO_PATH}Voices/";
        public static readonly string ResourcesAmbiencePath = $"{RESOURCES_AUDIO_PATH}Ambience/";
        public static readonly string ResourcesBgmPath = $"{RESOURCES_AUDIO_PATH}Music/";

        public static readonly string ResourcesDialoguePath = "DialogueFiles/";
            
        public static string ResolveHomeDirectoryPath(string defaultPath, string resourceName)
        {
            if (resourceName.StartsWith(HOME_DIRECTORY_SYMBOL))
            {
                return resourceName.Substring(HOME_DIRECTORY_SYMBOL.Length);
            }

            return defaultPath + resourceName;
        }
    }
}