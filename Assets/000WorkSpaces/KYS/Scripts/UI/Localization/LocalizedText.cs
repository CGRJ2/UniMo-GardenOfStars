using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace KYS
{
    /// <summary>
    /// 다국어 텍스트 컴포넌트 (LocalizationManager 싱글톤 사용)
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField] private string localizationKey = "";
        [SerializeField] private bool updateOnLanguageChange = true;
        
        // UI 컴포넌트들
        private TextMeshProUGUI tmpText;
        private Text legacyText;
        private TMP_InputField tmpInputField;
        private InputField legacyInputField;
        
        private void Awake()
        {
            // UI 컴포넌트 찾기
            tmpText = GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
            {
                legacyText = GetComponent<Text>();
            }
            
            tmpInputField = GetComponent<TMP_InputField>();
            if (tmpInputField == null)
            {
                legacyInputField = GetComponent<InputField>();
            }
        }
        
        private void Start()
        {
            // LocalizationManager 초기화 대기
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                InitializeLocalizedText();
            }
            else
            {
                // 초기화가 완료되지 않은 경우 대기
                StartCoroutine(WaitForInitialization());
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            try
            {
                if (LocalizationManager.Instance != null && LocalizationManager.Instance.gameObject != null)
                {
                    LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LocalizedText] OnDestroy에서 LocalizationManager 이벤트 구독 해제 중 오류: {e.Message}");
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
            
            InitializeLocalizedText();
        }
        
        /// <summary>
        /// LocalizedText 초기화
        /// </summary>
        private void InitializeLocalizedText()
        {
            Debug.Log($"[LocalizedText] 초기화 시작 - 키: {localizationKey}, 게임오브젝트: {gameObject.name}");
            
            // 초기 텍스트 설정
            UpdateText();
            
            // 언어 변경 이벤트 구독
            if (updateOnLanguageChange && LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
                Debug.Log($"[LocalizedText] 언어 변경 이벤트 구독 완료 - 키: {localizationKey}");
            }
        }
        
        /// <summary>
        /// 언어 변경 이벤트 핸들러
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
            {
                Debug.LogWarning($"[LocalizedText] localizationKey가 비어있음 - 게임오브젝트: {gameObject.name}");
                return;
            }
            
            string translatedText = GetTranslatedText();
            Debug.Log($"[LocalizedText] 텍스트 업데이트 - 키: {localizationKey}, 번역: {translatedText}, 게임오브젝트: {gameObject.name}");
            
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
            if (LocalizationManager.Instance == null)
            {
                Debug.LogWarning($"[LocalizedText] LocalizationManager.Instance가 null - 키: {localizationKey}");
                return localizationKey;
            }
            
            if (!LocalizationManager.Instance.IsInitialized)
            {
                Debug.LogWarning($"[LocalizedText] LocalizationManager가 초기화되지 않음 - 키: {localizationKey}");
                return localizationKey;
            }
            
            string result = LocalizationManager.Instance.GetText(localizationKey);
            Debug.Log($"[LocalizedText] 번역 결과 - 키: {localizationKey}, 결과: {result}");
            return result;
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
        /// 직접 텍스트 설정 (로컬라이제이션 키 무시)
        /// </summary>
        public void SetText(string text)
        {
            // TextMeshProUGUI
            if (tmpText != null)
            {
                tmpText.text = text;
            }
            
            // Legacy Text
            if (legacyText != null)
            {
                legacyText.text = text;
            }
            
            // TMP_InputField
            if (tmpInputField != null)
            {
                tmpInputField.text = text;
            }
            
            // Legacy InputField
            if (legacyInputField != null)
            {
                legacyInputField.text = text;
            }
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
        /// 언어 변경 이벤트 구독 설정
        /// </summary>
        public void SetUpdateOnLanguageChange(bool update)
        {
            if (updateOnLanguageChange == update)
                return;
                
            updateOnLanguageChange = update;
            
            if (LocalizationManager.Instance != null)
            {
                if (update)
                {
                    LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
                }
                else
                {
                    LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
                }
            }
        }
        
        /// <summary>
        /// Inspector에서 키 변경 시 자동 업데이트
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateText();
            }
        }
    }
}
