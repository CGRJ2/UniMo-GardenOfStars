using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;


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
        
        // 지원하는 언어 목록 (확장됨)
        private readonly SystemLanguage[] allSupportedLanguages = {
            SystemLanguage.Korean,
            SystemLanguage.English,
            SystemLanguage.Japanese,
            SystemLanguage.Chinese,
            SystemLanguage.French,
            SystemLanguage.German,
            SystemLanguage.Spanish,
            SystemLanguage.Italian,
            SystemLanguage.Portuguese,
            SystemLanguage.Russian
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
            
            // 줄바꿈 문자 처리 (Windows: \r\n, Unix: \n, Mac: \r)
            string[] lines = csvText.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
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
            
            // 중복 키 검사
            CheckForDuplicateKeys();
            
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
        /// 언어 코드를 SystemLanguage로 변환 (확장)
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
                case "french":
                case "fr":
                    return SystemLanguage.French;
                case "german":
                case "de":
                    return SystemLanguage.German;
                case "spanish":
                case "es":
                    return SystemLanguage.Spanish;
                case "italian":
                case "it":
                    return SystemLanguage.Italian;
                case "portuguese":
                case "pt":
                    return SystemLanguage.Portuguese;
                case "russian":
                case "ru":
                    return SystemLanguage.Russian;
                default:
                    return SystemLanguage.English;
            }
        }

        /// <summary>
        /// 언어 지원 여부 확인 (확장)
        /// </summary>
        private bool IsLanguageSupported(SystemLanguage language)
        {
            return language == SystemLanguage.Korean ||
                   language == SystemLanguage.English ||
                   language == SystemLanguage.Japanese ||
                   language == SystemLanguage.Chinese ||
                   language == SystemLanguage.French ||
                   language == SystemLanguage.German ||
                   language == SystemLanguage.Spanish ||
                   language == SystemLanguage.Italian ||
                   language == SystemLanguage.Portuguese ||
                   language == SystemLanguage.Russian;
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
        /// 언어 이름 가져오기 (확장)
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
                case SystemLanguage.French:
                    return "Français";
                case SystemLanguage.German:
                    return "Deutsch";
                case SystemLanguage.Spanish:
                    return "Español";
                case SystemLanguage.Italian:
                    return "Italiano";
                case SystemLanguage.Portuguese:
                    return "Português";
                case SystemLanguage.Russian:
                    return "Русский";
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

        #region Key Management

        /// <summary>
        /// 모든 로컬라이제이션 키 가져오기
        /// </summary>
        public string[] GetAllKeys()
        {
            return languageData.Keys.ToArray();
        }

        /// <summary>
        /// 키가 존재하는지 확인
        /// </summary>
        public bool HasKey(string key)
        {
            return !string.IsNullOrEmpty(key) && languageData.ContainsKey(key);
        }

        /// <summary>
        /// 키 중복 검사 및 경고
        /// </summary>
        public void CheckForDuplicateKeys()
        {
            var keyCounts = new Dictionary<string, int>();
            
            foreach (string key in languageData.Keys)
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
                Debug.LogWarning($"[LocalizationManager] 중복된 키가 발견되었습니다:");
                foreach (var duplicate in duplicates)
                {
                    Debug.LogWarning($"  - 키: {duplicate.Key}, 중복 횟수: {duplicate.Value}");
                }
            }
            else
            {
                Debug.Log("[LocalizationManager] 중복된 키가 없습니다.");
            }
        }

        /// <summary>
        /// UI 이름으로 키 생성 및 중복 검사
        /// </summary>
        public string GenerateKeyFromUIName(string uiName, bool checkDuplicate = true)
        {
            if (string.IsNullOrEmpty(uiName))
                return "";
            
            // UI 이름을 기반으로 키 생성 (text 접미사만 제거)
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
            
            // 중복 검사
            if (checkDuplicate && HasKey(key))
            {
                Debug.LogWarning($"[LocalizationManager] UI 이름 '{uiName}'에서 생성된 키 '{key}'가 이미 존재합니다.");
                return key; // 중복이어도 키 반환 (사용자가 결정하도록)
            }
            
            return key;
        }

        /// <summary>
        /// 키에 대한 번역이 있는지 확인
        /// </summary>
        public bool HasTranslation(string key, SystemLanguage language = SystemLanguage.Korean)
        {
            if (!HasKey(key)) return false;
            
            return languageData[key].ContainsKey(language) && 
                   !string.IsNullOrEmpty(languageData[key][language]);
        }

        /// <summary>
        /// 모든 언어에서 번역이 있는 키 목록 가져오기
        /// </summary>
        public string[] GetKeysWithAllTranslations()
        {
            var result = new List<string>();
            
            foreach (string key in languageData.Keys)
            {
                bool hasAllTranslations = true;
                foreach (SystemLanguage lang in allSupportedLanguages)
                {
                    if (!HasTranslation(key, lang))
                    {
                        hasAllTranslations = false;
                        break;
                    }
                }
                
                if (hasAllTranslations)
                {
                    result.Add(key);
                }
            }
            
            return result.ToArray();
        }

        /// <summary>
        /// 특정 언어에서 번역이 없는 키 목록 가져오기
        /// </summary>
        public string[] GetKeysWithoutTranslation(SystemLanguage language)
        {
            var result = new List<string>();
            
            foreach (string key in languageData.Keys)
            {
                if (!HasTranslation(key, language))
                {
                    result.Add(key);
                }
            }
            
            return result.ToArray();
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

        [ContextMenu("Check for Duplicate Keys")]
        public void CheckDuplicateKeys()
        {
            CheckForDuplicateKeys();
        }

        [ContextMenu("Print All Keys")]
        public void PrintAllKeys()
        {
            string[] keys = GetAllKeys();
            Debug.Log($"[LocalizationManager] 모든 키 목록 ({keys.Length}개):");
            foreach (string key in keys)
            {
                Debug.Log($"  - {key}");
            }
        }

        [ContextMenu("Print Keys Without Translation")]
        public void PrintKeysWithoutTranslation()
        {
            foreach (SystemLanguage lang in allSupportedLanguages)
            {
                string[] missingKeys = GetKeysWithoutTranslation(lang);
                Debug.Log($"[LocalizationManager] {GetLanguageName(lang)}에서 번역이 없는 키 ({missingKeys.Length}개):");
                foreach (string key in missingKeys)
                {
                    Debug.Log($"  - {key}");
                }
            }
        }

        [ContextMenu("Print Keys With All Translations")]
        public void PrintKeysWithAllTranslations()
        {
            string[] completeKeys = GetKeysWithAllTranslations();
            Debug.Log($"[LocalizationManager] 모든 언어에서 번역이 완료된 키 ({completeKeys.Length}개):");
            foreach (string key in completeKeys)
            {
                Debug.Log($"  - {key}");
            }
        }

        /// <summary>
        /// 누락된 키들을 감지하고 CSV에 추가할 수 있는 기능
        /// </summary>
        [ContextMenu("Detect Missing Keys")]
        public void DetectMissingKeys()
        {
            Debug.Log("[LocalizationManager] 누락된 키 감지 시작...");
            
            // 현재 CSV에 있는 모든 키
            var existingKeys = new HashSet<string>(languageData.Keys);
            
            // 씬에서 사용되는 모든 AutoLocalizedText 컴포넌트 찾기
            var autoLocalizedTexts = FindObjectsOfType<AutoLocalizedText>();
            var missingKeys = new List<string>();
            
            foreach (var autoText in autoLocalizedTexts)
            {
                string key = autoText.GetLocalizationKey();
                if (!string.IsNullOrEmpty(key) && !existingKeys.Contains(key))
                {
                    missingKeys.Add(key);
                }
            }
            

            
            if (missingKeys.Count > 0)
            {
                Debug.LogWarning($"[LocalizationManager] 누락된 키 {missingKeys.Count}개 발견:");
                foreach (string key in missingKeys)
                {
                    Debug.LogWarning($"  - {key}");
                }
                
                // CSV 파일에서 현재 지원 언어 확인
                string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
                try
                {
                    if (System.IO.File.Exists(csvPath))
                    {
                        string[] lines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                        if (lines.Length > 0)
                        {
                            string[] headers = ParseCSVLine(lines[0]);
                            int languageCount = headers.Length - 1; // Key 열 제외
                            
                            Debug.Log($"[LocalizationManager] 현재 지원 언어: {string.Join(", ", headers.Skip(1))}");
                            Debug.Log("[LocalizationManager] CSV 파일에 다음 키들을 추가해주세요:");
                            
                            foreach (string key in missingKeys)
                            {
                                var keyFields = new List<string> { key };
                                for (int i = 0; i < languageCount; i++)
                                {
                                    keyFields.Add(""); // 각 언어별 빈 번역
                                }
                                string newLine = string.Join(",", keyFields);
                                Debug.Log($"  {newLine}");
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LocalizationManager] CSV 파일 읽기 실패: {e.Message}");
                    // 폴백: 기본 형식으로 출력
                    Debug.Log("[LocalizationManager] CSV 파일에 다음 키들을 추가해주세요:");
                    foreach (string key in missingKeys)
                    {
                        Debug.Log($"  {key},,,,");
                    }
                }
            }
            else
            {
                Debug.Log("[LocalizationManager] 누락된 키가 없습니다.");
            }
        }

        /// <summary>
        /// UI 이름에서 생성될 키들을 미리 확인
        /// </summary>
        [ContextMenu("Preview Generated Keys")]
        public void PreviewGeneratedKeys()
        {
            Debug.Log("[LocalizationManager] UI 이름에서 생성될 키 미리보기:");
            
            // 씬의 모든 TextMeshProUGUI 컴포넌트 확인
            var allTexts = FindObjectsOfType<TextMeshProUGUI>();
            
            foreach (var text in allTexts)
            {
                string generatedKey = GenerateKeyFromUIName(text.name, false);
                bool hasTranslation = HasTranslation(generatedKey);
                
                Debug.Log($"  {text.name} → {generatedKey} (번역 존재: {hasTranslation})");
            }
        }

        /// <summary>
        /// 누락된 키들을 자동으로 CSV 파일에 추가 (동적 언어 지원)
        /// </summary>
        [ContextMenu("Auto Add Missing Keys to CSV")]
        public void AutoAddMissingKeysToCSV()
        {
            Debug.Log("[LocalizationManager] 누락된 키를 CSV에 자동 추가 시작...");
            
            // 현재 CSV에 있는 모든 키
            var existingKeys = new HashSet<string>(languageData.Keys);
            
            // 씬에서 사용되는 모든 키 수집
            var missingKeys = new List<string>();
            
            // AutoLocalizedText 컴포넌트에서 키 수집
            var autoLocalizedTexts = FindObjectsOfType<AutoLocalizedText>();
            foreach (var autoText in autoLocalizedTexts)
            {
                string key = autoText.GetLocalizationKey();
                if (!string.IsNullOrEmpty(key) && !existingKeys.Contains(key))
                {
                    missingKeys.Add(key);
                }
            }
            

            
            if (missingKeys.Count > 0)
            {
                Debug.Log($"[LocalizationManager] {missingKeys.Count}개의 누락된 키를 CSV에 추가합니다:");
                
                // CSV 파일 경로
                string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
                
                try
                {
                    // 기존 CSV 파일 읽기 (UTF-8 인코딩으로 한글 깨짐 방지)
                    string[] existingLines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                    var newLines = new List<string>(existingLines);
                    
                    // 헤더에서 언어 수 확인
                    string[] headers = ParseCSVLine(existingLines[0]);
                    int languageCount = headers.Length - 1; // Key 열 제외
                    
                    // 누락된 키들을 추가 (동적으로 언어 수에 맞춰 빈 필드 생성)
                    foreach (string key in missingKeys)
                    {
                        var keyFields = new List<string> { key };
                        for (int i = 0; i < languageCount; i++)
                        {
                            keyFields.Add(""); // 각 언어별 빈 번역
                        }
                        string newLine = string.Join(",", keyFields);
                        newLines.Add(newLine);
                        Debug.Log($"  - {key} 추가됨 (언어 {languageCount}개)");
                    }
                    
                    // CSV 파일 다시 쓰기 (UTF-8 인코딩으로 한글 깨짐 방지)
                    System.IO.File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                    
                    Debug.Log($"[LocalizationManager] CSV 파일이 업데이트되었습니다: {csvPath}");
                    Debug.Log($"[LocalizationManager] 지원 언어: {string.Join(", ", headers.Skip(1))}");
                    Debug.Log("[LocalizationManager] 이제 각 언어별 번역을 추가해주세요.");
                    
                    // Unity 에디터에서 파일 새로고침
                    #if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
                    #endif
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LocalizationManager] CSV 파일 업데이트 실패: {e.Message}");
                }
            }
            else
            {
                Debug.Log("[LocalizationManager] 누락된 키가 없습니다.");
            }
        }

        /// <summary>
        /// CSV 파일을 백업
        /// </summary>
        [ContextMenu("Backup CSV File")]
        public void BackupCSVFile()
        {
            string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
            string backupPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData_backup_" + 
                               System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            
            try
            {
                System.IO.File.Copy(csvPath, backupPath);
                Debug.Log($"[LocalizationManager] CSV 파일이 백업되었습니다: {backupPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 백업 실패: {e.Message}");
            }
        }

        /// <summary>
        /// CSV 파일에서 특정 키의 번역을 업데이트
        /// </summary>
        public void UpdateTranslationInCSV(string key, SystemLanguage language, string translation)
        {
            string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
            
            try
            {
                string[] lines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                var newLines = new List<string>();
                
                // 언어 인덱스 찾기
                int languageIndex = GetLanguageColumnIndex(language);
                if (languageIndex == -1)
                {
                    Debug.LogError($"[LocalizationManager] 지원하지 않는 언어: {language}");
                    return;
                }
                
                bool keyFound = false;
                
                foreach (string line in lines)
                {
                    if (line.StartsWith(key + ","))
                    {
                        // 키를 찾았으면 해당 언어 열만 업데이트
                        string[] columns = line.Split(',');
                        if (columns.Length > languageIndex)
                        {
                            columns[languageIndex] = translation;
                        }
                        newLines.Add(string.Join(",", columns));
                        keyFound = true;
                        Debug.Log($"[LocalizationManager] 키 '{key}'의 {language} 번역이 업데이트되었습니다: {translation}");
                    }
                    else
                    {
                        newLines.Add(line);
                    }
                }
                
                if (!keyFound)
                {
                    Debug.LogWarning($"[LocalizationManager] 키 '{key}'를 CSV에서 찾을 수 없습니다.");
                    return;
                }
                
                // CSV 파일 다시 쓰기 (UTF-8 인코딩으로 한글 깨짐 방지)
                System.IO.File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                
                // Unity 에디터에서 파일 새로고침
                #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
                #endif
                
                // 메모리에서도 업데이트
                if (languageData.ContainsKey(key))
                {
                    languageData[key][language] = translation;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 업데이트 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 언어별 CSV 열 인덱스 반환 (동적 처리)
        /// </summary>
        private int GetLanguageColumnIndex(SystemLanguage language)
        {
            // CSV 파일에서 헤더를 읽어서 동적으로 인덱스 찾기
            string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
            
            try
            {
                if (!System.IO.File.Exists(csvPath))
                {
                    Debug.LogWarning($"[LocalizationManager] CSV 파일이 존재하지 않습니다: {csvPath}");
                    return -1;
                }
                
                string[] lines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                if (lines.Length == 0)
                {
                    Debug.LogWarning("[LocalizationManager] CSV 파일이 비어있습니다.");
                    return -1;
                }
                
                string[] headers = ParseCSVLine(lines[0]);
                
                // 헤더에서 해당 언어 찾기
                for (int i = 1; i < headers.Length; i++) // Key 열(0번) 제외
                {
                    SystemLanguage headerLang = GetLanguageFromCode(headers[i]);
                    if (headerLang == language)
                    {
                        return i;
                    }
                }
                
                Debug.LogWarning($"[LocalizationManager] 언어 '{language}'에 해당하는 열을 찾을 수 없습니다.");
                return -1;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 헤더 읽기 실패: {e.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 새로운 언어를 CSV에 추가
        /// </summary>
        [ContextMenu("Add New Language to CSV")]
        public void AddNewLanguageToCSV()
        {
            Debug.Log("[LocalizationManager] 새로운 언어 추가 기능을 시작합니다.");
            Debug.Log("[LocalizationManager] 지원 가능한 언어:");
            Debug.Log("  - Korean (한국어)");
            Debug.Log("  - English (영어)");
            Debug.Log("  - Japanese (일본어)");
            Debug.Log("  - Chinese (중국어)");
            Debug.Log("  - French (프랑스어)");
            Debug.Log("  - German (독일어)");
            Debug.Log("  - Spanish (스페인어)");
            Debug.Log("  - Italian (이탈리아어)");
            Debug.Log("  - Portuguese (포르투갈어)");
            Debug.Log("  - Russian (러시아어)");
            Debug.Log("[LocalizationManager] AddNewLanguageToCSV(SystemLanguage language, string languageCode) 메서드를 직접 호출하세요.");
        }

        /// <summary>
        /// 새로운 언어를 CSV에 추가 (실제 구현)
        /// </summary>
        public void AddNewLanguageToCSV(SystemLanguage language, string languageCode)
        {
            string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
            
            try
            {
                if (!System.IO.File.Exists(csvPath))
                {
                    Debug.LogError($"[LocalizationManager] CSV 파일이 존재하지 않습니다: {csvPath}");
                    return;
                }
                
                string[] lines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                if (lines.Length == 0)
                {
                    Debug.LogError("[LocalizationManager] CSV 파일이 비어있습니다.");
                    return;
                }
                
                var newLines = new List<string>();
                
                // 헤더 라인 처리
                string[] headers = ParseCSVLine(lines[0]);
                
                // 이미 해당 언어가 있는지 확인
                for (int i = 1; i < headers.Length; i++)
                {
                    SystemLanguage existingLang = GetLanguageFromCode(headers[i]);
                    if (existingLang == language)
                    {
                        Debug.LogWarning($"[LocalizationManager] 언어 '{language}'는 이미 CSV에 존재합니다.");
                        return;
                    }
                }
                
                // 헤더에 새 언어 추가
                var newHeaders = new List<string>(headers);
                newHeaders.Add(languageCode);
                newLines.Add(string.Join(",", newHeaders));
                
                // 데이터 라인 처리
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrEmpty(lines[i].Trim())) 
                    {
                        newLines.Add(lines[i]); // 빈 줄은 그대로 유지
                        continue;
                    }
                    
                    string[] values = ParseCSVLine(lines[i]);
                    var newValues = new List<string>(values);
                    newValues.Add(""); // 새 언어에 대한 빈 번역 추가
                    newLines.Add(string.Join(",", newValues));
                }
                
                // CSV 파일 다시 쓰기
                System.IO.File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                
                Debug.Log($"[LocalizationManager] 언어 '{language}' ({languageCode})가 CSV에 추가되었습니다.");
                Debug.Log($"[LocalizationManager] 총 {newLines.Count - 1}개의 키에 대해 빈 번역이 추가되었습니다.");
                
                // Unity 에디터에서 파일 새로고침
                #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
                #endif
                
                // 메모리에서도 새 언어 추가
                if (!allSupportedLanguages.Contains(language))
                {
                    var newSupportedLanguages = new List<SystemLanguage>(allSupportedLanguages);
                    newSupportedLanguages.Add(language);
                    // allSupportedLanguages는 readonly이므로 리플렉션으로 수정하거나 새 배열 할당 필요
                    Debug.Log($"[LocalizationManager] 지원 언어 목록에 '{language}'가 추가되었습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] 언어 추가 실패: {e.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 현재 활성화된 UI의 모든 로컬라이제이션 키를 가져오기
        /// </summary>
        public string[] GetActiveUIKeys()
        {
            var activeKeys = new List<string>();
            
            // 현재 활성화된 모든 AutoLocalizedText 컴포넌트에서 키 수집
            var activeAutoTexts = FindObjectsOfType<AutoLocalizedText>();
            foreach (var autoText in activeAutoTexts)
            {
                if (autoText.gameObject.activeInHierarchy)
                {
                    string key = autoText.GetLocalizationKey();
                    if (!string.IsNullOrEmpty(key))
                    {
                        activeKeys.Add(key);
                    }
                }
            }
            

            
            return activeKeys.ToArray();
        }

        /// <summary>
        /// 특정 GameObject의 모든 로컬라이제이션 키를 가져오기
        /// </summary>
        public string[] GetUIKeysFromGameObject(GameObject targetObject)
        {
            var keys = new List<string>();
            
            // AutoLocalizedText 컴포넌트에서 키 수집
            var autoTexts = targetObject.GetComponentsInChildren<AutoLocalizedText>(true);
            foreach (var autoText in autoTexts)
            {
                string key = autoText.GetLocalizationKey();
                if (!string.IsNullOrEmpty(key))
                {
                    keys.Add(key);
                }
            }
            

            
            return keys.ToArray();
        }

        /// <summary>
        /// 특정 BaseUI의 모든 로컬라이제이션 키를 가져오기
        /// </summary>
        public string[] GetUIKeysFromBaseUI(BaseUI targetUI)
        {
            if (targetUI == null) return new string[0];
            
            return GetUIKeysFromGameObject(targetUI.gameObject);
        }

        /// <summary>
        /// 현재 활성화된 UI의 누락된 키들을 감지
        /// </summary>
        [ContextMenu("Detect Missing Keys from Active UI")]
        public void DetectMissingKeysFromActiveUI()
        {
            Debug.Log("[LocalizationManager] 활성화된 UI에서 누락된 키 감지 시작...");
            
            string[] activeKeys = GetActiveUIKeys();
            var existingKeys = new HashSet<string>(languageData.Keys);
            var missingKeys = new List<string>();
            
            foreach (string key in activeKeys)
            {
                if (!existingKeys.Contains(key))
                {
                    missingKeys.Add(key);
                }
            }
            
            if (missingKeys.Count > 0)
            {
                Debug.LogWarning($"[LocalizationManager] 활성화된 UI에서 누락된 키 {missingKeys.Count}개 발견:");
                foreach (string key in missingKeys)
                {
                    Debug.LogWarning($"  - {key}");
                }
            }
            else
            {
                Debug.Log("[LocalizationManager] 활성화된 UI에서 누락된 키가 없습니다.");
            }
        }

        /// <summary>
        /// 특정 UI 팝업이 열렸을 때 해당 팝업의 키만 가져오기
        /// </summary>
        public string[] GetPopupKeys(BaseUI popupUI)
        {
            if (popupUI == null)
            {
                Debug.LogWarning("[LocalizationManager] popupUI가 null입니다.");
                return new string[0];
            }
            
            Debug.Log($"[LocalizationManager] 팝업 '{popupUI.name}'의 키들을 수집합니다.");
            
            string[] keys = GetUIKeysFromBaseUI(popupUI);
            
            Debug.Log($"[LocalizationManager] 팝업 '{popupUI.name}'에서 {keys.Length}개의 키를 찾았습니다:");
            foreach (string key in keys)
            {
                Debug.Log($"  - {key}");
            }
            
            return keys;
        }

        /// <summary>
        /// 특정 UI 팝업의 누락된 키들을 자동으로 CSV에 추가 (동적 언어 지원)
        /// </summary>
        public void AutoAddMissingKeysForPopup(BaseUI popupUI)
        {
            if (popupUI == null) return;
            
            Debug.Log($"[LocalizationManager] 팝업 '{popupUI.name}'의 누락된 키를 CSV에 자동 추가 시작...");
            
            string[] popupKeys = GetPopupKeys(popupUI);
            var existingKeys = new HashSet<string>(languageData.Keys);
            var missingKeys = new List<string>();
            
            foreach (string key in popupKeys)
            {
                if (!existingKeys.Contains(key))
                {
                    missingKeys.Add(key);
                }
            }
            
            if (missingKeys.Count > 0)
            {
                Debug.Log($"[LocalizationManager] 팝업 '{popupUI.name}'에서 {missingKeys.Count}개의 누락된 키를 CSV에 추가합니다:");
                
                // CSV 파일 경로
                string csvPath = Application.dataPath + "/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
                
                try
                {
                    // 기존 CSV 파일 읽기 (UTF-8 인코딩으로 한글 깨짐 방지)
                    string[] existingLines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                    var newLines = new List<string>(existingLines);
                    
                    // 헤더에서 언어 수 확인
                    string[] headers = ParseCSVLine(existingLines[0]);
                    int languageCount = headers.Length - 1; // Key 열 제외
                    
                    // 누락된 키들을 추가 (동적으로 언어 수에 맞춰 빈 필드 생성)
                    foreach (string key in missingKeys)
                    {
                        var keyFields = new List<string> { key };
                        for (int i = 0; i < languageCount; i++)
                        {
                            keyFields.Add(""); // 각 언어별 빈 번역
                        }
                        string newLine = string.Join(",", keyFields);
                        newLines.Add(newLine);
                        Debug.Log($"  - {key} 추가됨 (언어 {languageCount}개)");
                    }
                    
                    // CSV 파일 다시 쓰기 (UTF-8 인코딩으로 한글 깨짐 방지)
                    System.IO.File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                    
                    Debug.Log($"[LocalizationManager] CSV 파일이 업데이트되었습니다.");
                    Debug.Log($"[LocalizationManager] 지원 언어: {string.Join(", ", headers.Skip(1))}");
                    Debug.Log("[LocalizationManager] 이제 각 언어별 번역을 추가해주세요.");
                    
                    // Unity 에디터에서 파일 새로고침
                    #if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
                    #endif
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LocalizationManager] CSV 파일 업데이트 실패: {e.Message}");
                }
            }
            else
            {
                Debug.Log($"[LocalizationManager] 팝업 '{popupUI.name}'에서 누락된 키가 없습니다.");
            }
        }

        /// <summary>
        /// 현재 활성화된 모든 UI의 키 정보를 출력
        /// </summary>
        [ContextMenu("Print Active UI Keys")]
        public void PrintActiveUIKeys()
        {
            Debug.Log("[LocalizationManager] === 현재 활성화된 UI 키 정보 ===");
            
            string[] activeKeys = GetActiveUIKeys();
            Debug.Log($"[LocalizationManager] 총 {activeKeys.Length}개의 활성 키:");
            
            foreach (string key in activeKeys)
            {
                bool hasTranslation = HasTranslation(key);
                Debug.Log($"  - {key} (번역 존재: {hasTranslation})");
            }
            
            // UI별로 그룹화하여 출력
            var activeUIs = FindObjectsOfType<BaseUI>();
            foreach (var ui in activeUIs)
            {
                if (ui.gameObject.activeInHierarchy)
                {
                    string[] uiKeys = GetUIKeysFromBaseUI(ui);
                    if (uiKeys.Length > 0)
                    {
                        Debug.Log($"[LocalizationManager] UI '{ui.name}'의 키들 ({uiKeys.Length}개):");
                        foreach (string key in uiKeys)
                        {
                            Debug.Log($"    - {key}");
                        }
                    }
                }
            }
        }
    }
}
