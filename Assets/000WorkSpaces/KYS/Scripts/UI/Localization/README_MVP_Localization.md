# MVP 패턴 다국어 시스템 가이드

## 개요

이 문서는 MVP(Model-View-Presenter) 패턴으로 개선된 다국어 시스템의 사용법을 설명합니다.

## 주요 개선사항

### 1. MVP 패턴 적용
- **Model**: `LanguageModel` - 언어 데이터 관리 및 비즈니스 로직
- **View**: `LanguageSettingsPanel` - UI 표시만 담당
- **Presenter**: `LanguageSettingsPresenter` - Model과 View 간 중재

### 2. CSV 파일 형식 채택
- **Excel/Google Sheets 직접 편집 가능**
- **번역가 친화적**
- **새 언어 추가 용이**

### 3. 향후 확장 준비
- **일본어/중국어 열 미리 준비**
- **빈칸으로 두어도 호환 가능**
- **활성 언어와 지원 언어 분리**

## 파일 구조

```
Assets/YS_Initial/Scripts/UI/Localization/
├── LanguageModel.cs              # MVP Model
├── LanguageSettingsPresenter.cs  # MVP Presenter
├── LanguageSettingsPanel.cs      # MVP View
├── LocalizedText.cs              # 다국어 텍스트 컴포넌트
├── LanguageData.csv              # CSV 언어 데이터
└── README_MVP_Localization.md    # 이 가이드
```

## 설정 방법

### 1. LanguageModel 설정

```csharp
// 씬에 LanguageModel GameObject 추가
GameObject languageModelObj = new GameObject("LanguageModel");
LanguageModel languageModel = languageModelObj.AddComponent<LanguageModel>();

// CSV 파일 할당
languageModel.csvLanguageFile = csvTextAsset;
```

### 2. CSV 파일 형식

```csv
Key,Korean,English,Japanese,Chinese
ui_confirm,확인,Confirm,,
menu_title,메뉴,Menu,,
```

**주요 특징:**
- **빈칸 허용**: 일본어/중국어 열을 빈칸으로 두어도 정상 작동
- **활성 언어 자동 감지**: 실제 번역이 있는 언어만 UI에 표시
- **주석 지원**: `#`으로 시작하는 줄은 주석으로 처리

### 3. UI에서 사용

```csharp
// LocalizedText 컴포넌트 사용
[SerializeField] private LocalizedText titleText;

// Inspector에서 설정
// - Localization Key: "menu_title"
// - Language Model: LanguageModel 참조
```

## MVP 패턴의 장점

### 1. 관심사 분리
```csharp
// ❌ 이전 방식 (View에 비즈니스 로직)
private void OnApplyClicked()
{
    LocalizationManager.Instance.SetLanguage(selectedLanguage);
    Hide();
}

// ✅ MVP 방식 (Presenter가 중재)
public void OnApplyLanguage()
{
    model.SetLanguage(selectedLanguage);  // Model에 위임
    view.Hide();                         // View에 위임
}
```

### 2. 테스트 가능성
```csharp
// Model 테스트
[Test]
public void TestLanguageChange()
{
    var model = new LanguageModel();
    model.SetLanguage(SystemLanguage.English);
    Assert.AreEqual(SystemLanguage.English, model.CurrentLanguage);
}

// Presenter 테스트
[Test]
public void TestPresenterLanguageChange()
{
    var presenter = new LanguageSettingsPresenter();
    presenter.OnApplyLanguage();
    // Mock Model과 View로 테스트 가능
}
```

### 3. 재사용성
```csharp
// 같은 Model을 다른 View에서 사용 가능
public class SimpleLanguageSelector : MonoBehaviour
{
    [SerializeField] private LanguageModel languageModel;
    
    public void ChangeLanguage(SystemLanguage language)
    {
        languageModel.SetLanguage(language);
    }
}
```

## 향후 확장 준비

### 1. 일본어/중국어 추가 준비

현재 CSV 파일에는 일본어와 중국어 열이 미리 준비되어 있습니다:

```csv
Key,Korean,English,Japanese,Chinese
ui_confirm,확인,Confirm,,
menu_title,메뉴,Menu,,
```

### 2. 번역 추가 방법

```csv
# 기존 (빈칸)
ui_confirm,확인,Confirm,,

# 번역 추가 후
ui_confirm,확인,Confirm,確認,确认
```

### 3. 활성 언어 자동 감지

- **빈칸이 아닌 번역이 있는 언어만 UI에 표시**
- **번역 완성도 확인 가능**
- **기본 언어는 항상 활성화**

## CSV vs 기존 형식 비교

### 기존 형식 (INI 스타일)
```
[Korean]
ui_confirm="확인"
menu_title="메뉴"

[English]
ui_confirm="Confirm"
menu_title="Menu"
```

**장점:**
- 간단한 파싱
- 가독성 좋음

