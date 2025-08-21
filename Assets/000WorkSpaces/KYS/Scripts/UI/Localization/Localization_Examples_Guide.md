# Example 파일 Localization 적용 가이드

## 개요

이 문서는 KYS UI 시스템의 Example 파일들에 Localization을 적용하는 방법을 설명합니다.

## 적용 방법

### 1. 기본 구조

모든 Example 클래스는 다음과 같은 구조로 Localization을 적용합니다:

```csharp
using KYS.UI.Localization;

public class ExamplePanel : BaseUI
{
    [Header("Localized Text Components")]
    [SerializeField] private LocalizedText titleText;
    [SerializeField] private LocalizedText buttonText;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Localization 초기화 대기
        if (Manager.localization != null && Manager.localization.IsInitialized)
        {
            SetupLocalizedTexts();
        }
        else
        {
            StartCoroutine(WaitForLocalization());
        }
    }
    
    private System.Collections.IEnumerator WaitForLocalization()
    {
        while (Manager.localization == null || !Manager.localization.IsInitialized)
        {
            yield return null;
        }
        
        SetupLocalizedTexts();
    }
    
    private void SetupLocalizedTexts()
    {
        // LocalizedText 컴포넌트가 있는 경우 자동으로 처리됨
        // 없으면 수동으로 설정
        if (titleText == null)
        {
            var titleComponent = GetUI<TextMeshProUGUI>("TitleText");
            if (titleComponent != null)
            {
                titleComponent.text = GetLocalizedText("ui_title");
            }
        }
    }
}
```

### 2. LocalizedText 컴포넌트 사용 (권장)

#### **Inspector에서 설정**
1. UI 요소에 `LocalizedText` 컴포넌트 추가
2. `Localization Key` 필드에 CSV 키 입력 (예: `ui_confirm`)
3. `Update On Language Change` 체크

#### **코드에서 설정**
```csharp
[Header("Localized Text Components")]
[SerializeField] private LocalizedText confirmButtonText;
[SerializeField] private LocalizedText cancelButtonText;
```

### 3. 수동 텍스트 설정 (대안)

LocalizedText 컴포넌트를 사용하지 않는 경우:

```csharp
private void SetupLocalizedTexts()
{
    // 버튼 텍스트 설정
    if (confirmButton != null)
    {
        var textComponent = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = GetLocalizedText("ui_confirm");
        }
    }
    
    // 라벨 텍스트 설정
    var titleComponent = GetUI<TextMeshProUGUI>("TitleText");
    if (titleComponent != null)
    {
        titleComponent.text = GetLocalizedText("settings_title");
    }
}
```

### 4. 동적 메시지 Localization

팝업 메시지나 동적 텍스트의 경우:

```csharp
// 기존 방식
UIManager.Instance.ShowConfirmPopUp("게임을 종료하시겠습니까?", callback);

// Localization 적용
string exitMessage = GetLocalizedText("menu_exit_confirm");
UIManager.Instance.ShowConfirmPopUp(exitMessage, callback);

// 포맷 문자열 사용
string itemCountText = GetLocalizedText("inventory_item_count");
itemCountText.text = string.Format(itemCountText, itemCount);
```

## 적용된 Example 파일들

### 1. PanelExamples.cs

#### **MenuPanel**
- **키**: `menu_resume`, `menu_settings`, `menu_exit`, `ui_close`
- **적용**: 버튼 텍스트, 종료 확인 메시지

#### **SettingsPanel**
- **키**: `settings_title`, `settings_bgm`, `settings_sfx`, `settings_fullscreen`, `ui_apply`, `ui_close`
- **적용**: 제목, 라벨, 버튼 텍스트

#### **InventoryPanel**
- **키**: `inventory_title`, `inventory_empty`, `inventory_item_count`, `ui_close`
- **적용**: 제목, 빈 상태 메시지, 아이템 개수 포맷

#### **GameExitConfirmPanel**
- **키**: `ui_confirm`, `ui_cancel`
- **적용**: 확인/취소 버튼 텍스트

### 2. PopupExamples.cs

#### **InteractionPopup**
- **키**: `ui_confirm`, `ui_close`
- **적용**: 상호작용/닫기 버튼 텍스트

#### **InfoPopup**
- **키**: `ui_confirm`, `ui_close`
- **적용**: 확인/닫기 버튼 텍스트

#### **LoadingPopup**
- **키**: `loading_text`
- **적용**: 기본 로딩 텍스트

## CSV 키 매핑

