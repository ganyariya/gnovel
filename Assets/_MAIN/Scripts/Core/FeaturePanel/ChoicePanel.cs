using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.FeaturePanel
{
    public class ChoicePanel : MonoBehaviour
    {
        public static ChoicePanel Instance { get; private set; }

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private VerticalLayoutGroup _buttonLayoutGroup;
        [SerializeField] private GameObject _buttonPrefab;

        private CanvasGroupController _canvasGroupController;
        private List<ChoiceButton> _cachedChoiceButtons = new();

        public ChoiceDecision lastDecision { get; private set; }
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
            lastDecision = new ChoiceDecision(question, choices);

            _canvasGroupController.Show();
            _canvasGroupController.ChangeInteractionBehaviour(true);

            IsEnteringChoice = true;
            _titleText.text = question;

            GenerateChoices(choices);
        }

        private void GenerateChoices(string[] choices)
        {
            for (int i = 0; i < choices.Length; i++)
            {
                ChoiceButton choiceButton;
                if (i < _cachedChoiceButtons.Count)
                {
                    choiceButton = _cachedChoiceButtons[i];
                }
                else
                {
                    GameObject gameObject = Instantiate(_buttonPrefab, _buttonLayoutGroup.transform);
                    gameObject.SetActive(true);

                    Button button = gameObject.GetComponent<Button>();
                    TextMeshProUGUI textMeshProUGUI = gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
                    choiceButton = new ChoiceButton
                        { button = button, text = textMeshProUGUI, layoutElement = layoutElement };

                    _cachedChoiceButtons.Add(choiceButton);
                }
            }
        }

        private void ChoiceAnswer(int index)
        {
            lastDecision.Answer(index);
            Hide();
        }

        public void Hide()
        {
            _canvasGroupController.Hide();
            _canvasGroupController.ChangeInteractionBehaviour(false);
        }


        public class ChoiceDecision
        {
            private const int INITIAL_ANSWER_INDEX = -1;

            public string question;
            public string[] choices;
            public int answerIndex;

            public ChoiceDecision(string question, string[] choices)
            {
                this.question = question;
                this.choices = choices;
                answerIndex = INITIAL_ANSWER_INDEX;
            }

            public void Answer(int index) => answerIndex = index;
        }

        private struct ChoiceButton
        {
            public Button button;
            public TextMeshProUGUI text;
            public LayoutElement layoutElement;
        }
    }
}