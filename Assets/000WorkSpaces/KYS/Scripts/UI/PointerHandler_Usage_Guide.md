# 포인터 핸들러 사용 가이드

## 📋 개요

`PointerHandler`는 Unity UI 이벤트를 간편하게 처리할 수 있도록 도와주는 컴포넌트입니다. BaseUI에서 제공하는 메서드들을 통해 다양한 포인터 이벤트와 터치 제스처를 쉽게 처리할 수 있습니다. 현재 프로젝트에서는 **InfoHUD 시스템**과 **중복 생성 방지** 기능과 함께 사용됩니다.

## 🎯 주요 특징

- **간편한 이벤트 처리**: Unity의 복잡한 이벤트 시스템을 단순화
- **SFX 통합**: 사운드 효과와 함께 이벤트 처리
- **터치 제스처 지원**: 롱프레스, 더블탭, 스와이프, 핀치 등 모바일 제스처
- **멀티터치 지원**: 동시 터치 감지 및 처리
- **터치 피드백**: 시각적 및 촉각적 피드백 제공
- **타입 안전성**: 컴파일 타임에 이벤트 타입 검증
- **메모리 효율성**: 자동 이벤트 정리 및 메모리 누수 방지
- **InfoHUD 통합**: TouchInfoManager와 연동하여 InfoHUD 제어
- **중복 생성 방지**: UI 요소의 중복 생성 자동 방지

## 🔧 기본 사용법

### 1. 포인터 핸들러 컴포넌트

```csharp
// PointerHandler는 다음 인터페이스들을 구현합니다:
public class PointerHandler : MonoBehaviour,
    IEventSystemHandler,
    IPointerClickHandler,
    IPointerUpHandler,
    IPointerDownHandler,
    IPointerMoveHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // 기본 이벤트들
    public event Action<PointerEventData> Click;
    public event Action<PointerEventData> Up;
    public event Action<PointerEventData> Down;
    public event Action<PointerEventData> Move;
    public event Action<PointerEventData> Enter;
    public event Action<PointerEventData> Exit;
    public event Action<PointerEventData> BeginDrag;
    public event Action<PointerEventData> Drag;
    public event Action<PointerEventData> EndDrag;

    // 터치 특화 이벤트들
    public event Action<PointerEventData> LongPress;
    public event Action<PointerEventData> DoubleTap;
    public event Action<PointerEventData> TouchStart;
    public event Action<PointerEventData> TouchEnd;
    public event Action<Vector2> Swipe;
    public event Action<float> Pinch;
    public event Action<Vector2> TouchMove;
}
```

### 2. BaseUI에서 제공하는 메서드들

```csharp
// 기본 이벤트 처리
GetEvent(string name)                    // 특정 UI 요소의 이벤트 핸들러
GetSelfEvent()                          // UI 자체의 이벤트 핸들러

// SFX가 포함된 이벤트 처리
GetEventWithSFX(string name, string soundName = null)  // 클릭 사운드와 함께
GetBackEvent(string name, string soundName = null)     // 뒤로가기 사운드와 함께

// 터치 제스처 이벤트 처리
GetLongPressEvent(string name, Action<PointerEventData> onLongPress)     // 롱프레스
GetDoubleTapEvent(string name, Action<PointerEventData> onDoubleTap)     // 더블탭
GetSwipeEvent(string name, Action<Vector2> onSwipe)                      // 스와이프
GetPinchEvent(string name, Action<float> onPinch)                        // 핀치
GetTouchStartEvent(string name, Action<PointerEventData> onTouchStart)   // 터치 시작
GetTouchEndEvent(string name, Action<PointerEventData> onTouchEnd)       // 터치 종료
GetTouchMoveEvent(string name, Action<Vector2> onTouchMove)              // 터치 이동

// 고급 터치 이벤트
GetAdvancedTouchEvent(string name, onClick, onLongPress, onDoubleTap)    // 복합 터치
GetDirectionalSwipeEvent(string name, onSwipeUp, onSwipeDown, onSwipeLeft, onSwipeRight)  // 방향별 스와이프
GetTouchFeedbackEvent(string name, onClick, enableHaptic)                // 터치 피드백
```

## 📖 사용 예제

