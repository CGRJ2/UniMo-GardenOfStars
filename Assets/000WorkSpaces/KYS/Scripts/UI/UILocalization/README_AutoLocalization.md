# 자동 로컬라이제이션 시스템 사용법

## 📋 개요

이 시스템은 UI 이름과 로컬라이제이션 키가 같으면 자동으로 번역을 적용하는 기능을 제공합니다. 기존의 복잡한 수동 설정 대신 UI 이름만으로 자동 번역이 가능합니다. 현재 프로젝트에서는 **InfoHUD 시스템**, **중복 생성 방지** 기능과 함께 사용됩니다.

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

### 3. InfoHUD 시스템과의 통합

InfoHUD 시스템에서도 자동 로컬라이제이션을 사용할 수 있습니다.

```csharp
public class TouchInfoHUD : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // 자동 로컬라이제이션 적용
        if (titleText != null)
        {
            titleText.text = LocalizationManager.Instance.GetLocalizedText("info_title");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = LocalizationManager.Instance.GetLocalizedText("info_description");
        }
    }
}
```

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

## 🎮 InfoHUD 시스템과의 통합

### 1. InfoHUD 자동 로컬라이제이션

```csharp
// TouchInfoHUD에서 자동 로컬라이제이션 사용
public class TouchInfoHUD : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
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

### 2. 언어별 InfoHUD 데이터

```csharp
// 언어별 InfoHUD 정보 관리
public class InfoHUDData : MonoBehaviour
{
    [System.Serializable]
    public class LocalizedInfo
    {
        public string titleKey;
        public string descriptionKey;
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
}
```

### 3. 동적 언어 변경 지원

```csharp
// InfoHUD 언어 변경 시 자동 업데이트
public class LocalizedInfoHUD : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private void Start()
    {
        // 언어 변경 이벤트 구독
        LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        UpdateTexts();
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
        UpdateTexts();
    }
    
    private void UpdateTexts()
    {
        if (titleText != null)
        {
            titleText.text = LocalizationManager.Instance.GetLocalizedText("info_title");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = LocalizationManager.Instance.GetLocalizedText("info_description");
        }
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

### 기본 구조
```csv
Key,Korean,English
title,제목,Title
description,설명,Description
confirm,확인,Confirm
cancel,취소,Cancel
info_title,정보 제목,Info Title
info_description,정보 설명,Info Description
touch_info,터치 정보,Touch Info
```

### InfoHUD 전용 키
```csv
Key,Korean,English
info_hud_title,정보 표시,Information Display
info_hud_description,이 오브젝트에 대한 정보입니다,Information about this object
info_hud_close,닫기,Close
info_hud_more_info,더 많은 정보,More Information
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

### 3. 성능 최적화

**캐싱 활용:**
```csharp
// 번역 결과 캐싱
private Dictionary<string, string> translationCache = new Dictionary<string, string>();

public string GetCachedTranslation(string key)
{
    if (translationCache.ContainsKey(key))
    {
        return translationCache[key];
    }
    
    string translation = LocalizationManager.Instance.GetLocalizedText(key);
    translationCache[key] = translation;
    return translation;
}
```

## 📚 추가 리소스

- [프로젝트 README.md](../README.md)
- [현재 사용 패턴 가이드](../현재_사용_패턴_가이드.md)
- [로컬라이제이션 예제 가이드](./Localization_Examples_Guide.md)

## 🎯 모범 사례

### 1. 키 네이밍 원칙
- **명확성**: 키 이름이 의미를 명확히 전달
- **일관성**: 일관된 네이밍 규칙 사용
- **구체성**: 중복을 피하기 위해 구체적인 이름 사용

### 2. InfoHUD 로컬라이제이션 원칙
- **동적 업데이트**: 언어 변경 시 자동 업데이트
- **키 분리**: InfoHUD 전용 키 사용
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
