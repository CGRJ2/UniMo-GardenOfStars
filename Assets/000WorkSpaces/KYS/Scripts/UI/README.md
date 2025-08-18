# 개선된 UI 시스템 가이드

## 개요

이 UI 시스템은 MVP(Model-View-Presenter) 패턴을 기반으로 하며, 레이어별 관리와 Stack 기반 UI 관리를 지원합니다.

## 주요 기능

### 1. 레이어별 UI 관리
- **HUD 그룹**: 재화 표시, 기본 UI 패널 및 버튼들 (가장 뒤에 위치)
- **패널 그룹**: 열고 닫기 가능한 UI패널
- **팝업 UI 그룹**: 상호작용 영역 진입 시 정보 표기 or 상호작용 버튼
- **로딩UI**: 모든 화면을 덮는 최상위 레이어 (가장 앞에 위치)

### 2. Stack 기반 UI 관리
- 패널 그룹과 팝업 그룹은 Stack으로 관리
- UI가 열리고 닫힐 때 Stack에 자동으로 추가/제거
- 이전 UI 자동 복원 기능

### 3. MVP 패턴 지원
- **Model**: 데이터와 비즈니스 로직
- **View**: UI 표시와 사용자 입력 처리
- **Presenter**: Model과 View 간의 중재자

### 4. DOTween 애니메이션 지원
- UI 표시/숨김 애니메이션 자동 적용
- 커스터마이징 가능한 애니메이션 설정

## 사용법

### 1. 기본 UI 클래스 생성

```csharp
public class MyUI : BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        layerType = UILayerType.Panel; // 레이어 타입 설정
        panelGroup = UIPanelGroup.Shop; // 패널 그룹 설정
    }
    
    public override void Initialize()
    {
        base.Initialize();
        // UI 초기화 로직
        UIManager.Instance.RegisterUI(this);
    }
    
    public override void Cleanup()
    {
        base.Cleanup();
        UIManager.Instance.UnregisterUI(this);
    }
}
```

### 2. MVP 패턴 구현

#### Model
```csharp
public class MyUIModel : BaseUIModel
{
    private int data = 0;
    public System.Action<int> OnDataChanged;
    
    public int Data
    {
        get => data;
        set
        {
            if (data != value)
            {
                data = value;
                OnDataChanged?.Invoke(data);
            }
        }
    }
}
```

#### Presenter
```csharp
public class MyUIPresenter : BaseUIPresenter
{
    private MyUI view;
    private MyUIModel model;
    
    protected override void OnInitialize()
    {
        view = GetView<MyUI>();
        model = GetModel<MyUIModel>();
        
        // Model 이벤트 구독
        model.OnDataChanged += OnDataChanged;
    }
    
    private void OnDataChanged(int data)
    {
        view?.UpdateUI(data);
    }
}
```

### 3. UI 표시/숨김

```csharp
// UI 표시
UIManager.Instance.ShowPopUp<MyUI>();

// UI 숨김
myUI.Hide();

// 모든 패널 닫기
UIManager.Instance.CloseAllPanels();

// 모든 팝업 닫기
UIManager.Instance.CloseAllPopups();
```

### 4. 그룹별 관리

```csharp
// HUD 그룹 기본 상태로 설정
UIManager.Instance.SetHUDToDefault();

// 특정 그룹의 모든 UI 닫기
UIManager.Instance.CloseAllUIsByGroup(UIPanelGroup.Shop);
```

### 5. 이벤트 처리

```csharp
// 버튼 클릭 이벤트 (SFX 포함)
GetEventWithSFX("ButtonName").Click += (data) => OnButtonClicked();

// 뒤로가기 버튼 이벤트
GetBackEvent("BackButton").Click += (data) => OnBackClicked();

// 일반 이벤트
GetEvent("ButtonName").Click += (data) => OnButtonClicked();
```

## 레이어 우선순위

1. **Loading** (최상위)
2. **Popup** 
3. **Panel**
4. **HUD** (최하위)

## 패널 그룹 분류

- **GameExit**: 게임 종료 관련 패널
- **Shop**: 상점, 인벤토리 관련 패널
- **Progress**: 진행 상황 관련 패널
- **Settings**: 설정 관련 패널
- **Other**: 기타 패널

## UI 동작 설정

BaseUI에서 다음 설정을 조정할 수 있습니다:

