# SafeArea 설정 가이드

## 1. 개요

SafeArea는 모바일 기기에서 노치, 홈 인디케이터, 상태바 등으로 인해 UI가 가려지지 않도록 안전한 영역을 제공합니다.

## 2. SafeArea 시스템 구성

### 2.1 SafeAreaManager
- 전체 SafeArea 계산 및 관리
- 모든 Canvas에 SafeArea 적용
- 디버그 정보 제공

### 2.2 SafeAreaPanel
- 개별 Canvas의 SafeArea 패널
- 자동으로 자식 UI 요소들을 SafeArea에 맞게 조정
- 런타임에 동적으로 크기 조정

## 3. 설정 방법

### 3.1 SafeAreaManager 설정
```csharp
// UIManager에서 자동으로 SafeArea 적용
private void ApplySafeAreaToCanvases()
{
    var safeAreaManager = FindObjectOfType<SafeAreaManager>();
    if (safeAreaManager != null)
    {
        if (hudCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(hudCanvas);
        if (panelCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(panelCanvas);
        if (popupCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(popupCanvas);
        if (loadingCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(loadingCanvas);
    }
}
```

### 3.2 Canvas 프리팹 설정
각 Canvas 프리팹에 SafeAreaPanel을 최상위 자식으로 추가:

```
Canvas (HUDCanvas, PanelCanvas, PopupCanvas, LoadingCanvas)
└── SafeAreaPanel
    └── UI Elements...
```

### 3.3 SafeAreaPanel 컴포넌트 설정
```csharp
[Header("SafeArea Settings")]
[SerializeField] private bool autoResizeChildren = true;
[SerializeField] private bool maintainAspectRatio = true;
```

## 4. 사용 예시

### 4.1 UIManager에서 자동 적용
```csharp
// UIManager의 InitializeAddressableCanvases에서 자동 호출
ApplySafeAreaToCanvases();
```

### 4.2 수동 적용
```csharp
// 특정 Canvas에 SafeArea 적용
SafeAreaManager safeAreaManager = FindObjectOfType<SafeAreaManager>();
if (safeAreaManager != null)
{
    safeAreaManager.ApplySafeAreaToCanvas(myCanvas);
}
```

### 4.3 SafeArea 정보 확인
```csharp
SafeAreaManager safeAreaManager = FindObjectOfType<SafeAreaManager>();
if (safeAreaManager != null)
{
    Rect safeArea = safeAreaManager.GetSafeArea();
    (Vector2 min, Vector2 max) anchors = safeAreaManager.GetSafeAreaAnchors();
    
    Debug.Log($"SafeArea: {safeArea}");
    Debug.Log($"Anchors: Min={anchors.min}, Max={anchors.max}");
}
```

## 5. 디버깅

### 5.1 SafeArea 정보 출력
```csharp
[ContextMenu("Print SafeArea Info")]
public void PrintSafeAreaInfo()
{
    Debug.Log($"Screen Size: {Screen.width} x {Screen.height}");
    Debug.Log($"SafeArea: {Screen.safeArea}");
    Debug.Log($"SafeArea Anchors: Min={anchorMin}, Max={anchorMax}");
}
```

### 5.2 디버그 시각화
```csharp
[SerializeField] private bool showDebugArea = false;
[SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
```

## 6. 주의사항

### 6.1 Canvas 설정
- Canvas의 Render Mode가 Screen Space - Overlay여야 함
- Canvas Scaler 설정 확인

### 6.2 UI 요소 배치
- SafeAreaPanel 내부의 UI 요소들은 자동으로 SafeArea에 맞춰짐
- SetActive(false) 상태에서도 크기 조정이 적용됨

### 6.3 성능 고려사항
- SafeArea 계산은 화면 회전 시에만 수행
- 자식 UI 요소들의 크기 조정은 필요시에만 수행

## 7. 마이그레이션

### 7.1 기존 UI에서 SafeArea 적용
1. Canvas에 SafeAreaPanel 추가
2. 기존 UI 요소들을 SafeAreaPanel 하위로 이동
3. SafeAreaPanel의 autoResizeChildren 활성화

### 7.2 UIManager와 통합
```csharp
// UIManager에서 자동으로 모든 Canvas에 SafeArea 적용
private void ApplySafeAreaToCanvases()
{
    try
    {
        var safeAreaManager = FindObjectOfType<SafeAreaManager>();
        if (safeAreaManager != null)
        {
            if (hudCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(hudCanvas);
            if (panelCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(panelCanvas);
            if (popupCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(popupCanvas);
            if (loadingCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(loadingCanvas);
            Debug.Log("[UIManager] SafeArea 적용 완료");
        }
    }
    catch (System.Exception e)
    {
        Debug.LogWarning($"[UIManager] SafeArea 적용 중 오류: {e.Message}");
    }
}
```

## 8. 결론

SafeArea 시스템을 통해 다양한 모바일 기기에서 일관된 UI 경험을 제공할 수 있습니다. UIManager와 통합하여 자동으로 모든 Canvas에 SafeArea가 적용되도록 구성되어 있습니다.
