# AutoLocalizedText 사용 가이드

## 📋 개요

`AutoLocalizedText`는 UI GameObject의 이름을 기반으로 자동으로 로컬라이제이션 키를 생성하고 번역을 적용하는 컴포넌트입니다. 현재 프로젝트에서는 **InfoHUD 시스템**, **중복 생성 방지** 기능과 함께 사용됩니다.

## 🎯 주요 기능

- **자동 키 생성**: UI 이름에서 자동으로 로컬라이제이션 키 생성
- **실시간 번역**: 언어 변경 시 자동으로 텍스트 업데이트
- **커스텀 키 지원**: 필요시 수동으로 키 지정 가능
- **디버그 지원**: 키 생성 및 번역 과정 로그 출력
- **InfoHUD 통합**: InfoHUD 시스템과 완벽한 통합
- **성능 최적화**: 캐싱을 통한 효율적인 번역 처리

## 🔧 기본 사용법

### 1. 컴포넌트 추가

```csharp
// UI GameObject에 AutoLocalizedText 컴포넌트 추가
public class SimpleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    
    private void Start()
    {
        // AutoLocalizedText 컴포넌트 자동 추가
        if (titleText != null && titleText.GetComponent<AutoLocalizedText>() == null)
        {
            titleText.gameObject.AddComponent<AutoLocalizedText>();
        }
    }
}
```

### 2. Inspector 설정

**AutoLocalizedText 컴포넌트 설정:**
- **Enable Auto Localization**: ✅ 체크 (자동 로컬라이제이션 활성화)
- **Use Custom Key**: ❌ 체크 해제 (UI 이름 기반 키 사용)
- **Custom Key**: 비워둠 (Use Custom Key가 false일 때)
- **Show Debug Logs**: 개발 중에만 체크

### 3. 키 생성 규칙

```
UI GameObject 이름: "StartButton"
→ 자동 생성되는 키: "startbutton"

UI GameObject 이름: "GameTitleText"
→ 자동 생성되는 키: "gametitle"

UI GameObject 이름: "SettingsPanel"
→ 자동 생성되는 키: "settingspanel"

UI GameObject 이름: "Title_Text"
→ 자동 생성되는 키: "title"

UI GameObject 이름: "Menu-Button"
→ 자동 생성되는 키: "menubutton"
```

**키 생성 과정:**
1. **"text" 접미사 제거**: UI 이름에서 "text" 문자열 제거
2. **소문자 변환**: 모든 문자를 소문자로 변환
3. **특수문자 제거**: 언더스코어(_), 하이픈(-), 공백 제거
4. **빈 문자열 처리**: 모든 제거 후 빈 문자열이 되면 원본 이름 사용

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

## 🔄 커스텀 키 사용

### 1. 수동 키 설정

```csharp
public class CustomKeyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI customText;
    
    private void Start()
    {
        SetupCustomKey();
    }
    
    private void SetupCustomKey()
    {
        if (customText != null)
        {
            AutoLocalizedText localizer = customText.GetComponent<AutoLocalizedText>();
            if (localizer == null)
            {
                localizer = customText.gameObject.AddComponent<AutoLocalizedText>();
            }
            
            // 커스텀 키 사용
            localizer.UseCustomKey = true;
            localizer.CustomKey = "custom_ui_text";
        }
    }
}
```

### 2. 런타임 키 변경

```csharp
public class DynamicKeyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dynamicText;
    
    public void ChangeKey(string newKey)
    {
        if (dynamicText != null)
        {
            AutoLocalizedText localizer = dynamicText.GetComponent<AutoLocalizedText>();
            if (localizer != null)
            {
                localizer.UseCustomKey = true;
                localizer.CustomKey = newKey;
                localizer.UpdateText(); // 즉시 업데이트
            }
        }
    }
}
```

## 🔍 디버그 기능

### 1. 디버그 로그 활성화

```csharp
public class DebugLocalization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    
    private void Start()
    {
        SetupDebugLocalization();
    }
    
    private void SetupDebugLocalization()
    {
        if (debugText != null)
        {
            AutoLocalizedText localizer = debugText.GetComponent<AutoLocalizedText>();
            if (localizer == null)
            {
                localizer = debugText.gameObject.AddComponent<AutoLocalizedText>();
            }
            
            // 디버그 로그 활성화
            localizer.ShowDebugLogs = true;
        }
    }
}
```

### 2. 키 생성 과정 추적

```csharp
public class KeyGenerationDebug : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] debugTexts;
    
    private void Start()
    {
        DebugKeyGeneration();
    }
    
    private void DebugKeyGeneration()
    {
        foreach (TextMeshProUGUI text in debugTexts)
        {
            if (text != null)
            {
                AutoLocalizedText localizer = text.GetComponent<AutoLocalizedText>();
                if (localizer != null)
                {
                    Debug.Log($"UI 이름: {text.gameObject.name}");
                    Debug.Log($"생성된 키: {localizer.GetGeneratedKey()}");
                    Debug.Log($"사용 중인 키: {localizer.GetCurrentKey()}");
                }
            }
        }
    }
}
```

## 📝 CSV 파일 구성

### 1. 기본 키 구조

```csv
Key,Korean,English
title,제목,Title
description,설명,Description
confirm,확인,Confirm
cancel,취소,Cancel
```

### 2. InfoHUD 전용 키

```csv
Key,Korean,English
info_hud_title,정보 표시,Information Display
info_hud_description,이 오브젝트에 대한 정보입니다,Information about this object
info_hud_close,닫기,Close
info_hud_more_info,더 많은 정보,More Information
touch_info_title,터치 정보,Touch Information
touch_info_description,터치한 오브젝트의 정보입니다,Information about the touched object
```

### 3. 동적 키 예시

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
- **해결**: UI GameObject 이름 확인
- **해결**: 접미사 제거 규칙 확인

**번역이 적용되지 않음:**
```
[AutoLocalizedText] 번역을 찾을 수 없음: start
```
- **해결**: CSV 파일에 해당 키 존재 확인
- **해결**: LocalizationManager 초기화 확인

### 2. InfoHUD 관련 문제

**InfoHUD 텍스트 업데이트 안됨:**
```
[TouchInfoHUD] AutoLocalizedText 업데이트 실패
```
- **해결**: AutoLocalizedText 컴포넌트 존재 확인
- **해결**: UpdateText() 메서드 호출 확인

**InfoHUD 키 중복:**
```
[AutoLocalizedText] 키 중복 발견: info_title
```
- **해결**: 더 구체적인 키 사용
- **해결**: 커스텀 키 설정

### 3. 성능 최적화

**캐싱 활용:**
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

## 📚 추가 리소스

- [프로젝트 README.md](../README.md)
- [현재 사용 패턴 가이드](../현재_사용_패턴_가이드.md)
- [자동 로컬라이제이션 시스템 사용법](./README_AutoLocalization.md)

## 🎯 모범 사례

### 1. 키 네이밍 원칙
- **명확성**: 키 이름이 의미를 명확히 전달
- **일관성**: 일관된 네이밍 규칙 사용
- **구체성**: 중복을 피하기 위해 구체적인 이름 사용

### 2. InfoHUD AutoLocalizedText 원칙
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
**주요 업데이트**: InfoHUD 시스템 통합, 중복 생성 방지, 동적 키 변경 지원, 성능 최적화