### 1. 기본 클릭 이벤트

```csharp
public class SimpleButton : BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        SetupEvents();
    }
    
    private void SetupEvents()
    {
        // 기본 클릭 이벤트
        GetEvent("Button").Click += (data) => OnButtonClicked();
    }
    
    private void OnButtonClicked()
    {
        Debug.Log("버튼이 클릭되었습니다!");
    }
}
```

### 2. 터치 제스처 이벤트

```csharp
public class GestureButton : BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        SetupGestureEvents();
    }
    
    private void SetupGestureEvents()
    {
        // 롱프레스 이벤트
        GetLongPressEvent("GestureButton", OnLongPress);
        
        // 더블탭 이벤트
        GetDoubleTapEvent("GestureButton", OnDoubleTap);
        
        // 스와이프 이벤트
        GetSwipeEvent("GestureButton", OnSwipe);
        
        // 핀치 이벤트
        GetPinchEvent("GestureButton", OnPinch);
    }
    
    private void OnLongPress(PointerEventData data)
    {
        Debug.Log("롱프레스 감지!");
        // 롱프레스 처리 로직
    }
    
    private void OnDoubleTap(PointerEventData data)
    {
        Debug.Log("더블탭 감지!");
        // 더블탭 처리 로직
    }
    
    private void OnSwipe(Vector2 direction)
    {
        Debug.Log($"스와이프 감지: {direction}");
        // 스와이프 처리 로직
    }
    
    private void OnPinch(float pinchDelta)
    {
        Debug.Log($"핀치 감지: {pinchDelta}");
        // 핀치 처리 로직
    }
}
```

### 3. 복합 터치 이벤트

```csharp
public class AdvancedButton : BaseUI
{
    private void SetupAdvancedEvents()
    {
        // 하나의 버튼에 여러 제스처 적용
        GetAdvancedTouchEvent("AdvancedButton",
            onClick: OnClick,
            onLongPress: OnLongPress,
            onDoubleTap: OnDoubleTap
        );
    }
    
    private void OnClick(PointerEventData data)
    {
        Debug.Log("클릭!");
    }
    
    private void OnLongPress(PointerEventData data)
    {
        Debug.Log("롱프레스!");
    }
    
    private void OnDoubleTap(PointerEventData data)
    {
        Debug.Log("더블탭!");
    }
}
```

### 4. 방향별 스와이프

```csharp
public class SwipeButton : BaseUI
{
    private void SetupSwipeEvents()
    {
        // 방향별 스와이프 처리
        GetDirectionalSwipeEvent("SwipeButton",
            onSwipeUp: OnSwipeUp,
            onSwipeDown: OnSwipeDown,
            onSwipeLeft: OnSwipeLeft,
            onSwipeRight: OnSwipeRight
        );
    }
    
    private void OnSwipeUp()
    {
        Debug.Log("위로 스와이프!");
    }
    
    private void OnSwipeDown()
    {
        Debug.Log("아래로 스와이프!");
    }
    
    private void OnSwipeLeft()
    {
        Debug.Log("왼쪽으로 스와이프!");
    }
    
    private void OnSwipeRight()
    {
        Debug.Log("오른쪽으로 스와이프!");
    }
}
```

### 5. InfoHUD와 연동된 터치 이벤트

```csharp
public class InfoHUDButton : BaseUI
{
    private void SetupInfoHUDEvents()
    {
        // InfoHUD 표시를 위한 터치 이벤트
        GetEvent("InfoButton").Click += (data) => OnInfoButtonClicked(data);
        
        // 롱프레스로 상세 정보 표시
        GetLongPressEvent("InfoButton", OnInfoButtonLongPress);
    }
    
    private void OnInfoButtonClicked(PointerEventData data)
    {
        // 기본 정보 표시
        _ = TouchInfoHUD.ShowInfoHUD(
            screenPosition: data.position,
            title: "기본 정보",
            description: "이것은 기본 정보입니다.",
            icon: null
        );
    }
    
    private void OnInfoButtonLongPress(PointerEventData data)
    {
        // 상세 정보 표시
        _ = TouchInfoHUD.ShowInfoHUD(
            screenPosition: data.position,
            title: "상세 정보",
            description: "이것은 상세한 정보입니다. 롱프레스로 표시됩니다.",
            icon: null
        );
    }
}
```

