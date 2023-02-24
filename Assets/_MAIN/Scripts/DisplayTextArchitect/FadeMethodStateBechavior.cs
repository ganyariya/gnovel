using System.Collections;
using UnityEngine;

public class FadeMethodStateBehavior : IBuildMethodStateBehavior
{
    private readonly DisplayTextArchitect arch;

    public FadeMethodStateBehavior(DisplayTextArchitect displayTextArchitect)
    {
        arch = displayTextArchitect;
    }

    public IEnumerator Building()
    {
        yield return null;
    }

    public void Prepare()
    {
        arch.TmProText.text = arch.PrevText;
        arch.TmProText.text += arch.TargetText;
        arch.TmProText.maxVisibleCharacters = int.MaxValue;
        arch.TmProText.ForceMeshUpdate();

        /*
        https://coposuke.hateblo.jp/entry/2020/06/07/020330
        https://qiita.com/Asalato/items/f84ca0a0924a273a34fc#%E5%AE%9F%E8%A3%85%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6
        - TextMeshPro の文字は 4 つの頂点で構成される。 
        - textInfo は array(meshInfo, textInfo) を持ち i 番目は i 個目の文字の情報を指す
        */
        var textInfo = arch.TmProText.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var cInfo = textInfo.characterInfo[i];
            if (!cInfo.isVisible) continue;
            var vertexColors = textInfo.meshInfo[cInfo.materialReferenceIndex].colors32;
            for (int v = 0; v < 4; v++) vertexColors[cInfo.vertexIndex + v] = new Color(arch.TextColor.r, arch.TextColor.g, arch.TextColor.b, i < arch.PrevText.Length ? 1 : 0);
        }
        arch.TmProText.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.Colors32);
    }

    public void ForceComplete()
    {

    }

    public BuildMethod GetBuildMethod()
    {
        return BuildMethod.fade;
    }
}
