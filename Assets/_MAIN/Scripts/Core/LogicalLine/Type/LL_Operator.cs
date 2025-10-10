using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.DisplayDialogue;
using Core.ScriptParser;
using UnityEngine;
using static Core.LogicalLine.LogicalLineUtils;

namespace Core.LogicalLine.Type
{
    public class LL_Operator : ILogicalLine
    {
        public string keyword { get; }

        public bool Match(DialogueLineData lineData)
        {
            return Regex.Match(lineData.rawData.Trim(), Expressions.REGEX_OPERATOR_LINE).Success;
        }

        public IEnumerator Execute(DialogueLineData lineData, DialogueSystemController dialogueSystemController)
        {
            var trimmedLine = lineData.rawData.Trim();
            var parts = Regex.Split(trimmedLine, Expressions.REGEX_ARITHMATIC);

            if (parts.Length < 3)
            {
                Debug.LogError($"Operator line is invalid. {trimmedLine}");
                yield break;
            }

            var variableName = parts[0].Trim().TrimStart(VariableStore.VARIABLE_IDENTIFIER);
            var op = parts[1].Trim();
            // parts の先頭 2 つを省いたものを remainingParts に移す ($a = 10 + 10 + 2 などの可能性があり、3 より大きくなる可能性がある)
            var remainingParts = new string[parts.Length - 2]; // ArraySize: Length - 2
            Array.Copy(parts, 2, remainingParts, 0, remainingParts.Length);

            var rhsValue = Expressions.EvaluateRhsExpression(remainingParts);
            if (rhsValue == null) yield break;

            ProcessOperator(variableName, op, rhsValue);
        }

        private void ProcessOperator(string variableName, string op, object rhsValue)
        {
            if (!VariableStore.Instance.TryGetVariableValue(variableName, out object currentValue))
            {
                VariableStore.Instance.CreateVariable(variableName, rhsValue);
                return;
            }

            switch (op)
            {
                case "=":
                    VariableStore.Instance.TrySetValue(variableName, rhsValue);
                    break;
                case "+=":
                    VariableStore.Instance.TrySetValue(variableName, Concatenate(rhsValue, currentValue));
                    break;
                case "-=":
                    VariableStore.Instance.TrySetValue(variableName, Convert.ToDouble(currentValue) - Convert.ToDouble(rhsValue));
                    break;
                case "*=":
                    VariableStore.Instance.TrySetValue(variableName, Convert.ToDouble(currentValue) * Convert.ToDouble(rhsValue));
                    break;
                case "/=":
                    VariableStore.Instance.TrySetValue(variableName, Convert.ToDouble(currentValue) / Convert.ToDouble(rhsValue));
                    break;
                default:
                    throw new ArgumentException($"Operator {op} is not supported.");
            }
        }

        private static object Concatenate(object value, object currentValue)
        {
            if (value is string) return currentValue.ToString() + value;
            return Convert.ToDouble(currentValue) + Convert.ToDouble(value);
        }
    }
}