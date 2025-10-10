using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.FeaturePanel;
using Core.LogicalLine;
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

        public string Inject(string text, bool injectTag = true, bool injectVariable = true)
        {
            if (injectTag) text = InjectTags(text);
            if (injectVariable) text = InjectVariables(text);
            return text;
        }

        /// <summary>
        /// `text` という文章が与えられる
        ///  この `text` に登録済みのタグが含まれていればそれを置換する
        /// </summary>
        private string InjectTags(string text)
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

        /// <summary>
        /// `value` のなかに $hoge があったら VariableStore から取り出して Inject する
        /// </summary>
        private string InjectVariables(string value)
        {
            var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_PATTERN).ToList();

            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                var variableName = match.Value.TrimStart(VariableStore.VARIABLE_IDENTIFIER);

                if (!VariableStore.Instance.TryGetVariableValue(variableName, out object variableValue))
                {
                    Debug.LogError($"Variable {variableValue} not found in string assignment.");
                    continue;
                }

                var lengthToBeRemoved = match.Index + match.Length > value.Length
                    ? value.Length - match.Index
                    : match.Length;

                value = value.Remove(match.Index, lengthToBeRemoved);
                value = value.Insert(match.Index, variableValue.ToString());
            }

            return value;
        }
    }
}