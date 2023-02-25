using System.Collections;

namespace Core.DisplayDialogue
{
    public class InstantMethodStateBehavior : IBuildMethodStateBehavior
    {
        private readonly DisplayTextArchitect arch;

        public InstantMethodStateBehavior(DisplayTextArchitect displayTextArchitect)
        {
            arch = displayTextArchitect;
        }

        public IEnumerator Building()
        {
            yield return null;
        }

        public void Prepare()
        {
            arch.TmProText.color = arch.TmProText.color;
            arch.TmProText.text = arch.FullTargetText;
            arch.TmProText.ForceMeshUpdate();
            arch.TmProText.maxVisibleCharacters = arch.TmProText.textInfo.characterCount;
        }

        public void ForceComplete()
        {

        }

        public BuildMethod GetBuildMethod()
        {
            return BuildMethod.instant;
        }
    }
}
