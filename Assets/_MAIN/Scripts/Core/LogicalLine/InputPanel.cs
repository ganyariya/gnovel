using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Core.LogicalLine
{
    public class InputPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _acceptButton;

        private CanvasGroupController _canvasGroupController;

        public string LastInputUserText { get; private set; } = string.Empty;
        public bool IsEnteringInput { get; private set; } = false;

        public void Start()
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
            SetCanvasGroupStatus(true);
            IsEnteringInput = true;
        }

        public void Hide()
        {
            _canvasGroupController.Hide();
            SetCanvasGroupStatus(false);
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

        /// <summary>
        /// https://docs.unity3d.com/ja/2018.4/Manual/class-CanvasGroup.html
        /// CanvasGroup の状態を変更することで UI 要素にアクセスできるようにする
        ///
        /// alpha = 0 としても blockRaycasts = true のままだと、裏側にある UI メニューなどにアクセスできない
        /// そのため InputPanel を非表示にするときは interactable と blockRaycasts を切る
        ///
        /// </summary>
        /// - interactable = 自分自身がクリック可能になるか？
        /// - blockRaycasts = 自分の「裏側」がクリック可能になるか？
        private void SetCanvasGroupStatus(bool active)
        {
            _canvasGroup.interactable = active;
            _canvasGroup.blocksRaycasts = active;
        }

        private bool HasValidInput()
        {
            return _inputField.text.Length > 0;
        }
    }
}