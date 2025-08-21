using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace KYS
{
    /// <summary>
    /// 언어 설정 패널 (LocalizationManager 싱글톤 사용)
    /// </summary>
    public class LanguageSettingsPanel : BaseUI
    {
        [Header("Language Settings")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private LocalizedText titleText;
        [SerializeField] private LocalizedText applyButtonText;
        [SerializeField] private LocalizedText closeButtonText;
        
        private SystemLanguage selectedLanguage;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Settings;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // LocalizationManager 초기화 대기
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                InitializeLanguageSettings();
            }
            else
            {
                // 초기화가 완료되지 않은 경우 대기
                StartCoroutine(WaitForInitialization());
            }
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            
            // 이벤트 구독 해제
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveAllListeners();
            }
        }
        
        /// <summary>
        /// LocalizationManager 초기화 대기
        /// </summary>
        private System.Collections.IEnumerator WaitForInitialization()
        {
            while (LocalizationManager.Instance == null || !LocalizationManager.Instance.IsInitialized)
            {
                yield return null;
            }
            
            InitializeLanguageSettings();
        }
        
        /// <summary>
        /// 언어 설정 초기화
        /// </summary>
        private void InitializeLanguageSettings()
        {
            SetupLanguageDropdown();
            SetupButtons();
        }
        
        /// <summary>
        /// 언어 드롭다운 설정
        /// </summary>
        public void SetupLanguageDropdown()
        {
            if (languageDropdown == null || LocalizationManager.Instance == null)
                return;
            
            // 활성 언어 목록 가져오기
            SystemLanguage[] activeLanguages = LocalizationManager.Instance.ActiveLanguages;
            
            // 드롭다운 옵션 설정
            languageDropdown.ClearOptions();
            for (int i = 0; i < activeLanguages.Length; i++)
            {
                string languageName = LocalizationManager.Instance.GetLanguageName(activeLanguages[i]);
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(languageName));
            }
            
            // 현재 언어 선택
            int currentIndex = LocalizationManager.Instance.GetCurrentLanguageIndex();
            languageDropdown.value = currentIndex;
            selectedLanguage = LocalizationManager.Instance.CurrentLanguage;
            
            // 이벤트 구독
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
        }
        
        private void SetupButtons()
        {
            // 적용 버튼
            GetEvent("ApplyButton").Click += (data) => OnApplyClicked();
            
            // 닫기 버튼
            GetEvent("CloseButton").Click += (data) => OnCloseClicked();
        }
        
        private void OnLanguageDropdownChanged(int index)
        {
            if (LocalizationManager.Instance != null)
            {
                selectedLanguage = LocalizationManager.Instance.GetLanguageByIndex(index);
            }
        }
        
        private void OnApplyClicked()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(selectedLanguage);
            }
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Hide();
        }
        
        /// <summary>
        /// 언어 설정 패널 표시 (정적 메서드)
        /// </summary>
        public static void ShowLanguageSettings()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPopUp<LanguageSettingsPanel>();
            }
        }
    }
}
