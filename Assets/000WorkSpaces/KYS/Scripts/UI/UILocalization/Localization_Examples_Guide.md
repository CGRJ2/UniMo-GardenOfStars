# 로컬라이제이션 예제 가이드

## 📋 개요

이 가이드는 KYS UI 시스템의 로컬라이제이션 기능을 사용하는 다양한 예제들을 제공합니다. 현재 프로젝트에서는 **InfoHUD 시스템**, **중복 생성 방지** 기능과 함께 사용됩니다.

## 🎯 주요 예제

### 1. 기본 로컬라이제이션

#### **간단한 텍스트 번역**
```csharp
public class SimpleLocalization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private void Start()
    {
        UpdateTexts();
    }
    
    private void UpdateTexts()
    {
        if (titleText != null)
        {
            titleText.text = LocalizationManager.Instance.GetLocalizedText("title");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = LocalizationManager.Instance.GetLocalizedText("description");
        }
    }
}
```

#### **언어 변경 감지**
```csharp
public class LanguageChangeHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] localizedTexts;
    
    private void Start()
    {
        // 언어 변경 이벤트 구독
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        UpdateAllTexts();
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        Debug.Log($"언어가 변경되었습니다: {newLanguage}");
        UpdateAllTexts();
    }
    
    private void UpdateAllTexts()
    {
        foreach (TextMeshProUGUI text in localizedTexts)
        {
            if (text != null)
            {
                // UI 이름에서 키 추출
                string key = ExtractKeyFromTextName(text.name);
                text.text = LocalizationManager.Instance.GetLocalizedText(key);
            }
        }
    }
    
    private string ExtractKeyFromTextName(string textName)
    {
        // LocalizationManager의 키 생성 메서드 사용
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GenerateKeyFromUIName(textName, false);
        }
        
        // 기본 처리: "text" 접미사 제거 및 소문자 변환
        string key = textName.ToLower()
            .Replace("text", "")
            .Replace("_", "")
            .Replace("-", "")
            .Replace(" ", "")
            .Trim();
            
        if (string.IsNullOrEmpty(key))
        {
            key = textName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        }
        
        return key;
    }
}
```

### 2. InfoHUD 시스템과의 통합

#### **InfoHUD 로컬라이제이션**
```csharp
public class LocalizedInfoHUD : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI closeButtonText;
    
    public override void Initialize()
    {
        base.Initialize();
        SetupLocalization();
    }
    
    private void SetupLocalization()
    {
        // 언어 변경 이벤트 구독
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        UpdateTexts();
    }
    
    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        UpdateTexts();
    }
    
    private void UpdateTexts()
    {
        if (titleText != null)
        {
            titleText.text = LocalizationManager.Instance.GetLocalizedText("info_hud_title");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = LocalizationManager.Instance.GetLocalizedText("info_hud_description");
        }
        
        if (closeButtonText != null)
        {
            closeButtonText.text = LocalizationManager.Instance.GetLocalizedText("info_hud_close");
        }
    }
    
    public void SetInfo(string titleKey, string descriptionKey)
    {
        if (titleText != null)
        {
            titleText.text = LocalizationManager.Instance.GetLocalizedText(titleKey);
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = LocalizationManager.Instance.GetLocalizedText(descriptionKey);
        }
    }
}
```

#### **동적 InfoHUD 데이터**
```csharp
public class DynamicInfoHUDData : MonoBehaviour
{
    [System.Serializable]
    public class LocalizedInfo
    {
        public string titleKey;
        public string descriptionKey;
        public Sprite icon;
    }
    
    [Header("Localized Info")]
    [SerializeField] private LocalizedInfo koreanInfo;
    [SerializeField] private LocalizedInfo englishInfo;
    
    public LocalizedInfo GetCurrentLanguageInfo()
    {
        SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        
        switch (currentLanguage)
        {
            case SystemLanguage.Korean:
                return koreanInfo;
            case SystemLanguage.English:
                return englishInfo;
            default:
                return englishInfo;
        }
    }
    
    public void ApplyToInfoHUD(TouchInfoHUD infoHUD)
    {
        LocalizedInfo info = GetCurrentLanguageInfo();
        
        if (infoHUD != null)
        {
            infoHUD.SetInfo(info.titleKey, info.descriptionKey);
            // 아이콘 설정도 가능
            // infoHUD.SetIcon(info.icon);
        }
    }
}
```

