using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
    public class CheckPopUp : BaseUI
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;



        // 이벤트
        public System.Action OnConfirmClicked;
        public System.Action OnCancelClicked;

        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Popup;
            }
        }

        protected override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "popup_confirm",
                "popup_cancel"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            SetupAutoLocalization();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            if (confirmButton != null)
                GetEventWithSFX("ConfirmButton").Click += OnConfirmButtonClicked;
            
            if (cancelButton != null)
                GetBackEvent("CancelButton").Click += OnCancelButtonClicked;
        }

        private void OnConfirmButtonClicked(PointerEventData data)
        {
            Debug.Log("[CheckPopUp] 확인 버튼 클릭");
            OnConfirmClicked?.Invoke();
            UIManager.Instance.ClosePopup();
        }

        private void OnCancelButtonClicked(PointerEventData data)
        {
            Debug.Log("[CheckPopUp] 취소 버튼 클릭");
            OnCancelClicked?.Invoke();
            UIManager.Instance.ClosePopup();
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
            if (confirmButton != null)
            {
                var textComponent = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                }
            }
        }

        /// <summary>
        /// 취소 버튼 텍스트 설정
        /// </summary>
        public void SetCancelText(string text)
        {
            if (cancelButton != null)
            {
                var textComponent = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = text;
                }
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

        public static void ShowCheckPopUp(string message, string confirmText = "확인", string cancelText = "취소",
                                        System.Action confirmCallback = null, System.Action cancelCallback = null)
        {
            UIManager.Instance.ShowPopUpAsync<CheckPopUp>((popup) => {
                if (popup != null)
                {
                    popup.SetMessage(message);
                    popup.SetConfirmText(confirmText);
                    popup.SetCancelText(cancelText);
                    popup.SetConfirmCallback(confirmCallback);
                    popup.SetCancelCallback(cancelCallback);
                }
            });
        }
    }
}