### 6. 중복 생성 방지가 포함된 이벤트

```csharp
public class SafeButton : BaseUI
{
    private bool isProcessing = false;
    
    private void SetupSafeEvents()
    {
        // 중복 클릭 방지가 포함된 이벤트
        GetEvent("SafeButton").Click += (data) => OnSafeButtonClicked();
    }
    
    private async void OnSafeButtonClicked()
    {
        if (isProcessing) return; // 중복 실행 방지
        
        isProcessing = true;
        
        try
        {
            // UI 생성 (중복 생성 방지 포함)
            await UIManager.Instance.ShowPopUpAsync<MessagePopup>((popup) => {
                if (popup != null)
                {
                    popup.SetMessage("안전한 버튼 클릭!");
                }
            });
        }
        finally
        {
            isProcessing = false;
        }
    }
}
```

## 🎮 InfoHUD 시스템과의 통합

### 1. TouchInfoManager와의 연동

```csharp
// TouchInfoManager에서 PointerHandler 이벤트 활용
public class TouchInfoManager : MonoBehaviour
{
    private void ProcessTouch(Vector2 screenPosition)
    {
        // UI 요소 클릭 확인
        if (IsPointerOverUI(screenPosition))
        {
            // UI 요소 클릭 시 InfoHUD 닫기
            CloseExistingTouchInfoHUD();
            return;
        }
        
        // UI가 아닌 오브젝트 클릭 시 InfoHUD 표시
        GameObject hitObject = GetObjectAtPosition(screenPosition);
        if (hitObject != null)
        {
            ShowInfoForObject(hitObject, screenPosition);
        }
    }
}
```

### 2. HUDBackdropUI와의 연동

```csharp
// HUDBackdropUI에서 PointerHandler 사용
public class HUDBackdropUI : MonoBehaviour
{
    private void SetupBackdropEvents()
    {
        // Backdrop 클릭 시 InfoHUD 닫기
        GetEvent("Backdrop").Click += (data) => OnBackdropClicked();
    }
    
    private void OnBackdropClicked()
    {
        // InfoHUD 닫기
        UIManager.Instance.DestroyAllInfoHUDs();
    }
}
```

## 🔧 고급 사용법

### 1. 커스텀 터치 제스처

```csharp
public class CustomGestureHandler : BaseUI
{
    private Vector2 startPosition;
    private float startTime;
    
    private void SetupCustomGestures()
    {
        // 터치 시작
        GetTouchStartEvent("CustomArea", OnTouchStart);
        
        // 터치 종료
        GetTouchEndEvent("CustomArea", OnTouchEnd);
        
        // 터치 이동
        GetTouchMoveEvent("CustomArea", OnTouchMove);
    }
    
    private void OnTouchStart(PointerEventData data)
    {
        startPosition = data.position;
        startTime = Time.time;
    }
    
    private void OnTouchEnd(PointerEventData data)
    {
        float duration = Time.time - startTime;
        float distance = Vector2.Distance(startPosition, data.position);
        
        // 커스텀 제스처 판정
        if (duration < 0.5f && distance < 50f)
        {
            OnQuickTap(data);
        }
        else if (duration > 1.0f && distance < 30f)
        {
            OnLongHold(data);
        }
    }
    
    private void OnTouchMove(Vector2 delta)
    {
        // 터치 이동 처리
    }
    
    private void OnQuickTap(PointerEventData data)
    {
        Debug.Log("빠른 탭!");
    }
    
    private void OnLongHold(PointerEventData data)
    {
        Debug.Log("길게 홀드!");
    }
}
```

### 2. 멀티터치 처리

