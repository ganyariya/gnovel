using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextReader
{
    public static List<string> ReadTextFile(string filePath, bool includeBlankLine = true)
    {
        if (!filePath.StartsWith('/')) filePath = Path.Combine(UnityRuntimePathToolBox.RootApplicationDataPath, filePath);

        var lines = new List<string>();
        try
        {
            using var sr = new StreamReader(filePath);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (includeBlankLine || !string.IsNullOrEmpty(line)) lines.Add(line);
            }
        }
        catch (FileNotFoundException e)
        {
            Debug.Log($"file not found: {e}");
        }

        return lines;
    }

    public static List<string> ReadTextAsset(string assetPath, bool includeBlankLine = true)
    {
        var textAsset = Resources.Load<TextAsset>(assetPath);
        if (textAsset == null)
        {
            Debug.Log($"asset not found: {assetPath}");
            return null;
        }

        return ReadTextAsset(textAsset, includeBlankLine);
    }

    private static List<string> ReadTextAsset(TextAsset textAsset, bool includeBlankLine)
    {
        var lines = new List<string>();

        using var sr = new StringReader(textAsset.text);
        while (sr.Peek() > -1)
        {
            var line = sr.ReadLine();
            if (includeBlankLine || !string.IsNullOrEmpty(line)) lines.Add(line);
        }

        return lines;
    }
}
