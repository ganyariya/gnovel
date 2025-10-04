using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Core.FeaturePanel
{
    public class InputPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _acceptButton;

        /// <summary>
        /// singleton
        /// LogicalLine LL_Input からアクセスするため
        /// </summary>
        public static InputPanel Instance;

        private CanvasGroupController _canvasGroupController;

        public string LastInputUserText { get; private set; } = string.Empty;
        public bool IsEnteringInput { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            _canvasGroupController = new CanvasGroupController(this, _canvasGroup);
            _canvasGroup.alpha = 0;
            _acceptButton.gameObject.SetActive(false);

            _acceptButton.onClick.AddListener(OnAccept);
            _inputField.onValueChanged.AddListener(OnInputChanged);

            Hide();
        }

        public void Show(string title)
        {
            _inputField.text = string.Empty;
            _titleText.text = title;
            _canvasGroupController.Show();
            _canvasGroupController.ChangeInteractionBehaviour(true);
            IsEnteringInput = true;
        }

        public void Hide()
        {
            _canvasGroupController.Hide();
            _canvasGroupController.ChangeInteractionBehaviour(false);
        }

        private void OnAccept()
        {
            LastInputUserText = _inputField.text;
            IsEnteringInput = false;
            Hide();
        }

        private void OnInputChanged(string _)
        {
            _acceptButton.gameObject.SetActive(HasValidInput());
        }

        private bool HasValidInput()
        {
            return _inputField.text.Length > 0;
        }
    }
}