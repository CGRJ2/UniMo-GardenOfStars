# 포인터 핸들러 사용 가이드

## 📋 개요

`PointerHandler`는 Unity UI 이벤트를 간편하게 처리할 수 있도록 도와주는 컴포넌트입니다. BaseUI에서 제공하는 메서드들을 통해 다양한 포인터 이벤트와 터치 제스처를 쉽게 처리할 수 있습니다.

## 🎯 주요 특징

- **간편한 이벤트 처리**: Unity의 복잡한 이벤트 시스템을 단순화
- **SFX 통합**: 사운드 효과와 함께 이벤트 처리
- **터치 제스처 지원**: 롱프레스, 더블탭, 스와이프, 핀치 등 모바일 제스처
- **멀티터치 지원**: 동시 터치 감지 및 처리
- **터치 피드백**: 시각적 및 촉각적 피드백 제공
- **타입 안전성**: 컴파일 타임에 이벤트 타입 검증
- **메모리 효율성**: 자동 이벤트 정리 및 메모리 누수 방지

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
        Debug.Log("위쪽 스와이프!");
    }
    
    private void OnSwipeDown()
    {
        Debug.Log("아래쪽 스와이프!");
    }
    
    private void OnSwipeLeft()
    {
        Debug.Log("왼쪽 스와이프!");
    }
    
    private void OnSwipeRight()
    {
        Debug.Log("오른쪽 스와이프!");
    }
}
```

### 5. 터치 피드백

```csharp
public class FeedbackButton : BaseUI
{
    private void SetupFeedbackEvents()
    {
        // 터치 피드백이 포함된 이벤트
        GetTouchFeedbackEvent("FeedbackButton", OnClick, enableHaptic: true);
    }
    
    private void OnClick(PointerEventData data)
    {
        Debug.Log("터치 피드백과 함께 클릭!");
    }
}
```

### 6. 게임에서의 활용

```csharp
public class GameUI : BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        SetupGameGestures();
    }
    
    private void SetupGameGestures()
    {
        // 인벤토리 버튼 - 롱프레스로 상세 정보
        GetAdvancedTouchEvent("InventoryButton",
            onClick: OnInventoryOpen,
            onLongPress: OnInventoryDetail,
            onDoubleTap: OnInventorySort
        );

        // 스킬 버튼 - 스와이프로 스킬 변경
        GetDirectionalSwipeEvent("SkillButton",
            onSwipeLeft: OnPreviousSkill,
            onSwipeRight: OnNextSkill
        );

        // 맵 버튼 - 핀치로 줌
        GetPinchEvent("MapButton", OnMapZoom);

        // 게임 영역 - 제스처
        GetAdvancedTouchEvent("GameArea",
            onClick: OnGameClick,
            onLongPress: OnContextMenu,
            onDoubleTap: OnQuickAction
        );
    }
    
    private void OnInventoryOpen(PointerEventData data)
    {
        Debug.Log("인벤토리 열기");
    }
    
    private void OnInventoryDetail(PointerEventData data)
    {
        Debug.Log("인벤토리 상세 정보");
    }
    
    private void OnInventorySort(PointerEventData data)
    {
        Debug.Log("인벤토리 정렬");
    }
    
    private void OnPreviousSkill()
    {
        Debug.Log("이전 스킬 선택");
    }
    
    private void OnNextSkill()
    {
        Debug.Log("다음 스킬 선택");
    }
    
    private void OnMapZoom(float pinchDelta)
    {
        Debug.Log($"맵 줌: {pinchDelta}");
    }
    
    private void OnGameClick(PointerEventData data)
    {
        Debug.Log($"게임 클릭: {data.position}");
    }
    
    private void OnContextMenu(PointerEventData data)
    {
        Debug.Log("컨텍스트 메뉴 표시");
    }
    
    private void OnQuickAction(PointerEventData data)
    {
        Debug.Log("빠른 액션 실행");
    }
}
```

## 🎮 실제 게임 UI 예제

### 1. 게임 메뉴 패널

```csharp
public class GameMenuPanel : BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        SetupMenuEvents();
    }
    
    private void SetupMenuEvents()
    {
        // 메뉴 버튼들
        GetEventWithSFX("StartButton").Click += (data) => OnStartGame();
        GetEventWithSFX("SettingsButton").Click += (data) => OnOpenSettings();
        GetEventWithSFX("ExitButton").Click += (data) => OnExitGame();
        
        // 뒤로가기 버튼
        GetBackEvent("BackButton").Click += (data) => OnBackClicked();
        
        // 호버 효과가 있는 버튼들
        SetupHoverEffects();
    }
    
    private void SetupHoverEffects()
    {
        string[] buttonNames = { "StartButton", "SettingsButton", "ExitButton" };
        
        foreach (string buttonName in buttonNames)
        {
            GetEvent(buttonName).Enter += (data) => OnButtonHover(buttonName);
            GetEvent(buttonName).Exit += (data) => OnButtonExit(buttonName);
        }
    }
    
    private void OnStartGame()
    {
        Debug.Log("게임 시작!");
        // 게임 시작 로직
    }
    
    private void OnOpenSettings()
    {
        Debug.Log("설정 열기!");
        // 설정 패널 열기
    }
    
    private void OnExitGame()
    {
        Debug.Log("게임 종료!");
        // 게임 종료 로직
    }
    
    private void OnBackClicked()
    {
        Debug.Log("뒤로가기!");
        Hide();
    }
    
    private void OnButtonHover(string buttonName)
    {
        // 호버 효과
        GetUI<Image>(buttonName).color = Color.yellow;
    }
    
    private void OnButtonExit(string buttonName)
    {
        // 호버 효과 제거
        GetUI<Image>(buttonName).color = Color.white;
    }
}
```

### 2. 인벤토리 아이템

```csharp
public class InventoryItem : BaseUI
{
    private void SetupItemEvents()
    {
        var itemHandler = GetEvent("ItemButton");
        
        // 클릭으로 아이템 선택
        itemHandler.Click += (data) => OnItemSelected();
        
        // 호버로 아이템 정보 표시
        itemHandler.Enter += (data) => OnItemHover();
        itemHandler.Exit += (data) => OnItemExit();
        
        // 드래그로 아이템 이동
        itemHandler.BeginDrag += (data) => OnBeginDragItem(data);
        itemHandler.Drag += (data) => OnDragItem(data);
        itemHandler.EndDrag += (data) => OnEndDragItem(data);
    }
    
