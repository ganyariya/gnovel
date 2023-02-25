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
    }
}