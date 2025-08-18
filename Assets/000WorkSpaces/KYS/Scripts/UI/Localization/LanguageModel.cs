using System;
using System.Collections.Generic;
using UnityEngine;
using KYS.UI.MVP;

namespace KYS.UI.Localization
{
    /// <summary>
    /// 다국어 시스템 MVP Model
    /// </summary>
    public class LanguageModel : BaseUIModel
    {
        [Header("Language Data")]
        [SerializeField] private TextAsset csvLanguageFile;
        
        // 언어 데이터
        private Dictionary<string, Dictionary<SystemLanguage, string>> languageData = new();
        private SystemLanguage currentLanguage = SystemLanguage.Korean;
        private SystemLanguage defaultLanguage = SystemLanguage.Korean;
        
        // 지원하는 언어 목록 (향후 확장 가능)
        private readonly SystemLanguage[] allSupportedLanguages = {
            SystemLanguage.Korean,
            SystemLanguage.English,
            SystemLanguage.Japanese,
            SystemLanguage.Chinese
        };
        
        // 현재 활성화된 언어 목록 (실제 번역이 있는 언어만)
        private List<SystemLanguage> activeLanguages = new();
        
        // 이벤트
        public event Action<SystemLanguage> OnLanguageChanged;
        public event Action<SystemLanguage[]> OnSupportedLanguagesLoaded;
        public event Action<SystemLanguage[]> OnActiveLanguagesLoaded;
        
        // 프로퍼티
        public SystemLanguage CurrentLanguage => currentLanguage;
        public SystemLanguage DefaultLanguage => defaultLanguage;
        public SystemLanguage[] AllSupportedLanguages => allSupportedLanguages;
        public SystemLanguage[] ActiveLanguages => activeLanguages.ToArray();
        
        protected override void OnInitialize()
        {
            LoadLanguageData();
        }
        
        /// <summary>
        /// CSV 파일에서 언어 데이터 로드
        /// </summary>
        private void LoadLanguageData()
        {
            if (csvLanguageFile == null)
            {
                Debug.LogError("[LanguageModel] CSV 언어 파일이 설정되지 않았습니다.");
                return;
            }
            
            ParseCSVLanguageFile(csvLanguageFile.text);
            OnSupportedLanguagesLoaded?.Invoke(allSupportedLanguages);
            OnActiveLanguagesLoaded?.Invoke(activeLanguages.ToArray());
        }
        
        /// <summary>
        /// CSV 파일 파싱
        /// </summary>
        private void ParseCSVLanguageFile(string csvText)
        {
            languageData.Clear();
            activeLanguages.Clear();
            
            string[] lines = csvText.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError("[LanguageModel] CSV 파일 형식이 올바르지 않습니다.");
                return;
            }
            
            // 헤더 라인 파싱 (언어 코드)
            string[] headers = ParseCSVLine(lines[0]);
            Dictionary<int, SystemLanguage> languageIndexMap = new();
            HashSet<SystemLanguage> languagesWithData = new();
            
            for (int i = 1; i < headers.Length; i++) // Key 열 제외
            {
                SystemLanguage lang = GetLanguageFromCode(headers[i]);
                if (IsLanguageSupported(lang))
                {
                    languageIndexMap[i] = lang;
                }
            }
            