    private void OnItemSelected()
    {
        Debug.Log("아이템 선택됨!");
        // 아이템 선택 로직
    }
    
    private void OnItemHover()
    {
        Debug.Log("아이템 호버!");
        // 툴팁 표시
    }
    
    private void OnItemExit()
    {
        Debug.Log("아이템 호버 해제!");
        // 툴팁 숨기기
    }
    
    private void OnBeginDragItem(PointerEventData data)
    {
        Debug.Log("아이템 드래그 시작!");
        // 드래그 시작 로직
    }
    
    private void OnDragItem(PointerEventData data)
    {
        // 드래그 중 로직
    }
    
    private void OnEndDragItem(PointerEventData data)
    {
        Debug.Log("아이템 드래그 종료!");
        // 드래그 종료 로직
    }
}
```

## ⚠️ 주의사항 및 모범 사례

### 1. 이벤트 정리

```csharp
protected override void OnDestroy()
{
    base.OnDestroy();
    
    // 이벤트 정리
    var buttonHandler = GetEvent("Button");
    if (buttonHandler != null)
    {
        buttonHandler.Click -= OnButtonClicked;
        buttonHandler.Enter -= OnButtonEnter;
        buttonHandler.Exit -= OnButtonExit;
        buttonHandler.LongPress -= OnLongPress;
        buttonHandler.DoubleTap -= OnDoubleTap;
    }
}
```

### 2. 성능 최적화

```csharp
public class OptimizedUI : BaseUI
{
    private PointerHandler cachedHandler;
    
    private void SetupOptimizedEvents()
    {
        // 핸들러 캐싱으로 성능 향상
        cachedHandler = GetEvent("Button");
        cachedHandler.Click += OnButtonClicked;
        cachedHandler.LongPress += OnLongPress;
    }
    
    private void OnButtonClicked(PointerEventData data)
    {
        // 최적화된 클릭 처리
    }
    