**단점:**
- Excel 편집 불가
- 새 언어 추가 복잡
- 커스텀 파싱 로직 필요

### CSV 형식
```csv
Key,Korean,English,Japanese,Chinese
ui_confirm,확인,Confirm,,
menu_title,메뉴,Menu,,
```

**장점:**
- Excel/Google Sheets 직접 편집
- 표 형태로 직관적
- 새 언어 추가 용이 (새 열만 추가)
- 표준 형식
- **빈칸 호환 가능**

**단점:**
- 쉼표와 따옴표 처리 필요
- 주석 처리 복잡

## 사용 예시

### 1. 기본 사용법

```csharp
// LanguageModel 초기화
LanguageModel languageModel = FindObjectOfType<LanguageModel>();

// 텍스트 가져오기
string text = languageModel.GetText("ui_confirm");

// 언어 변경
languageModel.SetLanguage(SystemLanguage.English);
```

### 2. 이벤트 구독

```csharp
// 언어 변경 이벤트 구독
languageModel.OnLanguageChanged += (language) => {
    Debug.Log($"언어가 변경되었습니다: {language}");
    // UI 업데이트
};

// 활성 언어 로드 이벤트 구독
languageModel.OnActiveLanguagesLoaded += (languages) => {
    Debug.Log($"활성 언어: {languages.Length}개");
    // 드롭다운 업데이트
};
```

### 3. LocalizedText 컴포넌트 사용

```csharp
// Inspector에서 설정
// - Localization Key: "menu_title"
// - Update On Language Change: true
// - Language Model: LanguageModel 참조

// 코드에서 키 변경
localizedText.SetLocalizationKey("new_key");
```

### 4. 번역 완성도 확인

```csharp
// 특정 언어의 번역 완성도 확인
float completeness = languageModel.GetTranslationCompleteness(SystemLanguage.Japanese);
Debug.Log($"일본어 번역 완성도: {completeness * 100}%");

// 활성 언어 확인
bool isActive = languageModel.IsLanguageActive(SystemLanguage.Chinese);
Debug.Log($"중국어 활성화: {isActive}");
```

## 마이그레이션 가이드

### 기존 LocalizationManager에서 LanguageModel으로

```csharp
// ❌ 기존 방식
LocalizationManager.Instance.SetLanguage(language);
string text = LocalizationManager.Instance.GetText("key");

// ✅ 새로운 방식
languageModel.SetLanguage(language);
string text = languageModel.GetText("key");
```

### 기존 텍스트 파일에서 CSV로

1. **기존 파일 분석**
   ```
   [Korean]
   ui_confirm="확인"
   ```

2. **CSV 형식으로 변환**
   ```csv
   Key,Korean
   ui_confirm,확인
   ```

3. **새 언어 추가**
   ```csv
   Key,Korean,English
   ui_confirm,확인,Confirm
   ```

4. **향후 확장 준비**
   ```csv
   Key,Korean,English,Japanese,Chinese
   ui_confirm,확인,Confirm,,
   ```

## 주의사항

1. **CSV 파일 인코딩**: UTF-8로 저장
2. **쉼표 처리**: 텍스트에 쉼표가 있으면 따옴표로 감싸기
3. **빈 줄**: 주석으로 처리 (`#` 사용)
4. **키 중복**: 동일한 키가 있으면 마지막 값이 사용됨
5. **빈칸 처리**: 빈칸으로 둔 언어는 자동으로 비활성화됨

## 확장 방법

### 1. 새 언어 추가
```csv
Key,Korean,English,Japanese,Chinese,Spanish
ui_confirm,확인,Confirm,確認,确认,Confirmar
```

### 2. 새 기능 추가
```csharp
// LanguageModel에 새 메서드 추가
public string GetTextWithFormat(string key, params object[] args)
{
    string text = GetText(key);
    return string.Format(text, args);
}
```

### 3. 동적 언어 로드
```csharp
// 런타임에 CSV 파일 로드
public void LoadLanguageFile(TextAsset csvFile)
{
    ParseCSVLanguageFile(csvFile.text);
}
```

### 4. 번역 완성도 모니터링
```csharp
// 모든 언어의 번역 완성도 확인
foreach (var language in model.AllSupportedLanguages)
{
    float completeness = model.GetTranslationCompleteness(language);
    Debug.Log($"{model.GetLanguageName(language)}: {completeness * 100}%");
}
```

## 결론

MVP 패턴과 CSV 형식을 조합한 다국어 시스템은:

1. **유지보수성 향상** - 관심사 분리
2. **테스트 가능성** - 각 컴포넌트 독립 테스트
3. **확장성** - 새 언어/기능 추가 용이
4. **번역가 친화적** - Excel 편집 가능
5. **향후 확장 준비** - 일본어/중국어 미리 준비
6. **빈칸 호환성** - 번역이 없어도 정상 작동

이 시스템을 통해 더 체계적이고 확장 가능한 다국어 지원이 가능합니다.
