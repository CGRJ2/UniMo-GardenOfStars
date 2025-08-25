# SafeArea 설정 가이드

## 📋 개요

SafeArea는 모바일 디바이스에서 노치, 홈 인디케이터, 상태바 등 시스템 UI와 겹치지 않는 안전한 영역을 정의합니다. 이 가이드는 Unity에서 SafeArea를 구현하고 관리하는 방법을 설명합니다. 현재 프로젝트에서는 **InfoHUD 시스템**과 **중복 생성 방지** 기능과 함께 사용됩니다.

## 🎯 주요 기능

- **자동 SafeArea 계산**: 디바이스별 안전 영역 자동 감지
- **UI 자동 조정**: 모든 UI 요소를 안전 영역에 맞게 자동 조정
- **디버그 모드**: 개발 중 SafeArea 시각화
- **플랫폼별 최적화**: iOS, Android 각각에 최적화된 처리
- **InfoHUD 통합**: TouchInfoHUD와 HUDBackdropUI에 SafeArea 자동 적용
- **중복 생성 방지**: UI 요소의 중복 생성 자동 방지

## 🏗️ 시스템 구조

### 1. SafeAreaManager
```csharp
public class SafeAreaManager : MonoBehaviour
{
    [Header("SafeArea Settings")]
    [SerializeField] private bool enableSafeArea = true;
    [SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private bool showDebugArea = false;
    
    // SafeArea 정보
    public static Rect SafeArea { get; private set; }
    public static Vector2 ScreenSize { get; private set; }
}
```

### 2. SafeAreaPanel
```csharp
public class SafeAreaPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;
    
    // SafeArea에 맞게 패널 크기 조정
    public void ApplySafeArea(Rect safeArea, Vector2 screenSize);
}
```

### 3. UIManager 통합
```csharp
// UIManager에서 SafeArea 자동 적용
private void ApplySafeAreaToCanvases()
{
    if (SafeAreaManager.Instance != null && SafeAreaManager.Instance.EnableSafeArea)
    {
        // 모든 Canvas에 SafeArea 적용
        // InfoHUD와 HUDBackdropUI에도 자동 적용
    }
}
```

## ⚙️ 설정 방법

### 1. SafeAreaManager 설정

1. **씬에 SafeAreaManager 추가**
   - 빈 GameObject 생성
   - SafeAreaManager 컴포넌트 추가
   - DontDestroyOnLoad 설정

2. **Inspector 설정**
   ```csharp
   [Header("SafeArea Settings")]
   [SerializeField] private bool enableSafeArea = true;
   [SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
   [SerializeField] private bool showDebugArea = false;
   ```

### 2. Canvas 프리팹 설정

각 Canvas 프리팹에 SafeAreaPanel 자동 추가:

```
Canvas (Canvas)
├── SafeAreaPanel (RectTransform)
│   └── Content (RectTransform)
│       └── UI Elements...
└── Background (RectTransform) - Optional
```

**SafeAreaPanel 설정:**
- **Anchor**: Stretch-Stretch (전체 화면)
- **Pivot**: (0.5, 0.5)
- **Size**: Canvas 크기에 맞춤

### 3. UIManager 설정

```csharp
[Header("SafeArea Settings")]
[SerializeField] private bool enableSafeArea = true;
[SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
[SerializeField] private bool showDebugArea = false;
```

### 4. InfoHUD SafeArea 설정

**TouchInfoHUD와 HUDBackdropUI에 SafeArea 자동 적용:**
```csharp
// TouchInfoHUD 생성 시 SafeArea 자동 적용
private void ApplySafeAreaToInfoHUD()
{
    if (SafeAreaManager.Instance != null && SafeAreaManager.Instance.EnableSafeArea)
    {
        // TouchInfoHUD 위치를 SafeArea 내로 조정
        Vector2 safePosition = GetSafeAreaPosition(screenPosition);
        // HUDBackdropUI도 SafeArea에 맞게 조정
    }
}
```

## 📱 플랫폼별 SafeArea

### 1. iOS SafeArea

**iPhone X 이상:**
- 상단: 노치 영역 제외
- 하단: 홈 인디케이터 영역 제외
- 좌우: 일반적으로 전체 사용 가능

**iPhone SE, 8 등:**
- 상단: 상태바 영역 제외
- 하단: 전체 사용 가능

### 2. Android SafeArea

**노치 디바이스:**
- 상단: 상태바 + 노치 영역 제외
- 하단: 네비게이션 바 영역 제외

**일반 디바이스:**
- 상단: 상태바 영역 제외
- 하단: 전체 사용 가능

## 🔧 구현 세부사항

### 1. SafeArea 계산

```csharp
private void UpdateSafeArea()
{
    SafeArea = Screen.safeArea;
    ScreenSize = new Vector2(Screen.width, Screen.height);
    
    // 디버그 정보 출력
    if (showDebugArea)
    {
        Debug.Log($"SafeArea: {SafeArea}, ScreenSize: {ScreenSize}");
    }
}
```