            // 데이터 라인 파싱
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim())) continue;
                
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 2) continue;
                
                string key = values[0].Trim();
                if (string.IsNullOrEmpty(key) || key.StartsWith("#")) continue; // 주석 처리
                
                // 각 언어별 번역 저장
                foreach (var kvp in languageIndexMap)
                {
                    int columnIndex = kvp.Key;
                    SystemLanguage language = kvp.Value;
                    
                    if (columnIndex < values.Length)
                    {
                        string translation = values[columnIndex].Trim();
                        // 따옴표 제거
                        if (translation.StartsWith("\"") && translation.EndsWith("\""))
                        {
                            translation = translation.Substring(1, translation.Length - 2);
                        }
                        
                        // 빈 번역이 아닌 경우에만 저장
                        if (!string.IsNullOrEmpty(translation))
                        {
                            if (!languageData.ContainsKey(key))
                            {
                                languageData[key] = new Dictionary<SystemLanguage, string>();
                            }
                            
                            languageData[key][language] = translation;
                            languagesWithData.Add(language);
                        }
                    }
                }
            }
            
            // 활성 언어 목록 업데이트
            activeLanguages.AddRange(languagesWithData);
            
            // 기본 언어가 활성 언어에 없으면 추가
            if (!activeLanguages.Contains(defaultLanguage))
            {
                activeLanguages.Add(defaultLanguage);
            }
            
            Debug.Log($"[LanguageModel] 언어 데이터 로드 완료: {languageData.Count}개 키, 활성 언어: {activeLanguages.Count}개");
        }
        
        /// <summary>
        /// CSV 라인 파싱 (쉼표와 따옴표 처리)
        /// </summary>
        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string currentValue = "";
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentValue);
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }
            
            result.Add(currentValue);
            return result.ToArray();
        }
        
        /// <summary>
        /// 언어 코드를 SystemLanguage로 변환
        /// </summary>
        private SystemLanguage GetLanguageFromCode(string code)
        {
            switch (code.ToLower().Trim())
            {
                case "korean":
                case "ko":
                case "kr":
                    return SystemLanguage.Korean;
                case "english":
                case "en":
                case "us":
                    return SystemLanguage.English;
                case "japanese":
                case "ja":
                case "jp":
                    return SystemLanguage.Japanese;
                case "chinese":
                case "zh":
                case "cn":
                    return SystemLanguage.Chinese;
                default:
                    return SystemLanguage.English;
            }
        }
        
        /// <summary>
        /// 언어 지원 여부 확인
        /// </summary>
        private bool IsLanguageSupported(SystemLanguage language)
        {
            return Array.Exists(allSupportedLanguages, lang => lang == language);
        }
        
        /// <summary>
        /// 언어가 활성화되어 있는지 확인
        /// </summary>
        public bool IsLanguageActive(SystemLanguage language)
        {
            return activeLanguages.Contains(language);
        }
        
        /// <summary>
        /// 언어 변경
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            if (!IsLanguageSupported(language))
            {
                Debug.LogWarning($"[LanguageModel] 지원하지 않는 언어: {language}");
                return;
            }
            
            // 활성 언어가 아니면 기본 언어로 변경
            if (!IsLanguageActive(language))
            {
                Debug.LogWarning($"[LanguageModel] 활성화되지 않은 언어: {language}, 기본 언어로 변경합니다.");
                language = defaultLanguage;
            }
            
            if (currentLanguage != language)
            {
                currentLanguage = language;
                PlayerPrefs.SetString("SelectedLanguage", language.ToString());
                OnLanguageChanged?.Invoke(language);
                
                Debug.Log($"[LanguageModel] 언어 변경: {GetLanguageName(language)}");
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
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(currentLanguage))
            {
                string translation = languageData[key][currentLanguage];
                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }
            }
            
            // 기본 언어에서 번역 찾기
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(defaultLanguage))
            {
                string translation = languageData[key][defaultLanguage];
                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }
            }
            
            // 영어에서 번역 찾기 (기본 언어가 영어가 아닌 경우)
            if (defaultLanguage != SystemLanguage.English && 
                languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(SystemLanguage.English))
            {
                string translation = languageData[key][SystemLanguage.English];
                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }
            }
            
            // 번역을 찾을 수 없는 경우 키 반환
            Debug.LogWarning($"[LanguageModel] 번역을 찾을 수 없습니다: {key}");
            return key;
        }
        
        /// <summary>
        /// 특정 언어의 텍스트 번역 가져오기
        /// </summary>
        public string GetText(string key, SystemLanguage language)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(language))
            {
                string translation = languageData[key][language];
                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }
            }
            
            return key;
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
        /// 현재 언어의 인덱스 가져오기 (활성 언어 기준)
        /// </summary>
        public int GetCurrentLanguageIndex()
        {
            return activeLanguages.IndexOf(currentLanguage);
        }
        
        /// <summary>
        /// 인덱스로 언어 가져오기 (활성 언어 기준)
        /// </summary>
        public SystemLanguage GetLanguageByIndex(int index)
        {
            if (index >= 0 && index < activeLanguages.Count)
            {
                return activeLanguages[index];
            }
            return defaultLanguage;
        }
        
        /// <summary>
        /// 활성 언어 목록에서 언어 인덱스 가져오기
        /// </summary>
        public int GetLanguageIndex(SystemLanguage language)
        {
            return activeLanguages.IndexOf(language);
        }
        
        /// <summary>
        /// 번역 완성도 확인
        /// </summary>
        public float GetTranslationCompleteness(SystemLanguage language)
        {
            if (!IsLanguageActive(language)) return 0f;
            
            int totalKeys = languageData.Count;
            if (totalKeys == 0) return 0f;
            
            int translatedKeys = 0;
            foreach (var keyData in languageData.Values)
            {
                if (keyData.ContainsKey(language) && !string.IsNullOrEmpty(keyData[language]))
                {
                    translatedKeys++;
                }
            }
            
            return (float)translatedKeys / totalKeys;
        }
        
        #region Excel 호환성 가이드
        
        /// <summary>
        /// Excel 호환성 문제 해결 가이드
        /// </summary>
        [ContextMenu("Excel 호환성 가이드")]
        public void ShowExcelCompatibilityGuide()
        {
            Debug.Log(@"[LanguageModel] Excel 호환성 해결 방법:
            
1. 메모장에서 CSV 파일 열기
2. 파일 → 다른 이름으로 저장
3. 인코딩: ANSI 선택
4. 저장 후 Excel에서 열기

또는

1. Excel에서 데이터 → 텍스트/CSV에서
2. 파일 선택 후 파일 원본: 65001: 유니코드(UTF-8) 선택
3. 로드

Unity에서는 어떤 인코딩이든 자동으로 읽을 수 있습니다!");
        }
        
        #endregion
    }
}
