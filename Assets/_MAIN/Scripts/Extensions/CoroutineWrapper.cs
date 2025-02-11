using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    /// <summary>
    /// UnityEngine.Coroutine を扱いやすくするヘルパークラス
    /// 
    /// Coroutine を生成した MonoBehavior も参照することで
    /// このヘルパークラスを通じて Coroutine を停止できる
    /// </summary>
    public class CoroutineWrapper
    {
        private MonoBehaviour behaviourIssuer { get; set; }
        private Coroutine coroutine { get; set; }

        /// <summary>
        /// Coroutine が終了済みかどうか？
        /// `Stop()` メソッドを呼ぶことで IsDone = true にして終了済み扱いにする
        /// </summary>
        public bool IsDone { get; private set; }

        public CoroutineWrapper(MonoBehaviour behaviourIssuer, Coroutine coroutine)
        {
            this.behaviourIssuer = behaviourIssuer;
            this.coroutine = coroutine;
            this.IsDone = false;
        }

        /// <summary>
        /// Coroutine を発行者によって停止させる
        /// IsDone = true の副作用も持つ
        /// </summary>
        public void Stop()
        {
            // 実行中の Coroutine を停止させる (もしキャラが動いていたら途中でとまる)
            // IEnumerator でない場合は coroutine = null になるためスキップする
            if (coroutine != null) behaviourIssuer.StopCoroutine(coroutine);
            // Coroutine が終了したことを伝える
            IsDone = true;
        }
    }
}
