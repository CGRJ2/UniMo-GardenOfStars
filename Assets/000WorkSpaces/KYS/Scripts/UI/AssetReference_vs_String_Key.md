# AssetReference vs 문자열 키 비교

## 1. 개요

Unity Addressables에서는 두 가지 방식으로 에셋을 참조할 수 있습니다:
1. **문자열 키 (String Key)**: 런타임에 동적으로 에셋을 로드
2. **AssetReference**: 컴파일 타임에 타입 안전성을 보장하는 참조

현재 프로젝트에서는 **InfoHUD 시스템**, **중복 생성 방지**, **로컬라이제이션** 기능과 함께 사용됩니다.

## 2. AssetReference (권장)

### 2.1 장점
- **타입 안전성**: 컴파일 타임에 타입 오류 감지
- **IntelliSense 지원**: 자동완성 및 리팩토링 지원
- **에디터 통합**: Unity 에디터에서 드래그 앤 드롭으로 설정
- **참조 추적**: 에셋이 삭제되거나 이동될 때 자동으로 참조 업데이트
- **런타임 키 검증**: `RuntimeKeyIsValid()` 메서드로 유효성 확인
- **중복 생성 방지**: UIManager와 연동하여 중복 생성 자동 방지

### 2.2 사용법
```csharp
[SerializeField] private AssetReferenceGameObject hudPanelReference;

public async void LoadHUD()
{
    if (hudPanelReference != null && hudPanelReference.RuntimeKeyIsValid())
    {
        var handle = hudPanelReference.InstantiateAsync();
        await handle.Task;
        // 사용 후 해제
        Addressables.ReleaseInstance(handle);
    }
}
```

### 2.3 AssetReference 타입들
```csharp
AssetReferenceGameObject          // GameObject 프리팹
AssetReferenceT<MyScript>         // 특정 컴포넌트 타입
AssetReferenceTexture2D           // 텍스처
AssetReferenceAudioClip           // 오디오 클립
AssetReferenceScene               // 씬
```

### 2.4 InfoHUD 시스템에서의 사용
```csharp
// TouchInfoHUD AssetReference 사용
[SerializeField] private AssetReferenceGameObject touchInfoHUDReference;
[SerializeField] private AssetReferenceGameObject hudBackdropReference;

public async Task<TouchInfoHUD> CreateInfoHUDAsync()
{
    if (touchInfoHUDReference != null && touchInfoHUDReference.RuntimeKeyIsValid())
    {
        // 중복 생성 방지와 함께 로드
        return await UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(touchInfoHUDReference);
    }
    return null;
}
```

## 3. 문자열 키 (String Key)

### 3.1 장점
- **동적 로드**: 런타임에 키를 동적으로 결정 가능
- **유연성**: 데이터 기반으로 에셋 로드
- **간단함**: 문자열만으로 에셋 접근
- **로컬라이제이션**: 언어별 에셋 동적 로드

### 3.2 단점
- **타입 안전성 없음**: 런타임에만 오류 발견
- **오타 위험**: 문자열 오타 시 런타임 오류
- **참조 추적 불가**: 에셋 이름 변경 시 수동 수정 필요
- **IntelliSense 지원 없음**: 자동완성 불가

### 3.3 사용법
```csharp
public async void LoadUI(string uiKey)
{
    var handle = Addressables.InstantiateAsync(uiKey);
    await handle.Task;
    // 사용 후 해제
    Addressables.ReleaseInstance(handle);
}
```

### 3.4 로컬라이제이션에서의 사용
```csharp
// 언어별 UI 프리팹 동적 로드
public async Task<BaseUI> LoadLocalizedUI(string uiType)
{
    string language = LocalizationManager.Instance.CurrentLanguage.ToString();
    string key = $"UI/{language}/{uiType}";
    
    return await UIManager.Instance.LoadUIAsync<BaseUI>(key);
}
```

