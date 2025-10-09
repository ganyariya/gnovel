using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.FeaturePanel;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// テキストスクリプトにおいて、動的に変更したい箇所がある
    /// 例: 今日は <date> だね！
    ///
    /// このとき `<xxx>` というフォーマットでテキストスクリプトを書いておき、それを TagManager で動的に置換する
    /// </summary>
    public sealed class TagManager
    {
        private static readonly Lazy<TagManager> _lazy = new Lazy<TagManager>(() => new TagManager());
        public static TagManager Instance => _lazy.Value;

        private readonly Dictionary<string, Func<string>> _tags;
        private readonly Regex _tagRegex = new("<\\w+>");

        /// <summary>
        /// https://www.youtube.com/watch?v=3uipcCWxRgQ&list=PLGSox0FgA5B58Ki4t4VqAPDycEpmkBd0i&index=73
        /// 動画だとすべての変数とメソッドを static にしているが、ユニットテストなどを考慮してシングルトンにする
        /// </summary>
        private TagManager()
        {
            _tags = new Dictionary<string, Func<string>>
            {
                { "<mainChara>", () => "Avira" },
                { "<time>", () => DateTime.Now.ToString("hh:mm tt") },
                { "<playerLevel>", () => "15" },
                { "<input>", () => InputPanel.Instance.LastInputUserText },
            };
        }

        /// <summary>
        /// `text` というもともとの文章が与えられる
        /// `text` に登録済みのタグが含まれていればそれを置換する
        /// </summary>
        public string Inject(string text)
        {
            if (string.IsNullOrEmpty(text) || !_tagRegex.IsMatch(text))
            {
                return text;
            }

            foreach (Match match in _tagRegex.Matches(text))
            {
                if (_tags.TryGetValue(match.Value, out var tagFunc))
                {
                    text = text.Replace(match.Value, tagFunc());
                }
            }

            return text;
        }
    }
}