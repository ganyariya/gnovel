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

        private BuildMethod _currentBuildMethod = BuildMethod.typewriter;
        public BuildMethod CurrentBuildMethod
        {
            get { return _currentBuildMethod; }
            set { _currentBuildMethod = value; SetupBuildMethodStateBehavior(); }
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
        private Coroutine buildProcess = null;
        public bool IsBuilding => buildProcess != null;
        public IBuildMethodStateBehavior buildMethodStateBehavior;

        public DisplayTextArchitect(TextMeshProUGUI tmProUI, TextMeshPro tmProWorld)
        {
            this.tmProUI = tmProUI;
            this.tmProWorld = tmProWorld;
            SetupBuildMethodStateBehavior();
        }

        /// <summary>
        /// 前回テキストをすべてクリアして新たな text を TMProText に出力する
        /// </summary>
        public Coroutine Build(string text)
        {
            PrevText = "";
            TargetText = text;

            Stop(); // TMProText に対して this.Building を非同期で実行させる
            buildProcess = TmProText.StartCoroutine(Building());
            return buildProcess;
        }

        /// <summary>
        /// 前回のテキストに対して新たに text を追加する
        /// </summary>
        public Coroutine Append(string text)
        {
            PrevText = TmProText.text;
            TargetText = text;

            Stop();
            buildProcess = TmProText.StartCoroutine(Building());
            return buildProcess;
        }

        public void Stop()
        {
            if (!IsBuilding) return;
            TmProText.StopCoroutine(buildProcess); // TMPro で動いている Coroutine を止める
            buildProcess = null;
        }

        /// <summary>
        /// Build/Append で指定された text を mode に従って非同期に表示する (tmPro で実行される)
        /// </summary>
        private IEnumerator Building()
        {
            Prepare();
            yield return buildMethodStateBehavior.Building();
            OnComplete();
        }

        /// <summary>
        /// text を処理する前の準備を行う (typeWriter では prevText を用意する)
        /// </summary>
        private void Prepare()
        {
            buildMethodStateBehavior.Prepare();
        }

        private void OnComplete()
        {
            buildProcess = null;
            HurryUp = false;
        }

        public void ForceComplete()
        {
            buildMethodStateBehavior.ForceComplete();
        }

        private void SetupBuildMethodStateBehavior()
        {
            switch (CurrentBuildMethod)
            {
                case BuildMethod.instant:
                    buildMethodStateBehavior = new InstantMethodStateBehavior(this); break;
                case BuildMethod.typewriter:
                    buildMethodStateBehavior = new TypeWriterMethodStateBehavior(this); break;
                case BuildMethod.fade:
                    buildMethodStateBehavior = new FadeMethodStateBehavior(this); break;
            }
        }
    }
}