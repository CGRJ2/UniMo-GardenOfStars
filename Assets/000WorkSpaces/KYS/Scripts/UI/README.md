# KYS UI 시스템 가이드

## 📋 목차
1. [시스템 개요](#시스템-개요)
2. [아키텍처](#아키텍처)
3. [핵심 클래스](#핵심-클래스)
4. [사용법](#사용법)
5. [포인터 핸들러 사용법](#포인터-핸들러-사용법)
6. [Addressable 설정](#addressable-설정)
7. [SafeArea 설정](#safearea-설정)
8. [예제](#예제)
9. [마이그레이션 가이드](#마이그레이션-가이드)
10. [문제 해결](#문제-해결)

## 🎯 시스템 개요

KYS UI 시스템은 Unity에서 효율적이고 확장 가능한 UI 관리를 위한 종합적인 솔루션입니다.

### 주요 특징
- **Addressable 기반**: 동적 로딩과 메모리 효율성
- **MVP 패턴**: 깔끔한 코드 구조와 유지보수성
- **레이어 시스템**: HUD, Panel, Popup, Loading 분리
- **Stack 관리**: UI 표시 순서 자동 관리
- **SafeArea 지원**: 모바일 디바이스 최적화
- **다국어 지원**: Localization 시스템 통합

## 🏗️ 아키텍처

### UI 레이어 구조
```
Loading (최상위)
├── Popup
├── Panel
└── HUD (최하위)
```

### MVP 패턴
```
Model (데이터) ←→ Presenter (로직) ←→ View (UI)
```

### Addressable 구조
```
Assets/000WorkSpaces/KYS/Prefabs/UI/
├── Canvas/
│   ├── HUDCanvas.prefab
│   ├── PanelCanvas.prefab
│   ├── PopupCanvas.prefab
│   └── LoadingCanvas.prefab
├── HUD/
├── Panel/
├── Popup/
└── Loading/
```

## 🔧 핵심 클래스

### UIManager
```csharp
// 싱글톤 패턴으로 구현된 메인 UI 관리자
public class UIManager : Singleton<UIManager>
{
    // Addressable 기반 UI 로드
    public async Task<T> LoadUIAsync<T>(string addressableKey);
    
    // 패널/팝업 관리
    public void OpenPanel(BaseUI panel);
    public void ClosePanel();
    public void OpenPopup(BaseUI popup);
    public void ClosePopup();
}
```

### BaseUI
```csharp
// 모든 UI의 기본 클래스
public class BaseUI : MonoBehaviour, IUIView
{
    [SerializeField] protected UILayerType layerType;
    [SerializeField] protected bool canCloseWithESC = true;
    
    // UI 요소 접근
    public T GetUI<T>(string name) where T : Component;
    public GameObject GetUI(string name);
}
```

### CheckPopUp
```csharp
// 확인 팝업 전용 클래스
public class CheckPopUp : BaseUI
{
    public void SetMessage(string message);
    public void SetConfirmCallback(Action callback);
    public void SetCancelCallback(Action callback);
}
```

## 📖 사용법

### 1. 기본 UI 로드 및 표시

```csharp
// Addressable 키로 UI 로드
BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");
UIManager.Instance.OpenPanel(mainMenu);

// AssetReference로 UI 로드
BaseUI settings = await UIManager.Instance.LoadUIAsync<BaseUI>(settingsReference);
UIManager.Instance.OpenPanel(settings);
```

### 2. 팝업 표시

```csharp
// 제네릭 팝업
UIManager.Instance.ShowPopUpAsync<MessagePopup>((popup) => {
    if (popup != null) {
        popup.SetMessage("메시지입니다.");
    }
});

// 확인 팝업
UIManager.Instance.ShowConfirmPopUpAsync(
    "정말 삭제하시겠습니까?",
    "확인",
    "취소",
    () => Debug.Log("확인됨"),
    () => Debug.Log("취소됨")
);
```

### 3. BaseUI 상속하여 커스텀 UI 만들기

```csharp
public class CustomPanel : BaseUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;
    
    protected override void Awake()
    {
        base.Awake();
        SetupUI();
    }
    
    private void SetupUI()
    {
        // UI 요소 설정
        closeButton.onClick.AddListener(() => Hide());
    }
    
    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }
}
```

### 4. MVP 패턴 사용

```csharp
// Model
public class GameDataModel : BaseUIModel
{
    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;
    
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }
}

// Presenter
public class GameUIPresenter : BaseUIPresenter
{
    private GameUI view;
    private GameDataModel model;
    
    protected override void OnInitialize()
    {
        view = GetView<GameUI>();
        model = GetModel<GameDataModel>();
        
        model.OnScoreChanged += OnScoreChanged;
    }
    
    private void OnScoreChanged(int newScore)
    {
        view.UpdateScore(newScore);
    }
}

// View
public class GameUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI scoreText;
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}
```

## 🖱️ 포인터 핸들러 사용법

### 1. 포인터 핸들러 개요

`PointerHandler`는 Unity UI 이벤트를 간편하게 처리할 수 있도록 도와주는 컴포넌트입니다. BaseUI에서 제공하는 메서드들을 통해 다양한 포인터 이벤트를 쉽게 처리할 수 있습니다.

### 2. 기본 사용법

```csharp
public class MyUI : BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        SetupPointerEvents();
    }
    
    private void SetupPointerEvents()
    {
        // 클릭 이벤트
        GetEvent("Button").Click += (data) => OnButtonClicked();
        
        // 드래그 이벤트
        GetEvent("DraggablePanel").BeginDrag += (data) => OnBeginDrag(data);
        GetEvent("DraggablePanel").Drag += (data) => OnDrag(data);
        GetEvent("DraggablePanel").EndDrag += (data) => OnEndDrag(data);
        
        // 호버 이벤트
        GetEvent("HoverButton").Enter += (data) => OnButtonHover();
        GetEvent("HoverButton").Exit += (data) => OnButtonExit();
    }
    
    private void OnButtonClicked()
    {
        Debug.Log("버튼이 클릭되었습니다!");
    }
    
    private void OnBeginDrag(PointerEventData data)
    {
        Debug.Log("드래그 시작");
    }
    
    private void OnDrag(PointerEventData data)
    {
        // 드래그 중 처리
        transform.position += (Vector3)data.delta;
    }
    
    private void OnEndDrag(PointerEventData data)
    {
        Debug.Log("드래그 종료");
    }
    
    private void OnButtonHover()
    {
        Debug.Log("버튼에 마우스가 올라갔습니다");
    }
    
    private void OnButtonExit()
    {
        Debug.Log("마우스가 버튼에서 벗어났습니다");
    }
}
```

### 3. SFX가 포함된 이벤트 처리

```csharp
private void SetupSFXEvents()
{
    // 클릭 사운드와 함께 이벤트 처리
    GetEventWithSFX("ConfirmButton", "SFX_ButtonClick").Click += (data) => OnConfirmClicked();
    
    // 뒤로가기 사운드와 함께 이벤트 처리
    GetBackEvent("BackButton", "SFX_ButtonBack").Click += (data) => OnBackClicked();
    
    // 기본 사운드 사용
    GetEventWithSFX("MenuButton").Click += (data) => OnMenuClicked();
}
```

### 4. 자체 이벤트 처리

```csharp
private void SetupSelfEvents()
{
    // UI 자체에 이벤트 처리
    GetSelfEvent().Click += (data) => OnUIClicked();
    GetSelfEvent().Enter += (data) => OnUIHover();
    GetSelfEvent().Exit += (data) => OnUIExit();
}

private void OnUIClicked()
{
    Debug.Log("UI 자체가 클릭되었습니다");
}

private void OnUIHover()
{
    Debug.Log("UI에 마우스가 올라갔습니다");
}

private void OnUIExit()
{
    Debug.Log("마우스가 UI에서 벗어났습니다");
}
```

### 5. 동적 UI 요소에 이벤트 추가

```csharp
public class DynamicUIExample : BaseUI
{
    private void CreateDynamicButton()
    {
        // 동적으로 버튼 생성
        GameObject button = new GameObject("DynamicButton");
        button.transform.SetParent(transform);
        
        // UI 요소로 등록
        AddUIToDictionary(button);
        
        // 이벤트 추가
        GetEvent("DynamicButton").Click += (data) => OnDynamicButtonClicked();
    }
    
    private void OnDynamicButtonClicked()
    {
        Debug.Log("동적 버튼이 클릭되었습니다!");
    }
}
```

### 6. 고급 이벤트 처리

```csharp
public class AdvancedUIExample : BaseUI
{
    private void SetupAdvancedEvents()
    {
        // 여러 이벤트를 한 번에 처리
        var buttonHandler = GetEvent("AdvancedButton");
        buttonHandler.Click += OnButtonClick;
        buttonHandler.Enter += OnButtonEnter;
        buttonHandler.Exit += OnButtonExit;
        buttonHandler.Down += OnButtonDown;
        buttonHandler.Up += OnButtonUp;
        
        // 드래그 가능한 패널
        var panelHandler = GetEvent("DraggablePanel");
        panelHandler.BeginDrag += OnBeginDrag;
        panelHandler.Drag += OnDrag;
        panelHandler.EndDrag += OnEndDrag;
    }
    
    private void OnButtonClick(PointerEventData data)
    {
        Debug.Log($"버튼 클릭: {data.position}");
    }
    
    private void OnButtonEnter(PointerEventData data)
    {
        // 호버 효과
        transform.localScale = Vector3.one * 1.1f;
    }
    
    private void OnButtonExit(PointerEventData data)
    {
        // 호버 효과 제거
        transform.localScale = Vector3.one;
    }
    
    private void OnButtonDown(PointerEventData data)
    {
        // 버튼 누름 효과
        transform.localScale = Vector3.one * 0.95f;
    }
    
    private void OnButtonUp(PointerEventData data)
    {
        // 버튼 놓음 효과
        transform.localScale = Vector3.one;
    }
}
```

### 7. 이벤트 정리

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
    }
}
```

### 8. 포인터 핸들러 메서드 목록

**BaseUI에서 제공하는 메서드들:**

```csharp
// 기본 이벤트 처리
GetEvent(string name)                    // 특정 UI 요소의 이벤트 핸들러
GetSelfEvent()                          // UI 자체의 이벤트 핸들러

// SFX가 포함된 이벤트 처리
GetEventWithSFX(string name, string soundName = null)  // 클릭 사운드와 함께
GetBackEvent(string name, string soundName = null)     // 뒤로가기 사운드와 함께

// 지원하는 이벤트 타입
Click      // 클릭
Up         // 마우스/터치 업
Down       // 마우스/터치 다운
Enter      // 포인터 진입
Exit       // 포인터 나감
Move       // 포인터 이동
BeginDrag  // 드래그 시작
Drag       // 드래그 중
EndDrag    // 드래그 종료
```

### 9. 실제 사용 예제

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
        GetEvent("StartButton").Enter += (data) => OnButtonHover("StartButton");
        GetEvent("StartButton").Exit += (data) => OnButtonExit("StartButton");
        
        GetEvent("SettingsButton").Enter += (data) => OnButtonHover("SettingsButton");
        GetEvent("SettingsButton").Exit += (data) => OnButtonExit("SettingsButton");
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

### 10. 주의사항

1. **이벤트 정리**: UI가 파괴될 때 반드시 이벤트를 정리해야 합니다.
2. **성능 고려**: 많은 UI 요소에 이벤트를 추가할 때는 성능을 고려해야 합니다.
3. **메모리 누수 방지**: 람다 표현식을 사용할 때는 클로저로 인한 메모리 누수를 주의해야 합니다.
4. **UI 요소 등록**: 동적으로 생성한 UI 요소는 `AddUIToDictionary()`로 등록해야 합니다.

## ⚙️ Addressable 설정

### 1. Unity 에디터에서 설정

1. **Window > Asset Management > Addressables > Groups** 열기
2. **Create Addressables Settings** 클릭 (처음 사용 시)
3. UI 프리팹들을 Addressable로 설정:
   - 프리팹 선택 → Inspector → **Addressable** 체크
   - 적절한 그룹으로 이동

### 2. UIManager 설정

```csharp
[Header("Addressable Canvas References")]
[SerializeField] private AssetReferenceGameObject hudCanvasReference;
[SerializeField] private AssetReferenceGameObject panelCanvasReference;
[SerializeField] private AssetReferenceGameObject popupCanvasReference;
[SerializeField] private AssetReferenceGameObject loadingCanvasReference;
```

### 3. 권장 키 구조

```
UI/Canvas/HUDCanvas
UI/Canvas/PanelCanvas
UI/Canvas/PopupCanvas
UI/Canvas/LoadingCanvas
UI/HUD/StatusPanel
UI/Panel/MainMenu
UI/Panel/Settings
UI/Popup/MessagePopup
UI/Popup/CheckPopUp
UI/Loading/LoadingScreen
```

## 📱 SafeArea 설정

### 1. SafeAreaManager 설정

```csharp
[Header("SafeArea Settings")]
[SerializeField] private bool enableSafeArea = true;
[SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
[SerializeField] private bool showDebugArea = false;
```

### 2. SafeAreaPanel 자동 적용

각 Canvas 프리팹에 SafeAreaPanel이 자동으로 추가되어 모바일 디바이스의 안전 영역에 맞게 UI가 조정됩니다.

## 📚 예제

### 1. 간단한 메시지 팝업

```csharp
public class MessagePopup : BaseUI
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;
    
    protected override void Awake()
    {
        base.Awake();
        okButton.onClick.AddListener(() => Hide());
    }
    
    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}

// 사용법
UIManager.Instance.ShowPopUpAsync<MessagePopup>((popup) => {
    popup?.SetMessage("안녕하세요!");
});
```

### 2. 설정 패널

```csharp
public class SettingsPanel : BaseUI
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button applyButton;
    
    protected override void Awake()
    {
        base.Awake();
        SetupControls();
    }
    
    private void SetupControls()
    {
        applyButton.onClick.AddListener(() => {
            // 설정 적용 로직
            Hide();
        });
    }
}
```

### 3. 로딩 화면

```csharp
public class LoadingScreen : BaseUI
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    
    public void SetProgress(float progress)
    {
        if (progressSlider != null)
            progressSlider.value = progress;
        
        if (progressText != null)
            progressText.text = $"{progress * 100:F0}%";
    }
}
```

## 🔄 마이그레이션 가이드

### 기존 UIManagerOld에서 UIManager로 전환

1. **기존 코드**
```csharp
// 기존 방식
UIManagerOld.Instance.ShowPopUp<MainMenu>();
```

2. **새로운 방식**
```csharp
// 새로운 방식
BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");
UIManager.Instance.OpenPanel(mainMenu);
```

3. **점진적 전환**
- 기존 UIManagerOld와 UIManager를 병행 사용
- UI별로 하나씩 전환
- 모든 UI 전환 완료 후 UIManagerOld 제거

## 🛠️ 문제 해결

### 1. 컴파일 에러

**DoTween 관련 에러**
```
error CS0246: The type or namespace name 'DG' could not be found
```
- **해결**: `README_DOTWEEN.md` 참조하여 DoTween 설치
- 또는 `DOTWEEN_AVAILABLE` 심볼 추가

**Addressable 관련 에러**
```
error CS1061: 'UIManager' does not contain a definition for 'ShowPopUp'
```
- **해결**: 새로운 `ShowPopUpAsync` 메서드 사용

### 2. 런타임 에러

**UI 로드 실패**
```
[UIManager] UI 로드 실패: UI/Panel/MainMenu
```
- **해결**: Addressable 키 확인, 프리팹이 올바른 그룹에 있는지 확인

**Canvas 참조 누락**
```
[UIManager] UI 부모를 찾을 수 없음
```
- **해결**: UIManager의 Canvas Reference 설정 확인

### 3. 한글 인코딩 문제

**Visual Studio에서 한글이 깨짐**
- **해결**: 파일을 UTF-8 BOM으로 저장
- Visual Studio에서 **File > Advanced Save Options > Encoding** 설정

### 4. 성능 최적화

**메모리 누수 방지**
```csharp
// UI 해제 시 반드시 호출
UIManager.Instance.ReleaseUI("UI/Panel/MainMenu");

// 씬 전환 시 모든 UI 해제
UIManager.Instance.ReleaseAllAddressables();
```

**미리 로드 활용**
```csharp
// 자주 사용하는 UI 미리 로드
await UIManager.Instance.PreloadUIAsync<BaseUI>("UI/Panel/MainMenu");
```

## 📁 파일 구조

```
Assets/000WorkSpaces/KYS/Scripts/UI/
├── UIManager.cs              # 메인 UI 관리자
├── UIManagerOld.cs           # 기존 호환성용
├── BaseUI.cs                 # 기본 UI 클래스
├── CheckPopUp.cs             # 확인 팝업
├── PopUpUI.cs                # 레거시 팝업 관리
├── Util.cs                   # 유틸리티
├── SafeAreaManager.cs        # SafeArea 관리
├── Enums/
│   ├── UILayerType.cs        # UI 레이어 타입
│   └── UIPanelGroup.cs       # 패널 그룹
├── MVP/
│   ├── IUIView.cs            # View 인터페이스
│   ├── IUIPresenter.cs       # Presenter 인터페이스
│   ├── IUIModel.cs           # Model 인터페이스
│   ├── BaseUIPresenter.cs    # 기본 Presenter
│   └── BaseUIModel.cs        # 기본 Model
├── Localization/
│   ├── LocalizationManager.cs
│   ├── LanguageModel.cs
│   └── LocalizedText.cs
├── Examples/
│   ├── AddressableUIExamples.cs
│   ├── BaseUIUsageExamples.cs
│   └── ...
└── README.md                 # 이 파일
```

## 🎯 모범 사례

### 1. UI 설계 원칙
- **단일 책임**: 각 UI는 하나의 명확한 목적만 가져야 함
- **재사용성**: 공통 UI 요소는 재사용 가능하게 설계
- **확장성**: 새로운 UI 추가가 용이하도록 설계

### 2. 성능 고려사항
- **지연 로딩**: 필요할 때만 UI 로드
- **메모리 관리**: 사용하지 않는 UI 즉시 해제
- **캐싱**: 자주 사용하는 UI 요소 캐시

### 3. 코드 품질
- **MVP 패턴**: 비즈니스 로직과 UI 로직 분리
- **에러 처리**: 적절한 예외 처리와 로깅
- **문서화**: 복잡한 로직에 대한 주석 작성

## 📞 지원

문제가 발생하거나 추가 도움이 필요한 경우:
1. 이 README 문서 확인
2. 예제 코드 참조
3. Unity Console 로그 확인
4. Addressable Groups 설정 확인

---

**버전**: 1.0  
**최종 업데이트**: 2024년  
**Unity 버전**: 2022.3 LTS 이상
