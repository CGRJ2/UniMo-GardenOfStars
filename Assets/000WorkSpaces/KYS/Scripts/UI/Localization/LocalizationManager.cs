using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace KYS
{
    /// <summary>
    /// CSV 기반 다국어 지원 관리자 (싱글톤 패턴 + Addressable)
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        #region Serialized Fields

        [Header("Addressable Localization Settings")]
        [SerializeField] private AssetReferenceT<TextAsset> csvLanguageFileReference;
        [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.Korean;
        
        [Header("Behavior Settings")]
        [Tooltip("최초 실행 시 시스템 언어 대신 기본 언어를 우선 적용할지 여부")]
        [SerializeField] private bool preferDefaultOnFirstRun = true;

        #endregion

        #region Private Fields

        // 언어 데이터 (CSV 기반)
        private Dictionary<string, Dictionary<SystemLanguage, string>> languageData = new();
        private SystemLanguage currentLanguage;
        
        // 지원하는 언어 목록 (향후 확장 가능)
        private readonly SystemLanguage[] allSupportedLanguages = {
            SystemLanguage.Korean,
            SystemLanguage.English,
            SystemLanguage.Japanese,
            SystemLanguage.Chinese
        };
        
        // 현재 활성화된 언어 목록 (실제 번역이 있는 언어만)
        private List<SystemLanguage> activeLanguages = new();
        
        // Addressable 핸들 관리
        private AsyncOperationHandle<TextAsset> csvHandle;
        private bool isInitialized = false;

        #endregion

        #region Events

        public event Action<SystemLanguage> OnLanguageChanged;
        public event Action<SystemLanguage[]> OnSupportedLanguagesLoaded;
        public event Action<SystemLanguage[]> OnActiveLanguagesLoaded;

        #endregion

        #region Properties

        public SystemLanguage CurrentLanguage => currentLanguage;
        public SystemLanguage DefaultLanguage => defaultLanguage;
        public SystemLanguage[] AllSupportedLanguages => allSupportedLanguages;
        public SystemLanguage[] ActiveLanguages => activeLanguages.ToArray();
        public bool IsInitialized => isInitialized;

        #endregion

        #region Unity Lifecycle

        private async void Awake()
        {
            SingletonInit();
            await InitializeLocalization();
        }

        private void OnDestroy()
        {
            ReleaseAddressables();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Localization 초기화
        /// </summary>
        private async System.Threading.Tasks.Task InitializeLocalization()
        {
            try
            {
                Debug.Log("[LocalizationManager] Localization 초기화 시작");
                
                // PlayerPrefs에서 저장된 언어 설정 우선 적용
                string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "");
                Debug.Log($"[LocalizationManager] 저장된 언어 설정: {savedLanguage}");
                SystemLanguage savedLang = default;
                bool hasSavedLanguage = !string.IsNullOrEmpty(savedLanguage) &&
                                         System.Enum.TryParse<SystemLanguage>(savedLanguage, out savedLang);
                
                if (hasSavedLanguage && IsLanguageSupported(savedLang))
                {
                    currentLanguage = savedLang;
                    Debug.Log($"[LocalizationManager] 저장된 언어 적용: {currentLanguage}");
                }
                else if (preferDefaultOnFirstRun)
                {
                    // 최초 실행 시 기본 언어 우선
                    currentLanguage = defaultLanguage;
                    Debug.Log($"[LocalizationManager] 저장된 언어 없음 → 기본 언어 적용: {currentLanguage}");
                }
                else
                {
                    // 시스템 언어 감지 후 지원 여부 확인
                    SystemLanguage systemLanguage = Application.systemLanguage;
                    Debug.Log($"[LocalizationManager] 시스템 언어: {systemLanguage}");
                    currentLanguage = IsLanguageSupported(systemLanguage) ? systemLanguage : defaultLanguage;
                    if (currentLanguage == defaultLanguage)
                    {
                        Debug.Log($"[LocalizationManager] 시스템 언어 미지원 → 기본 언어 적용: {defaultLanguage}");
                    }
                }
                
                Debug.Log($"[LocalizationManager] 최종 선택된 언어: {currentLanguage} ({GetLanguageName(currentLanguage)})");
                
                // Addressable에서 CSV 파일 로드
                await LoadCSVFileFromAddressable();
                
                isInitialized = true;
                Debug.Log($"[LocalizationManager] Localization 초기화 완료. 언어: {GetLanguageName(currentLanguage)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] 초기화 중 오류 발생: {e.Message}");
            }
        }

        /// <summary>
        /// Addressable에서 CSV 파일 로드
        /// </summary>
        private async System.Threading.Tasks.Task LoadCSVFileFromAddressable()
        {
            if (csvLanguageFileReference == null || !csvLanguageFileReference.RuntimeKeyIsValid())
            {
                Debug.LogError("[LocalizationManager] CSV 파일 참조가 설정되지 않았습니다.");
                return;
            }

            try
            {
                Debug.Log("[LocalizationManager] Addressable에서 CSV 파일 로드 시작");
                
                csvHandle = Addressables.LoadAssetAsync<TextAsset>(csvLanguageFileReference);
                TextAsset csvFile = await csvHandle.Task;
                
                if (csvFile != null)
                {
                    ParseCSVLanguageFile(csvFile.text);
                    OnSupportedLanguagesLoaded?.Invoke(allSupportedLanguages);
                    OnActiveLanguagesLoaded?.Invoke(activeLanguages.ToArray());
                    Debug.Log($"[LocalizationManager] CSV 파일 로드 완료. 활성 언어: {activeLanguages.Count}개");
                }
                else
                {
                    Debug.LogError("[LocalizationManager] CSV 파일 로드 실패");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 파일 로드 중 오류: {e.Message}");
            }
        }

        #endregion

        #region CSV Parsing

        /// <summary>
        /// CSV 파일 파싱
        /// </summary>
        private void ParseCSVLanguageFile(string csvText)
        {
            Debug.Log("[LocalizationManager] CSV 파일 파싱 시작");
            Debug.Log($"[LocalizationManager] CSV 텍스트 길이: {csvText.Length}");
            
            languageData.Clear();
            activeLanguages.Clear();
            
            string[] lines = csvText.Split('\n');
            Debug.Log($"[LocalizationManager] CSV 라인 수: {lines.Length}");
            if (lines.Length < 2)
            {
                Debug.LogError("[LocalizationManager] CSV 파일 형식이 올바르지 않습니다.");
                return;
            }
            
            // 헤더 라인 파싱 (언어 코드)
            string[] headers = ParseCSVLine(lines[0]);
            Debug.Log($"[LocalizationManager] 헤더: {string.Join(", ", headers)}");
            
            Dictionary<int, SystemLanguage> languageIndexMap = new();
            HashSet<SystemLanguage> languagesWithData = new();
            
            for (int i = 1; i < headers.Length; i++) // Key 열 제외
            {
                SystemLanguage lang = GetLanguageFromCode(headers[i]);
                Debug.Log($"[LocalizationManager] 헤더 {i}: '{headers[i]}' -> {lang}");
                if (IsLanguageSupported(lang))
                {
                    languageIndexMap[i] = lang;
                    Debug.Log($"[LocalizationManager] 지원되는 언어 추가: {lang} (인덱스 {i})");
                }
                else
                {
                    Debug.LogWarning($"[LocalizationManager] 지원되지 않는 언어: {lang} (헤더: {headers[i]})");
                }
            }
            
            // 데이터 라인 파싱
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim())) continue;
                
                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 2) continue;
                
                string key = values[0].Trim();
                if (string.IsNullOrEmpty(key) || key.StartsWith("#")) continue;
                
                // 각 언어별 번역 데이터 저장
                for (int j = 1; j < values.Length && j < headers.Length; j++)
                {
                    if (languageIndexMap.TryGetValue(j, out SystemLanguage lang))
                    {
                        string translation = values[j].Trim();
                        if (!string.IsNullOrEmpty(translation))
                        {
                            if (!languageData.ContainsKey(key))
                            {
                                languageData[key] = new Dictionary<SystemLanguage, string>();
                            }
                            languageData[key][lang] = translation;
                            languagesWithData.Add(lang);
                        }
                    }
                }
            }
            
            // 활성 언어 목록 업데이트
            activeLanguages.Clear();
            foreach (var lang in languagesWithData)
            {
                activeLanguages.Add(lang);
            }
            
            // 기본 언어는 항상 활성화
            if (!activeLanguages.Contains(defaultLanguage))
            {
                activeLanguages.Add(defaultLanguage);
            }
            
            Debug.Log($"[LocalizationManager] 파싱 완료 - 총 키 수: {languageData.Count}");
            Debug.Log($"[LocalizationManager] 활성 언어: {string.Join(", ", activeLanguages.Select(l => GetLanguageName(l)))}");
            
            // 현재 언어에 대한 번역 데이터 확인
            if (languageData.Count > 0)
            {
                var sampleKey = languageData.Keys.First();
                if (languageData[sampleKey].ContainsKey(currentLanguage))
                {
                    Debug.Log($"[LocalizationManager] 샘플 번역 확인 - 키: {sampleKey}, 언어: {GetLanguageName(currentLanguage)}, 번역: {languageData[sampleKey][currentLanguage]}");
                }
                else
                {
                    Debug.LogWarning($"[LocalizationManager] 현재 언어({GetLanguageName(currentLanguage)})에 대한 번역이 없음. 사용 가능한 언어: {string.Join(", ", languageData[sampleKey].Keys.Select(k => GetLanguageName(k)))}");
                }
            }
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

        #endregion

        #region Language Management

        /// <summary>
        /// 언어 코드를 SystemLanguage로 변환
        /// </summary>
        private SystemLanguage GetLanguageFromCode(string code)
        {
            switch (code.Trim().ToLower())
            {
                case "korean":
                case "ko":
                case "kr":
                    return SystemLanguage.Korean;
                case "english":
                case "en":
                case "eng":
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
                PlayerPrefs.Save();
                OnLanguageChanged?.Invoke(language);
                Debug.Log($"[LocalizationManager] 언어가 변경되었습니다: {GetLanguageName(language)}");
            }
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
        /// 언어가 활성화되어 있는지 확인
        /// </summary>
        public bool IsLanguageActive(SystemLanguage language)
        {
            return activeLanguages.Contains(language);
        }

        /// <summary>
        /// 현재 언어의 인덱스 가져오기
        /// </summary>
        public int GetCurrentLanguageIndex()
        {
            for (int i = 0; i < allSupportedLanguages.Length; i++)
            {
                if (allSupportedLanguages[i] == currentLanguage)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 인덱스로 언어 가져오기
        /// </summary>
        public SystemLanguage GetLanguageByIndex(int index)
        {
            if (index >= 0 && index < allSupportedLanguages.Length)
            {
                return allSupportedLanguages[index];
            }
            return defaultLanguage;
        }

        /// <summary>
        /// 언어의 인덱스 가져오기
        /// </summary>
        public int GetLanguageIndex(SystemLanguage language)
        {
            for (int i = 0; i < allSupportedLanguages.Length; i++)
            {
                if (allSupportedLanguages[i] == language)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region Text Retrieval

        /// <summary>
        /// 텍스트 번역 가져오기
        /// </summary>
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            
            // 디버그 로그 추가
            Debug.Log($"[LocalizationManager] GetText 호출: key={key}, currentLanguage={currentLanguage}, isInitialized={isInitialized}");
            
            // 현재 언어에서 번역 찾기
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(currentLanguage))
            {
                string result = languageData[key][currentLanguage];
                Debug.Log($"[LocalizationManager] 번역 찾음: {key} -> {result}");
                return result;
            }
            
            // 기본 언어에서 번역 찾기
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(defaultLanguage))
            {
                string result = languageData[key][defaultLanguage];
                Debug.Log($"[LocalizationManager] 기본 언어 번역 사용: {key} -> {result}");
                return result;
            }
            
            // 번역을 찾을 수 없는 경우 키 반환
            Debug.LogWarning($"[LocalizationManager] 번역을 찾을 수 없습니다: {key}, languageData.Count={languageData.Count}");
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
                return languageData[key][language];
            }
            
            return key;
        }

        #endregion

        #region Translation Analysis

        /// <summary>
        /// 특정 언어의 번역 완성도 확인
        /// </summary>
        public float GetTranslationCompleteness(SystemLanguage language)
        {
            if (!IsLanguageSupported(language))
                return 0f;
            
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

        #endregion

        #region Addressable Management

        /// <summary>
        /// Addressable에서 CSV 파일 동적 로드
        /// </summary>
        public async System.Threading.Tasks.Task LoadCSVFileFromAddressable(string addressableKey)
        {
            try
            {
                Debug.Log($"[LocalizationManager] Addressable에서 CSV 파일 로드: {addressableKey}");
                
                // 기존 핸들 해제
                if (csvHandle.IsValid())
                {
                    Addressables.Release(csvHandle);
                }
                
                csvHandle = Addressables.LoadAssetAsync<TextAsset>(addressableKey);
                TextAsset csvFile = await csvHandle.Task;
                
                if (csvFile != null)
                {
                    ParseCSVLanguageFile(csvFile.text);
                    OnSupportedLanguagesLoaded?.Invoke(allSupportedLanguages);
                    OnActiveLanguagesLoaded?.Invoke(activeLanguages.ToArray());
                    Debug.Log($"[LocalizationManager] CSV 파일 로드 완료. 활성 언어: {activeLanguages.Count}개");
                }
                else
                {
                    Debug.LogError($"[LocalizationManager] CSV 파일 로드 실패: {addressableKey}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 파일 로드 중 오류: {e.Message}");
            }
        }

        /// <summary>
        /// Addressable 리소스 해제
        /// </summary>
        private void ReleaseAddressables()
        {
            if (csvHandle.IsValid())
            {
                Addressables.Release(csvHandle);
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("Print Localization Info")]
        public void PrintLocalizationInfo()
        {
            Debug.Log($"[LocalizationManager] 현재 언어: {GetLanguageName(currentLanguage)}");
            Debug.Log($"[LocalizationManager] 활성 언어: {activeLanguages.Count}개");
            foreach (var lang in activeLanguages)
            {
                float completeness = GetTranslationCompleteness(lang);
                Debug.Log($"  - {GetLanguageName(lang)}: {completeness * 100:F1}%");
            }
            Debug.Log($"[LocalizationManager] 총 번역 키: {languageData.Count}개");
            Debug.Log($"[LocalizationManager] 초기화 상태: {isInitialized}");
        }

        [ContextMenu("Reset Saved Language (Delete PlayerPrefs)")]
        public void ResetSavedLanguage()
        {
            PlayerPrefs.DeleteKey("SelectedLanguage");
            PlayerPrefs.Save();
            Debug.Log("[LocalizationManager] 저장된 언어 설정(SelectedLanguage) 삭제 완료");
        }

        #endregion
    }
}
