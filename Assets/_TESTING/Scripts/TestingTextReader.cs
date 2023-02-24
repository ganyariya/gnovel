using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Testing
{
    public class TestingTextReader : MonoBehaviour
    {
        public string textFileName = "testScript.txt";
        // AssetFile は拡張子がいらない
        public string assetFileName = "testResourceScript";

        void Start()
        {
            StartCoroutine(ReadTextFile());
            StartCoroutine(ReadAssetFile());
        }

        IEnumerator ReadTextFile()
        {
            var lines = TextReader.ReadApplicationDataPathTextFile(textFileName, false);
            foreach (string line in lines)
            {
                Debug.Log(line);
            }

            yield return null;
        }

        IEnumerator ReadAssetFile()
        {
            yield return new WaitForSeconds(2);

            var lines = TextReader.ReadTextAsset(assetFileName, true);
            foreach (string line in lines)
            {
                Debug.Log(line);
            }
        }
    }
}