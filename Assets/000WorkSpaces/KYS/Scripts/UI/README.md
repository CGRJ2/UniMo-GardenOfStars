# KYS UI 시스템 가이드

## 📋 목차
1. [시스템 개요](#시스템-개요)
2. [아키텍처](#아키텍처)
3. [핵심 클래스](#핵심-클래스)
4. [사용법](#사용법)
5. [InfoHUD 시스템](#infohud-시스템)
6. [중복 생성 방지](#중복-생성-방지)
7. [로컬라이제이션](#로컬라이제이션)
8. [MVP 패턴 사용 (선택적)](#mvp-패턴-사용-선택적)
9. [Addressable 설정](#addressable-설정)
10. [SafeArea 설정](#safearea-설정)
11. [예제](#예제)
12. [마이그레이션 가이드](#마이그레이션-가이드)
13. [문제 해결](#문제-해결)

## 🎯 시스템 개요

KYS UI 시스템은 Unity에서 효율적이고 확장 가능한 UI 관리를 위한 종합적인 솔루션입니다.

### 주요 특징
- **Addressable 기반**: 동적 로딩과 메모리 효율성
- **선택적 MVP 패턴**: 간단한 UI는 View만, 복잡한 UI는 MVP 전체 사용
- **레이어 시스템**: HUD, Panel, Popup, Loading 분리
- **Stack 관리**: UI 표시 순서 자동 관리
- **SafeArea 지원**: 모바일 디바이스 최적화
- **다국어 지원**: Localization 시스템 통합
- **중복 생성 방지**: UI 중복 생성 및 메모리 누수 방지
- **InfoHUD 관리**: 터치 기반 정보 표시 시스템

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
│   ├── TouchInfoHUD.prefab
│   ├── HUDBackdropUI.prefab
│   └── HUDAllPanel.prefab
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
    public async Task<T> LoadUIAsync<T>(AssetReferenceGameObject reference);
    
    // 패널/팝업 관리 (중복 생성 방지 포함)
    public async Task<T> ShowPanelAsync<T>(System.Action<T> onComplete = null);
    public async Task<T> ShowPopUpAsync<T>(System.Action<T> onComplete = null);
    public void ClosePanel();
    public void ClosePopup();
    
    // LoadingScreen 관리
    public async Task<LoadingScreen> ShowLoadingScreenAsync(string message = null);
    public void HideLoadingScreen();
    
    // InfoHUD 전용 관리
    public async Task<T> ShowSingleInfoHUDAsync<T>(Vector2 screenPosition, string title, string description, Sprite icon);
    public bool DestroyAllInfoHUDs();
    
    // Stack 관리
    public void CloseAllPanels();
    public void CloseAllPopups();
    public void CleanAllUI();
    
    // Backdrop 관리
    public void ShowBackdrop(Color? color = null);
    public void HideBackdrop();
}
```

### BaseUI
```csharp
// 모든 UI의 기본 클래스 (MVP 패턴 지원)
public class BaseUI : MonoBehaviour, IUIView
{
    [Header("UI Layer Settings")]
    [SerializeField] protected UILayerType layerType = UILayerType.Panel;
    [SerializeField] protected UIPanelGroup panelGroup = UIPanelGroup.Other;
    
    [Header("UI Animation Settings")]
    [SerializeField] protected bool useAnimation = true;
    [SerializeField] protected bool useShowAnimation = true;
    [SerializeField] protected bool useHideAnimation = true;
    [SerializeField] protected float animationDuration = 0.3f;
    
    [Header("UI Behavior Settings")]
    [SerializeField] protected bool canCloseWithESC = false;
    [SerializeField] protected bool canCloseWithBackdrop = false;
    [SerializeField] protected bool hidePreviousUI = false;
    [SerializeField] protected bool disablePreviousUI = false;
    [SerializeField] protected bool createBackdropForPopup = false;
    
    // UI 요소 접근 (캐시된 결과 반환)
    public T GetUI<T>(string name) where T : Component;
    public GameObject GetUI(string name);
    public PointerHandler GetEvent(string name);
    public PointerHandler GetEventWithSFX(string name, string sfxKey = null);
    
    // 자동 로컬라이제이션
    protected virtual string[] GetAutoLocalizeKeys();
    
    // MVP 패턴 지원
    public void SetPresenter(IUIPresenter presenter);
    public void SetModel(IUIModel model);
    
    // 애니메이션 제어
    protected virtual void PlayShowAnimation();
    protected virtual void PlayHideAnimation(System.Action onComplete = null);
    
    // Properties
    public UILayerType LayerType => layerType;
    public UIPanelGroup PanelGroup => panelGroup;
    public bool CanCloseWithESC => canCloseWithESC;
    public bool IsActive => gameObject.activeInHierarchy;
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

### TouchInfoHUD
```csharp
// 터치 기반 정보 표시 HUD
public class TouchInfoHUD : BaseUI
{
    // 정적 메서드로 HUD 생성
    public static async Task ShowInfoHUD(Vector2 screenPosition, string title, string description, Sprite icon = null);
    
    // HUD 위치 및 정보 설정
    public void SetHUDPosition(Vector2 screenPosition);
    public void SetInfo(string title, string description, Sprite icon = null);
}
```

### TouchInfoManager
```csharp
// 터치 감지 및 InfoHUD 관리
public class TouchInfoManager : MonoBehaviour
{
    // 터치 처리 (InfoHUD 자체 클릭 방지)
    private void ProcessTouch(Vector2 screenPosition);
    
    // InfoHUD 영역 클릭 확인
    private bool IsTouchInfoHUDClicked(Vector2 screenPosition);
}
```

## 🎯 InfoHUD 시스템

### InfoHUD 개요
InfoHUD는 게임 오브젝트를 터치했을 때 해당 오브젝트의 정보를 표시하는 시스템입니다.

### 주요 특징
- **단일 활성 인스턴스**: 한 번에 하나의 InfoHUD만 표시
- **자동 메모리 관리**: 닫을 때 완전히 제거되어 메모리 누수 방지
- **Backdrop 지원**: InfoHUD 외부 클릭 시 자동 닫기
- **위치 자동 조정**: 화면 경계를 벗어나지 않도록 위치 조정

### 사용법

```csharp
// 정적 메서드로 InfoHUD 표시
await TouchInfoHUD.ShowInfoHUD(
    screenPosition, 
    "오브젝트 이름", 
    "오브젝트 설명", 
    iconSprite
);

// TouchInfoManager를 통한 자동 감지
// TouchInfoManager가 자동으로 오브젝트 터치를 감지하여 InfoHUD 표시
```

### InfoHUD 동작 방식
1. **터치 감지**: TouchInfoManager가 터치 입력 감지
2. **오브젝트 확인**: 터치된 위치의 게임 오브젝트 확인
3. **기존 HUD 제거**: 활성화된 InfoHUD가 있으면 제거
4. **새 HUD 생성**: UIManager를 통해 새로운 TouchInfoHUD 생성
5. **Backdrop 생성**: HUDBackdropUI를 생성하여 외부 클릭 감지
6. **정보 표시**: 오브젝트 정보를 HUD에 표시

## 🚫 중복 생성 방지

### 중복 생성 방지 시스템
UI 시스템은 다음과 같은 중복 생성 방지 메커니즘을 제공합니다:

#### 1. 플래그 기반 방지
```csharp
// UIManager 내부 플래그
private bool isCreatingPopup = false;
private bool isCreatingPanel = false;
private bool isCreatingUI = false;
```

#### 2. 스택 기반 중복 확인
```csharp
// 패널/팝업 스택에서 중복 확인
if (panelStack.Count > 0 && panelStack.Peek().GetType() == typeof(TargetPanel))
{
    // 기존 패널을 스택 최상위로 이동
    var existingPanel = panelStack.Pop();
    panelStack.Push(existingPanel);
    return;
}
```

#### 3. InfoHUD 전용 관리
```csharp
// InfoHUD는 항상 기존 인스턴스를 제거 후 생성
public async Task<T> ShowSingleInfoHUDAsync<T>(Vector2 screenPosition, string title, string description, Sprite icon)
{
    // 기존 모든 InfoHUD 제거
    DestroyAllInfoHUDs();
    
    // 새로운 InfoHUD 생성
    return await CreateHUDAsync<T>(addressableKey);
}
```

### 사용법

#### 1. 패널 표시
```csharp
// 기본 패널 표시
UIManager.Instance.ShowPanelAsync<TitlePanel>((panel) => {
    if (panel != null)
    {
        Debug.Log("TitlePanel 표시됨");
    }
});

// 콜백 없이 패널 표시
var panel = await UIManager.Instance.ShowPanelAsync<SettingsPanel>();
```

#### 2. 팝업 표시
```csharp
// 기본 팝업 표시
UIManager.Instance.ShowPopUpAsync<MessagePopUp>((popup) => {
    popup?.SetMessage("안녕하세요!");
});

// 콜백 없이 팝업 표시
var popup = await UIManager.Instance.ShowPopUpAsync<CheckPopUp>();
```

#### 3. LoadingScreen 사용
```csharp
// 로딩 화면 표시
var loadingScreen = await UIManager.Instance.ShowLoadingScreenAsync("데이터 로딩 중...");

// 로딩 완료 후 숨기기
UIManager.Instance.HideLoadingScreen();
```

#### 4. InfoHUD 사용
```csharp
// InfoHUD 표시
await UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(
    screenPosition, 
    "오브젝트 이름", 
    "오브젝트 설명", 
    iconSprite
);
```

#### 5. Backdrop 관리
```csharp
// Backdrop 표시
UIManager.Instance.ShowBackdrop(new Color(0, 0, 0, 0.5f));

// Backdrop 숨기기
UIManager.Instance.HideBackdrop();
```

## 🌐 로컬라이제이션

### 자동 로컬라이제이션
UI 시스템은 자동 로컬라이제이션을 지원합니다.

#### 1. AutoLocalizedText 컴포넌트
```csharp
// UI 요소에 AutoLocalizedText 컴포넌트 추가
[SerializeField] private AutoLocalizedText titleText;

// 자동으로 UI 이름을 기반으로 로컬라이제이션 키 생성
// 예: "TitleText" → "ui_titletext"
```

#### 2. BaseUI 자동 로컬라이제이션
```csharp
public class CustomPanel : BaseUI
{
    protected override string[] GetAutoLocalizeKeys()
    {
        return new string[] {
            "TitleText",
            "DescriptionText",
            "ConfirmButtonText"
        };
    }
}
```

#### 3. 언어별 언어명 표시
```csharp
// LocalizationManager의 새로운 기능
public string GetLocalizedLanguageName(SystemLanguage language)
{
    // 현재 언어에 맞는 언어명 반환
    // 한국어: "한국어", "영어"
    // 영어: "Korean", "English"
}
```

### 사용법

#### 1. 기본 로컬라이제이션
```csharp
// 수동 로컬라이제이션
string translatedText = LocalizationManager.Instance.GetText("ui_title");

// 언어 설정
LocalizationManager.Instance.SetLanguage(SystemLanguage.English);

// 언어 변경 이벤트 구독
LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
```

#### 2. CSV 기반 로컬라이제이션
```csharp
// CSV 파일에서 로컬라이제이션 데이터 로드
// Assets/StreamingAssets/Localization/ 폴더에 CSV 파일 배치
// 예: Localization_Korean.csv, Localization_English.csv

// 자동 로컬라이제이션 키 생성 규칙
// UI 요소 이름에서 "Text" 접미사 제거 후 소문자 변환
// "TitleText" → "ui_title"
// "ConfirmButtonText" → "ui_confirmbutton"
```

#### 3. 다국어 지원
```csharp
// 지원 언어 목록
SystemLanguage[] supportedLanguages = {
    SystemLanguage.Korean,
    SystemLanguage.English,
    SystemLanguage.Chinese,
    SystemLanguage.Japanese
};

// 현재 언어 확인
SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;

// 언어 변경
LocalizationManager.Instance.SetLanguage(SystemLanguage.English);
```


## 🏗️ MVP 패턴 사용 (선택적)

현재 프로젝트에서는 **선택적으로 MVP 패턴을 사용**합니다:

### **간단한 UI (View만 사용) - 권장**
```csharp
// MenuPopUp.cs - View만 사용 (현재 대부분의 UI)
public class MenuPopUp : BaseUI
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    
    protected override void Awake()
    {
        base.Awake();
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }
    
    private void OnStartButtonClicked()
    {
        Debug.Log("[MenuPopUp] 시작 버튼 클릭");
        Manager.ui.ClosePopup();
    }
    
    private void OnSettingsButtonClicked()
    {
        Debug.Log("[MenuPopUp] 설정 버튼 클릭");
        // 설정 패널 열기
    }
}
```

### **복잡한 UI (MVP 전체 사용) - 필요시에만**
```csharp
// Model
public class GameDataModel : BaseUIModel
{
    public int Score { get; private set; }
    public int Level { get; private set; }
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLevelChanged;
    
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
        
        // 레벨업 체크
        int newLevel = Score / 1000;
        if (newLevel > Level)
        {
            Level = newLevel;
            OnLevelChanged?.Invoke(Level);
        }
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
        model.OnLevelChanged += OnLevelChanged;
    }
    
    private void OnScoreChanged(int newScore)
    {
        view.UpdateScore(newScore);
    }
    
    private void OnLevelChanged(int newLevel)
    {
        view.UpdateLevel(newLevel);
        view.ShowLevelUpEffect();
    }
}

// View
public class GameUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject levelUpEffect;
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
    
    public void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Level: {level}";
    }
    
    public void ShowLevelUpEffect()
    {
        if (levelUpEffect != null)
            levelUpEffect.SetActive(true);
    }
}
```

### **사용 기준**
- **간단한 UI**: 버튼 클릭만, 데이터 저장/로드 없음 → **View만 사용 (권장)**
- **복잡한 UI**: 데이터 저장/로드, 복잡한 비즈니스 로직 → **MVP 전체 사용**

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
UI/Popup/MessagePopup
UI/Loading/LoadingScreen
```

### 4. Addressable 그룹 설정

```
UI_Canvas/          # Canvas 프리팹들
UI_HUD/            # HUD 프리팹들
UI_Panel/          # Panel 프리팹들
UI_Popup/          # Popup 프리팹들
UI_Loading/        # Loading 프리팹들
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
    [SerializeField] private Button closeButton;
    
    protected override void Awake()
    {
        base.Awake();
        SetupControls();
    }
    
    private void SetupControls()
    {
        // 슬라이더 값 변경 이벤트
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        // 토글 변경 이벤트
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        
        // 버튼 이벤트
        applyButton.onClick.AddListener(OnApplyButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    
    private void OnBGMVolumeChanged(float value)
    {
        // BGM 볼륨 설정
        AudioManager.Instance.SetBGMVolume(value);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        // SFX 볼륨 설정
        AudioManager.Instance.SetSFXVolume(value);
    }
    
    private void OnFullscreenToggled(bool isFullscreen)
    {
        // 전체화면 토글
        Screen.fullScreen = isFullscreen;
    }
    
    private void OnApplyButtonClicked()
    {
        // 설정 저장
        SaveSettings();
        Hide();
    }
    
    private void OnCloseButtonClicked()
    {
        Hide();
    }
    
    private void SaveSettings()
    {
        // 설정 저장 로직
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
    }
}
```

### 3. LoadingScreen (DoTween 애니메이션 포함)

```csharp
public class LoadingScreen : BaseUI
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI loadingMessageText;
    [SerializeField] private Image loadingIcon;
    
    public void SetProgress(float progress)
    {
        if (progressSlider != null)
        {
            // DoTween으로 부드러운 진행률 애니메이션
            progressSlider.DOValue(progress, 0.3f).SetEase(Ease.OutQuad);
        }
        
        if (progressText != null)
        {
            progressText.text = $"{progress * 100:F0}%";
        }
    }
    
    public void SetLoadingMessage(string message)
    {
        if (loadingMessageText != null)
        {
            loadingMessageText.text = message;
        }
    }
    
    public void StartLoadingIconAnimation()
    {
        if (loadingIcon != null)
        {
            // 회전 애니메이션
            loadingIcon.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }
    }
    
    public void StopLoadingIconAnimation()
    {
        if (loadingIcon != null)
        {
            loadingIcon.transform.DOKill();
        }
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

### 3. InfoHUD 관련 문제

**InfoHUD가 생성되지 않음**
```
[TouchInfoHUD] 이미 HUD 생성 중이므로 무시합니다.
```
- **해결**: UIManager의 ShowSingleInfoHUDAsync 사용

**InfoHUD가 자동으로 닫힘**
- **해결**: TouchInfoManager의 IsTouchInfoHUDClicked 확인

### 4. 중복 생성 문제

**패널이 중복 생성됨**
- **해결**: UIManager의 중복 확인 로직 확인
- **해결**: ShowPanelAsync 사용하여 중복 방지

### 5. 메모리 누수

**InfoHUD 인스턴스가 누적됨**
- **해결**: DestroyAllInfoHUDs() 호출 확인
- **해결**: TouchInfoHUD.Hide()에서 완전한 제거 확인

### 6. 로컬라이제이션 문제

**언어 변경이 안됨**
- **해결**: LocalizationManager 초기화 확인
- **해결**: CSV 파일 경로 및 형식 확인

**언어명이 잘못 표시됨**
- **해결**: GetLocalizedLanguageName() 사용

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
├── UIManager.cs              # 메인 UI 관리자 (중복 생성 방지 포함)
├── BaseUI.cs                 # 기본 UI 클래스 (자동 로컬라이제이션 포함)
├── CheckPopUp.cs             # 확인 팝업
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
├── UILocalization/
│   ├── LocalizationManager.cs # 로컬라이제이션 관리 (언어별 언어명 포함)
│   ├── AutoLocalizedText.cs  # 자동 로컬라이제이션 컴포넌트
│   ├── LanguageSettingsPanel.cs # 언어 설정 패널
│   └── Comprehensive_Localization_Guide.md # 통합된 로컬라이제이션 가이드
├── UIHUD/
│   ├── TouchInfoHUD.cs       # 터치 기반 정보 HUD
│   ├── TouchInfoManager.cs   # 터치 감지 및 HUD 관리
│   ├── HUDBackdropUI.cs      # HUD용 Backdrop
│   ├── HUDAllPanel.cs        # 전체 HUD 패널
│   └── LayerSpecificHUD_Usage_Guide.md # HUD 사용 가이드
├── UIPanel/
│   └── ChoicePanel_README.md # ChoicePanel 사용 가이드
├── UIPopup/
│   ├── MessagePopUp.cs
│   └── CheckPopUp.cs
├── Examples/
│   ├── AddressableUIExamples.cs
│   ├── BaseUIUsageExamples.cs
│   ├── TouchGestureExamples.cs
│   └── PopupExamples.cs
├── Archives/                 # 보관된 문서들
│   ├── UI_Reference_Comparison.md
│   ├── AssetReference_vs_String_Key.md
│   ├── MVP_Setup_Guide.md
│   ├── README_AutoLocalization.md
│   ├── Localization_Examples_Guide.md
│   └── AutoLocalizedText_Usage_Guide.md
├── Current_Usage_Pattern_Guide.md    # 현재 프로젝트 사용 패턴
├── Unity_Editor_Setup_Guide.md # Unity 에디터 설정 가이드
├── Addressable_UI_Setup_Guide.md # Addressable UI 설정 가이드
├── SafeArea_Setup_Guide.md   # SafeArea 설정 가이드
├── UIManager_LoadingScreen_Usage.md # DoTween 애니메이션 적용 로딩 화면 사용 가이드
└── README.md                 # 이 파일
```

## 🎯 모범 사례

### 1. UI 설계 원칙
- **단일 책임**: 각 UI는 하나의 명확한 목적만 가져야 함
- **재사용성**: 공통 UI 요소는 재사용 가능하게 설계
- **확장성**: 새로운 UI 추가가 용이하도록 설계

### 2. MVP 패턴 사용 원칙
- **간단한 UI**: 버튼 클릭만, 데이터 없음 → View만 사용
- **보통 UI**: 여러 버튼, 간단한 상태 → View + Presenter
- **복잡한 UI**: 데이터 저장/로드, 복잡한 로직 → MVP 전체

### 3. InfoHUD 사용 원칙
- **단일 활성**: 한 번에 하나의 InfoHUD만 활성화
- **자동 제거**: 다른 곳 클릭 시 자동으로 닫힘
- **메모리 관리**: 완전한 제거로 메모리 누수 방지

### 4. 중복 생성 방지 원칙
- **중복 확인**: UI 생성 전 기존 인스턴스 확인
- **Stack 관리**: 패널/팝업은 Stack으로 관리
- **상태 추적**: 생성 중 상태 플래그로 중복 방지

### 5. 로컬라이제이션 원칙
- **자동 키 생성**: UI 이름 기반 자동 키 생성
- **언어별 표시**: 언어명도 현재 언어로 표시
- **CSV 관리**: 중앙화된 CSV 파일로 관리

### 6. 성능 고려사항
- **지연 로딩**: 필요할 때만 UI 로드
- **메모리 관리**: 사용하지 않는 UI 즉시 해제
- **캐싱**: 자주 사용하는 UI 요소 캐시

### 7. 코드 품질
- **MVP 패턴**: 비즈니스 로직과 UI 로직 분리
- **에러 처리**: 적절한 예외 처리와 로깅
- **문서화**: 복잡한 로직에 대한 주석 작성

## 📊 현재 프로젝트 상태

### 구현된 기능
- ✅ **Addressable 기반 UI 시스템**: 완전 구현
- ✅ **선택적 MVP 패턴**: 기본 구조 완성, 실제로는 View 중심 사용
- ✅ **InfoHUD 시스템**: 터치 기반 정보 표시 완전 구현
- ✅ **중복 생성 방지**: 모든 UI 레이어에 적용
- ✅ **로컬라이제이션**: CSV 기반 다국어 지원 완료
- ✅ **SafeArea 지원**: 모바일 디바이스 최적화
- ✅ **포인터 이벤트**: 다양한 터치 제스처 지원

### 실제 사용 패턴
- **간단한 UI (View만)**: `MenuPopUp`, `CheckPopUp`, `TitlePanel` 등
- **복잡한 UI (MVP 전체)**: 현재 구현된 예시 없음 (필요시 확장 가능)
- **InfoHUD**: `TouchInfoHUD` + `HUDBackdropUI` 조합으로 완전 구현

### 향후 개선 방향
- **MVP 패턴 확장**: 복잡한 UI에서 실제 MVP 사용 사례 추가
- **성능 최적화**: 메모리 사용량 모니터링 및 최적화
- **테스트 코드**: 자동화된 테스트 케이스 추가

## 📚 문서 관리

### 현재 활성 문서
- **README.md**: 메인 시스템 가이드
- **Current_Usage_Pattern_Guide.md**: 프로젝트별 사용 패턴
- **Comprehensive_Localization_Guide.md**: 통합된 로컬라이제이션 가이드
- **Unity_Editor_Setup_Guide.md**: Unity 에디터 설정
- **Addressable_UI_Setup_Guide.md**: Addressable UI 설정
- **SafeArea_Setup_Guide.md**: SafeArea 설정
- **UIManager_LoadingScreen_Usage.md**: DoTween 애니메이션 적용 로딩 화면 사용법
- **LayerSpecificHUD_Usage_Guide.md**: HUD 사용 가이드
- **ChoicePanel_README.md**: ChoicePanel 사용 가이드

### Archives 폴더
중복되거나 통합된 문서들은 `Archives/` 폴더에 보관되어 있습니다:
- **UI_Reference_Comparison.md**: UI 참조 방식 비교 (통합됨)
- **AssetReference_vs_String_Key.md**: AssetReference vs String Key 비교 (통합됨)
- **MVP_Setup_Guide.md**: MVP 설정 가이드 (현재 사용 패턴에 통합됨)
- **README_AutoLocalization.md**: 자동 로컬라이제이션 가이드 (통합됨)
- **Localization_Examples_Guide.md**: 로컬라이제이션 예제 (통합됨)
- **AutoLocalizedText_Usage_Guide.md**: AutoLocalizedText 사용법 (통합됨)

## 📞 지원

문제가 발생하거나 추가 도움이 필요한 경우:
1. 이 README 문서 확인
2. 관련 가이드 문서 확인
3. Archives 폴더의 상세 문서 참조
4. 예제 코드 참조
5. Unity Console 로그 확인
6. Addressable Groups 설정 확인
7. 로컬라이제이션 CSV 파일 확인

---

**버전**: 2.3  
**최종 업데이트**: 2025년 9월 15일  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: UIManager API 최신화, BaseUI 기능 확장, DoTween 애니메이션 지원, 로컬라이제이션 시스템 개선, 예제 코드 현대화
