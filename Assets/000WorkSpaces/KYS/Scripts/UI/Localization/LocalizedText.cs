using UnityEngine;
using TMPro;
using UnityEngine.UI;
using KYS.UI.Localization;

namespace KYS.UI.Localization
{
    /// <summary>
    /// 다중언어 지원 텍스트 컴포넌트 (MVP Model 연동)
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField] private string localizationKey;
        [SerializeField] private bool updateOnLanguageChange = true;
        [SerializeField] private LanguageModel languageModel; // 직접 참조 가능
        
        private TextMeshProUGUI tmpText;
        private Text legacyText;
        private TMP_InputField tmpInputField;
        private InputField legacyInputField;
        
        private void Awake()
        {
            // 텍스트 컴포넌트 찾기
            tmpText = GetComponent<TextMeshProUGUI>();
            legacyText = GetComponent<Text>();
            tmpInputField = GetComponent<TMP_InputField>();
            legacyInputField = GetComponent<InputField>();
            
            // LanguageModel 찾기
            if (languageModel == null)
            {
                languageModel = FindObjectOfType<LanguageModel>();
            }
        }
        
        private void Start()
        {
            // 언어 변경 이벤트 구독
            if (updateOnLanguageChange && languageModel != null)
            {
                languageModel.OnLanguageChanged += OnLanguageChanged;
            }
            
            UpdateText();
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (languageModel != null)
            {
                languageModel.OnLanguageChanged -= OnLanguageChanged;
            }
        }
        
        /// <summary>
        /// 언어 변경 시 호출
        /// </summary>
        private void OnLanguageChanged(SystemLanguage language)
        {
            UpdateText();
        }
        
        /// <summary>
        /// 텍스트 업데이트
        /// </summary>
        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey))
                return;
            
            string translatedText = GetTranslatedText();
            
            // TextMeshProUGUI
            if (tmpText != null)
            {
                tmpText.text = translatedText;
            }
            
            // Legacy Text
            if (legacyText != null)
            {
                legacyText.text = translatedText;
            }
            
            // TMP_InputField
            if (tmpInputField != null)
            {
                tmpInputField.text = translatedText;
            }
            
            // Legacy InputField
            if (legacyInputField != null)
            {
                legacyInputField.text = translatedText;
            }
        }
        
        /// <summary>
        /// 번역된 텍스트 가져오기
        /// </summary>
        private string GetTranslatedText()
        {
            // 새로운 LanguageModel 사용
            if (languageModel != null)
            {
                return languageModel.GetText(localizationKey);
            }
            
            // 기존 LocalizationManager 사용 (하위 호환성)
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(localizationKey);
            }
            
            return localizationKey;
        }
        
        /// <summary>
        /// 로컬라이제이션 키 설정
        /// </summary>
        public void SetLocalizationKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }
        
        /// <summary>
        /// 현재 텍스트 가져오기
        /// </summary>
        public string GetCurrentText()
        {
            if (tmpText != null)
                return tmpText.text;
            if (legacyText != null)
                return legacyText.text;
            if (tmpInputField != null)
                return tmpInputField.text;
            if (legacyInputField != null)
                return legacyInputField.text;
            
            return string.Empty;
        }
        
        /// <summary>
        /// LanguageModel 설정
        /// </summary>
        public void SetLanguageModel(LanguageModel model)
        {
            if (languageModel != null)
            {
                languageModel.OnLanguageChanged -= OnLanguageChanged;
            }
            
            languageModel = model;
            
            if (languageModel != null && updateOnLanguageChange)
            {
                languageModel.OnLanguageChanged += OnLanguageChanged;
            }
        }
    }
}
