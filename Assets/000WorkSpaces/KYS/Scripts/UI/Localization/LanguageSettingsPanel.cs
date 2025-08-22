using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


namespace KYS
{
    /// <summary>
    /// 언어 설정 패널 (최신 LocalizationManager + AutoLocalizedText 사용)
    /// </summary>
    public class LanguageSettingsPanel : BaseUI
    {
        [Header("Language Settings UI")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;
        
        [Header("Language Info UI")]
        [SerializeField] private TextMeshProUGUI currentLanguageText;
        [SerializeField] private TextMeshProUGUI translationCompletenessText;
        [SerializeField] private Slider translationProgressSlider;
        
        [Header("Language Preview")]
        [SerializeField] private TextMeshProUGUI previewText;
        [SerializeField] private string previewKey = "ui_confirm";
        

        
        private SystemLanguage selectedLanguage;
        private Dictionary<SystemLanguage, float> languageCompleteness = new Dictionary<SystemLanguage, float>();
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            panelGroup = UIPanelGroup.Settings;
            
            // 기본 설정
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false;
            createBackdropForPopup = true;
        }
        
        protected override string[] GetAutoLocalizeKeys()
        {
            return new string[] {
                "language_settings_title",
                "language_settings_apply",
                "language_settings_close",
                "language_settings_reset",
                "language_settings_current",
                "language_settings_completeness",
                "language_settings_preview"
            };
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
            Debug.Log("[LanguageSettingsPanel] 언어 설정 초기화 시작");
            
            SetupLanguageDropdown();
            SetupButtons();
            UpdateLanguageInfo();
            UpdatePreviewText();
            
            Debug.Log("[LanguageSettingsPanel] 언어 설정 초기화 완료");
        }
        
        /// <summary>
        /// 언어 드롭다운 설정
        /// </summary>
        private void SetupLanguageDropdown()
        {
            if (languageDropdown == null || LocalizationManager.Instance == null)
                return;
            
            Debug.Log("[LanguageSettingsPanel] 언어 드롭다운 설정 시작");
            
            // 활성 언어 목록 가져오기
            SystemLanguage[] activeLanguages = LocalizationManager.Instance.ActiveLanguages;
            Debug.Log($"[LanguageSettingsPanel] 활성 언어 수: {activeLanguages.Length}");
            
            // 각 언어의 번역 완성도 계산
            languageCompleteness.Clear();
            foreach (var lang in activeLanguages)
            {
                float completeness = LocalizationManager.Instance.GetTranslationCompleteness(lang);
                languageCompleteness[lang] = completeness;
                Debug.Log($"[LanguageSettingsPanel] {LocalizationManager.Instance.GetLanguageName(lang)}: {completeness * 100:F1}%");
            }
            
            // 드롭다운 옵션 설정
            languageDropdown.ClearOptions();
            for (int i = 0; i < activeLanguages.Length; i++)
            {
                string languageName = LocalizationManager.Instance.GetLanguageName(activeLanguages[i]);
                float completeness = languageCompleteness[activeLanguages[i]];
                string optionText = $"{languageName} ({completeness * 100:F0}%)";
                
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(optionText));
            }
            
            // 현재 언어 선택
            int currentIndex = LocalizationManager.Instance.GetCurrentLanguageIndex();
            languageDropdown.value = currentIndex;
            selectedLanguage = LocalizationManager.Instance.CurrentLanguage;
            
            // 이벤트 구독
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
            
            Debug.Log($"[LanguageSettingsPanel] 현재 언어: {LocalizationManager.Instance.GetLanguageName(selectedLanguage)} (인덱스: {currentIndex})");
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButtons()
        {
            Debug.Log("[LanguageSettingsPanel] 버튼 설정 시작");
            
            // 적용 버튼
            if (applyButton != null)
            {
                GetEventWithSFX("ApplyButton", "SFX_ButtonClick").Click += (data) => OnApplyClicked();
            }
            
            // 닫기 버튼
            if (closeButton != null)
            {
                GetEventWithSFX("CloseButton", "SFX_ButtonClickBack").Click += (data) => OnCloseClicked();
            }
            
            // 리셋 버튼
            if (resetButton != null)
            {
                GetEventWithSFX("ResetButton", "SFX_ButtonClick").Click += (data) => OnResetClicked();
            }
            
            Debug.Log("[LanguageSettingsPanel] 버튼 설정 완료");
        }
        
