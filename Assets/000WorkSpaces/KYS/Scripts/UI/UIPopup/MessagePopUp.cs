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
        private object[] messageFormatArgs; // Format 인수 저장

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
            SetupTextMeshPro();
        }
        
        /// <summary>
        /// TextMeshPro 설정 (Rich Text, 줄바꿈 등)
        /// </summary>
        private void SetupTextMeshPro()
        {
            if (messageText != null)
            {
                // Rich Text 활성화 (컬러 태그 지원)
                messageText.richText = true;
                // 줄바꿈 활성화
                messageText.enableWordWrapping = true;
                
                Debug.Log($"[MessagePopUp] TextMeshPro 설정 - richText: {messageText.richText}, enableWordWrapping: {messageText.enableWordWrapping}");
            }
            else
            {
                Debug.LogError("[MessagePopUp] messageText가 null입니다!");
            }
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
                
                // Format 시도 후 에러 시 원본 텍스트 사용
                try
                {
                    if (messageFormatArgs != null && messageFormatArgs.Length > 0)
                    {
                        messageText.text = string.Format(localizedText, messageFormatArgs);
                    }
                    else
                    {
                        Debug.Log($"[MessagePopUp] Format 인수 없음 - 키: {messageLocalizationKey}, 텍스트: {localizedText}");
                        messageText.text = string.Format(localizedText);
                    }
                }
                catch (System.FormatException ex)
                {
                    Debug.LogError($"[MessagePopUp] 포맷팅 에러 - 키: {messageLocalizationKey}, 텍스트: {localizedText}, 인수: [{string.Join(", ", messageFormatArgs ?? new object[0])}], 에러: {ex.Message}");
                    messageText.text = localizedText; // 포맷팅 실패 시 원본 텍스트 표시
                }
                
                // \n을 실제 줄바꿈으로 변환 (Format 후에 적용)
                messageText.text = messageText.text.Replace("\\n", "\n");
                
                // 강제로 Rich Text 설정 (런타임에서 덮어써질 수 있음)
                messageText.richText = true;
                messageText.enableWordWrapping = true;
                
                Debug.Log($"[MessagePopUp] 최종 텍스트 적용 - richText: {messageText.richText}, 텍스트: '{messageText.text}'");
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
                messageFormatArgs = null; // Format 인수 초기화
            }
        }

        /// <summary>
        /// 메시지 설정 (Format 지원, 줄바꿈 가능)
        /// </summary>
        public void SetMessage(string format, params object[] args)
        {
            if (messageText != null)
            {
                messageText.text = string.Format(format, args);
                messageLocalizationKey = null; // 일반 텍스트로 설정
                messageFormatArgs = null; // Format 인수 초기화
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
                messageFormatArgs = null; // Format 인수 초기화
                UpdateMessageText();
            }
        }

        /// <summary>
        /// 메시지 설정 (로컬라이제이션 키 + Format 지원, 줄바꿈 가능)
        /// </summary>
        public void SetMessageKey(string localizationKey, params object[] args)
        {
            if (messageText != null)
            {
                messageLocalizationKey = localizationKey;
                messageFormatArgs = args; // Format 인수 저장
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
            UIManager.Instance.ShowTutorialPopUpAsync(message, closeCallback);
        }

        /// <summary>
        /// 정적 메서드 - 메시지 팝업 표시 (Format 지원, 줄바꿈 가능)
        /// </summary>
        public static void ShowMessagePopUp(string format, System.Action closeCallback, params object[] args)
        {
            string message = string.Format(format, args);
            UIManager.Instance.ShowTutorialPopUpAsync(message, closeCallback);
        }

        /// <summary>
        /// 정적 메서드 - 메시지 팝업 표시 (로컬라이제이션 키 사용)
        /// </summary>
        public static void ShowMessagePopUpWithKey(string messageKey, System.Action closeCallback = null)
        {
            UIManager.Instance.ShowTutorialPopUpWithKeyAsync(messageKey, closeCallback);
        }

        /// <summary>
        /// 정적 메서드 - 메시지 팝업 표시 (로컬라이제이션 키 + Format 지원, 줄바꿈 가능)
        /// </summary>
        public static void ShowMessagePopUpWithKey(string messageKey, System.Action closeCallback, params object[] args)
        {
            UIManager.Instance.ShowPopUpAsync<TutorialPopUp>((popup) => {
                if (popup != null)
                {
                    popup.SetMessageKey(messageKey, args);
                    popup.SetCloseCallback(closeCallback);
                }
            });
        }
    }
}
