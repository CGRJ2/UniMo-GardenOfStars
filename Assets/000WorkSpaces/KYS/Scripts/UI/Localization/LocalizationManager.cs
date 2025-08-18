using System.Collections.Generic;
using UnityEngine;
using System;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;

namespace KYS.UI.Localization
{
    /// <summary>
    /// 다중언어 지원 관리자
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LocalizationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("LocalizationManager");
                        instance = go.AddComponent<LocalizationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Localization Settings")]
        [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.Korean;
        [SerializeField] private TextAsset[] languageFiles; // AddressableAssetReference[] languageFileReferences;
        
        private Dictionary<SystemLanguage, Dictionary<string, string>> languageData = new Dictionary<SystemLanguage, Dictionary<string, string>>();
        private SystemLanguage currentLanguage;
        
        public event Action<SystemLanguage> OnLanguageChanged;
        
        public SystemLanguage CurrentLanguage => currentLanguage;
        public SystemLanguage DefaultLanguage => defaultLanguage;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocalization();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLocalization()
        {
            // 현재 시스템 언어 감지
            currentLanguage = Application.systemLanguage;
            
            // 지원하지 않는 언어인 경우 기본 언어 사용
            if (!IsLanguageSupported(currentLanguage))
            {
                currentLanguage = defaultLanguage;
            }
            
            LoadLanguageData();
        }

        private void LoadLanguageData()
        {
            languageData.Clear();
            
            foreach (var languageFile in languageFiles)
            {
                if (languageFile != null)
                {
                    ParseLanguageFile(languageFile);
                }
            }
        }

        private void ParseLanguageFile(TextAsset file)
        {
            string[] lines = file.text.Split('\n');
            SystemLanguage language = SystemLanguage.English; // 기본값
            
            Dictionary<string, string> translations = new Dictionary<string, string>();
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;
                
                // 언어 헤더 확인
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    string languageName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    language = GetLanguageFromString(languageName);
                    continue;
                }
                
                // 키-값 쌍 파싱
                int separatorIndex = trimmedLine.IndexOf('=');
                if (separatorIndex > 0)
                {
                    string key = trimmedLine.Substring(0, separatorIndex).Trim();
                    string value = trimmedLine.Substring(separatorIndex + 1).Trim();
                    
                    // 따옴표 제거
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    
                    translations[key] = value;
                }
            }
            
            languageData[language] = translations;
        }

        private SystemLanguage GetLanguageFromString(string languageName)
        {
            switch (languageName.ToLower())
            {
                case "korean":
                case "ko":
                    return SystemLanguage.Korean;
                case "english":
                case "en":
                    return SystemLanguage.English;
                case "japanese":
                case "ja":
                    return SystemLanguage.Japanese;
                case "chinese":
                case "zh":
                    return SystemLanguage.Chinese;
                default:
                    return SystemLanguage.English;
            }
        }

        private bool IsLanguageSupported(SystemLanguage language)
        {
            return language == SystemLanguage.Korean ||
                   language == SystemLanguage.English ||
                   language == SystemLanguage.Japanese ||
                   language == SystemLanguage.Chinese;
        }

        /// <summary>
        /// 언어 변경
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            if (currentLanguage != language && IsLanguageSupported(language))
            {
                currentLanguage = language;
                PlayerPrefs.SetString("SelectedLanguage", language.ToString());
                OnLanguageChanged?.Invoke(language);
            }
        }

        /// <summary>
        /// 텍스트 번역 가져오기
        /// </summary>
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            
            // 현재 언어에서 번역 찾기
            if (languageData.ContainsKey(currentLanguage) && 
                languageData[currentLanguage].ContainsKey(key))
            {
                return languageData[currentLanguage][key];
            }
            
            // 기본 언어에서 번역 찾기
            if (languageData.ContainsKey(defaultLanguage) && 
                languageData[defaultLanguage].ContainsKey(key))
            {
                return languageData[defaultLanguage][key];
            }
            
            // 번역을 찾을 수 없는 경우 키 반환
            Debug.LogWarning($"[LocalizationManager] 번역을 찾을 수 없습니다: {key}");
            return key;
        }

        /// <summary>
        /// 특정 언어의 텍스트 번역 가져오기
        /// </summary>
        public string GetText(string key, SystemLanguage language)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            
            if (languageData.ContainsKey(language) && 
                languageData[language].ContainsKey(key))
            {
                return languageData[language][key];
            }
            
            return key;
        }

        /// <summary>
        /// 지원하는 언어 목록 가져오기
        /// </summary>
        public SystemLanguage[] GetSupportedLanguages()
        {
            List<SystemLanguage> supportedLanguages = new List<SystemLanguage>();
            
            foreach (var language in languageData.Keys)
            {
                if (IsLanguageSupported(language))
                {
                    supportedLanguages.Add(language);
                }
            }
            
            return supportedLanguages.ToArray();
        }

        /// <summary>
        /// 언어 이름 가져오기
        /// </summary>
        public string GetLanguageName(SystemLanguage language)
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
        /// 어드레서블에서 언어 파일 동적 로드 (임시)
        /// </summary>
        public void LoadLanguageFileFromAddressable(string addressableKey)
        {
            Debug.LogWarning($"[LocalizationManager] {addressableKey} - Addressables 설정이 완료되지 않아 일반 파일을 사용합니다.");
        }

        /// <summary>
        /// 모든 언어 파일을 어드레서블에서 로드 (임시)
        /// </summary>
        public void LoadAllLanguageFilesFromAddressable()
        {
            Debug.LogWarning("[LocalizationManager] Addressables 설정이 완료되지 않아 일반 파일을 사용합니다.");
        }
    }
}
