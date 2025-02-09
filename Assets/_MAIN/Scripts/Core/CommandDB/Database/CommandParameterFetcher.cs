using System.Collections.Generic;

namespace Core.CommandDB
{
    public class CommandParameterFetcher
    {
        private const char PARAMETER_IDENTIFIER = '-';

        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        public CommandParameterFetcher(string[] parameters)
        {
            static bool isFloat(string s) => float.TryParse(s, out _);

            for (int i = 0; i < parameters.Length; i++)
            {
                // -y -20 などに対応できるようにする
                // ただし、現状 [`-10` で 10 を変数にする] はしないため, 起こらないはず
                if (parameters[i].StartsWith(PARAMETER_IDENTIFIER) && !isFloat(parameters[i]))
                {
                    // 動画と実装を変えている
                    // https://www.youtube.com/watch?v=z47NZQhTAh0&list=PLGSox0FgA5B58Ki4t4VqAPDycEpmkBd0i&index=40

                    // 次がないのであればスキップする
                    if (i + 1 >= parameters.Length) continue;

                    string key = parameters[i];
                    string value = parameters[i + 1];
                    this.parameters.Add(key, value);
                    i++;
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
