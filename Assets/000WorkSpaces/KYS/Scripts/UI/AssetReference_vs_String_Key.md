# AssetReference vs 문자열 키 비교

## 1. 개요

Unity Addressables에서는 두 가지 방식으로 에셋을 참조할 수 있습니다:
1. **문자열 키 (String Key)**: 런타임에 동적으로 에셋을 로드
2. **AssetReference**: 컴파일 타임에 타입 안전성을 보장하는 참조

## 2. AssetReference (권장)

### 2.1 장점
- **타입 안전성**: 컴파일 타임에 타입 오류 감지
- **IntelliSense 지원**: 자동완성 및 리팩토링 지원
- **에디터 통합**: Unity 에디터에서 드래그 앤 드롭으로 설정
- **참조 추적**: 에셋이 삭제되거나 이동될 때 자동으로 참조 업데이트
- **런타임 키 검증**: `RuntimeKeyIsValid()` 메서드로 유효성 확인

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

## 3. 문자열 키 (String Key)

### 3.1 장점
- **동적 로드**: 런타임에 키를 동적으로 결정 가능
- **유연성**: 데이터 기반으로 에셋 로드
- **간단함**: 문자열만으로 에셋 접근

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

## 5. 권장 사용 패턴

### 5.1 고정된 UI (AssetReference 권장)
- 메인 메뉴, 설정 패널 등 항상 존재하는 UI
- Canvas 참조
- 자주 사용되는 UI 프리팹

```csharp
[SerializeField] private AssetReferenceGameObject mainMenuReference;
[SerializeField] private AssetReferenceGameObject settingsReference;
```

### 5.2 동적 UI (문자열 키 사용)
- 사용자 생성 콘텐츠
- 데이터 기반 UI
- 런타임에 결정되는 UI

```csharp
public async void LoadDynamicUI(string uiType, string uiName)
{
    string key = $"UI/{uiType}/{uiName}";
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
    
    [Header("동적 UI (문자열 키)")]
    [SerializeField] private string dynamicUIPrefix = "UI/Dynamic/";
    
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
}
```

## 6. 성능 비교

### 6.1 AssetReference
- **컴파일 타임**: 타입 검증으로 안전성 확보
- **런타임**: 약간의 오버헤드 (타입 검증)
- **메모리**: 참조 객체 크기만큼 추가 메모리

### 6.2 문자열 키
- **컴파일 타임**: 검증 없음
- **런타임**: 문자열 처리 오버헤드
- **메모리**: 문자열 크기만큼 메모리

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
2. **동적 UI는 유지**: 런타임에 결정되는 UI는 문자열 키 유지
3. **하이브리드 사용**: 두 방식을 적절히 혼합

## 8. 결론

### 8.1 AssetReference 사용 권장 상황
- 고정된 UI 프리팹
- Canvas 참조
- 자주 사용되는 에셋
- 타입 안전성이 중요한 경우

### 8.2 문자열 키 사용 권장 상황
- 동적으로 결정되는 UI
- 데이터 기반 콘텐츠
- 런타임에 키가 결정되는 경우

### 8.3 최종 권장사항
**AssetReference를 기본으로 사용하고, 동적 로드가 필요한 경우에만 문자열 키를 사용하세요.**

이렇게 하면 타입 안전성과 개발 편의성을 모두 확보할 수 있습니다.
