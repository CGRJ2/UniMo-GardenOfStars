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
        [SerializeField] private bool enableAutoLocalization = false;
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
                Debug.Log($"[AutoLocalizedText] {gameObject.name} 자동 로컬라이제이션 초기화:");
                Debug.Log($"  - 키: {localizationKey}");
                Debug.Log($"  - LocalizationManager 초기화 상태: {LocalizationManager.Instance?.IsInitialized}");
                Debug.Log($"  - 현재 언어: {LocalizationManager.Instance?.CurrentLanguage}");
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
                string originalName = gameObject.name;
                localizationKey = GenerateKeyFromName(originalName);
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} 이름 기반 키 생성:");
                    Debug.Log($"  - 원본 이름: {originalName}");
                    Debug.Log($"  - 생성된 키: {localizationKey}");
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
            
            string key;
            
            // LocalizationManager의 키 생성 메서드 사용
            if (LocalizationManager.Instance != null)
            {
                key = LocalizationManager.Instance.GenerateKeyFromUIName(uiName, false); // 중복 검사 비활성화
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] LocalizationManager 키 생성: {uiName} -> {key}");
                }
            }
            else
            {
                // LocalizationManager가 없는 경우 기본 처리
                key = uiName.ToLower()
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
                
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] 기본 키 생성: {uiName} -> {key}");
                }
            }
            
            return key;
        }
        
        /// <summary>
        /// 텍스트 업데이트
        /// </summary>
        private void UpdateText()
        {
            if (!enableAutoLocalization || string.IsNullOrEmpty(localizationKey))
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning($"[AutoLocalizedText] {gameObject.name} 텍스트 업데이트 건너뜀 - enableAutoLocalization: {enableAutoLocalization}, 키: {localizationKey}");
                }
                return;
            }
            
            string translatedText = GetTranslatedText();
            
            if (showDebugLogs)
            {
                Debug.Log($"[AutoLocalizedText] {gameObject.name} 텍스트 업데이트 시작:");
                Debug.Log($"  - 키: {localizationKey}");
                Debug.Log($"  - 번역된 텍스트: {translatedText}");
            }
            
            // TextMeshProUGUI
            if (tmpText != null)
            {
                tmpText.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} TextMeshProUGUI 업데이트 완료: {translatedText}");
                }
            }
            
            // Legacy Text
            if (legacyText != null)
            {
                legacyText.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} Legacy Text 업데이트 완료: {translatedText}");
                }
            }
            
            // TMP_InputField
            if (tmpInputField != null)
            {
                tmpInputField.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} TMP_InputField 업데이트 완료: {translatedText}");
                }
            }
            
            // Legacy InputField
            if (legacyInputField != null)
            {
                legacyInputField.text = translatedText;
                if (showDebugLogs)
                {
                    Debug.Log($"[AutoLocalizedText] {gameObject.name} Legacy InputField 업데이트 완료: {translatedText}");
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
            
            // 키 존재 여부 확인
            bool hasKey = LocalizationManager.Instance.HasKey(localizationKey);
            if (showDebugLogs)
            {
                Debug.Log($"[AutoLocalizedText] 키 존재 여부 확인 - 키: {localizationKey}, 존재: {hasKey}");
            }
            
            string result = LocalizationManager.Instance.GetText(localizationKey);
            if (showDebugLogs)
            {
                Debug.Log($"[AutoLocalizedText] 번역 결과 - 키: {localizationKey}, 결과: {result}");
            }
            
            // 키가 존재하지 않으면 경고 로그
            if (!hasKey)
            {
                Debug.LogWarning($"[AutoLocalizedText] 키가 CSV 파일에 존재하지 않습니다: {localizationKey}");
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
        
        /// <summary>
        /// 현재 상태 정보 출력 (디버그용)
        /// </summary>
        [ContextMenu("Print Debug Info")]
        public void PrintDebugInfo()
        {
            Debug.Log($"[AutoLocalizedText] {gameObject.name} 디버그 정보:");
            Debug.Log($"  - 활성화 상태: {enableAutoLocalization}");
            Debug.Log($"  - 커스텀 키 사용: {useCustomKey}");
            Debug.Log($"  - 커스텀 키: {customKey}");
            Debug.Log($"  - 현재 키: {localizationKey}");
            Debug.Log($"  - LocalizationManager 초기화: {LocalizationManager.Instance?.IsInitialized}");
            Debug.Log($"  - 키 존재 여부: {LocalizationManager.Instance?.HasKey(localizationKey)}");
            Debug.Log($"  - 현재 텍스트: {GetCurrentText()}");
            
            // 키가 존재하지 않으면 CSV에 추가 제안
            if (LocalizationManager.Instance != null && !LocalizationManager.Instance.HasKey(localizationKey))
            {
                Debug.LogWarning($"[AutoLocalizedText] 키 '{localizationKey}'가 CSV 파일에 존재하지 않습니다.");
                Debug.LogWarning($"[AutoLocalizedText] CSV 파일에 다음 라인을 추가하세요:");
                Debug.LogWarning($"[AutoLocalizedText] {localizationKey},, ");
            }
        }
        
        /// <summary>
        /// 키를 CSV에 자동 추가 (디버그용)
        /// </summary>
        [ContextMenu("Add Key to CSV")]
        public void AddKeyToCSV()
        {
            if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(localizationKey))
            {
                LocalizationManager.Instance.UpdateTranslationInCSV(localizationKey, SystemLanguage.Korean, "");
                Debug.Log($"[AutoLocalizedText] 키 '{localizationKey}'를 CSV에 추가했습니다.");
                
                // 텍스트 업데이트
                UpdateText();
                Debug.Log($"[AutoLocalizedText] 텍스트를 업데이트했습니다.");
            }
        }
        
  
    }
}
