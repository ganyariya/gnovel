using UnityEngine;

namespace Extensions
{
    public class ColorExtension
    {
        public static Color CreateColorFromString(string s)
        {
            return s.ToLower() switch
            {
                "red" => Color.red,
                "green" => Color.green,
                "yellow" => Color.yellow,
                "black" => Color.black,
                "white" => Color.white,
                _ => Color.clear,
            };
        }
    }
}
