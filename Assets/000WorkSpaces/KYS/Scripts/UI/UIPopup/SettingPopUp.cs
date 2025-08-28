using KYS;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingPopUp : BaseUI
{

    [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
    [SerializeField] private string BGMText = "BGMText";
    [SerializeField] private string SFXText = "SFXText";
    [SerializeField] private string VibrationText = "VibrationText";
    [SerializeField] private string closeButtonName = "CloseButton";
    [SerializeField] private string languageDropdownName = "LanguageDropdown";

    private SystemLanguage selectedLanguage;
    private Dictionary<SystemLanguage, float> languageCompleteness = new Dictionary<SystemLanguage, float>();

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
    }


    private void SetupButtons()
    {
        var confirmEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
        if (confirmEventHandler != null)
        {
            confirmEventHandler.Click += OnCloseButton;

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

}
