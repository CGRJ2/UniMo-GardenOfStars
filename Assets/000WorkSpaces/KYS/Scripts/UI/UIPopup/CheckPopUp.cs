using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
    public class CheckPopUp : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string messageTextName = "MessageText";
        [SerializeField] private string confirmButtonName = "ConfirmButton";
        [SerializeField] private string cancelButtonName = "CancelButton";
        // 이벤트
        public System.Action OnConfirmClicked;
        public System.Action OnCancelClicked;

        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private TextMeshProUGUI messageText => GetUI<TextMeshProUGUI>(messageTextName);
        private Button confirmButton => GetUI<Button>(confirmButtonName);
        private Button cancelButton => GetUI<Button>(cancelButtonName);
        #endregion

        // 로컬라이제이션 키 관리
        private string messageLocalizationKey;
        private string confirmLocalizationKey;
        private string cancelLocalizationKey;

        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Popup;
            }
        }

        public override string[] GetAutoLocalizeKeys()
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
            SetupMessageLocalization();
        }

        public override void Cleanup()
        {
            // 언어 변경 이벤트 구독 해제
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var confirmEventHandler = GetEventWithSFX(confirmButtonName, "SFX_ButtonClick");
            if (confirmEventHandler != null)
            {
                confirmEventHandler.Click += OnConfirmButtonClicked;
            }
            
            var cancelEventHandler = GetBackEvent(cancelButtonName, "SFX_ButtonClickBack");
            if (cancelEventHandler != null)
            {
                cancelEventHandler.Click += OnCancelButtonClicked;
            }
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

        private void SetupMessageLocalization()
        {
            // 메시지 텍스트는 동적으로 설정되므로 별도 처리
            if (messageText != null)
            {
                // 언어 변경 이벤트 구독
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
                }
            }
        }

        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            // 메시지가 로컬라이제이션 키로 설정된 경우에만 업데이트
            if (!string.IsNullOrEmpty(messageLocalizationKey))
            {
                UpdateMessageText();
            }
        }

        private void UpdateMessageText()
        {
            if (messageText != null && !string.IsNullOrEmpty(messageLocalizationKey))
            {
                string localizedText = GetLocalizedText(messageLocalizationKey);
                messageText.text = localizedText;
            }
        }

        /// <summary>
        /// 메시지 설정 (일반 텍스트)
        /// </summary>
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageLocalizationKey = null; // 일반 텍스트로 설정
            }
        }

        /// <summary>
        /// 메시지 설정 (로컬라이제이션 키 사용)
        /// </summary>
        public void SetMessageKey(string localizationKey)
        {
            if (messageText != null)
            {
                messageLocalizationKey = localizationKey;
                UpdateMessageText();
            }
        }

        /// <summary>
        /// 확인 버튼 텍스트 설정 (일반 텍스트)
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
        /// 확인 버튼 텍스트 설정 (로컬라이제이션 키 사용)
        /// </summary>
        public void SetConfirmTextKey(string localizationKey)
        {
            if (confirmButton != null)
            {
                var textComponent = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    string localizedText = GetLocalizedText(localizationKey);
                    textComponent.text = localizedText;
                    confirmLocalizationKey = localizationKey;
                }
            }
        }

        /// <summary>
        /// 취소 버튼 텍스트 설정 (일반 텍스트)
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
        /// 취소 버튼 텍스트 설정 (로컬라이제이션 키 사용)
        /// </summary>
        public void SetCancelTextKey(string localizationKey)
        {
            if (cancelButton != null)
            {
                var textComponent = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    string localizedText = GetLocalizedText(localizationKey);
                    textComponent.text = localizedText;
                    cancelLocalizationKey = localizationKey;
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

        /// <summary>
        /// 로컬라이제이션 키를 사용하는 정적 메서드
        /// </summary>
        public static void ShowCheckPopUpWithKeys(string messageKey, string confirmKey = "popup_confirm", 
                                                string cancelKey = "popup_cancel",
                                                System.Action confirmCallback = null, System.Action cancelCallback = null)
        {
            UIManager.Instance.ShowPopUpAsync<CheckPopUp>((popup) => {
                if (popup != null)
                {
                    popup.SetMessageKey(messageKey);
                    popup.SetConfirmTextKey(confirmKey);
                    popup.SetCancelTextKey(cancelKey);
                    popup.SetConfirmCallback(confirmCallback);
                    popup.SetCancelCallback(cancelCallback);
                }
            });
        }
    }
}