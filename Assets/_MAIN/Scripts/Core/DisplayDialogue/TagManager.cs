using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// テキストスクリプトにおいて、動的に変更したい箇所がある
    /// 例: 今日は <date> だね！
    ///
    /// このとき `<xxx>` というフォーマットでテキストスクリプトを書いておき、それを TagManager で動的に置換する
    /// </summary>
    public class TagManager
    {
        private readonly Dictionary<string, Func<string>> tags = new();
        private readonly Regex tagRegex = new("<\\w+>");

        public TagManager()
        {
            InitializeTags();
        }

        private void InitializeTags()
        {
            /**
             * TODO:  スクリプトや設定ファイルから設定できるようにする
             */
            tags["<mainChara>"] = () => "Avira";
            tags["<time>"] = () => DateTime.Now.ToString("hh:mm tt");
            tags["<playerLevel>"] = () => "15";
            tags["<tempVal1>"] = () => "42";
        }

        /// <summary>
        /// `text` というもともとの文章が与えられる
        /// `text` に登録済みのタグが含まれていればそれを置換する
        /// </summary>
        public string Inject(string text)
        {
            if (!tagRegex.IsMatch(text))
            {
                return text;
            }

            foreach (Match match in tagRegex.Matches(text))
            {
                if (tags.TryGetValue(match.Value, out var tagFunc))
                {
                    text = text.Replace(match.Value, tagFunc());
                }
            }

            return text;
        }
    }
}