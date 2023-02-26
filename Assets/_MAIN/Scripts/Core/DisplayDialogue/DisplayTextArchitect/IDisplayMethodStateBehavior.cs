using System.Collections;
using UnityEngine;

namespace Core.DisplayDialogue
{
    public interface IDisplayMethodStateBehavior
    {
        public IEnumerator Displaying();
        public void Prepare();
        public void ForceComplete();
        public DisplayMethod GetDisplayMethod();
    }

}