### 2. UI 조정 로직

```csharp
public void ApplySafeArea(Rect safeArea, Vector2 screenSize)
{
    if (panelRectTransform == null) return;
    
    // SafeArea를 0-1 범위로 정규화
    Vector2 anchorMin = safeArea.position;
    Vector2 anchorMax = safeArea.position + safeArea.size;
    
    anchorMin.x /= screenSize.x;
    anchorMin.y /= screenSize.y;
    anchorMax.x /= screenSize.x;
    anchorMax.y /= screenSize.y;
    
    // RectTransform 설정
    panelRectTransform.anchorMin = anchorMin;
    panelRectTransform.anchorMax = anchorMax;
    panelRectTransform.offsetMin = Vector2.zero;
    panelRectTransform.offsetMax = Vector2.zero;
}
```

### 3. 자동 적용 시스템

```csharp
// UIManager에서 Canvas 생성 시 자동 적용
private void ApplySafeAreaToCanvas(Canvas canvas)
{
    if (!enableSafeArea) return;
    
    SafeAreaPanel safeAreaPanel = canvas.GetComponentInChildren<SafeAreaPanel>();
    if (safeAreaPanel != null)
    {
        safeAreaPanel.ApplySafeArea(SafeAreaManager.SafeArea, SafeAreaManager.ScreenSize);
    }
}

// InfoHUD에 SafeArea 적용
private void ApplySafeAreaToInfoHUD(Vector2 screenPosition)
{
    if (!enableSafeArea) return screenPosition;
    
    Rect safeArea = SafeAreaManager.SafeArea;
    Vector2 screenSize = SafeAreaManager.ScreenSize;
    
    // 화면 위치를 SafeArea 내로 조정
    float x = Mathf.Clamp(screenPosition.x, safeArea.xMin, safeArea.xMax);
    float y = Mathf.Clamp(screenPosition.y, safeArea.yMin, safeArea.yMax);
    
    return new Vector2(x, y);
}
```

## 📖 사용법

### 1. 기본 사용

```csharp
// SafeArea 정보 가져오기
Rect safeArea = SafeAreaManager.SafeArea;
Vector2 screenSize = SafeAreaManager.ScreenSize;

// SafeArea 활성화/비활성화
SafeAreaManager.Instance.EnableSafeArea = true;

// 디버그 모드 활성화
SafeAreaManager.Instance.ShowDebugArea = true;
```

### 2. 수동 SafeArea 적용

```csharp
// 특정 UI에 SafeArea 수동 적용
public class CustomUI : MonoBehaviour
{
    [SerializeField] private RectTransform contentRect;
    
    private void Start()
    {
        ApplySafeArea();
    }
    
    private void ApplySafeArea()
    {
        if (SafeAreaManager.Instance != null)
        {
            SafeAreaPanel safeAreaPanel = GetComponent<SafeAreaPanel>();
            if (safeAreaPanel != null)
            {
                safeAreaPanel.ApplySafeArea(
                    SafeAreaManager.SafeArea, 
                    SafeAreaManager.ScreenSize
                );
            }
        }
    }
}
```

### 3. InfoHUD SafeArea 적용

```csharp
// TouchInfoHUD 표시 시 SafeArea 자동 적용
await TouchInfoHUD.ShowInfoHUD(
    screenPosition: Input.mousePosition, // SafeArea 내로 자동 조정됨
    title: "아이템 정보",
    description: "이 아이템은 매우 강력합니다.",
    icon: itemSprite
);

// HUDBackdropUI도 SafeArea에 맞게 자동 조정
// - 전체 화면을 덮지만 SafeArea 내에서만 활성화
```

### 4. SafeArea 변경 감지

```csharp
// 화면 회전 등으로 SafeArea 변경 시 자동 업데이트
private void OnRectTransformDimensionsChange()
{
    if (SafeAreaManager.Instance != null)
    {
        ApplySafeArea();
    }
}
```

## 🎨 디버그 및 테스트

### 1. 디버그 모드

```csharp
// SafeArea 시각화
[SerializeField] private bool showDebugArea = false;
[SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);

private void OnGUI()
{
    if (showDebugArea)
    {
        // SafeArea 영역을 빨간색으로 표시
        GUI.color = debugColor;
        GUI.Box(SafeArea, "");
        GUI.color = Color.white;
    }
}
```

### 2. Unity 에디터 테스트

**Game View 설정:**
1. **Game View**에서 **Resolution** 설정
2. **iPhone X** 또는 **Android 노치** 해상도 선택
3. **SafeArea** 확인

**Simulator 설정:**
```csharp
#if UNITY_EDITOR
[Header("Editor Testing")]
[SerializeField] private bool simulateNotch = false;
[SerializeField] private Rect simulatedSafeArea = new Rect(0, 100, 375, 812);
#endif
```

