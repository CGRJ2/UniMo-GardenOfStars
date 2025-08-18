using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KYS.UI;
using KYS.UI.MVP;

namespace KYS.UI.Examples
{
    /// <summary>
    /// 메뉴 패널 예시 (패널 그룹)
    /// </summary>
    public class MenuPanel : BaseUI
    {
        [Header("Menu Elements")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button closeButton;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Other;
            hidePreviousUI = true; // 이전 UI 숨김
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // 이벤트 설정
            if (resumeButton != null)
            {
                GetEventWithSFX("ResumeButton").Click += (data) => OnResumeClicked();
            }
            
            if (settingsButton != null)
            {
                GetEventWithSFX("SettingsButton").Click += (data) => OnSettingsClicked();
            }
            
            if (exitButton != null)
            {
                GetEventWithSFX("ExitButton").Click += (data) => OnExitClicked();
            }
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
            
            // UIManager에 등록
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        #region Event Handlers
        
        private void OnResumeClicked()
        {
            Debug.Log("[MenuPanel] 게임 재개");
            Hide();
        }
        
        private void OnSettingsClicked()
        {
            Debug.Log("[MenuPanel] 설정 패널 열기");
            UIManager.Instance.ShowPopUp<SettingsPanel>();
        }
        
        private void OnExitClicked()
        {
            Debug.Log("[MenuPanel] 게임 종료 확인");
            UIManager.Instance.ShowConfirmPopUp("게임을 종료하시겠습니까?", () => {
                Application.Quit();
            });
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[MenuPanel] 메뉴 닫기");
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 설정 패널 예시 (패널 그룹)
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
    /// 인벤토리 패널 예시 (패널 그룹)
    /// </summary>
    public class InventoryPanel : BaseUI
    {
        [Header("Inventory Elements")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI itemCountText;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Shop; // 인벤토리는 상점 그룹에 포함
            hidePreviousUI = true; // 이전 UI 숨김
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
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
        
        private void RefreshInventory()
        {
            // 인벤토리 아이템 표시 로직
            if (itemCountText != null)
            {
                itemCountText.text = "아이템 개수: 0";
            }
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[InventoryPanel] 인벤토리 닫기");
            Hide();
        }
    }
    
    /// <summary>
    /// 게임 종료 확인 패널 예시 (게임 종료 그룹)
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
            canCloseWithESC = true;
            hidePreviousUI = true; // 이전 UI 숨김
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
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
        
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
        
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
    }
}
