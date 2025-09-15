using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
    public class MessagePopUp : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string messageTextName = "MessageText";
        
        [Header("Close Settings")]
        [SerializeField] private bool canCloseWithPanelClick = true; // 패널 클릭으로 닫기 가능 여부

        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private TextMeshProUGUI messageText => GetUI<TextMeshProUGUI>(messageTextName);
        #endregion

        // 로컬라이제이션 키 관리
        private string messageLocalizationKey;

        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Popup;
            }
            
            // Backdrop 설정
            createBackdropForPopup = true;
            canCloseWithBackdrop = true;
        }


        public override void Initialize()
        {
            base.Initialize();
            SetupPanelClick();
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

        private void SetupPanelClick()
        {
            if (!canCloseWithPanelClick) return;

            // 패널에 Button 컴포넌트가 없으면 추가
            Button panelButton = GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = gameObject.AddComponent<Button>();
            }

            // 패널 클릭 이벤트 설정
            panelButton.onClick.AddListener(OnPanelClicked);
        }

        private void OnPanelClicked()
        {
            Debug.Log("[MessagePopUp] 패널 클릭 - 팝업 닫기");
            ClosePopup();
        }

        private void ClosePopup()
        {
            OnClosed?.Invoke();
            Manager.ui.ClosePopup(); 
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
        /// 닫기 이벤트 설정
        /// </summary>
        public void SetCloseCallback(System.Action callback)
        {
            OnClosed = callback;
        }

        /// <summary>
        /// 패널 클릭으로 닫기 기능 설정
        /// </summary>
        public void SetPanelClickable(bool clickable)
        {
            canCloseWithPanelClick = clickable;
            
            Button panelButton = GetComponent<Button>();
            if (panelButton != null)
            {
                panelButton.onClick.RemoveAllListeners();
                
                if (clickable)
                {
                    panelButton.onClick.AddListener(OnPanelClicked);
                }
            }
        }

        /// <summary>
        /// 정적 메서드 - 메시지 팝업 표시 (일반 텍스트)
        /// </summary>
        public static void ShowMessagePopUp(string message, System.Action closeCallback = null)
        {
            UIManager.Instance.ShowMessagePopUpAsync(message, closeCallback);
        }

        /// <summary>
        /// 정적 메서드 - 메시지 팝업 표시 (로컬라이제이션 키 사용)
        /// </summary>
        public static void ShowMessagePopUpWithKey(string messageKey, System.Action closeCallback = null)
        {
            UIManager.Instance.ShowMessagePopUpWithKeyAsync(messageKey, closeCallback);
        }
    }
}