### 3. 실제 디바이스 테스트

**iOS:**
- iPhone X 이상 디바이스에서 테스트
- 다양한 방향(세로/가로) 테스트
- 앱 전환 시 SafeArea 변화 확인

**Android:**
- 노치가 있는 디바이스에서 테스트
- 다양한 해상도에서 테스트
- 시스템 UI 숨김/표시 테스트

## 🛠️ 문제 해결

### 1. 일반적인 문제들

**SafeArea가 적용되지 않음:**
```
[SafeAreaManager] SafeArea 적용 실패
```
- **해결**: SafeAreaManager가 씬에 있는지 확인
- **해결**: Canvas에 SafeAreaPanel이 있는지 확인

**UI가 잘림:**
- **해결**: SafeAreaPanel의 Content 영역 확인
- **해결**: UI 요소의 Anchor 설정 확인

**디버그 모드가 작동하지 않음:**
- **해결**: showDebugArea가 true인지 확인
- **해결**: OnGUI 메서드가 호출되는지 확인

### 2. InfoHUD SafeArea 문제

**InfoHUD가 SafeArea 밖에 표시됨:**
```
[TouchInfoHUD] InfoHUD가 SafeArea 밖에 생성됨
```
- **해결**: TouchInfoHUD의 SafeArea 적용 로직 확인
- **해결**: HUDBackdropUI의 SafeArea 설정 확인

**InfoHUD가 잘림:**
- **해결**: InfoHUD 크기가 SafeArea를 초과하지 않는지 확인
- **해결**: InfoHUD의 Anchor 설정 확인

### 3. 플랫폼별 문제

**iOS에서 SafeArea가 잘못 계산됨:**
```csharp
// iOS 전용 SafeArea 계산
#if UNITY_IOS
private Rect GetIOSSafeArea()
{
    Rect safeArea = Screen.safeArea;
    
    // iPhone X 이상 노치 처리
    if (Screen.height >= 812) // iPhone X, XS, XR, 11, 12, 13
    {
        // 추가 노치 보정
    }
    
    return safeArea;
}
#endif
```

**Android에서 상태바 영역 문제:**
```csharp
// Android 전용 SafeArea 계산
#if UNITY_ANDROID
private Rect GetAndroidSafeArea()
{
    Rect safeArea = Screen.safeArea;
    
    // Android 상태바 높이 보정
    int statusBarHeight = GetStatusBarHeight();
    safeArea.y += statusBarHeight;
    safeArea.height -= statusBarHeight;
    
    return safeArea;
}
#endif
```

### 4. 성능 최적화

**SafeArea 계산 최적화:**
```csharp
// 변경이 있을 때만 계산
private Rect lastSafeArea;
private Vector2 lastScreenSize;

private void UpdateSafeArea()
{
    if (Screen.safeArea != lastSafeArea || 
        new Vector2(Screen.width, Screen.height) != lastScreenSize)
    {
        // SafeArea 변경 시에만 업데이트
        ApplySafeArea();
        
        lastSafeArea = Screen.safeArea;
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }
}
```

## 📚 추가 리소스

- [Unity SafeArea 공식 문서](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html)
- [iOS Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/ios/visual-design/adaptivity-and-layout/)
- [Android Notch Guidelines](https://developer.android.com/guide/topics/display-cutout)
- [프로젝트 README.md](./README.md)
- [현재 사용 패턴 가이드](./현재_사용_패턴_가이드.md)

## 🎯 모범 사례

### 1. UI 설계 원칙
- **중앙 정렬**: 중요한 UI는 화면 중앙에 배치
- **여백 확보**: SafeArea 경계에서 충분한 여백 확보
- **반응형 디자인**: 다양한 해상도에 대응

### 2. 코드 구조
- **자동화**: 가능한 한 자동으로 SafeArea 적용
- **확장성**: 새로운 디바이스에 쉽게 대응
- **디버깅**: 개발 중 SafeArea 시각화 지원

### 3. InfoHUD SafeArea 원칙
- **자동 조정**: InfoHUD 위치를 SafeArea 내로 자동 조정
- **Backdrop 통합**: HUDBackdropUI도 SafeArea에 맞게 조정
- **사용자 경험**: SafeArea 밖으로 나가지 않도록 보장

### 4. 테스트 전략
- **다양한 디바이스**: 노치가 있는/없는 디바이스 모두 테스트
- **다양한 방향**: 세로/가로 모드 모두 테스트
- **시스템 UI**: 상태바, 네비게이션 바 변화 테스트
- **InfoHUD 테스트**: 다양한 위치에서 InfoHUD 표시 테스트

---

**버전**: 2.1  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**지원 플랫폼**: iOS, Android  
**주요 업데이트**: InfoHUD 시스템 통합, 중복 생성 방지, 로컬라이제이션 지원
