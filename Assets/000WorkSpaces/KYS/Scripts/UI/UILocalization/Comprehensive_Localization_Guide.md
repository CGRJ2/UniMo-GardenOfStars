# 로컬라이제이션 종합 가이드

## 📋 개요

이 가이드는 KYS UI 시스템의 로컬라이제이션 기능을 종합적으로 다룹니다. **자동 로컬라이제이션**, **InfoHUD 시스템 통합**, **다양한 사용 예제**를 포함하여 완전한 로컬라이제이션 솔루션을 제공합니다.

## 🎯 주요 컴포넌트

### 1. AutoLocalizedText 컴포넌트

UI GameObject에 직접 추가하여 사용하는 컴포넌트입니다.

#### 설정 옵션
- **Enable Auto Localization**: 자동 로컬라이제이션 활성화/비활성화
- **Custom Key**: UI 이름과 다른 키를 사용하고 싶을 때 설정
- **Use Custom Key**: Custom Key 사용 여부
- **Show Debug Logs**: 디버그 로그 출력 여부

#### 사용법

1. **기본 사용법** (UI 이름 기반)
   ```
   UI GameObject 이름: "StartButton"
   → 자동 생성되는 키: "start"
   ```

2. **커스텀 키 사용**
   ```
   UI GameObject 이름: "StartButton"
   Custom Key: "game_start_button"
   Use Custom Key: true
   → 사용되는 키: "game_start_button"
   ```

### 2. BaseUI 자동 로컬라이제이션

BaseUI를 상속받는 클래스에서 자동으로 모든 TextMeshProUGUI에 AutoLocalizedText 컴포넌트를 추가합니다.

#### 설정 옵션
- **Enable Auto Localization**: 자동 로컬라이제이션 활성화/비활성화
- **Auto Localize Keys**: 수동으로 키를 설정하고 싶을 때 사용

## 🔑 키 생성 규칙

### UI 이름에서 키 생성 시 제거되는 요소들
- **"text" 접미사**: UI 이름에서 "text" 문자열이 제거됩니다
- **특수문자**: 언더스코어(_), 하이픈(-), 공백이 모두 제거됩니다
- **소문자 변환**: 모든 키는 소문자로 변환됩니다
- **빈 문자열 처리**: 모든 제거 후 빈 문자열이 되면 원본 이름을 사용합니다

### 예시
```
"StartButtonText" → "startbutton"
"GameTitleLabel" → "gametitlelabel"
"SettingsPanel" → "settingspanel"
"MainMenuWindow" → "mainmenuwindow"
"InfoHUDTitle" → "infohudtitle"
"TouchInfoDescription" → "touchinfodescription"
"Title_Text" → "title"
"Menu-Button" → "menubutton"
```

## 🎮 InfoHUD 시스템과의 통합

### 1. InfoHUD 자동 로컬라이제이션

```csharp
public class TouchInfoHUD : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // InfoHUD 텍스트에 자동 로컬라이제이션 적용
        SetupAutoLocalization();
    }
    
    private void SetupAutoLocalization()
    {
        // 제목 텍스트에 AutoLocalizedText 추가
        if (titleText != null)
        {
            AutoLocalizedText titleLocalizer = titleText.GetComponent<AutoLocalizedText>();
            if (titleLocalizer == null)
            {
                titleLocalizer = titleText.gameObject.AddComponent<AutoLocalizedText>();
            }
            
            // InfoHUD 전용 키 사용
            titleLocalizer.UseCustomKey = true;
            titleLocalizer.CustomKey = "info_hud_title";
        }
        
        // 설명 텍스트에 AutoLocalizedText 추가
        if (descriptionText != null)
        {
            AutoLocalizedText descLocalizer = descriptionText.GetComponent<AutoLocalizedText>();
            if (descLocalizer == null)
            {
                descLocalizer = descriptionText.gameObject.AddComponent<AutoLocalizedText>();
            }
            
            descLocalizer.UseCustomKey = true;
            descLocalizer.CustomKey = "info_hud_description";
        }
    }
}
```

### 2. 동적 InfoHUD 텍스트

