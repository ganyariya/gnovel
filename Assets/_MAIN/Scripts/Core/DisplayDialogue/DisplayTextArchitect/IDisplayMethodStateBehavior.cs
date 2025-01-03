using System.Collections;
using UnityEngine;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// DisplayTextArchitect の DisplayMethod に応じた振る舞いを定義する
    /// 
    /// TypeWriter であれば 1 文字ずつ表示するときの処理を実装する
    /// </summary>
    public interface IDisplayMethodStateBehavior
    {
        public IEnumerator Displaying();
        public void Prepare();
        public void ForceComplete();
        public DisplayMethod GetDisplayMethod();
    }
}
