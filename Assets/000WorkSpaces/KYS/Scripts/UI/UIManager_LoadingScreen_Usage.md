# UIManager LoadingScreen 사용 가이드

## 개요

UIManager를 통해 LoadingScreen을 쉽게 사용할 수 있도록 다양한 편의 메서드들을 제공합니다. LoadingScreen은 파괴되지 않고 재사용되며, 필요할 때만 활성화/비활성화됩니다. UIManager는 게임 시작 시 자동으로 LoadingScreen을 초기화하고, 필요에 따라 자동으로 로딩 화면을 표시합니다. 이 가이드는 **DoTween 애니메이션**을 적용한 최신 버전으로, 부드럽고 전문적인 로딩 경험을 제공합니다.

## 주요 특징

- ✅ **재사용 가능**: LoadingScreen 인스턴스를 파괴하지 않고 재사용
- ✅ **메모리 효율적**: 한 번 생성된 인스턴스를 계속 사용
- ✅ **빠른 전환**: 인스턴스가 이미 존재하면 즉시 활성화
- ✅ **상태 초기화**: 재사용 시 이전 상태를 초기화
- ✅ **강제 재생성**: 문제 발생 시 강제로 재생성 가능
- ✅ **자동 초기화**: UIManager가 게임 시작 시 자동으로 LoadingScreen 초기화
- ✅ **자동 표시**: 설정에 따라 게임 시작 시 자동으로 로딩 화면 표시
- ✅ **DoTween 애니메이션**: 부드러운 진행률 바 애니메이션과 전환 효과
- ✅ **전문적인 UI**: 페이드, 스케일, 진행률 애니메이션이 포함된 현대적인 로딩 화면

## DoTween 애니메이션 기능

### 1. 진행률 바 애니메이션
- **부드러운 진행률**: 이징 곡선을 사용한 애니메이션 진행률 바
- **단계별 애니메이션**: 사용자 정의 지속 시간으로 점진적 진행률 업데이트
- **바운스 효과**: 완료 시 선택적 바운스 효과

### 2. UI 전환 효과
- **페이드 인/아웃**: 표시/숨김을 위한 부드러운 알파 전환
- **스케일 애니메이션**: 로딩 요소에 대한 스케일 효과
- **회전 효과**: 로딩 인디케이터를 위한 회전 애니메이션

### 3. 텍스트 애니메이션
- **타이핑 효과**: 문자별 텍스트 표시
- **페이드 전환**: 부드러운 텍스트 변경 애니메이션
- **색상 전환**: 상태 업데이트를 위한 애니메이션 색상 변경

## UIManager 자동 초기화

UIManager는 게임 시작 시 자동으로 LoadingScreen을 초기화하고 관리합니다:

### 1. 자동 초기화 과정

```csharp
// UIManager.Awake()에서 자동으로 실행되는 과정:
// 1. Addressable Canvas들 초기화
// 2. LoadingScreen 초기화 (DoTween이 포함된 새로운 패턴 사용)
if (loadingCanvas != null)
{
    await InitializeLoadingScreen();
}

// 3. 게임 시작 시 로딩 화면 표시 (설정에 따라)
if (showLoadingScreenOnStart)
{
    StartCoroutine(ShowInitialLoadingScreen());
}
```

### 2. 설정 옵션

UIManager에서 다음 설정을 통해 자동 동작을 제어할 수 있습니다:

```csharp
[SerializeField] private bool showLoadingScreenOnStart = true; // 게임 시작 시 로딩 화면 표시 여부
[SerializeField] private string initialLoadingMessage = "loading_data"; // 초기 로딩 메시지 키
[SerializeField] private float loadingScreenHideDelay = 1f; // 로딩 화면 숨김 지연 시간
[SerializeField] private bool useDoTweenAnimations = true; // DoTween 애니메이션 사용 여부
[SerializeField] private float animationDuration = 0.5f; // 기본 애니메이션 지속 시간
```

## 기본 사용법

### 1. 간단한 로딩 화면 표시

```csharp
// DoTween 애니메이션과 함께 기본 로딩 화면 표시
UIManager.Instance.ShowLoadingScreen("로딩 중...");

// 부드러운 애니메이션으로 진행률 업데이트
UIManager.Instance.SetLoadingProgress(0.5f);

// 애니메이션과 함께 로딩 완료
UIManager.Instance.FinishLoading();
```