        /// <summary>
        /// 언어 정보 업데이트
        /// </summary>
        private void UpdateLanguageInfo()
        {
            if (LocalizationManager.Instance == null) return;
            
            // 현재 언어 정보
            if (currentLanguageText != null)
            {
                string currentLangName = LocalizationManager.Instance.GetLanguageName(selectedLanguage);
                currentLanguageText.text = $"현재 언어: {currentLangName}";
            }
            
            // 번역 완성도 정보
            if (translationCompletenessText != null && translationProgressSlider != null)
            {
                float completeness = languageCompleteness.ContainsKey(selectedLanguage) 
                    ? languageCompleteness[selectedLanguage] 
                    : LocalizationManager.Instance.GetTranslationCompleteness(selectedLanguage);
                
                translationCompletenessText.text = $"번역 완성도: {completeness * 100:F1}%";
                translationProgressSlider.value = completeness;
            }
        }
        
        /// <summary>
        /// 미리보기 텍스트 업데이트
        /// </summary>
        private void UpdatePreviewText()
        {
            if (previewText == null || LocalizationManager.Instance == null) return;
            
            string previewTranslation = LocalizationManager.Instance.GetText(previewKey, selectedLanguage);
            previewText.text = $"미리보기 ({previewKey}): {previewTranslation}";
        }
        
        /// <summary>
        /// 언어 드롭다운 변경 이벤트
        /// </summary>
        private void OnLanguageDropdownChanged(int index)
        {
            if (LocalizationManager.Instance == null) return;
            
            selectedLanguage = LocalizationManager.Instance.GetLanguageByIndex(index);
            Debug.Log($"[LanguageSettingsPanel] 언어 선택 변경: {LocalizationManager.Instance.GetLanguageName(selectedLanguage)}");
            
            UpdateLanguageInfo();
            UpdatePreviewText();
        }
        
        /// <summary>
        /// 적용 버튼 클릭
        /// </summary>
        private void OnApplyClicked()
        {
            if (LocalizationManager.Instance == null) return;
            
            Debug.Log($"[LanguageSettingsPanel] 언어 적용: {LocalizationManager.Instance.GetLanguageName(selectedLanguage)}");
            
            // 언어 변경
            LocalizationManager.Instance.SetLanguage(selectedLanguage);
            
            // 성공 사운드 재생
            PlaySuccessSound();
            
            // 패널 닫기
            Manager.ui.ClosePopup();
        }
        
        /// <summary>
        /// 닫기 버튼 클릭
        /// </summary>
        private void OnCloseClicked()
        {
            Debug.Log("[LanguageSettingsPanel] 패널 닫기");
            Manager.ui.ClosePopup();
        }
        
        /// <summary>
        /// 리셋 버튼 클릭
        /// </summary>
        private void OnResetClicked()
        {
            if (LocalizationManager.Instance == null) return;
            
            Debug.Log("[LanguageSettingsPanel] 언어 설정 리셋");
            
            // 기본 언어로 리셋
            LocalizationManager.Instance.SetLanguage(LocalizationManager.Instance.DefaultLanguage);
            
            // 드롭다운 업데이트
            SetupLanguageDropdown();
            UpdateLanguageInfo();
            UpdatePreviewText();
            
            // 성공 사운드 재생
            PlaySuccessSound();
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
            else
            {
                Debug.LogWarning("[LanguageSettingsPanel] UIManager.Instance가 null입니다.");
            }
        }
        
        /// <summary>
        /// 언어 설정 패널 표시 (비동기)
        /// </summary>
        public static void ShowLanguageSettingsAsync(System.Action<LanguageSettingsPanel> onComplete = null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPopUpAsync<LanguageSettingsPanel>(onComplete);
            }
            else
            {
                Debug.LogWarning("[LanguageSettingsPanel] UIManager.Instance가 null입니다.");
                onComplete?.Invoke(null);
            }
        }
        
        /// <summary>
        /// 현재 선택된 언어 가져오기
        /// </summary>
        public SystemLanguage GetSelectedLanguage()
        {
            return selectedLanguage;
        }
        
        /// <summary>
        /// 언어 변경 이벤트 구독
        /// </summary>
        public void SubscribeToLanguageChange(System.Action<SystemLanguage> onLanguageChanged)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += onLanguageChanged;
            }
        }
        
        /// <summary>
        /// 언어 변경 이벤트 구독 해제
        /// </summary>
        public void UnsubscribeFromLanguageChange(System.Action<SystemLanguage> onLanguageChanged)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= onLanguageChanged;
            }
        }
    }
}
