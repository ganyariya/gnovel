using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.FeaturePanel
{
    public class ChoicePanel : MonoBehaviour
    {
        private const int MINIMUM_BUTTON_WIDTH = 50;
        private const int MAXIMUM_BUTTON_WIDTH = 1000;
        private const int BUTTON_PADDING_WIDTH = 40;

        public static ChoicePanel Instance { get; private set; }

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private VerticalLayoutGroup _buttonLayoutGroup;
        [SerializeField] private GameObject _buttonPrefab;

        private CanvasGroupController _canvasGroupController;
        private readonly List<ChoiceButton> _cachedChoiceButtons = new();

        public ChoiceDecision LastDecision { get; private set; }
        public bool IsEnteringChoice { get; private set; }

        public void Awake()
        {
            Instance = this;

            // TestingChoicePanel の Start メソッドで ChoicePanel.Show を呼び出すとする
            // このとき CanvasGroupController のインスタンス化を Start メソッドでやるとエラーになることがある
            // これは Start メソッドの実行順序が不同のため
            // あらかじめ Awake でインスタンス化しておく必要がある
            _canvasGroupController = new CanvasGroupController(this, _canvasGroup);
            _canvasGroupController.ChangeInteractionBehaviour(false);
            _canvasGroupController.Alpha = 0;
        }

        public void Start()
        {
        }

        public void Show(string question, string[] choices)
        {
            LastDecision = new ChoiceDecision(question, choices);

            _canvasGroupController.Show();
            _canvasGroupController.ChangeInteractionBehaviour(true);

            IsEnteringChoice = true;
            _titleText.text = question;

            GenerateChoices(choices);
        }

        private void GenerateChoices(string[] choices)
        {
            float maxWidth = 0;

            for (int i = 0; i < choices.Length; i++)
            {
                ChoiceButton choiceButton;

                if (i < _cachedChoiceButtons.Count) choiceButton = _cachedChoiceButtons[i];
                else
                {
                    var gameObject = Instantiate(_buttonPrefab, _buttonLayoutGroup.transform);
                    gameObject.SetActive(true);

                    choiceButton = new ChoiceButton
                    {
                        button = gameObject.GetComponent<Button>(),
                        text = gameObject.GetComponentInChildren<TextMeshProUGUI>(),
                        layoutElement = gameObject.GetComponent<LayoutElement>()
                    };
                    _cachedChoiceButtons.Add(choiceButton);
                }

                var index = i;
                choiceButton.text.text = choices[i];
                choiceButton.button.onClick.RemoveAllListeners();
                // button がクリックされたらその index をもとに lastDecision を更新する
                // i をそのまま渡すと Closure の問題で length - 1 の値になってしまうため注意する
                choiceButton.button.onClick.AddListener(() => ChoiceAnswer(index));

                float buttonWidth = Mathf.Clamp(
                    // TextMeshPro の Text では該当のテキストを描画するために必要な Width を自動計算してくれる
                    BUTTON_PADDING_WIDTH + choiceButton.text.preferredWidth + BUTTON_PADDING_WIDTH,
                    MINIMUM_BUTTON_WIDTH,
                    MAXIMUM_BUTTON_WIDTH
                );
                maxWidth = Mathf.Max(maxWidth, buttonWidth);
            }

            foreach (var choiceButton in _cachedChoiceButtons)
            {
                // layoutElement を操作することで AutoLayout 時における Button などの UI サイズを動的に変更できる
                // Button の Image Type を Sliced にしているため、Button のサイズを変更したとしても正しく画像が適応される
                choiceButton.layoutElement.preferredWidth = maxWidth;
            }
        }

        private void ChoiceAnswer(int index)
        {
            LastDecision.Answer(index);
            Hide();
        }

        public void Hide()
        {
            _canvasGroupController.Hide();
            _canvasGroupController.ChangeInteractionBehaviour(false);
        }

        /// <summary>
        /// 前回選択した選択肢の結果を保存する
        /// </summary>
        public class ChoiceDecision
        {
            private const int INITIAL_ANSWER_INDEX = -1;

            public string Question;
            public string[] Choices;
            public int AnswerIndex;

            public ChoiceDecision(string question, string[] choices)
            {
                this.Question = question;
                this.Choices = choices;
                AnswerIndex = INITIAL_ANSWER_INDEX;
            }

            public void Answer(int index) => AnswerIndex = index;
        }

        /// <summary>
        /// 生成された `選択肢ボタン GameObject` のコンポーネントリスト
        /// 実装しやすくするためのヘルパークラス
        /// </summary>
        private struct ChoiceButton
        {
            public Button button;
            public TextMeshProUGUI text;
            public LayoutElement layoutElement;
        }
    }
}