### 2. 로컬라이제이션 사용

```csharp
// 로컬라이제이션 키로 로딩 화면 표시
UIManager.Instance.ShowLoadingScreenByKey("loading_data", "데이터를 불러오는 중...");

// 애니메이션과 함께 메시지 변경
UIManager.Instance.SetLoadingMessageByKey("loading_almost_done", "거의 완료되었습니다...");

// 애니메이션과 함께 완료
UIManager.Instance.FinishLoading();
```

### 3. 편의 메서드 사용

```csharp
// DoTween 페이드인과 함께 간단한 로딩 시작
UIManager.Instance.StartLoading("로딩 중...");

// 애니메이션과 함께 단계별 진행률 (25%, 50%, 75%, 100%)
UIManager.Instance.SetLoadingProgressStep(1); // 25% 애니메이션
UIManager.Instance.SetLoadingProgressStep(2); // 50% 애니메이션

// 부드러운 애니메이션과 함께 퍼센트로 진행률 설정
UIManager.Instance.SetLoadingProgressPercent(30); // 30% 애니메이션

// DoTween 애니메이션과 함께 로딩 완료
UIManager.Instance.FinishLoading();
```

### 4. 재사용 관련 메서드

```csharp
// LoadingScreen 상태 초기화 (재사용을 위해)
UIManager.Instance.ResetLoadingScreen();

// LoadingScreen 강제 재생성 (문제가 있을 때)
UIManager.Instance.RecreateLoadingScreen();

// LoadingScreen 즉시 활성화 (초기화 없이)
UIManager.Instance.ActivateLoadingScreen("즉시 표시");

// LoadingScreen 즉시 비활성화 (애니메이션 없이)
UIManager.Instance.DeactivateLoadingScreen();

// LoadingScreen 인스턴스 존재 여부 확인
bool hasInstance = UIManager.Instance.HasLoadingScreenInstance();
```

## 고급 사용법

### 1. 비동기 로딩 화면 표시

```csharp
// DoTween 애니메이션과 함께 비동기로 로딩 화면 표시
LoadingScreen loadingScreen = await UIManager.Instance.ShowLoadingScreenAsync("로딩 중...");

// 로딩 화면이 준비된 후 추가 설정
if (loadingScreen != null)
{
    loadingScreen.SetProgress(0.5f);
    loadingScreen.SetCenterMessage("데이터 로딩 중...");
}
```

### 2. 로컬라이제이션 + 비동기

```csharp
// 로컬라이제이션 키로 비동기 로딩 화면 표시
LoadingScreen loadingScreen = await UIManager.Instance.ShowLoadingScreenByKeyAsync("loading_data", "데이터를 불러오는 중...");
```

### 3. 상태 확인

```csharp
// 로딩 화면이 활성화되어 있는지 확인
if (UIManager.Instance.IsLoadingScreenActive())
{
    // 현재 진행률 가져오기
    float currentProgress = UIManager.Instance.GetCurrentLoadingProgress();
    Debug.Log($"현재 진행률: {currentProgress * 100:F1}%");
}
```

## DoTween 애니메이션 메서드

### 1. 진행률 애니메이션

```csharp
// 부드러운 진행률 애니메이션 (1초 동안 0%에서 50%로)
await UIManager.Instance.AnimateLoadingProgress(0.5f, 1f);

// 더 세밀한 애니메이션 (120단계로 나누어 애니메이션)
await UIManager.Instance.AnimateLoadingProgress(0.8f, 2f, 120);

// 커스텀 이징으로 진행률 애니메이션
await UIManager.Instance.AnimateLoadingProgressWithEasing(0.7f, 1.5f, Ease.OutBounce);
```

### 2. UI 전환 애니메이션

```csharp
// 페이드 인 애니메이션
await UIManager.Instance.FadeInLoadingScreen(0.5f);

// 페이드 아웃 애니메이션
await UIManager.Instance.FadeOutLoadingScreen(0.3f);

// 로딩 요소에 대한 스케일 애니메이션
await UIManager.Instance.ScaleLoadingElements(1.2f, 0.5f);

// 로딩 인디케이터 회전 애니메이션
UIManager.Instance.StartRotatingIndicator(2f); // 2초당 1회전
UIManager.Instance.StopRotatingIndicator();
```

### 3. 텍스트 애니메이션

