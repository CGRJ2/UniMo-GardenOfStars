using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace KYS
{

    
    /// <summary>
    /// 설정 패널 예시 (패널 그룹) - Localization 적용
    /// </summary>
    public class SettingsPanel : BaseUI
    {
        [Header("Settings Elements")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;
        

        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Settings;
            hidePreviousUI = true; // 이전 UI 숨김
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
            
            // 초기 설정 로드
            LoadSettings();
            
            // 이벤트 설정
            if (applyButton != null)
            {
                GetEventWithSFX("ApplyButton").Click += (data) => OnApplyClicked();
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
            // LocalizedText 컴포넌트가 있는 경우 자동으로 처리됨
            // 없으면 수동으로 설정
            var titleComponent = GetUI<TextMeshProUGUI>("TitleText");
            if (titleComponent != null)
            {
                titleComponent.text = GetLocalizedText("settings_title");
            }
            
            var bgmLabelComponent = GetUI<TextMeshProUGUI>("BGMLabel");
            if (bgmLabelComponent != null)
            {
                bgmLabelComponent.text = GetLocalizedText("settings_bgm");
            }
            
            var sfxLabelComponent = GetUI<TextMeshProUGUI>("SFXLabel");
            if (sfxLabelComponent != null)
            {
                sfxLabelComponent.text = GetLocalizedText("settings_sfx");
            }
            
            var fullscreenLabelComponent = GetUI<TextMeshProUGUI>("FullscreenLabel");
            if (fullscreenLabelComponent != null)
            {
                fullscreenLabelComponent.text = GetLocalizedText("settings_fullscreen");
            }
            
            if (applyButton != null)
            {
                var textComponent = applyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_apply");
                }
            }
            
            if (closeButton != null)
            {
                var textComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_close");
                }
            }
        }
        
        private void LoadSettings()
        {
            if (bgmSlider != null)
            {
                bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            
            if (sfxSlider != null)
            {
                sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }
        }
        
        #region Event Handlers
        
        private void OnBGMVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            // AudioManager에 적용
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            // AudioManager에 적용
        }
        
        private void OnFullscreenChanged(bool isFullscreen)
        {
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            Screen.fullScreen = isFullscreen;
        }
        
        private void OnApplyClicked()
        {
            Debug.Log("[SettingsPanel] 설정 적용");
            PlayerPrefs.Save();
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[SettingsPanel] 설정 닫기");
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 인벤토리 패널 예시 (패널 그룹) - Localization 적용
    /// </summary>
    public class InventoryPanel : BaseUI
    {
        [Header("Inventory Elements")]
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI itemCountText;
        

        
        private int itemCount = 0;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Shop;
            hidePreviousUI = true;
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
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
            
            RefreshInventory();
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
            // LocalizedText 컴포넌트가 있는 경우 자동으로 처리됨
                        var titleComponent = GetUI<TextMeshProUGUI>("TitleText");
            if (titleComponent != null)
            {
                titleComponent.text = GetLocalizedText("inventory_title");
            }

            var emptyComponent = GetUI<TextMeshProUGUI>("EmptyText");
            if (emptyComponent != null)
            {
                emptyComponent.text = GetLocalizedText("inventory_empty");
            }
            
            if (closeButton != null)
            {
                var textComponent = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_close");
                }
            }
        }
        
        private void RefreshInventory()
        {
            // 아이템 개수 업데이트 (Localization 적용)
            if (itemCountText != null)
            {
                string localizedText = GetLocalizedText("inventory_item_count");
                itemCountText.text = string.Format(localizedText, itemCount);
            }
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[InventoryPanel] 인벤토리 닫기");
            Hide();
        }
    }
    
    /// <summary>
    /// 게임 종료 확인 패널 예시 (패널 그룹) - Localization 적용
    /// </summary>
    public class GameExitConfirmPanel : BaseUI
    {
        [Header("Exit Confirm Elements")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        

        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.GameExit;
            hidePreviousUI = true;
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
            
            if (confirmButton != null)
            {
                GetEventWithSFX("ConfirmButton").Click += (data) => OnConfirmClicked();
            }
            
            if (cancelButton != null)
            {
                GetBackEvent("CancelButton").Click += (data) => OnCancelClicked();
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
            if (confirmButton != null)
            {
                var textComponent = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_confirm");
                }
            }
            
            if (cancelButton != null)
            {
                var textComponent = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = GetLocalizedText("ui_cancel");
                }
            }
        }
        
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
        
        #region Event Handlers
        
        private void OnConfirmClicked()
        {
            Debug.Log("[GameExitConfirmPanel] 게임 종료 확인");
            Application.Quit();
        }
        
        private void OnCancelClicked()
        {
            Debug.Log("[GameExitConfirmPanel] 게임 종료 취소");
            Hide();
        }
        
        #endregion
    }
}