## 4. UIManager에서의 사용

### 4.1 Canvas 참조 (AssetReference 권장)
```csharp
[Header("Addressable Canvas References")]
[SerializeField] private AssetReferenceGameObject hudCanvasReference;
[SerializeField] private AssetReferenceGameObject panelCanvasReference;
[SerializeField] private AssetReferenceGameObject popupCanvasReference;
[SerializeField] private AssetReferenceGameObject loadingCanvasReference;
```

### 4.2 UI 로드 (두 방식 모두 지원)
```csharp
// 문자열 키 사용
BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");

// AssetReference 사용
BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>(mainMenuReference);
```

### 4.3 InfoHUD 로드 (AssetReference 권장)
```csharp
// InfoHUD는 AssetReference 사용 권장 (타입 안전성)
[SerializeField] private AssetReferenceGameObject touchInfoHUDReference;

public async Task<TouchInfoHUD> ShowInfoHUDAsync(Vector2 position, string title, string description)
{
    if (touchInfoHUDReference != null && touchInfoHUDReference.RuntimeKeyIsValid())
    {
        // 중복 생성 방지와 함께 로드
        return await UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(touchInfoHUDReference);
    }
    return null;
}
```

## 5. 권장 사용 패턴

### 5.1 고정된 UI (AssetReference 권장)
- 메인 메뉴, 설정 패널 등 항상 존재하는 UI
- Canvas 참조
- 자주 사용되는 UI 프리팹
- InfoHUD 시스템 관련 프리팹

```csharp
[SerializeField] private AssetReferenceGameObject mainMenuReference;
[SerializeField] private AssetReferenceGameObject settingsReference;
[SerializeField] private AssetReferenceGameObject touchInfoHUDReference;
[SerializeField] private AssetReferenceGameObject hudBackdropReference;
```

### 5.2 동적 UI (문자열 키 사용)
- 사용자 생성 콘텐츠
- 데이터 기반 UI
- 런타임에 결정되는 UI
- 로컬라이제이션 관련 UI

```csharp
public async void LoadDynamicUI(string uiType, string uiName)
{
    string key = $"UI/{uiType}/{uiName}";
    BaseUI ui = await UIManager.Instance.LoadUIAsync<BaseUI>(key);
}

// 로컬라이제이션 UI
public async void LoadLocalizedUI(string uiType)
{
    string language = LocalizationManager.Instance.CurrentLanguage.ToString();
    string key = $"UI/Localized/{language}/{uiType}";
    BaseUI ui = await UIManager.Instance.LoadUIAsync<BaseUI>(key);
}
```

### 5.3 하이브리드 접근법
```csharp
public class UILoader : MonoBehaviour
{
    [Header("고정 UI (AssetReference)")]
    [SerializeField] private AssetReferenceGameObject mainMenuReference;
    [SerializeField] private AssetReferenceGameObject settingsReference;
    [SerializeField] private AssetReferenceGameObject touchInfoHUDReference;
    
    [Header("동적 UI (문자열 키)")]
    [SerializeField] private string dynamicUIPrefix = "UI/Dynamic/";
    [SerializeField] private string localizedUIPrefix = "UI/Localized/";
    
    // 고정 UI 로드
    public async Task<BaseUI> LoadMainMenu()
    {
        return await UIManager.Instance.LoadUIAsync<BaseUI>(mainMenuReference);
    }
    
    // 동적 UI 로드
    public async Task<BaseUI> LoadDynamicUI(string uiName)
    {
        string key = $"{dynamicUIPrefix}{uiName}";
        return await UIManager.Instance.LoadUIAsync<BaseUI>(key);
    }
    
    // 로컬라이제이션 UI 로드
    public async Task<BaseUI> LoadLocalizedUI(string uiType)
    {
        string language = LocalizationManager.Instance.CurrentLanguage.ToString();
        string key = $"{localizedUIPrefix}{language}/{uiType}";
        return await UIManager.Instance.LoadUIAsync<BaseUI>(key);
    }
    
    // InfoHUD 로드
    public async Task<TouchInfoHUD> LoadInfoHUD()
    {
        return await UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(touchInfoHUDReference);
    }
}
```

