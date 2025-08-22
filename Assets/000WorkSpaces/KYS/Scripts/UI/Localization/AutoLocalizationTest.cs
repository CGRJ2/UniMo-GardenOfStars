using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

namespace KYS
{
    /// <summary>
    /// 자동 로컬라이제이션 시스템 테스트용 스크립트
    /// </summary>
    public class AutoLocalizationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private string[] testUINames = {
            "StartButton",
            "SettingsPanel",
            "GameTitle",
            "ExitButton",
            "MainMenuWindow"
        };
        
        [Header("Test Results")]
        [SerializeField] private bool showTestResults = true;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                RunTests();
            }
        }
        
        /// <summary>
        /// 자동 로컬라이제이션 테스트 실행
        /// </summary>
        [ContextMenu("Run Auto Localization Tests")]
        public void RunTests()
        {
            Debug.Log("[AutoLocalizationTest] 자동 로컬라이제이션 테스트 시작");
            
            if (LocalizationManager.Instance == null)
            {
                Debug.LogError("[AutoLocalizationTest] LocalizationManager.Instance가 null입니다.");
                return;
            }
            
            if (!LocalizationManager.Instance.IsInitialized)
            {
                Debug.LogWarning("[AutoLocalizationTest] LocalizationManager가 초기화되지 않았습니다.");
                return;
            }
            
            TestKeyGeneration();
            TestDuplicateKeyDetection();
            TestTranslationAvailability();
            
            Debug.Log("[AutoLocalizationTest] 자동 로컬라이제이션 테스트 완료");
        }
        
        /// <summary>
        /// 키 생성 테스트
        /// </summary>
        private void TestKeyGeneration()
        {
            Debug.Log("[AutoLocalizationTest] === 키 생성 테스트 ===");
            
            foreach (string uiName in testUINames)
            {
                string generatedKey = LocalizationManager.Instance.GenerateKeyFromUIName(uiName, false);
                bool hasTranslation = LocalizationManager.Instance.HasTranslation(generatedKey);
                
                Debug.Log($"[AutoLocalizationTest] UI 이름: {uiName} → 키: {generatedKey} → 번역 존재: {hasTranslation}");
            }
        }
        
        /// <summary>
        /// 중복 키 검사 테스트
        /// </summary>
        private void TestDuplicateKeyDetection()
        {
            Debug.Log("[AutoLocalizationTest] === 중복 키 검사 테스트 ===");
            
            // 테스트용 중복 키 생성
            string[] testKeys = { "start", "start", "settings", "game", "exit", "exit" };
            var keyCounts = new System.Collections.Generic.Dictionary<string, int>();
            
            foreach (string key in testKeys)
            {
                if (keyCounts.ContainsKey(key))
                {
                    keyCounts[key]++;
                }
                else
                {
                    keyCounts[key] = 1;
                }
            }
            
            var duplicates = keyCounts.Where(kvp => kvp.Value > 1).ToList();
            
            if (duplicates.Count > 0)
            {
                Debug.LogWarning("[AutoLocalizationTest] 중복 키 발견:");
                foreach (var duplicate in duplicates)
                {
                    Debug.LogWarning($"  - 키: {duplicate.Key}, 중복 횟수: {duplicate.Value}");
                }
            }
            else
            {
                Debug.Log("[AutoLocalizationTest] 중복 키가 없습니다.");
            }
        }
        
        /// <summary>
        /// 번역 가용성 테스트
        /// </summary>
        private void TestTranslationAvailability()
        {
            Debug.Log("[AutoLocalizationTest] === 번역 가용성 테스트 ===");
            
            string[] allKeys = LocalizationManager.Instance.GetAllKeys();
            SystemLanguage[] supportedLanguages = LocalizationManager.Instance.AllSupportedLanguages;
            
            Debug.Log($"[AutoLocalizationTest] 총 키 수: {allKeys.Length}");
            Debug.Log($"[AutoLocalizationTest] 지원 언어: {string.Join(", ", supportedLanguages)}");
            
            foreach (SystemLanguage lang in supportedLanguages)
            {
                string[] missingKeys = LocalizationManager.Instance.GetKeysWithoutTranslation(lang);
                float completeness = LocalizationManager.Instance.GetTranslationCompleteness(lang);
                
                Debug.Log($"[AutoLocalizationTest] {lang}: 완성도 {completeness * 100:F1}% ({missingKeys.Length}개 누락)");
                
                if (missingKeys.Length > 0 && showTestResults)
                {
                    Debug.LogWarning($"[AutoLocalizationTest] {lang}에서 번역이 없는 키들:");
                    foreach (string key in missingKeys.Take(5)) // 처음 5개만 표시
                    {
                        Debug.LogWarning($"  - {key}");
                    }
                    if (missingKeys.Length > 5)
                    {
                        Debug.LogWarning($"  ... 외 {missingKeys.Length - 5}개");
                    }
                }
            }
        }
        
        /// <summary>
        /// UI 이름으로 키 생성 테스트
        /// </summary>
        [ContextMenu("Test UI Name to Key Generation")]
        public void TestUINameToKeyGeneration()
        {
            Debug.Log("[AutoLocalizationTest] === UI 이름 → 키 생성 테스트 ===");
            
            string[] testNames = {
                "StartButtonText",
                "GameTitleLabel", 
                "SettingsPanel",
                "MainMenuWindow",
                "ExitButton",
                "PlayButton",
                "PauseButton",
                "ResumeButton",
                "OptionsButton",
                "HelpButton"
            };
            
            foreach (string uiName in testNames)
            {
                string key = LocalizationManager.Instance.GenerateKeyFromUIName(uiName, false);
                Debug.Log($"[AutoLocalizationTest] {uiName} → {key}");
            }
        }
        
        /// <summary>
        /// 자동 로컬라이제이션 컴포넌트 테스트
        /// </summary>
        [ContextMenu("Test AutoLocalizedText Components")]
        public void TestAutoLocalizedTextComponents()
        {
            Debug.Log("[AutoLocalizationTest] === AutoLocalizedText 컴포넌트 테스트 ===");
            
            AutoLocalizedText[] components = FindObjectsOfType<AutoLocalizedText>();
            Debug.Log($"[AutoLocalizationTest] 발견된 AutoLocalizedText 컴포넌트: {components.Length}개");
            
            foreach (AutoLocalizedText component in components)
            {
                string key = component.GetLocalizationKey();
                string currentText = component.GetCurrentText();
                bool hasTranslation = LocalizationManager.Instance.HasTranslation(key);
                
                Debug.Log($"[AutoLocalizationTest] {component.name}: 키={key}, 현재텍스트={currentText}, 번역존재={hasTranslation}");
            }
        }
        
        /// <summary>
        /// 모든 테스트 실행
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("[AutoLocalizationTest] ===== 모든 테스트 실행 =====");
            
            RunTests();
            TestUINameToKeyGeneration();
            TestAutoLocalizedTextComponents();
            
            Debug.Log("[AutoLocalizationTest] ===== 모든 테스트 완료 =====");
        }
    }
}