```csharp
// 로딩 메시지에 대한 타이핑 효과
await UIManager.Instance.TypewriterLoadingMessage("데이터 로딩 중...", 0.05f);

// 텍스트 변경에 대한 페이드 전환
await UIManager.Instance.FadeLoadingMessage("거의 완료되었습니다...", 0.3f);

// 상태 업데이트에 대한 색상 전환
await UIManager.Instance.ColorTransitionMessage("완료!", Color.green, 0.5f);
```

### 4. 로딩 완료 애니메이션

```csharp
// 로딩 완료 애니메이션 (0.5초 동안 100%까지 애니메이션 후 자동 숨김)
await UIManager.Instance.AnimateLoadingComplete("완료!", 0.5f);

// 로컬라이제이션 키로 완료 애니메이션
await UIManager.Instance.AnimateLoadingCompleteByKey("loading_complete", "완료!", 0.5f);

// 바운스 효과가 포함된 완료 애니메이션
await UIManager.Instance.AnimateLoadingCompleteWithBounce("완료!", 0.5f, 1.2f);
```

## 편의 메서드들

### 1. 게임 로딩 시나리오

```csharp
// DoTween 애니메이션과 함께 미리 정의된 게임 로딩 시나리오 실행
await UIManager.Instance.ExecuteGameLoadingScenario();
```

이 메서드는 부드러운 애니메이션과 함께 다음 단계를 자동으로 실행합니다:
1. 게임 시작 로딩 (페이드 인)
2. 데이터 로딩 (20% 진행률 애니메이션)
3. 리소스 로딩 (40% 진행률 애니메이션)
4. 씬 로딩 (60% 진행률 애니메이션)
5. 초기화 (80% 진행률 애니메이션)
6. 완료 (100% 바운스 효과 및 페이드 아웃)

### 2. 단계별 로딩 진행

```csharp
// DoTween 애니메이션과 함께 커스텀 단계별 로딩
string[] messages = { "초기화 중...", "데이터 로딩 중...", "리소스 준비 중...", "완료!" };
float[] progressValues = { 0.25f, 0.5f, 0.75f, 1.0f };
int[] delays = { 1000, 1500, 1000, 500 }; // 각 단계별 지연 시간 (밀리초)

await UIManager.Instance.ExecuteStepLoading(messages, progressValues, delays);
```

### 3. 로컬라이제이션 키를 사용한 단계별 로딩

```csharp
// 로컬라이제이션 키로 단계별 로딩
string[] keys = { "loading_init", "loading_data", "loading_resources", "loading_complete" };
string[] fallbacks = { "초기화 중...", "데이터 로딩 중...", "리소스 준비 중...", "완료!" };
float[] progressValues = { 0.25f, 0.5f, 0.75f, 1.0f };

await UIManager.Instance.ExecuteStepLoadingByKeys(keys, fallbacks, progressValues);
```

### 4. 고급 진행률 애니메이션

```csharp
// 커스텀 이징으로 부드러운 진행률 애니메이션
await UIManager.Instance.AnimateLoadingProgressWithEasing(0.5f, 1f, Ease.OutCubic);

// 콜백이 포함된 진행률 애니메이션
await UIManager.Instance.AnimateLoadingProgressWithCallback(0.8f, 1.5f, (progress) => {
    Debug.Log($"진행률: {progress * 100:F1}%");
});

// 진행률 바 펄스 효과
UIManager.Instance.StartPulsingProgressBar(1f, 0.1f); // 1초 주기, 0.1 스케일 변경
UIManager.Instance.StopPulsingProgressBar();
```

## 실제 사용 시나리오

### 1. 게임 시작 시 로딩

```csharp
public async void StartGame()
{
    // DoTween 페이드인과 함께 로딩 시작
    UIManager.Instance.StartLoadingByKey("loading_game_start", "게임을 시작합니다...");
    
    // 진행률 애니메이션과 함께 게임 데이터 로드
    await LoadGameData();
    await UIManager.Instance.AnimateLoadingProgress(0.3f, 0.5f);
    UIManager.Instance.SetLoadingMessageByKey("loading_data", "데이터를 불러오는 중...");
    
    // 진행률 애니메이션과 함께 리소스 로드
    await LoadResources();
    await UIManager.Instance.AnimateLoadingProgress(0.6f, 0.5f);
    UIManager.Instance.SetLoadingMessageByKey("loading_resources", "리소스를 불러오는 중...");
    
    // 진행률 애니메이션과 함께 씬 전환
    await LoadScene();
    await UIManager.Instance.AnimateLoadingProgress(0.9f, 0.5f);
    UIManager.Instance.SetLoadingMessageByKey("loading_scene", "씬을 불러오는 중...");
    
    // 바운스 애니메이션과 함께 완료
    await UIManager.Instance.AnimateLoadingCompleteWithBounce("완료!", 0.5f, 1.2f);
}
```

