using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class TypeWriterMethodStateBehavior : IBuildMethodStateBehavior
{
    private readonly DisplayTextArchitect arch;

    public TypeWriterMethodStateBehavior(DisplayTextArchitect displayTextArchitect)
    {
        arch = displayTextArchitect;
    }

    public IEnumerator Building()
    {
        while (arch.TmProText.maxVisibleCharacters < arch.TmProText.textInfo.characterCount)
        {
            arch.TmProText.maxVisibleCharacters += arch.HurryUp ? arch.AppearCharactersNumPerFrame * 5 : arch.AppearCharactersNumPerFrame;
            yield return new WaitForSeconds(0.015f / arch.BaseSpeed);
        }
    }

    public void Prepare()
    {
        arch.TmProText.color = arch.TmProText.color;
        arch.TmProText.maxVisibleCharacters = 0;
        arch.TmProText.text = arch.PrevText;

        // PrevText はすぐに表示できるようにする (maxVisibleCharacters)
        if (arch.PrevText != "")
        {
            arch.TmProText.ForceMeshUpdate();
            arch.TmProText.maxVisibleCharacters = arch.TmProText.textInfo.characterCount;
        }
        // text のみ増やす (maxVisibleCharacters をあとから増やしていく)
        arch.TmProText.text += arch.TargetText;
        arch.TmProText.ForceMeshUpdate();
    }

    public void ForceComplete()
    {
        arch.TmProText.maxVisibleCharacters = arch.TmProText.textInfo.characterCount;
    }

    public BuildMethod GetBuildMethod()
    {
        return BuildMethod.typewriter;
    }
}