```csharp
public class DynamicInfoHUD : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    public void SetInfo(string titleKey, string descriptionKey)
    {
        // 동적으로 키 설정
        if (titleText != null)
        {
            AutoLocalizedText titleLocalizer = titleText.GetComponent<AutoLocalizedText>();
            if (titleLocalizer != null)
            {
                titleLocalizer.UseCustomKey = true;
                titleLocalizer.CustomKey = titleKey;
                titleLocalizer.UpdateText(); // 즉시 업데이트
            }
        }
        
        if (descriptionText != null)
        {
            AutoLocalizedText descLocalizer = descriptionText.GetComponent<AutoLocalizedText>();
            if (descLocalizer != null)
            {
                descLocalizer.UseCustomKey = true;
                descLocalizer.CustomKey = descriptionKey;
                descLocalizer.UpdateText(); // 즉시 업데이트
            }
        }
    }
}
```

### 3. 언어별 InfoHUD 데이터

```csharp
public class LocalizedInfoHUDData : MonoBehaviour
{
    [System.Serializable]
    public class InfoHUDLocalization
    {
        public string titleKey;
        public string descriptionKey;
        public string closeKey;
    }
    
    [Header("Localization Data")]
    [SerializeField] private InfoHUDLocalization koreanData;
    [SerializeField] private InfoHUDLocalization englishData;
    
    public InfoHUDLocalization GetCurrentLanguageData()
    {
        SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        
        switch (currentLanguage)
        {
            case SystemLanguage.Korean:
                return koreanData;
            case SystemLanguage.English:
                return englishData;
            default:
                return englishData;
        }
    }
    
    public void ApplyLocalizationToInfoHUD(TouchInfoHUD infoHUD)
    {
        InfoHUDLocalization data = GetCurrentLanguageData();
        
        if (infoHUD != null)
        {
            infoHUD.SetInfo(data.titleKey, data.descriptionKey);
        }
    }
}
```

## 🔄 언어 변경 감지

### 1. 기본 언어 변경 처리

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

### 2. 언어 설정 UI

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

## 🔄 키 중복 관리

### 중복 검사 기능
LocalizationManager에서 제공하는 기능들:

1. **중복 키 검사**
   ```csharp
   LocalizationManager.Instance.CheckForDuplicateKeys();
   ```

2. **키 존재 여부 확인**
   ```csharp
   bool exists = LocalizationManager.Instance.HasKey("start");
   ```

3. **UI 이름으로 키 생성 (중복 검사 포함)**
   ```csharp
   string key = LocalizationManager.Instance.GenerateKeyFromUIName("StartButton", true);
   ```

### 중복 발생 시 대응 방법

1. **커스텀 키 사용**
   - AutoLocalizedText 컴포넌트에서 Custom Key 설정
   - Use Custom Key를 true로 설정

2. **UI 이름 변경**
   - 더 구체적인 이름으로 변경
   - 예: "StartButton" → "MainMenuStartButton"

3. **수동 키 설정**
   - BaseUI의 Auto Localize Keys 배열에 수동으로 키 추가

## 🎯 고급 로컬라이제이션

### 1. 조건부 번역

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

### 2. 복수형 처리

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

## 🚀 성능 최적화

### 1. 번역 캐싱

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

### 2. AutoLocalizedText 캐싱

```csharp
public class CachedAutoLocalizedText : AutoLocalizedText
{
    private Dictionary<string, string> translationCache = new Dictionary<string, string>();
    
    protected override string GetLocalizedText(string key)
    {
        if (translationCache.ContainsKey(key))
        {
            return translationCache[key];
        }
        
        string translation = base.GetLocalizedText(key);
        translationCache[key] = translation;
        return translation;
    }
    
    public void ClearCache()
    {
        translationCache.Clear();
    }
}
```

## 🔍 디버그 기능

### LocalizationManager Context Menu
- **Check for Duplicate Keys**: 중복 키 검사
- **Print All Keys**: 모든 키 목록 출력
- **Print Keys Without Translation**: 번역이 없는 키 목록 출력
- **Print Keys With All Translations**: 모든 언어에서 번역이 완료된 키 목록 출력

