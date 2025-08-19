using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KYS
{
    public class CheckPopUp : BaseUI
    {
        [Header("CheckPopUp Elements")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private TextMeshProUGUI cancelText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        // 이벤트
        public System.Action OnConfirmClicked;
        public System.Action OnCancelClicked;

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(() => {
                    OnConfirmClicked?.Invoke();
                    Hide();
                });
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => {
                    OnCancelClicked?.Invoke();
                    Hide();
                });
            }
        }

        /// <summary>
        /// 메시지 설정
        /// </summary>
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        /// <summary>
        /// 확인 버튼 텍스트 설정
        /// </summary>
        public void SetConfirmText(string text)
        {
            if (confirmText != null)
            {
                confirmText.text = text;
            }
        }

        /// <summary>
        /// 취소 버튼 텍스트 설정
        /// </summary>
        public void SetCancelText(string text)
        {
            if (cancelText != null)
            {
                cancelText.text = text;
            }
        }

        /// <summary>
        /// 확인 버튼 이벤트 설정
        /// </summary>
        public void SetConfirmCallback(System.Action callback)
        {
            OnConfirmClicked = callback;
        }

        /// <summary>
        /// 취소 버튼 이벤트 설정
        /// </summary>
        public void SetCancelCallback(System.Action callback)
        {
            OnCancelClicked = callback;
        }

        protected override void OnDestroy()
        {
            // 이벤트 정리
            OnConfirmClicked = null;
            OnCancelClicked = null;
            base.OnDestroy();
        }
    }
}