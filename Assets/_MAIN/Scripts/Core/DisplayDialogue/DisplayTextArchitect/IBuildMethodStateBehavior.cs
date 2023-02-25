using System.Collections;
using UnityEngine;

namespace Core.DisplayDialogue
{
    public interface IBuildMethodStateBehavior
    {
        public IEnumerator Building();
        public void Prepare();
        public void ForceComplete();
        public BuildMethod GetBuildMethod();
    }

}
