using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Core.ScriptParser
{
    /// <summary>
    /// speaker Name をさらにパースしたもの
    /// 名前・cast,position, layer をもつ
    /// </summary>
    public class DLD_SpeakerData
    {
        public string name;

        /// <summary>
        /// `???` のように cast する
        /// </summary>
        public string castName;

        /// <summary>
        /// 画面に表示される名前
        /// </summary>
        public string DisplayName => castName != string.Empty ? castName : name;

        public Vector2 castPosition;
        public List<(int layer, string expression)> CastExpressions { get; set; }

        private const string NAME_CAST_ID = " as ";
        private const string POSITION_CAST_ID = " at ";
        private const string EXPRESSION_CAST_ID = @" [";
        private const char POSITION_AXIS_DELIMITER = ':';
        private const char EXPRESSION_LAYER_JOINER = ',';
        private const char EXPRESSION_LAYER_DELIMITER = ':';
        private readonly static Regex parsePattern = new Regex($@"{NAME_CAST_ID}|{POSITION_CAST_ID}|{EXPRESSION_CAST_ID.Insert(EXPRESSION_CAST_ID.Length - 1, @"\")}");

        public bool HasSpeaker => DisplayName != string.Empty;

        public DLD_SpeakerData(string rawSpeaker)
        {
            var parsed = ParseSpeakerData(rawSpeaker);
            this.name = parsed.name;
            this.castName = parsed.castName;
            this.castPosition = parsed.castPosition;
            this.CastExpressions = parsed.castExpressions;
            Debug.Log(@$"DLD_SpeakerData Parsed [original={rawSpeaker}][name={name}][castName={castName}][castPosition={castPosition}][castExpressions={string.Join(',', CastExpressions.Select(x => $"{x.layer}:{x.expression}"))}]");
        }

        public DLD_SpeakerData(string name, string castName, Vector2 castPosition, List<(int layer, string expression)> castExpressions)
        {
            this.name = name;
            this.castName = castName;
            this.castPosition = castPosition;
            this.CastExpressions = castExpressions;
        }

        public static (string name, string castName, Vector2 castPosition, List<(int layer, string expression)> castExpressions) ParseSpeakerData(string rawSpeaker)
        {
            MatchCollection matches = parsePattern.Matches(rawSpeaker);
            string name = "";
            string castName = "";
            Vector2 castPosition = Vector2.zero;
            var castExpressions = new List<(int layer, string expression)>();

            if (matches.Count == 0)
            {
                name = rawSpeaker;
                return (name, castName, castPosition, castExpressions);
            }

            int index = matches[0].Index;
            name = rawSpeaker.Substring(0, index);

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int startIndex = 0, endIndex = 0;

                if (match.Value == NAME_CAST_ID)
                {
                    startIndex = match.Index + NAME_CAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    castName = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                }
                if (match.Value == POSITION_CAST_ID)
                {
                    startIndex = match.Index + POSITION_CAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    string castPositionStr = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                    string[] axis = castPositionStr.Split(POSITION_AXIS_DELIMITER, System.StringSplitOptions.RemoveEmptyEntries);

                    float.TryParse(axis[0], out castPosition.x);
                    if (axis.Length > 1) float.TryParse(axis[1], out castPosition.y);
                }
                if (match.Value == EXPRESSION_CAST_ID)
                {
                    startIndex = match.Index + EXPRESSION_CAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    string expressionStr = rawSpeaker.Substring(startIndex, endIndex - (startIndex + 1));

                    castExpressions = expressionStr.Split(EXPRESSION_LAYER_JOINER).Select(x =>
                    {
                        var parts = x.Trim().Split(EXPRESSION_LAYER_DELIMITER);
                        return (int.Parse(parts[0]), parts[1]);
                    }).ToList();
                }
            }

            return (name, castName, castPosition, castExpressions);
        }
    }
}
