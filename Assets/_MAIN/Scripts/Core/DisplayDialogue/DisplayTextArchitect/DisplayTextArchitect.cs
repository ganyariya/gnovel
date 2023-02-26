using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Core.DisplayDialogue
{
    /// <summary>
    /// コンストラクタで与えられた TMProUGUI GameObject に対して動的にテキストを表示する
    /// 構文解析などは行わず Instant / TypeWriter で表示するのみ
    /// テキストの描画自体は TMProUGUI GameObject 自身が行うので DisplayTextArchitect の知るところではない
    /// </summary>
    public class DisplayTextArchitect
    {
        /// <summary>
        /// DisplayTextArchitect が扱う TMP_Text GameObject。
        /// TmProText に対してテキストを更新する。
        /// </summary>
        public TMP_Text TmProText => tmProUI != null ? tmProUI : tmProWorld;
        private readonly TextMeshProUGUI tmProUI;
        private readonly TextMeshPro tmProWorld;

        public string CurrentText => TmProText.text;
        /// <summary>
        /// ビルド中のテキスト
        /// </summary>
        public string TargetText { get; private set; } = "";
        /// <summary>
        /// 前回のテキスト。 クリアせずそのままテキストを下に追加する Append で利用する
        /// </summary>
        public string PrevText { get; private set; }
        /// <summary>
        /// 実際に画面に出力されるテキスト
        /// </summary>
        public string FullTargetText => PrevText + TargetText;

        private DisplayMethod _currentDisplayMethod = DisplayMethod.typewriter;
        public DisplayMethod CurrentDisplayMethod
        {
            get { return _currentDisplayMethod; }
            set { _currentDisplayMethod = value; SetupDisplayMethodStateBehavior(); }
        }

        public Color TextColor { get { return TmProText.color; } set { TmProText.color = value; } }

        public float BaseSpeed { get { return baseBaseSpeed * baseSpeedMultiplier; } set { baseSpeedMultiplier = value; } }
        private const float baseBaseSpeed = 1;
        private float baseSpeedMultiplier = 1;

        public int AppearCharactersNumPerFrame { get { return BaseSpeed <= 2f ? appearCharactersSpeedMultiplier : BaseSpeed <= 2.5f ? appearCharactersSpeedMultiplier * 2 : appearCharactersSpeedMultiplier * 3; } }
        private const int appearCharactersSpeedMultiplier = 1;

        public bool HurryUp { get; set; }

        /// <summary>
        /// TMProText に実行させているコルーチン処理
        /// </summary>
        private Coroutine displayingProcess = null;
        public bool IsDisplaying => displayingProcess != null;
        public IDisplayMethodStateBehavior displayMethodStateBehavior;

        public DisplayTextArchitect(TextMeshProUGUI tmProUI, TextMeshPro tmProWorld)
        {
            this.tmProUI = tmProUI;
            this.tmProWorld = tmProWorld;
            SetupDisplayMethodStateBehavior();
        }

        /// <summary>
        /// 前回テキストをすべてクリアして新たな text を TMProText に出力する
        /// </summary>
        public Coroutine Display(string text)
        {
            PrevText = "";
            TargetText = text;

            Stop(); // TMProText に対して this.Displaying を非同期で実行させる
            displayingProcess = TmProText.StartCoroutine(Displaying());
            return displayingProcess;
        }

        /// <summary>
        /// 前回のテキストに対して新たに text を追加する
        /// </summary>
        public Coroutine AppendDisplay(string text)
        {
            PrevText = TmProText.text;
            TargetText = text;

            Stop();
            displayingProcess = TmProText.StartCoroutine(Displaying());
            return displayingProcess;
        }

        public void Stop()
        {
            if (!IsDisplaying) return;
            TmProText.StopCoroutine(displayingProcess); // TMPro で動いている Coroutine を止める
            displayingProcess = null;
        }

        /// <summary>
        /// Display/Append で指定された text を mode に従って非同期に表示する (tmPro で実行される)
        /// </summary>
        private IEnumerator Displaying()
        {
            Prepare();
            yield return displayMethodStateBehavior.Displaying();
            OnComplete();
        }

        /// <summary>
        /// text を処理する前の準備を行う (typeWriter では prevText を用意する)
        /// </summary>
        private void Prepare()
        {
            displayMethodStateBehavior.Prepare();
        }

        private void OnComplete()
        {
            displayingProcess = null;
            HurryUp = false;
        }

        public void ForceComplete()
        {
            displayMethodStateBehavior.ForceComplete();
        }

        private void SetupDisplayMethodStateBehavior()
        {
            switch (CurrentDisplayMethod)
            {
                case DisplayMethod.instant:
                    displayMethodStateBehavior = new InstantMethodStateBehavior(this); break;
                case DisplayMethod.typewriter:
                    displayMethodStateBehavior = new TypeWriterMethodStateBehavior(this); break;
                case DisplayMethod.fade:
                    displayMethodStateBehavior = new FadeMethodStateBehavior(this); break;
            }
        }
    }
}