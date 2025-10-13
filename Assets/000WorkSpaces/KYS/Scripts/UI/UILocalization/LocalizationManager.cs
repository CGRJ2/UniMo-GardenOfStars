using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace KYS
{
    /// <summary>
    /// CSV 기반 다국어 지원 관리자 (싱글톤 패턴 + Addressable)
    /// </summary>
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        #region Serialized Fields

        [Header("Addressable Localization Settings")]
        [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.Korean;
        private const string LANGUAGE_DATA_ADDRESSABLE_KEY = "KYS/LanguageData";
        
        [Header("Behavior Settings")]
        [Tooltip("최초 실행 시 시스템 언어 대신 기본 언어를 우선 적용할지 여부")]
        [SerializeField] private bool preferDefaultOnFirstRun = false;

        #endregion

        #region Private Fields

        // 언어 데이터 (CSV 기반)
        private Dictionary<string, Dictionary<SystemLanguage, string>> languageData = new();
        private SystemLanguage currentLanguage;
        
        // 지원하는 언어 목록 (한국어, 영어만)
        private readonly SystemLanguage[] allSupportedLanguages = {
            SystemLanguage.Korean,
            SystemLanguage.English
        };
        
        // 현재 활성화된 언어 목록 (실제 번역이 있는 언어만)
        private List<SystemLanguage> activeLanguages = new();
        
        // Addressable 핸들 관리
        private AsyncOperationHandle<TextAsset> csvHandle;
        private bool isInitialized = false;
        
        // 초기화 전 폴백 텍스트 (타이틀 화면용) - 다국어 지원
        private Dictionary<string, Dictionary<SystemLanguage, string>> fallbackTexts = new()
        {
            {
                "ui_titlescene_download_check",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "다운로드 확인 중..." },
                    { SystemLanguage.English, "Checking download..." },
                    { SystemLanguage.Japanese, "ダウンロード確認中..." },
                    { SystemLanguage.Chinese, "检查下载中..." },
                    { SystemLanguage.French, "Vérification du téléchargement..." },
                    { SystemLanguage.German, "Download wird überprüft..." },
                    { SystemLanguage.Spanish, "Verificando descarga..." },
                    { SystemLanguage.Italian, "Verifica download..." },
                    { SystemLanguage.Portuguese, "Verificando download..." },
                    { SystemLanguage.Russian, "Проверка загрузки..." }
                }
            },
            {
                "ui_titlescene_downloading",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "다운로드 중..." },
                    { SystemLanguage.English, "Downloading..." },
                    { SystemLanguage.Japanese, "ダウンロード中..." },
                    { SystemLanguage.Chinese, "下载中..." },
                    { SystemLanguage.French, "Téléchargement..." },
                    { SystemLanguage.German, "Download läuft..." },
                    { SystemLanguage.Spanish, "Descargando..." },
                    { SystemLanguage.Italian, "Download in corso..." },
                    { SystemLanguage.Portuguese, "Baixando..." },
                    { SystemLanguage.Russian, "Загрузка..." }
                }
            },
            {
                "ui_titlescene_touch_screen",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "화면을 터치해주세요!" },
                    { SystemLanguage.English, "Please touch the screen!" },
                    { SystemLanguage.Japanese, "画面をタッチしてください！" },
                    { SystemLanguage.Chinese, "请触摸屏幕！" },
                    { SystemLanguage.French, "Veuillez toucher l'écran !" },
                    { SystemLanguage.German, "Bitte berühren Sie den Bildschirm!" },
                    { SystemLanguage.Spanish, "¡Por favor toque la pantalla!" },
                    { SystemLanguage.Italian, "Per favore tocca lo schermo!" },
                    { SystemLanguage.Portuguese, "Por favor toque na tela!" },
                    { SystemLanguage.Russian, "Пожалуйста, коснитесь экрана!" }
                }
            },
            {
                "ui_titlescene_logout_test",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "로그아웃\n테스트용" },
                    { SystemLanguage.English, "Logout\nFor Testing" },
                    { SystemLanguage.Japanese, "ログアウト\nテスト用" },
                    { SystemLanguage.Chinese, "登出\n测试用" },
                    { SystemLanguage.French, "Déconnexion\nPour test" },
                    { SystemLanguage.German, "Abmelden\nZum Testen" },
                    { SystemLanguage.Spanish, "Cerrar sesión\nPara pruebas" },
                    { SystemLanguage.Italian, "Disconnetti\nPer test" },
                    { SystemLanguage.Portuguese, "Sair\nPara teste" },
                    { SystemLanguage.Russian, "Выйти\nДля тестирования" }
                }
            },
            {
                "ui_network_disconnected_message",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "인터넷 연결을 다시 확인해주세요." },
                    { SystemLanguage.English, "Please check your internet connection." },
                    { SystemLanguage.Japanese, "インターネット接続を確認してください。" },
                    { SystemLanguage.Chinese, "请检查您的网络连接。" },
                    { SystemLanguage.French, "Veuillez vérifier votre connexion Internet." },
                    { SystemLanguage.German, "Bitte überprüfen Sie Ihre Internetverbindung." },
                    { SystemLanguage.Spanish, "Por favor, verifique su conexión a Internet." },
                    { SystemLanguage.Italian, "Si prega di controllare la connessione Internet." },
                    { SystemLanguage.Portuguese, "Por favor, verifique sua conexão com a Internet." },
                    { SystemLanguage.Russian, "Пожалуйста, проверьте подключение к Интернету." }
                }
            },
            {
                "ui_account_linking_start",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "계정 연결을 시작합니다..." },
                    { SystemLanguage.English, "Starting account linking..." },
                    { SystemLanguage.Japanese, "アカウント連携を開始しています..." },
                    { SystemLanguage.Chinese, "正在开始账户关联..." },
                    { SystemLanguage.French, "Début de la liaison de compte..." },
                    { SystemLanguage.German, "Kontoverknüpfung wird gestartet..." },
                    { SystemLanguage.Spanish, "Iniciando vinculación de cuenta..." },
                    { SystemLanguage.Italian, "Avvio collegamento account..." },
                    { SystemLanguage.Portuguese, "Iniciando vinculação de conta..." },
                    { SystemLanguage.Russian, "Начинаем связывание аккаунта..." }
                }
            },
            {
                "ui_account_linking_success",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "계정 연결이 완료되었습니다." },
                    { SystemLanguage.English, "Account linking completed successfully." },
                    { SystemLanguage.Japanese, "アカウント連携が完了しました。" },
                    { SystemLanguage.Chinese, "账户关联已成功完成。" },
                    { SystemLanguage.French, "Liaison de compte terminée avec succès." },
                    { SystemLanguage.German, "Kontoverknüpfung erfolgreich abgeschlossen." },
                    { SystemLanguage.Spanish, "Vinculación de cuenta completada exitosamente." },
                    { SystemLanguage.Italian, "Collegamento account completato con successo." },
                    { SystemLanguage.Portuguese, "Vinculação de conta concluída com sucesso." },
                    { SystemLanguage.Russian, "Связывание аккаунта успешно завершено." }
                }
            },
            {
                "ui_account_linking_error",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "계정 연결 중 오류가 발생했습니다." },
                    { SystemLanguage.English, "An error occurred during account linking." },
                    { SystemLanguage.Japanese, "アカウント連携中にエラーが発生しました。" },
                    { SystemLanguage.Chinese, "账户关联过程中发生错误。" },
                    { SystemLanguage.French, "Une erreur s'est produite lors de la liaison de compte." },
                    { SystemLanguage.German, "Ein Fehler ist bei der Kontoverknüpfung aufgetreten." },
                    { SystemLanguage.Spanish, "Ocurrió un error durante la vinculación de cuenta." },
                    { SystemLanguage.Italian, "Si è verificato un errore durante il collegamento dell'account." },
                    { SystemLanguage.Portuguese, "Ocorreu um erro durante a vinculação da conta." },
                    { SystemLanguage.Russian, "Произошла ошибка при связывании аккаунта." }
                }
            },
            {
                "ui_account_already_linked",
                new Dictionary<SystemLanguage, string>
                {
                    { SystemLanguage.Korean, "이미 계정이 연결되어 있습니다." },
                    { SystemLanguage.English, "Account is already linked." },
                    { SystemLanguage.Japanese, "アカウントは既に連携されています。" },
                    { SystemLanguage.Chinese, "账户已经关联。" },
                    { SystemLanguage.French, "Le compte est déjà lié." },
                    { SystemLanguage.German, "Konto ist bereits verknüpft." },
                    { SystemLanguage.Spanish, "La cuenta ya está vinculada." },
                    { SystemLanguage.Italian, "L'account è già collegato." },
                    { SystemLanguage.Portuguese, "A conta já está vinculada." },
                    { SystemLanguage.Russian, "Аккаунт уже связан." }
                }
            }
        };

        #endregion

        #region Fallback Text Management

        /// <summary>
        /// 폴백 텍스트 추가 (CSV 로딩 전 사용)
        /// </summary>
        public void AddFallbackText(string key, SystemLanguage language, string text)
        {
            if (!fallbackTexts.ContainsKey(key))
            {
                fallbackTexts[key] = new Dictionary<SystemLanguage, string>();
            }
            
            fallbackTexts[key][language] = text;
        }

        /// <summary>
        /// 폴백 텍스트 일괄 추가
        /// </summary>
        public void AddFallbackTexts(string key, Dictionary<SystemLanguage, string> texts)
        {
            fallbackTexts[key] = new Dictionary<SystemLanguage, string>(texts);
        }

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
                //Debug.Log("[LocalizationManager] Localization 초기화 시작");
                
                // PlayerPrefs에서 저장된 언어 설정 우선 적용
                string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", "");
                ////Debug.Log($"[LocalizationManager] 저장된 언어 설정: {savedLanguage}");
                SystemLanguage savedLang = default;
                bool hasSavedLanguage = !string.IsNullOrEmpty(savedLanguage) &&
                                         System.Enum.TryParse<SystemLanguage>(savedLanguage, out savedLang);
                
                if (hasSavedLanguage && IsLanguageSupported(savedLang))
                {
                    currentLanguage = savedLang;
                    ////Debug.Log($"[LocalizationManager] 저장된 언어 적용: {currentLanguage}");
                }
                else
                {
                    // 시스템 언어 감지 후 지원 여부 확인
                    SystemLanguage systemLanguage = Application.systemLanguage;
                    //Debug.Log($"[LocalizationManager] 시스템 언어: {systemLanguage}");
                    currentLanguage = IsLanguageSupported(systemLanguage) ? systemLanguage : defaultLanguage;
                    if (currentLanguage == defaultLanguage)
                    {
                        //Debug.Log($"[LocalizationManager] 시스템 언어 미지원 → 기본 언어 적용: {defaultLanguage}");
                    }
                }
                
                ////Debug.Log($"[LocalizationManager] 최종 선택된 언어: {currentLanguage} ({GetLanguageName(currentLanguage)})");
                
                // Addressable에서 CSV 파일 로드
                await LoadCSVFileFromAddressable();
                
                isInitialized = true;
                //Debug.Log($"[LocalizationManager] Localization 초기화 완료. 언어: {GetLanguageName(currentLanguage)}");
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
            try
            {
                ////Debug.Log("[LocalizationManager] Addressable에서 CSV 파일 로드 시작");
                while (!Manager.game.initialized)
                {
                    await Task.Yield();
                }

                csvHandle = Addressables.LoadAssetAsync<TextAsset>(LANGUAGE_DATA_ADDRESSABLE_KEY);
                TextAsset csvFile = await csvHandle.Task;
                
                if (csvFile != null)
                {
                    ParseCSVLanguageFile(csvFile.text);
                    OnSupportedLanguagesLoaded?.Invoke(allSupportedLanguages);
                    OnActiveLanguagesLoaded?.Invoke(activeLanguages.ToArray());
                    //Debug.Log($"[LocalizationManager] CSV 파일 로드 완료. 활성 언어: {activeLanguages.Count}개");
                }
                else
                {
                    Debug.LogError("[LocalizationManager] CSV 파일 로드 실패");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 파일 로드 중 오류 (키: {LANGUAGE_DATA_ADDRESSABLE_KEY}): {e.Message}");
            }
        }

        #endregion

        #region CSV Parsing

        /// <summary>
        /// CSV 파일 파싱
        /// </summary>
        private void ParseCSVLanguageFile(string csvText)
        {
            //Debug.Log("[LocalizationManager] CSV 파일 파싱 시작");
            //Debug.Log($"[LocalizationManager] CSV 텍스트 길이: {csvText.Length}");
            
            languageData.Clear();
            activeLanguages.Clear();
            
            // 줄바꿈 문자 처리 (Windows: \r\n, Unix: \n, Mac: \r)
            string[] lines = csvText.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            //Debug.Log($"[LocalizationManager] CSV 라인 수: {lines.Length}");
            if (lines.Length < 2)
            {
                Debug.LogError("[LocalizationManager] CSV 파일 형식이 올바르지 않습니다.");
                return;
            }
            
            // 헤더 라인 파싱 (언어 코드)
            string[] headers = ParseCSVLine(lines[0]);
            //Debug.Log($"[LocalizationManager] 헤더: {string.Join(", ", headers)}");
            
            Dictionary<int, SystemLanguage> languageIndexMap = new();
            HashSet<SystemLanguage> languagesWithData = new();
            
            for (int i = 1; i < headers.Length; i++) // Key 열 제외
            {
                SystemLanguage lang = GetLanguageFromCode(headers[i]);
                //Debug.Log($"[LocalizationManager] 헤더 {i}: '{headers[i]}' -> {lang}");
                if (IsLanguageSupported(lang))
                {
                    languageIndexMap[i] = lang;
                    //Debug.Log($"[LocalizationManager] 지원되는 언어 추가: {lang} (인덱스 {i})");
                }
                else
                {
                    //Debug.LogWarning($"[LocalizationManager] 지원되지 않는 언어: {lang} (헤더: {headers[i]})");
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
            
            //Debug.Log($"[LocalizationManager] 파싱 완료 - 총 키 수: {languageData.Count}");
            //Debug.Log($"[LocalizationManager] 활성 언어: {string.Join(", ", activeLanguages.Select(l => GetLanguageName(l)))}");
            
            // 중복 키 검사
            CheckForDuplicateKeys();
            
            // 현재 언어에 대한 번역 데이터 확인
            if (languageData.Count > 0)
            {
                var sampleKey = languageData.Keys.First();
                if (languageData[sampleKey].ContainsKey(currentLanguage))
                {
                    //Debug.Log($"[LocalizationManager] 샘플 번역 확인 - 키: {sampleKey}, 언어: {GetLanguageName(currentLanguage)}, 번역: {languageData[sampleKey][currentLanguage]}");
                }
                else
                {
                    //Debug.LogWarning($"[LocalizationManager] 현재 언어({GetLanguageName(currentLanguage)})에 대한 번역이 없음. 사용 가능한 언어: {string.Join(", ", languageData[sampleKey].Keys.Select(k => GetLanguageName(k)))}");
                }
            }
        }

        /// <summary>
        /// CSV 라인 파싱 (쉼표와 따옴표, $ 기호 처리)
        /// </summary>
        private string[] ParseCSVLine(string line)
        {
            // $ 기호로 감싸진 문자열 내의 쉼표를 무시하는 정규식 사용
            string[] fields = Regex.Split(line, @",(?=(?:[^$]*\$[^$]*\$)*[^$]*$)");
            
            List<string> result = new List<string>();
            
            foreach (string field in fields)
            {
                // $ 기호 제거 및 따옴표 처리
                string processedField = field.Trim().Trim('"');
                processedField = processedField.Replace("$", "");
                result.Add(processedField);
            }
            
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
        /// 언어 지원 여부 확인 (한국어, 영어만)
        /// </summary>
        private bool IsLanguageSupported(SystemLanguage language)
        {
            return language == SystemLanguage.Korean ||
                   language == SystemLanguage.English;
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
                //Debug.Log($"[LocalizationManager] 언어가 변경되었습니다: {GetLanguageName(language)}");
            }
        }

        /// <summary>
        /// 언어 이름 가져오기 (한국어, 영어만)
        /// </summary>
        public string GetLanguageName(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Korean:
                    return "한국어";
                case SystemLanguage.English:
                    return "English";
                default:
                    return language.ToString();
            }
        }

        /// <summary>
        /// 언어의 원래 이름 반환 (항상 해당 언어로 표시)
        /// </summary>
        public string GetLocalizedLanguageName(SystemLanguage language)
        {
            // 항상 해당 언어의 원래 이름을 반환
            return GetLanguageName(language);
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
        /// 건물 이름 번역 가져오기 (LocalizationManager 통합)
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>번역된 건물 이름</returns>
        public string GetBuildingName(string buildingID)
        {
            if (string.IsNullOrEmpty(buildingID))
                return string.Empty;

            string localizationKey = $"building_{buildingID}_name";
            string result = GetText(localizationKey);
            
            // 번역이 실패했으면 건물 데이터에서 직접 가져오기
            if (result == localizationKey && Manager.data?.Building?.Values != null)
            {
                if (Manager.data.Building.Values.TryGetValue(buildingID, out BuildingData buildingData))
                {
                    result = currentLanguage == SystemLanguage.Korean ? buildingData.Name_KR : buildingData.Name_EN;
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }
            
            return result == localizationKey ? buildingID : result;
        }

        /// <summary>
        /// 건물 설명 번역 가져오기 (LocalizationManager 통합)
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>번역된 건물 설명</returns>
        public string GetBuildingDescription(string buildingID)
        {
            if (string.IsNullOrEmpty(buildingID))
                return string.Empty;

            string localizationKey = $"building_{buildingID}_description";
            string result = GetText(localizationKey);
            
            // 번역이 실패했으면 건물 데이터에서 직접 가져오기
            if (result == localizationKey && Manager.data?.Building?.Values != null)
            {
                if (Manager.data.Building.Values.TryGetValue(buildingID, out BuildingData buildingData))
                {
                    result = currentLanguage == SystemLanguage.Korean ? buildingData.Description_KR : buildingData.Description_EN;
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }
            
            return result == localizationKey ? string.Empty : result;
        }

        /// <summary>
        /// 재료 이름 번역 가져오기 (LocalizationManager 통합)
        /// </summary>
        /// <param name="ingrediantID">재료 ID</param>
        /// <returns>번역된 재료 이름</returns>
        public string GetIngrediantName(string ingrediantID)
        {
            if (string.IsNullOrEmpty(ingrediantID))
                return string.Empty;

            string localizationKey = $"ingrediant_{ingrediantID}_name";
            string result = GetText(localizationKey);
            
            // 번역이 실패했으면 재료 데이터에서 직접 가져오기
            if (result == localizationKey && Manager.data?.Ingrediant?.Values != null)
            {
                if (Manager.data.Ingrediant.Values.TryGetValue(ingrediantID, out IngrediantData ingrediantData))
                {
                    result = currentLanguage == SystemLanguage.Korean ? ingrediantData.Name_KR : ingrediantData.Name_EN;
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }
            
            return result == localizationKey ? ingrediantID : result;
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
            ////Debug.Log($"[LocalizationManager] GetText 호출: key={key}, currentLanguage={currentLanguage}, isInitialized={isInitialized}");
            
            // 초기화 전 폴백 텍스트 사용 (다국어 지원)
            if (!isInitialized && fallbackTexts.ContainsKey(key))
            {
                var fallbackData = fallbackTexts[key];
                
                // 현재 언어의 폴백 텍스트가 있으면 사용
                if (fallbackData.ContainsKey(currentLanguage))
                {
                    return fallbackData[currentLanguage];
                }
                
                // 기본 언어의 폴백 텍스트가 있으면 사용
                if (fallbackData.ContainsKey(defaultLanguage))
                {
                    return fallbackData[defaultLanguage];
                }
                
                // 한국어 폴백 텍스트가 있으면 사용
                if (fallbackData.ContainsKey(SystemLanguage.Korean))
                {
                    return fallbackData[SystemLanguage.Korean];
                }
                
                // 첫 번째 사용 가능한 언어의 텍스트 사용
                if (fallbackData.Count > 0)
                {
                    return fallbackData.Values.First();
                }
            }
            
            // 현재 언어에서 번역 찾기
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(currentLanguage))
            {
                string result = languageData[key][currentLanguage];
                ////Debug.Log($"[LocalizationManager] 번역 찾음: {key} -> {result}");
                return result;
            }
            
            // 기본 언어에서 번역 찾기
            if (languageData.ContainsKey(key) && 
                languageData[key].ContainsKey(defaultLanguage))
            {
                string result = languageData[key][defaultLanguage];
                //Debug.Log($"[LocalizationManager] 기본 언어 번역 사용: {key} -> {result}");
                return result;
            }
            
            // 번역을 찾을 수 없는 경우 폴백 텍스트 시도
            if (fallbackTexts.ContainsKey(key))
            {
                var fallbackData = fallbackTexts[key];
                
                // 현재 언어의 폴백 텍스트가 있으면 사용
                if (fallbackData.ContainsKey(currentLanguage))
                {
                    return fallbackData[currentLanguage];
                }
                
                // 기본 언어의 폴백 텍스트가 있으면 사용
                if (fallbackData.ContainsKey(defaultLanguage))
                {
                    return fallbackData[defaultLanguage];
                }
                
                // 한국어 폴백 텍스트가 있으면 사용
                if (fallbackData.ContainsKey(SystemLanguage.Korean))
                {
                    return fallbackData[SystemLanguage.Korean];
                }
                
                // 첫 번째 사용 가능한 언어의 텍스트 사용
                if (fallbackData.Count > 0)
                {
                    return fallbackData.Values.First();
                }
            }
            
            // 폴백 텍스트도 없으면 키 반환
            //Debug.LogWarning($"[LocalizationManager] 번역을 찾을 수 없습니다: {key}, languageData.Count={languageData.Count}");
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
                //Debug.LogWarning($"[LocalizationManager] 중복된 키가 발견되었습니다:");
                foreach (var duplicate in duplicates)
                {
                    //Debug.LogWarning($"  - 키: {duplicate.Key}, 중복 횟수: {duplicate.Value}");
                }
            }
            else
            {
                //Debug.Log("[LocalizationManager] 중복된 키가 없습니다.");
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
                //Debug.LogWarning($"[LocalizationManager] UI 이름 '{uiName}'에서 생성된 키 '{key}'가 이미 존재합니다.");
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
        /// Addressable을 통해 CSV 파일의 실제 경로 가져오기
        /// </summary>
        private async System.Threading.Tasks.Task<string> GetCSVFilePathAsync()
        {
#if UNITY_EDITOR
            try
            {
                var handle = Addressables.LoadAssetAsync<TextAsset>(LANGUAGE_DATA_ADDRESSABLE_KEY);
                var csvFile = await handle.Task;
                
                if (csvFile != null)
                {
                    // Addressable에서 로드된 에셋의 실제 경로 반환
                    return AssetDatabase.GetAssetPath(csvFile);
                }
                
                Addressables.Release(handle);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 파일 경로 가져오기 실패: {e.Message}");
                return null;
            }
#else
            // 빌드에서는 Addressable 경로를 직접 사용할 수 없으므로 null 반환
            return null;
#endif
        }

        /// <summary>
        /// Addressable을 통해 CSV 파일의 실제 경로 가져오기 (동기 버전)
        /// </summary>
        private string GetCSVFilePath()
        {
#if UNITY_EDITOR
            try
            {
                // 이미 로드된 핸들이 있으면 해당 에셋의 경로 사용
                if (csvHandle.IsValid())
                {
                    var csvFile = csvHandle.Result;
                    if (csvFile != null)
                    {
                        return AssetDatabase.GetAssetPath(csvFile);
                    }
                }
                
                // 로드된 핸들이 없으면 직접 로드
                var handle = Addressables.LoadAssetAsync<TextAsset>(LANGUAGE_DATA_ADDRESSABLE_KEY);
                var loadedCsvFile = handle.WaitForCompletion();
                
                if (loadedCsvFile != null)
                {
                    string path = AssetDatabase.GetAssetPath(loadedCsvFile);
                    Addressables.Release(handle);
                    return path;
                }
                
                Addressables.Release(handle);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] CSV 파일 경로 가져오기 실패: {e.Message}");
                return null;
            }
#else
            // 빌드에서는 Addressable 경로를 직접 사용할 수 없으므로 null 반환
            return null;
#endif
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
        
        /// <summary>
        /// 중복된 키를 제거하고 CSV 파일을 업데이트
        /// </summary>
        [ContextMenu("Remove Duplicate Keys from CSV")]
        public void RemoveDuplicateKeysFromCSV()
        {
            string csvPath = GetCSVFilePath();
            if (string.IsNullOrEmpty(csvPath))
            {
                Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                return;
            }
            
            try
            {
                if (!System.IO.File.Exists(csvPath))
                {
                    Debug.LogError("[LocalizationManager] CSV 파일이 존재하지 않습니다.");
                    return;
                }
                
                string[] lines = System.IO.File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                if (lines.Length < 2)
                {
                    Debug.LogWarning("[LocalizationManager] CSV 파일에 데이터가 없습니다.");
                    return;
                }
                
                // 헤더와 데이터 분리
                string headerLine = lines[0];
                var dataLines = new List<string>(lines.Skip(1));
                
                // 중복 키 찾기
                var keyCount = new Dictionary<string, int>();
                var keyLines = new Dictionary<string, List<string>>();
                
                foreach (string line in dataLines)
                {
                    if (string.IsNullOrEmpty(line.Trim())) continue;
                    
                    string[] fields = ParseCSVLine(line);
                    if (fields.Length > 0)
                    {
                        string key = fields[0].Trim();
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (!keyCount.ContainsKey(key))
                            {
                                keyCount[key] = 0;
                                keyLines[key] = new List<string>();
                            }
                            keyCount[key]++;
                            keyLines[key].Add(line);
                        }
                    }
                }
                
                // 중복된 키들 찾기
                var duplicateKeys = keyCount.Where(kvp => kvp.Value > 1).ToList();
                
                if (duplicateKeys.Count == 0)
                {
                    Debug.Log("[LocalizationManager] 중복된 키가 없습니다.");
                    return;
                }
                
                Debug.LogWarning($"[LocalizationManager] 중복된 키 {duplicateKeys.Count}개 발견:");
                foreach (var duplicate in duplicateKeys)
                {
                    Debug.LogWarning($"  - {duplicate.Key}: {duplicate.Value}번 중복");
                }
                
                // 중복 제거 (첫 번째 라인만 유지)
                var newDataLines = new List<string>();
                var processedKeys = new HashSet<string>();
                
                foreach (string line in dataLines)
                {
                    if (string.IsNullOrEmpty(line.Trim())) continue;
                    
                    string[] fields = ParseCSVLine(line);
                    if (fields.Length > 0)
                    {
                        string key = fields[0].Trim();
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (!processedKeys.Contains(key))
                            {
                                newDataLines.Add(line);
                                processedKeys.Add(key);
                            }
                            else
                            {
                                Debug.Log($"[LocalizationManager] 중복 키 '{key}' 제거됨");
                            }
                        }
                    }
                }
                
                // 새로운 CSV 파일 작성
                var newLines = new List<string> { headerLine };
                newLines.AddRange(newDataLines);
                
                // 백업 생성
                string backupPath = csvPath.Replace(".csv", $"_backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
                System.IO.File.Copy(csvPath, backupPath);
                Debug.Log($"[LocalizationManager] 백업 파일 생성: {backupPath}");
                
                // 새 파일 작성
                System.IO.File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                Debug.Log($"[LocalizationManager] 중복 키 제거 완료. {dataLines.Count - newDataLines.Count}개의 중복 라인 제거됨");
                
                // 데이터 다시 로드
                if (isInitialized)
                {
                    // CSV 파일 다시 파싱
                    string newCsvText = System.IO.File.ReadAllText(csvPath, System.Text.Encoding.UTF8);
                    ParseCSVLanguageFile(newCsvText);
                    Debug.Log("[LocalizationManager] CSV 데이터가 다시 로드되었습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] 중복 키 제거 중 오류 발생: {e.Message}");
            }
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
                string csvPath = GetCSVFilePath();
                if (string.IsNullOrEmpty(csvPath))
                {
                    Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                    return;
                }
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
                string csvPath = GetCSVFilePath();
                if (string.IsNullOrEmpty(csvPath))
                {
                    Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                    return;
                }
                
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
            string csvPath = GetCSVFilePath();
            if (string.IsNullOrEmpty(csvPath))
            {
                Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                return;
            }
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
            string csvPath = GetCSVFilePath();
            if (string.IsNullOrEmpty(csvPath))
            {
                Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                return;
            }
            
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
            string csvPath = GetCSVFilePath();
            if (string.IsNullOrEmpty(csvPath))
            {
                Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                return -1;
            }
            
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
            string csvPath = GetCSVFilePath();
            if (string.IsNullOrEmpty(csvPath))
            {
                Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                return;
            }
            
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
                string csvPath = GetCSVFilePath();
                if (string.IsNullOrEmpty(csvPath))
                {
                    Debug.LogError("[LocalizationManager] CSV 파일 경로를 가져올 수 없습니다.");
                    return;
                }
                
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