### 애니메이션 설정
```csharp
[Header("UI Animation Settings")]
[SerializeField] protected bool useAnimation = true;
[SerializeField] protected float animationDuration = 0.3f;
[SerializeField] protected Ease showEase = Ease.OutBack;
[SerializeField] protected Ease hideEase = Ease.InBack;
```

### UI 동작 설정
```csharp
[Header("UI Behavior Settings")]
[SerializeField] protected bool canCloseWithESC = true;        // ESC로 닫기 가능
[SerializeField] protected bool canCloseWithBackdrop = true;   // 배경 클릭으로 닫기 가능
[SerializeField] protected bool destroyOnClose = false;        // 닫을 때 파괴
[SerializeField] protected bool hidePreviousUI = true;         // 이전 UI 숨김
[SerializeField] protected bool disablePreviousUI = false;     // 이전 UI 비활성화
```

## UI 동작 옵션 설명

### 1. 이전 UI 처리 방식
- **hidePreviousUI = true**: 이전 UI를 완전히 숨김 (기본값)
- **disablePreviousUI = true**: 이전 UI를 비활성화만 함
- **둘 다 false**: 이전 UI에 영향 없음 (오버레이 방식)

### 2. ESC 키 처리
- **canCloseWithESC = true**: ESC 키로 닫기 가능 (기본값)
- **canCloseWithESC = false**: ESC 키로 닫을 수 없음 (중요한 UI용)

### 3. 닫기 시 동작
- **destroyOnClose = false**: 닫을 때 비활성화만 (기본값)
- **destroyOnClose = true**: 닫을 때 GameObject 파괴

## 주의사항

1. **UI 등록**: 모든 UI는 `Initialize()`에서 `UIManager.Instance.RegisterUI(this)` 호출 필요
2. **UI 정리**: 모든 UI는 `Cleanup()`에서 `UIManager.Instance.UnregisterUI(this)` 호출 필요
3. **레이어 설정**: `Awake()`에서 `layerType`과 `panelGroup` 설정 필요
4. **ESC 처리**: `canCloseWithESC` 설정으로 ESC 키 처리 제어 가능
5. **이전 UI 처리**: `hidePreviousUI`와 `disablePreviousUI` 설정으로 이전 UI 처리 방식 제어

## 예시 코드

프로젝트의 `Examples` 폴더에서 다양한 UI 예시를 확인할 수 있습니다:

- `HUDExample.cs`: HUD 레이어 예시
- `PanelExamples.cs`: 패널 그룹 예시들
- `PopupExamples.cs`: 팝업 UI 예시들
- `AdvancedUIExamples.cs`: 고급 UI 옵션 예시들
  - `OverlayPopup`: 이전 UI를 숨기지 않는 오버레이 팝업
  - `ModalPopup`: 이전 UI를 비활성화하는 모달 팝업
  - `ImportantPanel`: ESC로 닫을 수 없는 중요 패널
  - `IndependentPopup`: 완전히 독립적인 팝업

## 다중언어 지원

### 1. LocalizationManager 설정

```csharp
// LocalizationManager 컴포넌트를 씬에 추가하고 언어 파일들을 할당
LocalizationManager.Instance.SetLanguage(SystemLanguage.Korean);
```

### 2. LocalizedText 컴포넌트 사용

```csharp
// UI 오브젝트에 LocalizedText 컴포넌트 추가
// Inspector에서 Localization Key 설정
// 예: "ui_confirm", "menu_title" 등
```

### 3. 코드에서 다중언어 텍스트 사용

```csharp
// BaseUI를 상속받은 클래스에서
string text = GetLocalizedText("ui_confirm");

// 직접 LocalizationManager 사용
string text = LocalizationManager.Instance.GetText("menu_title");
```

### 4. 언어 파일 형식

```
[Korean]
ui_confirm="확인"
menu_title="메뉴"

[English]
ui_confirm="Confirm"
menu_title="Menu"
```

### 5. 언어 설정 패널

```csharp
// 언어 설정 패널 표시
LanguageSettingsPanel.ShowLanguageSettings();
```

## 확장 방법

1. **새로운 레이어 타입**: `UILayerType` enum에 추가
2. **새로운 패널 그룹**: `UIPanelGroup` enum에 추가
3. **커스텀 애니메이션**: `BaseUI`의 `PlayShowAnimation()`, `PlayHideAnimation()` 오버라이드
4. **커스텀 이벤트**: `PointerHandler` 확장하여 새로운 이벤트 타입 추가
5. **새로운 언어 추가**: `LocalizationManager`의 `IsLanguageSupported()` 메서드 수정
