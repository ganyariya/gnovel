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

        private static readonly string HOME_DIRECTORY_SYMBOL = "~/";

        private static readonly string ResourcesGraphicsPath = "Graphics/";
        public static readonly string ResourcesBackgroundImagePath = $"{ResourcesGraphicsPath}BG Images/";
        public static readonly string ResourcesBackgroundVideoPath = $"{ResourcesGraphicsPath}BG Videos/";
        public static readonly string ResourcesBlendTexturePath = $"{ResourcesGraphicsPath}Transition Effects/";

        private static readonly string ResourcesAudioPath = "Audio/";

        public static readonly string ResourcesSfxPath = $"{ResourcesAudioPath}SFX/";
        public static readonly string ResourcesVoicePath = $"{ResourcesAudioPath}Voices/";

        public static readonly string ResourcesAmbiencePath = $"{ResourcesAudioPath}Ambience/";
        public static readonly string ResourcesBgmPath = $"{ResourcesAudioPath}Music/";

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