### 3. 언어 설정 UI

#### **언어 선택 드롭다운**
```csharp
public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    
    private void Start()
    {
        SetupLanguageDropdown();
    }
    
    private void SetupLanguageDropdown()
    {
        if (languageDropdown == null) return;
        
        // 드롭다운 옵션 설정
        languageDropdown.ClearOptions();
        
        List<string> options = new List<string>
        {
            LocalizationManager.Instance.GetLocalizedLanguageName(SystemLanguage.Korean),
            LocalizationManager.Instance.GetLocalizedLanguageName(SystemLanguage.English)
        };
        
        languageDropdown.AddOptions(options);
        
        // 현재 언어 설정
        SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        int currentIndex = GetLanguageIndex(currentLanguage);
        languageDropdown.value = currentIndex;
        
        // 이벤트 구독
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }
    
    private int GetLanguageIndex(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Korean:
                return 0;
            case SystemLanguage.English:
                return 1;
            default:
                return 0;
        }
    }
    
    private void OnLanguageChanged(int index)
    {
        SystemLanguage newLanguage = GetLanguageFromIndex(index);
        LocalizationManager.Instance.SetLanguage(newLanguage);
    }
    
    private SystemLanguage GetLanguageFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return SystemLanguage.Korean;
            case 1:
                return SystemLanguage.English;
            default:
                return SystemLanguage.Korean;
        }
    }
}
```

#### **언어 정보 표시**
```csharp
public class LanguageInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentLanguageText;
    [SerializeField] private TextMeshProUGUI languageCountText;
    
    private void Start()
    {
        UpdateLanguageInfo();
        
        // 언어 변경 이벤트 구독
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
    }
    
    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        UpdateLanguageInfo();
    }
    
    private void UpdateLanguageInfo()
    {
        if (currentLanguageText != null)
        {
            string languageName = LocalizationManager.Instance.GetLocalizedLanguageName(
                LocalizationManager.Instance.CurrentLanguage
            );
            currentLanguageText.text = $"현재 언어: {languageName}";
        }
        
        if (languageCountText != null)
        {
            int keyCount = LocalizationManager.Instance.GetKeyCount();
            languageCountText.text = $"번역 키 수: {keyCount}";
        }
    }
}
```

### 4. 고급 로컬라이제이션

#### **조건부 번역**
```csharp
public class ConditionalLocalization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI conditionalText;
    [SerializeField] private bool isPremiumUser = false;
    
    private void Start()
    {
        UpdateConditionalText();
    }
    
    private void UpdateConditionalText()
    {
        if (conditionalText == null) return;
        
        string key = isPremiumUser ? "premium_welcome" : "free_welcome";
        string baseText = LocalizationManager.Instance.GetLocalizedText(key);
        
        // 추가 정보 삽입
        string userName = "사용자";
        string formattedText = string.Format(baseText, userName);
        
        conditionalText.text = formattedText;
    }
}
```

#### **복수형 처리**
```csharp
public class PluralLocalization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private int itemCount = 0;
    
    private void Start()
    {
        UpdateItemCountText();
    }
    
    private void UpdateItemCountText()
    {
        if (itemCountText == null) return;
        
        string key = GetPluralKey(itemCount);
        string baseText = LocalizationManager.Instance.GetLocalizedText(key);
        
        string formattedText = string.Format(baseText, itemCount);
        itemCountText.text = formattedText;
    }
    
    private string GetPluralKey(int count)
    {
        if (count == 0)
        {
            return "item_count_zero";
        }
        else if (count == 1)
        {
            return "item_count_one";
        }
        else
        {
            return "item_count_many";
        }
    }
    
    public void SetItemCount(int count)
    {
        itemCount = count;
        UpdateItemCountText();
    }
}
```

### 5. 성능 최적화

