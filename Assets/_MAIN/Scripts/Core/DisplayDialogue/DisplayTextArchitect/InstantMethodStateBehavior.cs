using System.Collections;

namespace Core.DisplayDialogue
{
    public class InstantMethodStateBehavior : IDisplayMethodStateBehavior
    {
        private readonly DisplayTextArchitect arch;

        public InstantMethodStateBehavior(DisplayTextArchitect displayTextArchitect)
        {
            arch = displayTextArchitect;
        }

        public IEnumerator Displaying()
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

        public DisplayMethod GetDisplayMethod()
        {
            return DisplayMethod.instant;
        }
    }
}
