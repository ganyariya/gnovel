using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.DisplayDialogue;
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
                
                public bool HasScenario => Lines is { Count: > 0 };
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
             * variableName は db.var を許容する
             */
            public static readonly string REGEX_OPERATOR_LINE = @"^\$[a-zA-Z0-9_.]+\s*(?:[+\-*/]=|=)\s*";

            public static readonly string BOOLEAN_EXCLAMATION_MARK = "!";

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

        public static class Conditions
        {
            /// <summary>
            /// `()` キャプチャグループで囲んでいるため、演算子も含んで分割される
            /// ex:
            /// "a == b" → ["a", "==", "b"]
            /// "x>y&&true" → ["x", ">", "y", "&&", true]
            /// </summary>
            public static readonly string REGEX_CONDITIONAL_OPERATOR = @"(==|!=|>=|<=|>|<|&&|\|\|)";

            /// <summary>
            /// 単項もしくは2項の条件式を評価する
            /// ex: ($name == "ganyariya"), ($boolValue), (!false) ($a && $b) 
            /// </summary>
            public static bool EvaluateCondition(string conditionExpression)
            {
                // TagManager 経由によって  if($boolValue) → if(true) に置換される
                conditionExpression =
                    TagManager.Instance.Inject(conditionExpression, injectTag: true, injectVariable: true);
                var parts = Regex
                    .Split(conditionExpression, REGEX_CONDITIONAL_OPERATOR)
                    .Select(s => s.Trim())
                    .ToArray();

                // 文字列は unwrap する
                for (var i = 0; i < parts.Length; i++)
                    if (parts[i].StartsWith("\"") && parts[i].EndsWith("\""))
                        parts[i] = parts[i].Substring(1, parts[i].Length - 2);

                if (parts.Length == 1)
                {
                    if (bool.TryParse(parts[0], out var boolValue)) return boolValue;

                    Debug.LogError($"invalid condition: {conditionExpression}");
                    return false;
                }

                return parts.Length == 3
                    ? EvaluateExpression(parts[0], parts[1], parts[2])
                    : throw new InvalidOperationException($"invalid condition: {conditionExpression}");
            }

            /// <summary>
            /// `T (l, r) を引数に取り bool を返す` 関数シグネチャを OperatorFunc[T] と定義する
            /// </summary>
            private delegate bool OperatorFunc<in T>(T left, T right);

            private static readonly Dictionary<string, OperatorFunc<bool>> BoolOperators = new()
            {
                { "&&", (l, r) => l && r },
                { "||", (l, r) => l || r },
                { "==", (l, r) => l == r },
                { "!=", (l, r) => l != r },
            };

            private static readonly Dictionary<string, OperatorFunc<float>> FloatOperators = new()
            {
                { "==", Mathf.Approximately },
                { "!=", (l, r) => !Mathf.Approximately(l, r) },
                { ">", (l, r) => l > r },
                { ">=", (l, r) => l >= r },
                { "<", (l, r) => l < r },
                { "<=", (l, r) => l <= r },
            };

            private static readonly Dictionary<string, OperatorFunc<int>> IntOperators = new()
            {
                { "==", (l, r) => l == r },
                { "!=", (l, r) => l != r },
                { ">", (l, r) => l > r },
                { ">=", (l, r) => l >= r },
                { "<", (l, r) => l < r },
                { "<=", (l, r) => l <= r },
            };

            private static bool EvaluateExpression(string left, string op, string right)
            {
                if (bool.TryParse(left, out var leftBool) && bool.TryParse(right, out var rightBool))
                    return BoolOperators[op].Invoke(leftBool, rightBool);
                if (int.TryParse(left, out var leftInt) && int.TryParse(right, out var rightInt))
                    return IntOperators[op].Invoke(leftInt, rightInt);
                if (float.TryParse(left, out var leftFloat) && float.TryParse(right, out var rightFloat))
                    return FloatOperators[op].Invoke(leftFloat, rightFloat);

                return op switch
                {
                    "==" => left == right,
                    "!=" => left != right,
                    _ => throw new InvalidOperationException($"invalid operator: {op}")
                };
            }
        };
    }
}