#### **번역 캐싱**
```csharp
public class CachedLocalization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] cachedTexts;
    
    private Dictionary<string, string> translationCache = new Dictionary<string, string>();
    
    private void Start()
    {
        // 초기 캐싱
        PreloadTranslations();
        UpdateAllTexts();
    }
    
    private void PreloadTranslations()
    {
        string[] keys = { "title", "description", "confirm", "cancel" };
        
        foreach (string key in keys)
        {
            string translation = LocalizationManager.Instance.GetLocalizedText(key);
            translationCache[key] = translation;
        }
    }
    
    private void UpdateAllTexts()
    {
        foreach (TextMeshProUGUI text in cachedTexts)
        {
            if (text != null)
            {
                string key = ExtractKeyFromTextName(text.name);
                string translation = GetCachedTranslation(key);
                text.text = translation;
            }
        }
    }
    
    private string GetCachedTranslation(string key)
    {
        if (translationCache.ContainsKey(key))
        {
            return translationCache[key];
        }
        
        string translation = LocalizationManager.Instance.GetLocalizedText(key);
        translationCache[key] = translation;
        return translation;
    }
    
    private string ExtractKeyFromTextName(string textName)
    {
        // LocalizationManager의 키 생성 메서드 사용
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GenerateKeyFromUIName(textName, false);
        }
        
        // 기본 처리: "text" 접미사 제거 및 소문자 변환
        string key = textName.ToLower()
            .Replace("text", "")
            .Replace("_", "")
            .Replace("-", "")
            .Replace(" ", "")
            .Trim();
            
        if (string.IsNullOrEmpty(key))
        {
            key = textName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        }
        
        return key;
    }
    
    public void ClearCache()
    {
        translationCache.Clear();
    }
}
```

## 📝 CSV 파일 예제

### **기본 번역 파일**
```csv
Key,Korean,English
title,제목,Title
description,설명,Description
confirm,확인,Confirm
cancel,취소,Cancel
start,시작,Start
settings,설정,Settings
exit,종료,Exit
```

### **InfoHUD 전용 번역**
```csv
Key,Korean,English
info_hud_title,정보 표시,Information Display
info_hud_description,이 오브젝트에 대한 정보입니다,Information about this object
info_hud_close,닫기,Close
info_hud_more_info,더 많은 정보,More Information
touch_info_title,터치 정보,Touch Information
touch_info_description,터치한 오브젝트의 정보입니다,Information about the touched object
```

### **언어 이름 번역**
```csv
Key,Korean,English
language_korean,한국어,Korean
language_english,English,English
current_language,현재 언어,Current Language
language_count,번역 키 수,Translation Key Count
```

### **조건부 번역**
```csv
Key,Korean,English
premium_welcome,프리미엄 사용자 {0}님 환영합니다,Welcome Premium User {0}
free_welcome,무료 사용자 {0}님 환영합니다,Welcome Free User {0}
item_count_zero,아이템이 없습니다,No items
item_count_one,아이템 {0}개,{0} item
item_count_many,아이템 {0}개,{0} items
```

## 🛠️ 문제 해결

### **일반적인 문제들**

**번역이 적용되지 않음:**
```
[LocalizationManager] 번역 키를 찾을 수 없음: title
```
- **해결**: CSV 파일에 해당 키 존재 확인
- **해결**: 키 이름 오타 확인

**언어 변경이 안됨:**
```
[LanguageDropdown] 언어 변경 실패
```
- **해결**: OnLanguageChanged 이벤트 구독 확인
- **해결**: LocalizationManager 초기화 확인

**InfoHUD 번역 문제:**
```
[TouchInfoHUD] InfoHUD 번역 실패
```
- **해결**: InfoHUD 전용 키 사용 확인
- **해결**: 동적 키 설정 확인

## 📚 추가 리소스

- [프로젝트 README.md](../README.md)
- [현재 사용 패턴 가이드](../현재_사용_패턴_가이드.md)
- [자동 로컬라이제이션 시스템 사용법](./README_AutoLocalization.md)

## 🎯 모범 사례

### 1. 키 네이밍 원칙
- **명확성**: 키 이름이 의미를 명확히 전달
- **일관성**: 일관된 네이밍 규칙 사용
- **구체성**: 중복을 피하기 위해 구체적인 이름 사용

### 2. InfoHUD 로컬라이제이션 원칙
- **전용 키 사용**: InfoHUD 전용 키 사용
- **동적 업데이트**: 언어 변경 시 자동 업데이트
- **성능 고려**: 캐싱을 통한 성능 최적화

### 3. 유지보수 원칙
- **정기적인 키 검사**: 중복 키 정기 검사
- **문서화**: 키 사용법 문서화
- **테스트**: 다양한 언어에서 테스트

---

**버전**: 2.1  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: InfoHUD 시스템 통합, 중복 생성 방지, 동적 언어 변경 지원, 성능 최적화
