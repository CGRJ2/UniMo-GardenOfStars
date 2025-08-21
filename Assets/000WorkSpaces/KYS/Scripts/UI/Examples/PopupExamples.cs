using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace KYS
{
    /// <summary>
    /// 상호작용 팝업 예시 (팝업 UI 그룹) - Localization 적용
    /// </summary>
    public class InteractionPopup : BaseUI
    {
        [Header("Interaction Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button interactButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image itemIcon;
        
        [Header("Localized Text Components")]
        [SerializeField] private LocalizedText interactButtonText;
        [SerializeField] private LocalizedText closeButtonText;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false; // 이전 UI 숨기지 않음
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Localization 초기화 대기
            if (Manager.localization != null && Manager.localization.IsInitialized)
            {
                SetupLocalizedTexts();
            }
            else
            {
                StartCoroutine(WaitForLocalization());
            }
            
            if (interactButton != null)
            {
                GetEventWithSFX("InteractButton").Click += (data) => OnInteractClicked();
            }
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        /// <summary>
        /// Localization 초기화 대기
        /// </summary>
        private System.Collections.IEnumerator WaitForLocalization()
        {
            while (Manager.localization == null || !Manager.localization.IsInitialized)
            {
                yield return null;
            }
            
            SetupLocalizedTexts();
        }
        
        /// <summary>
        /// Localized 텍스트 설정
        /// </summary>
        private void SetupLocalizedTexts()
        {
            if (interactButtonText == null && interactButton != null)
            {
                var textComponent = interactButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_confirm");
                }
            }
            
            if (closeButtonText == null && closeButton != null)
            {
                var textComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_close");
                }
            }
        }
        
        public void SetInteractionData(string title, string description, Sprite icon = null)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
            
            if (itemIcon != null && icon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.gameObject.SetActive(true);
            }
            else if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(false);
            }
        }
        
        #region Event Handlers
        
        private void OnInteractClicked()
        {
            Debug.Log("[InteractionPopup] 상호작용 실행");
            // 실제 상호작용 로직 실행
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[InteractionPopup] 팝업 닫기");
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 정보 표시 팝업 예시 (팝업 UI 그룹) - Localization 적용
    /// </summary>
    public class InfoPopup : BaseUI
    {
        [Header("Info Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button okButton;
        [SerializeField] private Button closeButton;
        
        [Header("Localized Text Components")]
        [SerializeField] private LocalizedText okButtonText;
        [SerializeField] private LocalizedText closeButtonText;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false;
            disablePreviousUI = false;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Localization 초기화 대기
            if (Manager.localization != null && Manager.localization.IsInitialized)
            {
                SetupLocalizedTexts();
            }
            else
            {
                StartCoroutine(WaitForLocalization());
            }
            
            if (okButton != null)
            {
                GetEventWithSFX("OkButton").Click += (data) => OnOkClicked();
            }
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        /// <summary>
        /// Localization 초기화 대기
        /// </summary>
        private System.Collections.IEnumerator WaitForLocalization()
        {
            while (Manager.localization == null || !Manager.localization.IsInitialized)
            {
                yield return null;
            }
            
            SetupLocalizedTexts();
        }
        
        /// <summary>
        /// Localized 텍스트 설정
        /// </summary>
        private void SetupLocalizedTexts()
        {
            if (okButtonText == null && okButton != null)
            {
                var textComponent = okButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_confirm");
                }
            }
            
            if (closeButtonText == null && closeButton != null)
            {
                var textComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_close");
                }
            }
        }
        
        public void SetInfo(string title, string content)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
            
            if (contentText != null)
            {
                contentText.text = content;
            }
        }
        
        #region Event Handlers
        
        private void OnOkClicked()
        {
            Debug.Log("[InfoPopup] 확인 버튼 클릭");
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[InfoPopup] 팝업 닫기");
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 로딩 팝업 예시 (로딩 UI 그룹) - Localization 적용
    /// </summary>
    public class LoadingPopup : BaseUI
    {
        [Header("Loading Elements")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image loadingIcon;
        
        [Header("Localized Text Components")]
        [SerializeField] private LocalizedText defaultLoadingText;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Loading;
            canCloseWithESC = false; // 로딩 중에는 ESC로 닫을 수 없음
            canCloseWithBackdrop = false;
            hidePreviousUI = false;
            disablePreviousUI = false;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Localization 초기화 대기
            if (Manager.localization != null && Manager.localization.IsInitialized)
            {
                SetupLocalizedTexts();
            }
            else
            {
                StartCoroutine(WaitForLocalization());
            }
            
            // 로딩 아이콘 회전 시작
            StartCoroutine(RotateLoadingIcon());
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        /// <summary>
        /// Localization 초기화 대기
        /// </summary>
        private System.Collections.IEnumerator WaitForLocalization()
        {
            while (Manager.localization == null || !Manager.localization.IsInitialized)
            {
                yield return null;
            }
            
            SetupLocalizedTexts();
        }
        
        /// <summary>
        /// Localized 텍스트 설정
        /// </summary>
        private void SetupLocalizedTexts()
        {
            if (defaultLoadingText == null && loadingText != null)
            {
                loadingText.text = GetLocalizedText("loading_text");
            }
        }
        
        public void SetLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }
        
        public void SetProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.value = Mathf.Clamp01(progress);
            }
        }
        
        public void SetProgressText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }
        
        private System.Collections.IEnumerator RotateLoadingIcon()
        {
            if (loadingIcon == null) yield break;
            
            while (gameObject.activeInHierarchy)
            {
                loadingIcon.transform.Rotate(0, 0, -90f * Time.deltaTime);
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// 툴팁 팝업 예시 (팝업 UI 그룹) - Localization 적용
    /// </summary>
    public class TooltipPopup : BaseUI
    {
        [Header("Tooltip Elements")]
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private RectTransform tooltipRect;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = false;
            canCloseWithBackdrop = false;
            hidePreviousUI = false;
            disablePreviousUI = false;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetTooltip(string text, Vector3 worldPosition)
        {
            if (tooltipText != null)
            {
                tooltipText.text = text;
            }
            
            if (tooltipRect != null)
            {
                // 월드 좌표를 스크린 좌표로 변환
                Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
                tooltipRect.position = screenPosition;
            }
        }
        
        public void SetTooltip(string text, Vector2 screenPosition)
        {
            if (tooltipText != null)
            {
                tooltipText.text = text;
            }
            
            if (tooltipRect != null)
            {
                tooltipRect.position = screenPosition;
            }
        }
    }
}
