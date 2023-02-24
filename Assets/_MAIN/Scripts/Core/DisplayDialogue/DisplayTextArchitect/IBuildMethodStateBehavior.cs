using System.Collections;
using UnityEngine;

public interface IBuildMethodStateBehavior
{
    public IEnumerator Building();
    public void Prepare();
    public void ForceComplete();
    public BuildMethod GetBuildMethod();
}

