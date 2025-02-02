using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.CommandDB
{
    public class CommandParameterFetcher
    {
        private const char PARAMETER_IDENTIFIER = '-';

        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        public CommandParameterFetcher(string[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].StartsWith(PARAMETER_IDENTIFIER))
                {
                    string key = parameters[i];
                    string value = "";

                    if (i + 1 < parameters.Length && !parameters[i + 1].StartsWith(PARAMETER_IDENTIFIER))
                    {
                        value = parameters[i + 1];
                        i++;
                    }

                    this.parameters.Add(key, value);
                }
            }
        }

        public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameterName }, out value, defaultValue);

        /// <summary>
        /// 指定した parameterNames に一致する値を検索して $value に設定する
        /// </summary>
        /// <returns>success: true</returns>
        public bool TryGetValue<T>(string[] parameterNames, out T value, T defaultValue = default(T))
        {
            foreach (string parameterName in parameterNames)
            {
                if (parameters.TryGetValue(parameterName, out string parameterValue))
                {
                    if (TryCastParameter(parameterValue, out value))
                    {
                        return true;
                    }
                }
            }

            // parameter と一致する設定値がなければ default を返す
            value = defaultValue;
            return false;
        }

        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }
            }
            if (typeof(T) == typeof(float))
            {
                if (float.TryParse(parameterValue, out float floatValue))
                {
                    value = (T)(object)floatValue;
                    return true;
                }
            }
            if (typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