### UI 기본 텍스트
```csv
ui_confirm,확인,Confirm,,
ui_cancel,취소,Cancel,,
ui_close,닫기,Close,,
ui_apply,적용,Apply,,
ui_back,뒤로,Back,,
ui_next,다음,Next,,
ui_previous,이전,Previous,,
```

### 메뉴 관련
```csv
menu_title,메뉴,Menu,,
menu_resume,게임 계속,Resume Game,,
menu_settings,설정,Settings,,
menu_exit,게임 종료,Exit Game,,
menu_inventory,인벤토리,Inventory,,
menu_exit_confirm,게임을 종료하시겠습니까?,Do you want to exit the game?,,
```

### 설정 관련
```csv
settings_title,설정,Settings,,
settings_language,언어,Language,,
settings_volume,볼륨,Volume,,
settings_bgm,배경음악,Background Music,,
settings_sfx,효과음,Sound Effects,,
settings_fullscreen,전체화면,Fullscreen,,
```

### 인벤토리 관련
```csv
inventory_title,인벤토리,Inventory,,
inventory_empty,비어있음,Empty,,
inventory_item_count,아이템 개수: {0},Item Count: {0},,
```

### 로딩 관련
```csv
loading_text,로딩 중...,Loading...,,
loading_please_wait,잠시만 기다려주세요,Please wait,,,
```

## Unity Editor 설정

### 1. LocalizedText 컴포넌트 추가

1. **UI 요소 선택**
2. **Add Component** → **LocalizedText**
3. **Localization Key** 입력 (예: `ui_confirm`)
4. **Update On Language Change** 체크

### 2. Inspector에서 설정

```
MenuPanel (Script)
├── Menu Elements
│   ├── Resume Button
│   ├── Settings Button
│   ├── Exit Button
│   └── Close Button
└── Localized Text Components
    ├── Resume Button Text (LocalizedText)
    ├── Settings Button Text (LocalizedText)
    ├── Exit Button Text (LocalizedText)
    └── Close Button Text (LocalizedText)
```

### 3. LocalizedText 컴포넌트 설정

```
LocalizedText (Script)
├── Localization Key: menu_resume
├── Update On Language Change: ✓
└── Current Text: (자동 업데이트)
```

## 테스트 방법

### 1. 런타임 테스트

```csharp
// 언어 변경 테스트
Manager.localization.SetLanguage(SystemLanguage.English);

// 텍스트 확인
string text = Manager.localization.GetText("ui_confirm"); // "Confirm"
```

### 2. 디버그 정보 출력

```csharp
// LocalizationManager에서 디버그 정보 출력
Manager.localization.PrintLocalizationInfo();
```

### 3. 언어 설정 UI 테스트

```csharp
// 언어 설정 패널 표시
LanguageSettingsPanel.ShowLanguageSettings();
```

## 주의사항

### 1. 초기화 순서
- `LocalizationManager`가 초기화되기 전에 텍스트를 설정하면 안됨
- `WaitForLocalization()` 코루틴으로 대기 필요

### 2. 키 관리
- CSV 파일의 키와 코드의 키가 정확히 일치해야 함
- 키 변경 시 모든 참조 업데이트 필요

### 3. 포맷 문자열
- `{0}`, `{1}` 등의 포맷 문자열 사용 시 주의
- `string.Format()`으로 올바르게 처리

### 4. 성능 최적화
- `LocalizedText` 컴포넌트 사용 권장 (자동 업데이트)
- 불필요한 `GetLocalizedText()` 호출 최소화

## 확장 방법

### 1. 새 언어 추가

```csv
Key,Korean,English,Japanese,Chinese,Spanish
ui_confirm,확인,Confirm,確認,确认,Confirmar
```

### 2. 새 키 추가

```csv
# 새 기능 추가
new_feature_title,새 기능,New Feature,,
new_feature_description,새로운 기능입니다,This is a new feature,,
```

### 3. 동적 키 생성

```csharp
// 아이템 이름 동적 생성
string itemKey = $"item_{itemId}_name";
string itemName = GetLocalizedText(itemKey);
```

## 결론

Example 파일들에 Localization을 적용하면:

1. **일관성**: 모든 UI가 동일한 방식으로 다국어 지원
2. **유지보수성**: CSV 파일로 중앙 관리
3. **확장성**: 새 언어/키 추가 용이
4. **테스트**: 런타임 언어 변경으로 즉시 확인 가능

이 가이드를 따라 다른 UI 클래스들에도 동일하게 Localization을 적용할 수 있습니다.
