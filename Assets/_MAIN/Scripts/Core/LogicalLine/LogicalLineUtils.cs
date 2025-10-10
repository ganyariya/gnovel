using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;

namespace Core.LogicalLine
{
    public static class LogicalLineUtils
    {
        public static class Encapsulator
        {
            /// <summary>
            /// choice "choiceTitle"
            /// {
            ///   -choiceA
            ///     ganyariya "hello"
            ///   -choiceB
            ///     ganyariya "hoge"
            /// }
            ///
            /// 上記フォーマットの選択肢領域を表すデータ構造
            /// </summary>
            public class EncapsulationData
            {
                public List<string> Lines;
                public int StartIndex;
                public int EndIndex;
            }

            private const char ENCAPSULATION_START = '{';
            private const char ENCAPSULATION_END = '}';

            public static bool IsEncapsulationStart(string s) => s.Trim().StartsWith(ENCAPSULATION_START);
            public static bool IsEncapsulationEnd(string s) => s.Trim().StartsWith(ENCAPSULATION_END);

            /// <param name="useHeaderEncapsulation">
            /// true であれば choice "title" {} という Header(title) と {}(encapsulation) も含める
            /// false の場合は title 行と {} を取り除く
            ///   ネストされている choice {} は取り出したいので encapsulationDepth をチェックする
            /// </param>
            public static EncapsulationData Encapsulate(Conversation conversation, int startIndex,
                bool useHeaderEncapsulation = true)
            {
                var exceptionMsg =
                    $"Choice syntax is invalid. {conversation.Progress}/{conversation.Count} {conversation.CurrentLine}";

                var encapsulationDepth = 0;
                List<string> lines = new();

                for (var i = startIndex; i < conversation.Count; i++)
                {
                    var line = conversation.GetTargetLine(i);

                    if (
                        useHeaderEncapsulation // use であれば必ずすべての行を追加
                        || encapsulationDepth > 0 && !IsEncapsulationEnd(line) // ネストされている LogicalLine は lines に含める
                    ) lines.Add(line);


                    // `choice` 構文が nest されることがある
                    // そのため `{` の数を数えて level = 0 のときの領域を fetch する
                    if (IsEncapsulationStart(line))
                    {
                        encapsulationDepth++;
                        continue;
                    }

                    if (IsEncapsulationEnd(line))
                    {
                        encapsulationDepth--;
                        if (encapsulationDepth == 0)
                        {
                            return new EncapsulationData { Lines = lines, StartIndex = startIndex, EndIndex = i };
                        }
                    }
                }

                throw new FormatException(exceptionMsg);
            }
        }

        public static class Expressions
        {
            public static HashSet<string> OPERATORS = new HashSet<string>()
            {
                "+", "-", "*", "/", "=",
                "-=", "+=", "*=", "/=",
            };

            /**
             * 単項算術演算子 & 複合代入演算子
             * +, -, =, +=, -=, ==, ...
             */
            public static readonly string REGEX_ARITHMATIC = "([-+*/=]=?)";

            /**
             * $variableName {+-/*}?= AnyString
             */
            public static readonly string REGEX_OPERATOR_LINE = @"^\$\w+\s*(=|\+=|-=|\*=|/=|)\s*";

            private static readonly string BOOLEAN_EXCLAMATION_MARK = "!";

            /// <summary>
            /// $money = 100 + $money * $tax
            /// における `=` より右側を評価する
            /// </summary>
            public static object EvaluateRhsExpression(string[] expressionParts)
            {
                var operandStrings = new List<string>();
                var operatorStrings = new List<string>();

                foreach (var t in expressionParts)
                {
                    var part = t.Trim();
                    if (part == string.Empty) continue;

                    if (OPERATORS.Contains(part)) operatorStrings.Add(part);
                    else operandStrings.Add(part);
                }

                var operands = operandStrings.Select(EvaluateRightOperand).ToList();

                Evaluate_Operations(operatorStrings, operands, new[] { "*", "/" });
                Evaluate_Operations(operatorStrings, operands, new[] { "+", "-" });

                return operands[0];
            }

            private static void Evaluate_Operations(List<string> operators, List<object> operands,
                string[] targetOperators)
            {
                for (var i = 0; i < operators.Count; i++)
                {
                    var op = operators[i];
                    if (!targetOperators.Contains(op)) continue;

                    var (l, r) = (Convert.ToDouble(operands[i]), Convert.ToDouble(operands[i + 1]));

                    double result;
                    switch (op)
                    {
                        case "*":
                            result = l * r;
                            break;
                        case "/":
                            if (r == 0) throw new DivideByZeroException();
                            result = l / r;
                            break;
                        case "+":
                            result = l + r;
                            break;
                        case "-":
                            result = l - r;
                            break;
                        default:
                            throw new InvalidOperationException($"invalid operator: {op}");
                    }

                    operands[i] = result;

                    operands.RemoveAt(i + 1); // 処理済み演算子とオペランドを消す
                    operators.RemoveAt(i);
                    i--;
                }
            }


            private static object EvaluateRightOperand(string value)
            {
                var negate = false;
                if (value.StartsWith(BOOLEAN_EXCLAMATION_MARK))
                {
                    negate = true;
                    value = value[1..];
                }

                if (value.StartsWith(VariableStore.VARIABLE_IDENTIFIER)) // if variable
                {
                    var variableName = value.TrimStart(VariableStore.VARIABLE_IDENTIFIER);
                    if (!VariableStore.Instance.HasVariable(variableName))
                        throw new ArgumentException($"Variable {variableName} is not defined.");

                    VariableStore.Instance.TryGetVariableValue<object>(variableName, out var variableValue);
                    if (variableValue is bool b && negate) return !b;
                    return variableValue;
                }

                // 文字列の場合、 $moneyTxt = "money is $money; <mainChara>" のように変数やタグを評価しないといけない
                if (value.StartsWith('\"') && value.EndsWith('\"'))
                {
                    value = TagManager.Instance.Inject(value);
                    return value.Trim('"');
                }

                // int, float, bool
                if (int.TryParse(value, out var intValue)) return intValue;
                if (float.TryParse(value, out var floatValue)) return floatValue;
                if (bool.TryParse(value, out var boolValue)) return negate ? !boolValue : boolValue;
                return value; // パースできなかったため string として扱う
            }
        }
    }
}