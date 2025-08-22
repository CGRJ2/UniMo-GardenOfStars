# AutoLocalizedText 컴포넌트 사용 가이드

## 개요
`AutoLocalizedText` 컴포넌트는 UI GameObject의 이름과 CSV 파일의 키가 같을 때 자동으로 번역을 적용하는 컴포넌트입니다.

## 컴포넌트 배치 위치

### ✅ 올바른 배치 방법
`AutoLocalizedText` 컴포넌트는 **텍스트를 표시하는 UI 컴포넌트가 있는 GameObject**에 직접 추가해야 합니다.

#### 지원되는 UI 컴포넌트:
- `TextMeshProUGUI`
- `Text` (Legacy)
- `TMP_InputField`
- `InputField` (Legacy)

#### 올바른 예시:
```
UI 구조:
├── MyPanel (BaseUI)
│   ├── ui_title (TextMeshProUGUI + AutoLocalizedText) ← 여기에 추가
│   ├── ui_description (TextMeshProUGUI + AutoLocalizedText) ← 여기에 추가
│   ├── ConfirmButton (Button)
│   │   └── ui_confirm (TextMeshProUGUI + AutoLocalizedText) ← 여기에 추가
│   └── CancelButton (Button)
│       └── ui_cancel (TextMeshProUGUI + AutoLocalizedText) ← 여기에 추가
```

### ❌ 잘못된 배치 방법
- 부모 GameObject (텍스트 컴포넌트가 없는 경우)
- 빈 GameObject
- Image, Button 등 텍스트가 아닌 컴포넌트만 있는 GameObject

## GameObject 이름 규칙

### CSV 키와 매칭되는 이름 사용
```
✅ 올바른 예시:
- "ui_confirm" → CSV 키 "ui_confirm"과 매칭
- "menu_title" → CSV 키 "menu_title"과 매칭
- "settings_language" → CSV 키 "settings_language"과 매칭

❌ 피해야 할 예시:
- "ConfirmButton" → CSV에 "ConfirmButton" 키가 없음
- "TitleText" → CSV에 "TitleText" 키가 없음
- "Button_Confirm" → CSV에 "Button_Confirm" 키가 없음
```

## 설정 옵션

### Inspector에서 설정 가능한 옵션:
1. **Enable Auto Localization**: 자동 로컬라이제이션 활성화/비활성화
2. **Custom Key**: UI 이름과 다른 키를 사용하고 싶을 때
3. **Use Custom Key**: customKey 사용 여부
4. **Show Debug Logs**: 디버그 로그 표시 여부

### 커스텀 키 사용 예시:
```csharp
// GameObject 이름: "ButtonText"
// Custom Key: "ui_confirm"
// Use Custom Key: true
// → CSV의 "ui_confirm" 키를 사용하여 번역
```

## 자동 추가 기능

### BaseUI에서 자동 설정
`BaseUI`를 상속받는 클래스에서는 `Initialize()` 메서드에서 자동으로 설정됩니다:

```csharp
public class MyPanel : BaseUI
{
    protected override void Initialize()
    {
        base.Initialize(); // 자동 로컬라이제이션 설정
        
        // 추가 초기화 코드
    }
}
```

### 수동 설정
```csharp
// TextMeshProUGUI가 있는 GameObject에 AutoLocalizedText 추가
GameObject textObject = GameObject.Find("ui_title");
if (textObject.GetComponent<AutoLocalizedText>() == null)
{
    textObject.AddComponent<AutoLocalizedText>();
}
```

## CSV 파일 관리

### 키 자동 추가
LocalizationManager에서 누락된 키를 자동으로 CSV에 추가할 수 있습니다:

```csharp
// Unity 에디터에서 우클릭 → Context Menu
LocalizationManager.Instance.AutoAddMissingKeysToCSV();
```

### CSV 파일 형식
```csv
Key,Korean,English,Japanese,Chinese
ui_confirm,확인,Confirm,,
ui_cancel,취소,Cancel,,
menu_title,메뉴,Menu,,
```

## 디버깅

### 로그 확인
- `[AutoLocalizedText]` 로그로 자동 번역 상태 확인
- `[LocalizationManager]` 로그로 언어 변경 및 CSV 로드 상태 확인

### 테스트 방법
```csharp
// AutoLocalizationTest 컴포넌트 사용
AutoLocalizationTest test = FindObjectOfType<AutoLocalizationTest>();
test.TestKeyGeneration();
test.TestTranslationAvailability();
```

## 주의사항

1. **GameObject 이름과 CSV 키 일치**: 번역이 제대로 작동하려면 GameObject 이름이 CSV 키와 정확히 일치해야 합니다.

2. **LocalizedText 컴포넌트와 충돌**: 같은 GameObject에 `LocalizedText` 컴포넌트가 있으면 `AutoLocalizedText`는 무시됩니다.

3. **언어 변경**: 언어가 변경되면 자동으로 모든 `AutoLocalizedText` 컴포넌트가 업데이트됩니다.

4. **초기화 순서**: `LocalizationManager`가 초기화된 후에 `AutoLocalizedText`가 작동합니다.

## 문제 해결

### 번역이 적용되지 않는 경우:
1. GameObject 이름이 CSV 키와 일치하는지 확인
2. CSV 파일에 해당 키가 있는지 확인
3. `AutoLocalizedText` 컴포넌트가 올바른 GameObject에 추가되었는지 확인
4. `LocalizationManager`가 초기화되었는지 확인

### 한글이 깨지는 경우:
1. CSV 파일이 UTF-8 인코딩으로 저장되었는지 확인
2. Unity 에디터에서 파일을 다시 로드
3. `LocalizationManager`의 UTF-8 인코딩 설정 확인
