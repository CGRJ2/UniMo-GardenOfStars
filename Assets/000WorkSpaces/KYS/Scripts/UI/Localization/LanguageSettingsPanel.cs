using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KYS.UI;
using KYS.UI.Localization;

namespace KYS.UI.Localization
{
    /// <summary>
    /// 언어 설정 패널 (MVP View)
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
        
        // MVP Components
        private new LanguageSettingsPresenter presenter;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Settings;
        }
        
        protected override void SetupMVP()
        {
            base.SetupMVP();
            
            // Presenter 설정
            presenter = GetComponent<LanguageSettingsPresenter>();
            if (presenter == null)
            {
                presenter = gameObject.AddComponent<LanguageSettingsPresenter>();
            }
            
            SetPresenter(presenter);
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            SetupButtons();
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        /// <summary>
        /// 언어 드롭다운 설정 (Presenter에서 호출)
        /// </summary>
        public void SetupLanguageDropdown(SystemLanguage[] supportedLanguages, int currentIndex)
        {
            if (languageDropdown == null) return;
            
            // 드롭다운 초기화
            languageDropdown.ClearOptions();
            
            // 드롭다운 옵션 추가
            for (int i = 0; i < supportedLanguages.Length; i++)
            {
                string languageName = GetLanguageDisplayName(supportedLanguages[i]);
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(languageName));
            }
            
            // 현재 언어 선택
            if (currentIndex >= 0 && currentIndex < supportedLanguages.Length)
            {
                languageDropdown.value = currentIndex;
            }
            
            // 드롭다운 이벤트 설정
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
        }
        
        private void SetupButtons()
        {
            if (applyButton != null)
            {
                GetEventWithSFX("ApplyButton").Click += (data) => OnApplyClicked();
            }
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
        }
        
        private void OnLanguageDropdownChanged(int index)
        {
            // Presenter에 위임
            presenter?.OnLanguageDropdownChanged(index);
        }
        
        private void OnApplyClicked()
        {
            // Presenter에 위임
            presenter?.OnApplyLanguage();
        }
        
        private void OnCloseClicked()
        {
            // Presenter에 위임
            presenter?.OnClose();
        }
        
        /// <summary>
        /// 언어 표시 이름 가져오기
        /// </summary>
        private string GetLanguageDisplayName(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Korean:
                    return "한국어";
                case SystemLanguage.English:
                    return "English";
                case SystemLanguage.Japanese:
                    return "日本語";
                case SystemLanguage.Chinese:
                    return "中文";
                default:
                    return language.ToString();
            }
        }
        
        /// <summary>
        /// 언어 설정 패널 표시
        /// </summary>
        public static void ShowLanguageSettings()
        {
            UIManager.Instance.ShowPopUp<LanguageSettingsPanel>();
        }
    }
}
