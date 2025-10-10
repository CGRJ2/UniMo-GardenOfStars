using KYS;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SettingPopUp : BaseUI
{

    [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
    [SerializeField] private string BGMText = "BGMText";
    [SerializeField] private string SFXText = "SFXText";
    [SerializeField] private string VibrationText = "VibrationText";
    [SerializeField] private string closeButtonName = "CloseButton";
    [SerializeField] private string languageDropdownName = "LanguageDropdown";
    [SerializeField] private string BGMSliderName = "BGMSlider";
    [SerializeField] private string SFXSliderName = "SFXSlider";
    [SerializeField] private string VibrationToggleName = "VibrationToggle";
    [SerializeField] private string logoutButtonName = "LogoutButton";
    [SerializeField] private string logoutButtonText = "LogoutButtonText";
    [SerializeField] private string accountButtonName = "AccountButton";
    [SerializeField] private string supportButtonName = "SupportButton";
    [SerializeField] private string emergencyEscapeButtonName = "EmergencyEscapeButton";
    private SystemLanguage selectedLanguage;
    private Dictionary<SystemLanguage, float> languageCompleteness = new Dictionary<SystemLanguage, float>();

    // 설정값 저장용 키
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string VIBRATION_ENABLED_KEY = "VibrationEnabled";

    // 기본값
    private const float DEFAULT_BGM_VOLUME = 0.8f;
    private const float DEFAULT_SFX_VOLUME = 0.8f;
    private const bool DEFAULT_VIBRATION_ENABLED = true;

    private new void Awake()
    {
        base.Awake();
        // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.Popup;
        }



    }

    private TextMeshProUGUI BGU => GetUI<TextMeshProUGUI>(BGMText);
    private TextMeshProUGUI SFXT => GetUI<TextMeshProUGUI>(SFXText);
    private TextMeshProUGUI Vibration => GetUI<TextMeshProUGUI>(VibrationText);
    private TMP_Dropdown languageDropdown => GetUI<TMP_Dropdown>(languageDropdownName);
    private Slider BGMSlider => GetUI<Slider>(BGMSliderName);
    private Slider SFXSlider => GetUI<Slider>(SFXSliderName);
    private Toggle VibrationToggle => GetUI<Toggle>(VibrationToggleName);
    private Button LogoutButton => GetUI<Button>(logoutButtonName);
    private TextMeshProUGUI LogoutButtonT => GetUI<TextMeshProUGUI>(logoutButtonText);
    private Button AccountButton => GetUI<Button>(accountButtonName);
    private Button SupportButton => GetUI<Button>(supportButtonName);
    private Button EmergencyEscapeButton => GetUI<Button>(emergencyEscapeButtonName);

    public override string[] GetAutoLocalizeKeys()
    {
        return new string[]
        {
            "setting_bgm",
            "setting_sfx",
            "setting_vibration"
        };
    }


    public override void Initialize()
    {
        base.Initialize();
        SetupAutoLocalization();
        SetupLanguageDropdown(); // 언어 드롭다운을 먼저 설정
        SetupButtons();
        LoadSettings(); // 설정값을 먼저 로드
        SetupVolumeControls(); // 볼륨 컨트롤 설정 (로드 후)
        SetupVibrationToggle(); // 진동 토글 설정 (로드 후)
    }


    private void SetupButtons()
    {
        Debug.Log($"[SettingPopUp] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

        // 이미 설정되었으면 중복 호출 방지
        if (isButtonsSetup)
        {
            Debug.Log($"[SettingPopUp] SetupButtons 이미 완료됨 - 중복 호출 방지");
            return;
        }

        var confirmEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
        if (confirmEventHandler != null)
        {
            confirmEventHandler.Click += OnCloseButton;
            isButtonsSetup = true; // 설정 완료 플래그
        }
        else
        {
            Debug.LogError($"[SettingPopUp] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
        }

        var logoutEventHandler = GetEventWithSFX(logoutButtonName, "SFX_ButtonClick");
        if (LogoutButton != null)
        {
            logoutEventHandler.Click += OnLogoutButton;

        }
        else
        {
            Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
        }
        if (!Manager.firebase.Auth.CurrentUser.IsAnonymous)
        {
            LogoutButton.gameObject.SetActive(false);
        }

        var accountEventHandler = GetEventWithSFX(accountButtonName, "SFX_ButtonClick");
        if (LogoutButton != null)
        {
            accountEventHandler.Click += OnAccountButton;

        }
        else
        {
            Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
        }
        if (!Manager.firebase.Auth.CurrentUser.IsAnonymous)
        {
            AccountButton.interactable = false;
        }

        var supportEventHandler = GetEventWithSFX(supportButtonName, "SFX_ButtonClick");
        if (SupportButton != null)
        {
            supportEventHandler.Click += OnSupportButton;
        }
        else
        {
            Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
        }
        
        var emergencyEscapeEventHandler = GetEventWithSFX(emergencyEscapeButtonName, "SFX_ButtonClick");
        if(EmergencyEscapeButton != null)
        {
            emergencyEscapeEventHandler.Click += OnEmergencyEscapeButton;
        }
        else
        {
            Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
        }

    }

    private void OnCloseButton(PointerEventData data)
    {
        Manager.ui.ClosePopup();
    }

    private void OnLogoutButton(PointerEventData data)
    {
        Manager.ui.ShowPopUpAsync<LogoutPopup>((popup) =>
        {

        });
    }

    private void OnAccountButton(PointerEventData data)
    {
        if (!Manager.firebase.Auth.CurrentUser.IsAnonymous) return;

        // Google Play Games 계정 연결 실행
        ExecuteGooglePlayGamesLink();
    }

    /// <summary>
    /// Google Play Games 계정 연결 실행
    /// </summary>
    private void ExecuteGooglePlayGamesLink()
    {
        try
        {
            // LinkPlayGamesController 찾기 또는 생성
            LinkPlayGamesController linkController = FindObjectOfType<LinkPlayGamesController>();
            if (linkController == null)
            {
                // LinkPlayGamesController가 없다면 새로 생성
                GameObject linkControllerObj = new GameObject("LinkPlayGamesController");
                linkController = linkControllerObj.AddComponent<LinkPlayGamesController>();
            }

            // 이미 연결되어 있는지 확인
            if (linkController.IsLinked)
            {
                Debug.Log("[SettingPopUp] 이미 Google Play Games 계정이 연결되어 있습니다.");
                ShowMessagePopUpWithKey("account_already_linked");
                return;
            }

            // 계정 연결 실행
            linkController.LinkPlayGames();
            
            Debug.Log("[SettingPopUp] Google Play Games 계정 연결을 시작합니다.");
            ShowMessagePopUpWithKey("account_linking_start");
            
            // 연결 상태 모니터링 시작
            StartCoroutine(MonitorLinkingStatus(linkController));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SettingPopUp] Google Play Games 계정 연결 중 오류: {e.Message}");
            ShowMessagePopUpWithKey("account_linking_error");
        }
    }

    /// <summary>
    /// 계정 연결 상태 모니터링
    /// </summary>
    private System.Collections.IEnumerator MonitorLinkingStatus(LinkPlayGamesController linkController)
    {
        float timeout = 30f; // 30초 타임아웃
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            if (linkController.IsLinked)
            {
                ShowMessagePopUpWithKey("account_linking_success");
                yield break;
            }
            
            elapsed += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
        
        // 타임아웃 시 오류 메시지 표시
        ShowMessagePopUpWithKey("account_linking_error");
    }

    /// <summary>
    /// 메시지 팝업 표시 (간단한 알림용)
    /// </summary>
    private void ShowMessagePopUp(string message)
    {
        Manager.ui.ShowMessagePopUpAsync(message);
    }

    /// <summary>
    /// 번역 키를 사용한 메시지 팝업 표시
    /// </summary>
    private void ShowMessagePopUpWithKey(string localizationKey, string fallbackMessage = null)
    {
        Manager.ui.ShowMessagePopUpWithKeyAsync(localizationKey, null);
    }


 

    private void OnSupportButton(PointerEventData data)
    {
        //Application.OpenURL("https://www.");

    }


    private void OnEmergencyEscapeButton(PointerEventData data)
    {
        // 긴급 탈출 - 플레이어를 최초 위치로 이동
        ResetPlayerToInitialPosition();
        Manager.ui.ClosePopup();
    }

    /// <summary>
    /// 플레이어를 최초 위치로 리셋
    /// </summary>
    private void ResetPlayerToInitialPosition()
    {
        try
        {
            // 플레이어 오브젝트 찾기
            GameObject playerObj = Manager.player?.PlayerObj;
            if (playerObj == null)
            {
                Debug.LogError("[SettingPopUp] 플레이어 오브젝트를 찾을 수 없습니다.");
                return;
            }

            // 최초 위치로 이동 (0, 0, 0 또는 원하는 위치)
            Vector3 initialPosition = new Vector3(0, 0, 0);
            playerObj.transform.position = initialPosition;

            // 속도 초기화
            var rigidbody = playerObj.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            Debug.Log($"[SettingPopUp] 플레이어를 {initialPosition}으로 이동 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SettingPopUp] 긴급 탈출 실행 중 오류: {e.Message}");
        }
    }


    /// <summary>
    /// 언어 드롭다운 설정
    /// </summary>
    private void SetupLanguageDropdown()
    {
        if (languageDropdown == null || LocalizationManager.Instance == null)
            return;

        //Debug.Log("[SettingPopUp] 언어 드롭다운 설정 시작");

        // 활성 언어 목록 가져오기
        SystemLanguage[] activeLanguages = LocalizationManager.Instance.ActiveLanguages;
        //Debug.Log($"[SettingPopUp] 활성 언어 수: {activeLanguages.Length}");

        // 각 언어의 번역 완성도 계산
        languageCompleteness.Clear();
        foreach (var lang in activeLanguages)
        {
            float completeness = LocalizationManager.Instance.GetTranslationCompleteness(lang);
            languageCompleteness[lang] = completeness;
            //Debug.Log($"[SettingPopUp] {LocalizationManager.Instance.GetLocalizedLanguageName(lang)}: {completeness * 100:F1}%");
        }

        // 드롭다운 옵션 설정
        languageDropdown.ClearOptions();
        for (int i = 0; i < activeLanguages.Length; i++)
        {
            string languageName = LocalizationManager.Instance.GetLocalizedLanguageName(activeLanguages[i]);
            float completeness = languageCompleteness[activeLanguages[i]];
            string optionText = $"{languageName}";

            languageDropdown.options.Add(new TMP_Dropdown.OptionData(optionText));
        }

        // 현재 언어 선택 (activeLanguages 배열에서 인덱스 찾기)
        int currentIndex = 0;
        for (int i = 0; i < activeLanguages.Length; i++)
        {
            if (activeLanguages[i] == LocalizationManager.Instance.CurrentLanguage)
            {
                currentIndex = i;
                break;
            }
        }
        languageDropdown.value = currentIndex;
        selectedLanguage = LocalizationManager.Instance.CurrentLanguage;

        // 이벤트 구독
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

        //Debug.Log($"[SettingPopUp] 현재 언어: {LocalizationManager.Instance.GetLocalizedLanguageName(selectedLanguage)} (인덱스: {currentIndex})");
    }

    /// <summary>
    /// 언어 드롭다운 변경 이벤트 - 즉시 적용
    /// </summary>
    private void OnLanguageDropdownChanged(int index)
    {
        if (LocalizationManager.Instance == null) return;

        // activeLanguages 배열에서 선택된 언어 가져오기
        SystemLanguage[] activeLanguages = LocalizationManager.Instance.ActiveLanguages;
        if (index >= 0 && index < activeLanguages.Length)
        {
            selectedLanguage = activeLanguages[index];

            // 즉시 언어 변경 적용
            LocalizationManager.Instance.SetLanguage(selectedLanguage);

            // 성공 사운드 재생
            PlaySuccessSound();

            Debug.Log($"[SettingPopUp] 언어 변경 완료: {LocalizationManager.Instance.GetLanguageName(selectedLanguage)}");
        }
        else
        {
            Debug.LogWarning($"[SettingPopUp] 잘못된 드롭다운 인덱스: {index}");
        }
    }

    /// <summary>
    /// 적용 버튼 클릭 (필요시 사용)
    /// </summary>
    private void OnApplyClicked()
    {
        if (LocalizationManager.Instance == null) return;

        Debug.Log($"[SettingPopUp] 언어 적용: {LocalizationManager.Instance.GetLanguageName(selectedLanguage)}");

        // 언어 변경
        LocalizationManager.Instance.SetLanguage(selectedLanguage);

        // 성공 사운드 재생
        PlaySuccessSound();

        // 패널 닫기
        Manager.ui.ClosePopup();
    }

    /// <summary>
    /// 볼륨 컨트롤 설정
    /// </summary>
    private void SetupVolumeControls()
    {
        // BGM 볼륨 슬라이더 설정
        if (BGMSlider != null)
        {
            BGMSlider.onValueChanged.RemoveAllListeners();
            BGMSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        else
        {
            Debug.LogWarning("[SettingPopUp] BGM Slider를 찾을 수 없습니다.");
        }

        // SFX 볼륨 슬라이더 설정
        if (SFXSlider != null)
        {
            SFXSlider.onValueChanged.RemoveAllListeners();
            SFXSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            // SFX 슬라이더에 PointerHandler 추가하여 손가락을 띄면 효과음 재생
            var pointerHandler = SFXSlider.GetComponent<PointerHandler>();
            if (pointerHandler == null)
            {
                pointerHandler = SFXSlider.gameObject.AddComponent<PointerHandler>();
            }
            pointerHandler.Up += OnSFXSliderPointerUp;
        }
        else
        {
            Debug.LogWarning("[SettingPopUp] SFX Slider를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 진동 토글 설정
    /// </summary>
    private void SetupVibrationToggle()
    {
        if (VibrationToggle != null)
        {
            VibrationToggle.onValueChanged.RemoveAllListeners();
            VibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
        }
        else
        {
            Debug.LogWarning("[SettingPopUp] Vibration Toggle을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// BGM 볼륨 변경 이벤트
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.BgmVolume = value;
            SaveSettings();
            Debug.Log($"[SettingPopUp] BGM 볼륨 변경: {value:F2}");
        }
    }

    /// <summary>
    /// SFX 볼륨 변경 이벤트
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SfxVolume = value;
            SaveSettings();
            Debug.Log($"[SettingPopUp] SFX 볼륨 변경: {value:F2}");
        }
    }

    /// <summary>
    /// SFX 슬라이더에서 손가락을 띄면 효과음 재생
    /// </summary>
    private void OnSFXSliderPointerUp(PointerEventData eventData)
    {
        // SFX 볼륨이 0이 아니고 AudioManager가 있을 때만 효과음 재생
        if (SFXSlider != null && SFXSlider.value > 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.SfxPlay("SFX_Money");
            Debug.Log("[SettingPopUp] SFX 슬라이더 효과음 재생");
        }
    }

    /// <summary>
    /// 진동 토글 변경 이벤트
    /// </summary>
    private void OnVibrationToggleChanged(bool isEnabled)
    {
        // 진동 설정 저장
        PlayerPrefs.SetInt(VIBRATION_ENABLED_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        // 진동 테스트 (토글이 켜질 때만)
        if (isEnabled)
        {
            TestVibration();
        }

        Debug.Log($"[SettingPopUp] 진동 설정 변경: {isEnabled}");
    }

    /// <summary>
    /// 진동 테스트
    /// </summary>
    private void TestVibration()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android에서 진동 테스트
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS에서 햅틱 피드백
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // iOS 햅틱 피드백 (iOS 13+)
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
        }
#else
        // 에디터에서는 로그만 출력
        Debug.Log("[SettingPopUp] 진동 테스트 (에디터에서는 실제 진동이 발생하지 않습니다)");
#endif
    }

    /// <summary>
    /// 설정값 로드
    /// </summary>
    private void LoadSettings()
    {
        // BGM 볼륨 로드
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, DEFAULT_BGM_VOLUME);
        if (BGMSlider != null)
        {
            BGMSlider.value = bgmVolume;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.BgmVolume = bgmVolume;
        }

        // SFX 볼륨 로드
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_SFX_VOLUME);
        if (SFXSlider != null)
        {
            SFXSlider.value = sfxVolume;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SfxVolume = sfxVolume;
        }

        // 진동 설정 로드
        bool vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_ENABLED_KEY, DEFAULT_VIBRATION_ENABLED ? 1 : 0) == 1;
        if (VibrationToggle != null)
        {
            VibrationToggle.isOn = vibrationEnabled;
        }

        Debug.Log($"[SettingPopUp] 설정 로드 완료 - BGM: {bgmVolume:F2}, SFX: {sfxVolume:F2}, 진동: {vibrationEnabled}");
    }

    /// <summary>
    /// 설정값 저장 (성능 최적화: 자주 호출되지 않도록 제한)
    /// </summary>
    private void SaveSettings()
    {
        // BGM 볼륨 저장
        if (BGMSlider != null)
        {
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMSlider.value);
        }

        // SFX 볼륨 저장
        if (SFXSlider != null)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFXSlider.value);
        }

        // PlayerPrefs.Save()는 성능상 자주 호출하지 않음
        // 앱 종료 시나 중요한 시점에만 호출
        PlayerPrefs.Save();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // SFX 슬라이더 PointerHandler 이벤트 정리
        if (SFXSlider != null)
        {
            var pointerHandler = SFXSlider.GetComponent<PointerHandler>();
            if (pointerHandler != null)
            {
                pointerHandler.Up -= OnSFXSliderPointerUp;
            }
        }
    }

    /// <summary>
    /// 진동이 활성화되어 있는지 확인
    /// </summary>
    public static bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt(VIBRATION_ENABLED_KEY, DEFAULT_VIBRATION_ENABLED ? 1 : 0) == 1;
    }

    /// <summary>
    /// 진동 실행 (다른 스크립트에서 호출 가능)
    /// </summary>
    public static void TriggerVibration()
    {
        if (IsVibrationEnabled())
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
#endif
        }
    }

}