    private void OnLongPress(PointerEventData data)
    {
        // 최적화된 롱프레스 처리
    }
}
```

### 3. 메모리 누수 방지

```csharp
public class SafeUI : BaseUI
{
    private void SetupSafeEvents()
    {
        // 람다 대신 메서드 참조 사용
        GetEvent("Button").Click += OnButtonClicked;
        
        // 클로저 사용 시 주의
        string buttonName = "Button";
        GetEvent(buttonName).Click += (data) => OnButtonClickedWithName(buttonName);
    }
    
    private void OnButtonClicked(PointerEventData data)
    {
        Debug.Log("버튼 클릭!");
    }
    
    private void OnButtonClickedWithName(string name)
    {
        Debug.Log($"{name} 버튼 클릭!");
    }
}
```

### 4. 에러 처리

```csharp
public class RobustUI : BaseUI
{
    private void SetupRobustEvents()
    {
        try
        {
            var handler = GetEvent("Button");
            if (handler != null)
            {
                handler.Click += OnButtonClicked;
                handler.LongPress += OnLongPress;
            }
            else
            {
                Debug.LogWarning("Button 핸들러를 찾을 수 없습니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이벤트 설정 중 오류: {e.Message}");
        }
    }
    
    private void OnButtonClicked(PointerEventData data)
    {
        try
        {
            Debug.Log("버튼 클릭!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"버튼 클릭 처리 중 오류: {e.Message}");
        }
    }
    
    private void OnLongPress(PointerEventData data)
    {
        try
        {
            Debug.Log("롱프레스!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"롱프레스 처리 중 오류: {e.Message}");
        }
    }
}
```

## 🔍 디버깅 및 문제 해결

### 1. 이벤트 디버깅

```csharp
public class DebugUI : BaseUI
{
    private void SetupDebugEvents()
    {
        var handler = GetEvent("Button");
        
        // 모든 이벤트에 디버그 로그 추가
        handler.Click += (data) => Debug.Log($"Click: {data.position}");
        handler.Enter += (data) => Debug.Log($"Enter: {data.position}");
        handler.Exit += (data) => Debug.Log($"Exit: {data.position}");
        handler.Down += (data) => Debug.Log($"Down: {data.position}");
        handler.Up += (data) => Debug.Log($"Up: {data.position}");
        handler.LongPress += (data) => Debug.Log($"LongPress: {data.position}");
        handler.DoubleTap += (data) => Debug.Log($"DoubleTap: {data.position}");
        handler.Swipe += (direction) => Debug.Log($"Swipe: {direction}");
        handler.Pinch += (delta) => Debug.Log($"Pinch: {delta}");
    }
}
```

### 2. 일반적인 문제들

**이벤트가 발생하지 않는 경우:**
- UI 요소에 `PointerHandler` 컴포넌트가 있는지 확인
- UI 요소가 활성화되어 있는지 확인
- UI 요소의 `Raycast Target`이 활성화되어 있는지 확인
- UI 요소가 다른 UI 요소에 가려져 있지 않은지 확인

**터치 제스처가 감지되지 않는 경우:**
- `enableGestureDetection`이 활성화되어 있는지 확인
- `enableTouchFeedback`이 활성화되어 있는지 확인
- 제스처 임계값 설정이 적절한지 확인

**SFX가 재생되지 않는 경우:**
- 사운드 파일이 올바른 경로에 있는지 확인
- AudioManager가 초기화되어 있는지 확인
- 사운드 이름이 올바른지 확인

**메모리 누수가 발생하는 경우:**
- UI 파괴 시 이벤트를 정리했는지 확인
- 람다 표현식에서 클로저를 사용하지 않았는지 확인
- 이벤트 핸들러가 중복 등록되지 않았는지 확인

## 📚 추가 리소스

- [Unity UI 이벤트 시스템](https://docs.unity3d.com/Manual/EventSystem.html)
- [Unity 터치 입력](https://docs.unity3d.com/Manual/MobileInput.html)
- [BaseUI 클래스 문서](./README.md)
- [PointerHandler 소스 코드](./PointerHandler.cs)
- [터치 제스처 예제](./Examples/TouchGestureExamples.cs)

---

**버전**: 2.0  
**최종 업데이트**: 2024년  
**Unity 버전**: 2022.3 LTS 이상
