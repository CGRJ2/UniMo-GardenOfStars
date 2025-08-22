using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace KYS
{
    /// <summary>
    /// UI 이름과 로컬라이제이션 키가 같으면 자동으로 번역을 적용하는 컴포넌트
    /// </summary>
    public class AutoLocalizedText : MonoBehaviour
    {
        [Header("Auto Localization Settings")]
        [SerializeField] private bool enableAutoLocalization = true;
        [SerializeField] private string customKey = ""; // UI 이름과 다른 키를 사용하고 싶을 때
        [SerializeField] private bool useCustomKey = false; // customKey 사용 여부
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = false;
        
        // UI 컴포넌트들
        private TextMeshProUGUI tmpText;
        private Text legacyText;
        private TMP_InputField tmpInputField;
        private InputField legacyInputField;
        
        // 로컬라이제이션 키
        private string localizationKey;
        
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
            
            // 로컬라이제이션 키 결정
            DetermineLocalizationKey();
        }
        
        private void Start()
        {
            if (enableAutoLocalization)
            {
                // LocalizationManager 초기화 대기
                if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
                {
                    InitializeAutoLocalization();
                }
                else
                {
                    // 초기화가 완료되지 않은 경우 대기
                    StartCoroutine(WaitForInitialization());
                }
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
                if (showDebugLogs)
                {
                    Debug.LogWarning($"[AutoLocalizedText] OnDestroy에서 LocalizationManager 이벤트 구독 해제 중 오류: {e.Message}");
                }
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
            
            InitializeAutoLocalization();
        }
        
        /// <summary>
        /// 자동 로컬라이제이션 초기화
        /// </summary>
        private void InitializeAutoLocalization()
        {
            if (!enableAutoLocalization) return;
            
            if (showDebugLogs)
            {
                Debug.Log($"[AutoLocalizedText] {gameObject.name} 자동 로컬라이제이션 초기화 - 키: {localizationKey}");
            }
            
            // 초기 텍스트 설정
            UpdateText();
            
            // 언어 변경 이벤트 구독
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
        }
        
        /// <summary>
        /// 로컬라이제이션 키 결정
        /// </summary>
        private void DetermineLocalizationKey()
        {
            if (useCustomKey && !string.IsNullOrEmpty(customKey))
            {
                localizationKey = customKey;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} 커스텀 키 사용: {localizationKey}");
                }
            }
            else
            {
                // UI 이름을 기반으로 키 생성
                localizationKey = GenerateKeyFromName(gameObject.name);
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} 이름 기반 키 생성: {localizationKey}");
                }
            }
        }
        
        /// <summary>
        /// UI 이름을 기반으로 로컬라이제이션 키 생성
        /// </summary>
        private string GenerateKeyFromName(string uiName)
        {
            if (string.IsNullOrEmpty(uiName))
                return "";
            
            // LocalizationManager의 키 생성 메서드 사용
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GenerateKeyFromUIName(uiName, false); // 중복 검사 비활성화
            }
            
            // LocalizationManager가 없는 경우 기본 처리
            string key = uiName.ToLower()
                .Replace("text", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Trim();
            
            // 빈 문자열이면 원본 이름 사용
            if (string.IsNullOrEmpty(key))
            {
                key = uiName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
            }
            
            return key;
        }
        
        /// <summary>
        /// 텍스트 업데이트
        /// </summary>
        private void UpdateText()
        {
            if (!enableAutoLocalization || string.IsNullOrEmpty(localizationKey))
                return;
            
            string translatedText = GetTranslatedText();
            
            // TextMeshProUGUI
            if (tmpText != null)
            {
                tmpText.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} TextMeshProUGUI 업데이트: {translatedText}");
                }
            }
            
            // Legacy Text
            if (legacyText != null)
            {
                legacyText.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} Legacy Text 업데이트: {translatedText}");
                }
            }
            
            // TMP_InputField
            if (tmpInputField != null)
            {
                tmpInputField.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} TMP_InputField 업데이트: {translatedText}");
                }
            }
            
            // Legacy InputField
            if (legacyInputField != null)
            {
                legacyInputField.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} Legacy InputField 업데이트: {translatedText}");
                }
            }
        }
        
        /// <summary>
        /// 번역된 텍스트 가져오기
        /// </summary>
        private string GetTranslatedText()
        {
            if (LocalizationManager.Instance == null)
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning($"[AutoLocalizedText] LocalizationManager.Instance가 null - 키: {localizationKey}");
                }
                return localizationKey;
            }
            
            if (!LocalizationManager.Instance.IsInitialized)
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning($"[AutoLocalizedText] LocalizationManager가 초기화되지 않음 - 키: {localizationKey}");
                }
                return localizationKey;
            }
            
            string result = LocalizationManager.Instance.GetText(localizationKey);
            if (showDebugLogs)
            {
                Debug.Log($"[AutoLocalizedText] 번역 결과 - 키: {localizationKey}, 결과: {result}");
            }
            return result;
        }
        
        /// <summary>
        /// 언어 변경 이벤트 핸들러
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[AutoLocalizedText] {gameObject.name} 언어 변경 감지: {newLanguage}");
            }
            UpdateText();
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
            
            return "";
        }
        
        /// <summary>
        /// 현재 로컬라이제이션 키 가져오기
        /// </summary>
        public string GetLocalizationKey()
        {
            return localizationKey;
        }
        
        /// <summary>
        /// 자동 로컬라이제이션 활성화/비활성화
        /// </summary>
        public void SetAutoLocalizationEnabled(bool enabled)
        {
            enableAutoLocalization = enabled;
            if (enabled)
            {
                UpdateText();
            }
        }
        
        /// <summary>
        /// 디버그 로그 활성화/비활성화
        /// </summary>
        public void SetDebugLogsEnabled(bool enabled)
        {
            showDebugLogs = enabled;
        }
    }
}