### AutoLocalizedText 디버그
- **Show Debug Logs**: 키 생성 및 번역 과정 로그 출력
- **Custom Key Validation**: 커스텀 키 유효성 검사

### InfoHUD 로컬라이제이션 디버그

```csharp
// InfoHUD 로컬라이제이션 디버그
public class InfoHUDLocalizationDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    
    private void Start()
    {
        if (enableDebug)
        {
            Debug.Log($"현재 언어: {LocalizationManager.Instance.CurrentLanguage}");
            Debug.Log($"InfoHUD 제목: {LocalizationManager.Instance.GetLocalizedText("info_title")}");
            Debug.Log($"InfoHUD 설명: {LocalizationManager.Instance.GetLocalizedText("info_description")}");
        }
    }
}
```

## 📝 CSV 파일 구성

### 1. 기본 번역 파일
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

### 2. InfoHUD 전용 번역
```csv
Key,Korean,English
info_hud_title,정보 표시,Information Display
info_hud_description,이 오브젝트에 대한 정보입니다,Information about this object
info_hud_close,닫기,Close
info_hud_more_info,더 많은 정보,More Information
touch_info_title,터치 정보,Touch Information
touch_info_description,터치한 오브젝트의 정보입니다,Information about the touched object
```

### 3. 언어 이름 번역
```csv
Key,Korean,English
language_korean,한국어,Korean
language_english,English,English
current_language,현재 언어,Current Language
language_count,번역 키 수,Translation Key Count
```

### 4. 조건부 번역
```csv
Key,Korean,English
premium_welcome,프리미엄 사용자 {0}님 환영합니다,Welcome Premium User {0}
free_welcome,무료 사용자 {0}님 환영합니다,Welcome Free User {0}
item_count_zero,아이템이 없습니다,No items
item_count_one,아이템 {0}개,{0} item
item_count_many,아이템 {0}개,{0} items
```

### 5. 동적 키 예시
```csv
Key,Korean,English
item_sword_info,검 정보,Sword Information
item_potion_info,포션 정보,Potion Information
item_armor_info,갑옷 정보,Armor Information
npc_merchant_info,상인 정보,Merchant Information
npc_guard_info,경비병 정보,Guard Information
```

## 🛠️ 문제 해결

### 1. 일반적인 문제들

**키가 생성되지 않음:**
```
[AutoLocalizedText] 키 생성 실패: StartButton
```
- **해결**: UI 이름이 올바른지 확인
- **해결**: 접미사 제거 규칙 확인

**번역이 적용되지 않음:**
```
[LocalizationManager] 번역 키를 찾을 수 없음: start
```
- **해결**: CSV 파일에 해당 키가 있는지 확인
- **해결**: 키 생성 규칙 확인

**언어 변경이 안됨:**
```
[LanguageDropdown] 언어 변경 실패
```
- **해결**: OnLanguageChanged 이벤트 구독 확인
- **해결**: LocalizationManager 초기화 확인

### 2. InfoHUD 관련 문제

**InfoHUD 언어 변경 안됨:**
```
[TouchInfoHUD] 언어 변경 시 텍스트 업데이트 실패
```
- **해결**: OnLanguageChanged 이벤트 구독 확인
- **해결**: UpdateTexts 메서드 호출 확인

**InfoHUD 키 중복:**
```
[LocalizationManager] InfoHUD 키 중복 발견: info_title
```
- **해결**: 더 구체적인 키 사용 (예: "item_info_title")
- **해결**: 커스텀 키 설정

**InfoHUD 텍스트 업데이트 안됨:**
```
[TouchInfoHUD] AutoLocalizedText 업데이트 실패
```
- **해결**: AutoLocalizedText 컴포넌트 존재 확인
- **해결**: UpdateText() 메서드 호출 확인

## 📚 추가 리소스

- [프로젝트 README.md](../README.md)
- [현재 사용 패턴 가이드](../현재_사용_패턴_가이드.md)
- [Unity Editor Setup Guide](../Unity_Editor_Setup_Guide.md)

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
**최종 업데이트**: 2025년 9월 15일  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: InfoHUD 시스템 통합, 중복 생성 방지, 동적 언어 변경 지원, 성능 최적화