## 6. 성능 비교

### 6.1 AssetReference
- **컴파일 타임**: 타입 검증으로 안전성 확보
- **런타임**: 약간의 오버헤드 (타입 검증)
- **메모리**: 참조 객체 크기만큼 추가 메모리
- **중복 생성 방지**: UIManager와 연동하여 효율적 관리

### 6.2 문자열 키
- **컴파일 타임**: 검증 없음
- **런타임**: 문자열 처리 오버헤드
- **메모리**: 문자열 크기만큼 메모리
- **로컬라이제이션**: 언어별 동적 로드로 유연성 확보

## 7. 마이그레이션 가이드

### 7.1 문자열 키에서 AssetReference로 전환
```csharp
// 기존 (문자열 키)
public async Task<BaseUI> LoadUI(string key)
{
    return await UIManager.Instance.LoadUIAsync<BaseUI>(key);
}

// 개선 (AssetReference)
[SerializeField] private AssetReferenceGameObject uiReference;
public async Task<BaseUI> LoadUI()
{
    return await UIManager.Instance.LoadUIAsync<BaseUI>(uiReference);
}
```

### 7.2 점진적 전환
1. **고정 UI부터 전환**: 자주 사용되는 UI를 AssetReference로 변경
2. **InfoHUD 시스템 전환**: TouchInfoHUD 관련 프리팹을 AssetReference로 변경
3. **동적 UI는 유지**: 런타임에 결정되는 UI는 문자열 키 유지
4. **로컬라이제이션 UI**: 언어별 동적 로드가 필요한 UI는 문자열 키 유지
5. **하이브리드 사용**: 두 방식을 적절히 혼합

### 7.3 InfoHUD 시스템 마이그레이션
```csharp
// 기존 (문자열 키)
public async Task<TouchInfoHUD> ShowInfoHUD(string key)
{
    return await UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(key);
}

// 개선 (AssetReference)
[SerializeField] private AssetReferenceGameObject touchInfoHUDReference;
public async Task<TouchInfoHUD> ShowInfoHUD()
{
    return await UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(touchInfoHUDReference);
}
```

## 8. 결론

### 8.1 AssetReference 사용 권장 상황
- 고정된 UI 프리팹
- Canvas 참조
- 자주 사용되는 에셋
- 타입 안전성이 중요한 경우
- InfoHUD 시스템 관련 프리팹
- 중복 생성 방지가 필요한 UI

### 8.2 문자열 키 사용 권장 상황
- 동적으로 결정되는 UI
- 데이터 기반 콘텐츠
- 런타임에 키가 결정되는 경우
- 로컬라이제이션 관련 UI
- 언어별 동적 로드가 필요한 경우

### 8.3 최종 권장사항
**AssetReference를 기본으로 사용하고, 동적 로드나 로컬라이제이션이 필요한 경우에만 문자열 키를 사용하세요.**

이렇게 하면 타입 안전성과 개발 편의성을 모두 확보하면서도, InfoHUD 시스템과 중복 생성 방지 기능의 이점을 최대한 활용할 수 있습니다.

### 8.4 현재 프로젝트 적용
현재 프로젝트에서는:
- **InfoHUD 시스템**: AssetReference 사용 (타입 안전성)
- **Canvas 참조**: AssetReference 사용 (고정 참조)
- **로컬라이제이션 UI**: 문자열 키 사용 (동적 로드)
- **일반 UI**: AssetReference 우선, 필요시 문자열 키 사용

---

**버전**: 2.1  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: InfoHUD 시스템, 중복 생성 방지, 로컬라이제이션 지원, 마이그레이션 가이드 개선