### 2. 데이터 다운로드 시 로딩

```csharp
public async void DownloadData()
{
    // 페이드인 애니메이션과 함께 로딩 시작
    UIManager.Instance.StartLoading("데이터 다운로드 중...");
    
    // 부드러운 애니메이션과 함께 다운로드 진행률 모니터링
    var downloadTask = DownloadDataAsync();
    
    while (!downloadTask.IsCompleted)
    {
        float progress = GetDownloadProgress();
        await UIManager.Instance.AnimateLoadingProgress(progress, 0.1f);
        UIManager.Instance.SetLoadingMessage($"다운로드 중... {progress * 100:F1}%");
        await System.Threading.Tasks.Task.Delay(100);
    }
    
    // 타이핑 효과와 함께 완료
    await UIManager.Instance.TypewriterLoadingMessage("다운로드 완료!", 0.05f);
    await UIManager.Instance.AnimateLoadingComplete("다운로드 완료!");
}
```

### 3. 복잡한 로딩 시나리오

```csharp
public async void ComplexLoadingScenario()
{
    // 1단계: 페이드인과 함께 초기화
    await UIManager.Instance.ShowLoadingScreenByKeyAsync("loading_init", "초기화 중...");
    await InitializeSystem();
    await UIManager.Instance.AnimateLoadingProgressWithEasing(0.2f, 0.5f, Ease.OutQuad);
    
    // 2단계: 진행률 애니메이션과 함께 데이터 로드
    UIManager.Instance.SetLoadingMessageByKey("loading_data", "데이터를 불러오는 중...");
    await LoadUserData();
    await UIManager.Instance.AnimateLoadingProgressWithEasing(0.4f, 0.5f, Ease.OutQuad);
    
    // 3단계: 진행률 애니메이션과 함께 리소스 로드
    UIManager.Instance.SetLoadingMessageByKey("loading_resources", "리소스를 불러오는 중...");
    await LoadGameResources();
    await UIManager.Instance.AnimateLoadingProgressWithEasing(0.6f, 0.5f, Ease.OutQuad);
    
    // 4단계: 진행률 애니메이션과 함께 설정 로드
    UIManager.Instance.SetLoadingMessageByKey("loading_settings", "설정을 불러오는 중...");
    await LoadSettings();
    await UIManager.Instance.AnimateLoadingProgressWithEasing(0.8f, 0.5f, Ease.OutQuad);
    
    // 5단계: 바운스 효과와 함께 완료
    UIManager.Instance.SetLoadingMessageByKey("loading_complete", "완료!");
    await UIManager.Instance.AnimateLoadingProgressWithBounce(1f, 0.5f, 1.2f);
    
    // 페이드아웃과 함께 로딩 화면 숨기기
    await UIManager.Instance.FadeOutLoadingScreen(0.3f);
}
```

## DoTween 설정

### 1. 애니메이션 설정

```csharp
// DoTween 애니메이션 설정 구성
UIManager.Instance.SetAnimationSettings(
    defaultDuration: 0.5f,           // 기본 애니메이션 지속 시간
    progressEasing: Ease.OutCubic,   // 진행률 바 이징
    fadeEasing: Ease.InOutQuad,      // 페이드 애니메이션 이징
    scaleEasing: Ease.OutBack,       // 스케일 애니메이션 이징
    enableBounce: true,              // 바운스 효과 활성화
    bounceScale: 1.2f                // 바운스 스케일 배수
);
```

### 2. 커스텀 이징 함수

```csharp
// 특정 애니메이션에 커스텀 이징 사용
await UIManager.Instance.AnimateLoadingProgressWithEasing(0.5f, 1f, Ease.OutBounce);
await UIManager.Instance.AnimateLoadingProgressWithEasing(0.8f, 1f, Ease.InOutCubic);
await UIManager.Instance.AnimateLoadingProgressWithEasing(1f, 1f, Ease.OutElastic);
```

