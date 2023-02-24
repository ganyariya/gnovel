using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TextReader
{

    /// <summary>
    /// Addressable からテキストを同期的に取得する
    /// </summary>
    public static List<string> ReadAddressableTextFileSync(string address, bool includeBlankLine = true)
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(address);
        var asset = handle.WaitForCompletion();
        return ReadTextAsset(asset, includeBlankLine);
    }

    /// <summary>
    /// Application.dataPath から同期的にテキストを取得する
    /// </summary>
    public static List<string> ReadApplicationDataPathTextFile(string filePath, bool includeBlankLine = true)
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

    /// <summary>
    /// アセット名を指定してResources からアセットを取得する(非推奨)
    /// </summary>
    public static List<string> ReadTextAsset(string assetName, bool includeBlankLine = true)
    {
        var textAsset = Resources.Load<TextAsset>(assetName);
        if (textAsset == null)
        {
            Debug.Log($"asset not found: {assetName}");
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