```csharp
public class MultiTouchHandler : BaseUI
{
    private Dictionary<int, Vector2> touchPositions = new Dictionary<int, Vector2>();
    
    private void SetupMultiTouchEvents()
    {
        // 멀티터치 이벤트 설정
        GetTouchStartEvent("MultiTouchArea", OnMultiTouchStart);
        GetTouchEndEvent("MultiTouchArea", OnMultiTouchEnd);
        GetTouchMoveEvent("MultiTouchArea", OnMultiTouchMove);
    }
    
    private void OnMultiTouchStart(PointerEventData data)
    {
        touchPositions[data.pointerId] = data.position;
        
        if (touchPositions.Count == 2)
        {
            OnTwoFingerTouch();
        }
    }
    
    private void OnMultiTouchEnd(PointerEventData data)
    {
        touchPositions.Remove(data.pointerId);
    }
    
    private void OnMultiTouchMove(Vector2 delta)
    {
        if (touchPositions.Count == 2)
        {
            // 두 손가락 제스처 처리
            ProcessTwoFingerGesture();
        }
    }
    
    private void OnTwoFingerTouch()
    {
        Debug.Log("두 손가락 터치!");
    }
    
    private void ProcessTwoFingerGesture()
    {
        // 핀치, 회전 등 처리
    }
}
```

## 🛠️ 문제 해결

### 1. 일반적인 문제들

**이벤트가 발생하지 않음:**
```
[PointerHandler] 이벤트가 발생하지 않음
```
- **해결**: UI 요소에 PointerHandler 컴포넌트가 있는지 확인
- **해결**: UI 요소의 Raycast Target이 활성화되어 있는지 확인

**터치 제스처가 인식되지 않음:**
- **해결**: 제스처 설정값 확인 (시간, 거리 등)
- **해결**: 터치 영역이 충분한지 확인

**InfoHUD와 충돌:**
```
[TouchInfoManager] InfoHUD와 UI 이벤트 충돌
```
- **해결**: TouchInfoManager의 UI 감지 로직 확인
- **해결**: InfoHUD 영역에서 UI 이벤트 처리 확인

### 2. 성능 최적화

**이벤트 최적화:**
```csharp
// 불필요한 이벤트 제거
private void OnDestroy()
{
    // 이벤트 정리
    if (pointerHandler != null)
    {
        pointerHandler.Click -= OnClick;
        pointerHandler.LongPress -= OnLongPress;
    }
}
```

**터치 감지 최적화:**
```csharp
// 터치 감지 영역 최적화
[SerializeField] private float touchThreshold = 0.1f;
[SerializeField] private float swipeThreshold = 50f;
```

### 3. 디버깅

**터치 이벤트 디버깅:**
```csharp
// 터치 이벤트 로깅
private void OnTouchStart(PointerEventData data)
{
    Debug.Log($"터치 시작: {data.position}, ID: {data.pointerId}");
}

private void OnTouchEnd(PointerEventData data)
{
    Debug.Log($"터치 종료: {data.position}, ID: {data.pointerId}");
}
```

## 📚 추가 리소스

- [Unity UI Event System](https://docs.unity3d.com/Manual/EventSystem.html)
- [Unity Touch Input](https://docs.unity3d.com/Manual/MobileInput.html)
- [프로젝트 README.md](./README.md)
- [현재 사용 패턴 가이드](./현재_사용_패턴_가이드.md)

## 🎯 모범 사례

### 1. 이벤트 처리 원칙
- **단일 책임**: 하나의 이벤트 핸들러는 하나의 기능만 처리
- **메모리 관리**: 이벤트 구독 해제로 메모리 누수 방지
- **성능 고려**: 불필요한 이벤트 처리 방지

### 2. 터치 제스처 원칙
- **사용자 친화적**: 직관적이고 예측 가능한 제스처
- **반응성**: 빠른 반응과 적절한 피드백
- **접근성**: 다양한 사용자가 사용할 수 있도록

### 3. InfoHUD 통합 원칙
- **충돌 방지**: InfoHUD와 UI 이벤트 간 충돌 방지
- **일관성**: InfoHUD 표시/숨김 로직의 일관성
- **사용자 경험**: 자연스러운 InfoHUD 전환

### 4. 중복 생성 방지 원칙
- **플래그 관리**: 중복 실행 방지를 위한 플래그 사용
- **비동기 처리**: async/await를 활용한 안전한 비동기 처리
- **에러 처리**: try-catch를 통한 안전한 에러 처리

---

**버전**: 2.1  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: InfoHUD 시스템 통합, 중복 생성 방지, 터치 제스처 개선, 메모리 관리 최적화