### 3. 애니메이션 시퀀스

```csharp
// 복잡한 애니메이션 시퀀스 생성
await UIManager.Instance.ExecuteAnimationSequence(new LoadingAnimationStep[] {
    new LoadingAnimationStep { type = AnimationType.FadeIn, duration = 0.3f },
    new LoadingAnimationStep { type = AnimationType.Progress, targetProgress = 0.3f, duration = 0.5f },
    new LoadingAnimationStep { type = AnimationType.Message, message = "로딩 중...", duration = 0.2f },
    new LoadingAnimationStep { type = AnimationType.Progress, targetProgress = 0.7f, duration = 0.8f },
    new LoadingAnimationStep { type = AnimationType.Bounce, duration = 0.3f },
    new LoadingAnimationStep { type = AnimationType.FadeOut, duration = 0.4f }
});
```

## API 참조

### 기본 메서드

- `ShowLoadingScreen(string message)` - DoTween 페이드인과 함께 로딩 화면 표시
- `ShowLoadingScreenByKey(string key, string fallback)` - 로컬라이제이션 키로 표시
- `ShowLoadingScreenAsync(string message)` - 비동기 로딩 화면 표시
- `ShowLoadingScreenByKeyAsync(string key, string fallback)` - 로컬라이제이션 키로 비동기 표시
- `HideLoadingScreen()` - DoTween 페이드아웃과 함께 로딩 화면 숨기기

### 편의 메서드

- `StartLoading(string message)` - 로딩 시작 (페이드인과 함께 0%로 초기화)
- `StartLoadingByKey(string key, string fallback)` - 로컬라이제이션 키로 로딩 시작
- `FinishLoading()` - 로딩 완료 (애니메이션과 함께 100%로 설정 후 숨김)

### 진행률 설정 메서드

- `SetLoadingProgress(float progress)` - 애니메이션과 함께 진행률 설정 (0.0 ~ 1.0)
- `SetLoadingProgressPercent(int percent)` - 애니메이션과 함께 퍼센트로 진행률 설정
- `SetLoadingProgressStep(int step)` - 애니메이션과 함께 단계별 진행률 (1=25%, 2=50%, 3=75%, 4=100%)

### 메시지 설정 메서드

- `SetLoadingMessage(string message)` - 페이드 전환과 함께 메시지 설정
- `SetLoadingMessageByKey(string key, string fallback)` - 로컬라이제이션 키로 메시지 설정

### DoTween 애니메이션 메서드

- `AnimateLoadingProgress(float targetProgress, float duration, int steps)` - 진행률 바 애니메이션
- `AnimateLoadingProgressWithEasing(float targetProgress, float duration, Ease easing)` - 커스텀 이징으로 애니메이션
- `AnimateLoadingProgressWithCallback(float targetProgress, float duration, Action<float> callback)` - 콜백이 포함된 애니메이션
- `FadeInLoadingScreen(float duration)` - 페이드 인 애니메이션
- `FadeOutLoadingScreen(float duration)` - 페이드 아웃 애니메이션
- `ScaleLoadingElements(float scale, float duration)` - 스케일 애니메이션
- `StartRotatingIndicator(float duration)` - 회전 애니메이션 시작
- `StopRotatingIndicator()` - 회전 애니메이션 중지
- `TypewriterLoadingMessage(string message, float charDelay)` - 타이핑 효과
- `FadeLoadingMessage(string message, float duration)` - 페이드 텍스트 전환
- `ColorTransitionMessage(string message, Color color, float duration)` - 색상 전환
- `AnimateLoadingComplete(string message, float duration)` - 완료 애니메이션
- `AnimateLoadingCompleteWithBounce(string message, float duration, float bounceScale)` - 바운스가 포함된 완료
- `StartPulsingProgressBar(float cycleDuration, float scaleChange)` - 펄스 효과 시작
- `StopPulsingProgressBar()` - 펄스 효과 중지

### 상태 확인 메서드

- `IsLoadingScreenActive()` - 로딩 화면이 활성화되어 있는지 확인
- `GetCurrentLoadingProgress()` - 현재 로딩 진행률 가져오기
- `GetCurrentLoadingScreen()` - 현재 활성화된 LoadingScreen 가져오기
- `HasLoadingScreenInstance()` - LoadingScreen 인스턴스가 존재하는지 확인

### 재사용 관련 메서드

- `ResetLoadingScreen()` - LoadingScreen 상태 초기화 (재사용을 위해)
- `RecreateLoadingScreen()` - LoadingScreen 강제 재생성 (문제가 있을 때)
- `ActivateLoadingScreen(string message)` - LoadingScreen 즉시 활성화 (초기화 없이)
- `DeactivateLoadingScreen()` - LoadingScreen 즉시 비활성화 (애니메이션 없이)

### 고급 편의 메서드

- `ExecuteGameLoadingScenario()` - 미리 정의된 게임 로딩 시나리오 실행
- `ExecuteStepLoading(string[] messages, float[] progressValues, int[] delays)` - 단계별 로딩 진행
- `ExecuteStepLoadingByKeys(string[] keys, string[] fallbacks, float[] progressValues, int[] delays)` - 로컬라이제이션 키로 단계별 로딩
- `SetAnimationSettings(float defaultDuration, Ease progressEasing, Ease fadeEasing, Ease scaleEasing, bool enableBounce, float bounceScale)` - 애니메이션 설정 구성
- `ExecuteAnimationSequence(LoadingAnimationStep[] steps)` - 복잡한 애니메이션 시퀀스 실행

## DoTween 요구사항

### 1. DoTween 설치

프로젝트에 DoTween이 설치되어 있는지 확인하세요:
- **Package Manager**: "DOTween" 검색 후 설치
- **Asset Store**: Unity Asset Store에서 다운로드
- **수동 설치**: DoTween 웹사이트에서 다운로드

### 2. DoTween 설정

```csharp
// DoTween 초기화 (보통 Awake 또는 Start에서)
using DG.Tweening;

void Awake()
{
    // DoTween 초기화
    DOTween.Init();
    
    // 선택사항: 전역 설정
    DOTween.defaultEaseType = Ease.OutCubic;
    DOTween.defaultAutoKill = true;
}
```

### 3. DoTween 심볼

DoTween을 사용할 수 없는 경우, 시스템은 기본 애니메이션으로 대체됩니다:

```csharp
// DoTween 사용 가능 여부 확인
#if DOTWEEN_AVAILABLE
    // DoTween 애니메이션 사용
    progressBar.DOScale(1.2f, 0.5f).SetEase(Ease.OutBounce);
#else
    // 기본 애니메이션으로 대체
    progressBar.transform.localScale = Vector3.one * 1.2f;
#endif
```

## 주의사항

1. **UIManager 초기화**: UIManager가 초기화된 후에 사용해야 합니다.
2. **로컬라이제이션**: LocalizationManager가 초기화되어야 로컬라이제이션 기능이 작동합니다.
3. **비동기 처리**: 비동기 메서드는 `await` 키워드와 함께 사용해야 합니다.
4. **메모리 관리**: 로딩 화면은 자동으로 관리되므로 수동으로 해제할 필요가 없습니다.
5. **재사용**: LoadingScreen은 파괴되지 않고 재사용되므로, 상태 초기화가 필요할 때는 `ResetLoadingScreen()`을 사용하세요.
6. **강제 재생성**: 문제가 발생할 경우 `RecreateLoadingScreen()`으로 강제 재생성할 수 있습니다.
7. **자동 초기화**: UIManager는 게임 시작 시 자동으로 LoadingScreen을 초기화합니다.
8. **자동 표시 설정**: UIManager의 `showLoadingScreenOnStart` 필드를 통해 게임 시작 시 자동 로딩 화면 표시를 제어할 수 있습니다.
9. **DoTween 의존성**: 고급 애니메이션이 작동하려면 DoTween이 설치되어 있어야 합니다.
10. **성능**: DoTween 애니메이션은 성능에 최적화되어 있지만, 너무 많은 동시 애니메이션은 피하세요.

## 예시 코드

더 자세한 사용 예시는 각 메서드의 XML 문서 주석을 참조하세요. 모든 메서드에는 상세한 설명과 사용 예시가 포함되어 있습니다.

---

**버전**: 2.2  
**최종 업데이트**: 2025년 9월 15일  
**Unity 버전**: 2022.3 LTS 이상  
**주요 기능**: 재사용 가능한 LoadingScreen, 자동 초기화, DoTween 애니메이션, 전문적인 UI 전환