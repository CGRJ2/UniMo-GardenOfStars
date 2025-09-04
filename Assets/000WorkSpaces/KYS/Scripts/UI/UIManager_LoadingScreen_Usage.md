# UIManager LoadingScreen 사용 가이드

## 개요

UIManager를 통해 LoadingScreen을 쉽게 사용할 수 있도록 다양한 편의 메서드들을 제공합니다. LoadingScreen은 파괴되지 않고 재사용되며, 필요할 때만 활성화/비활성화됩니다. UIManager는 게임 시작 시 자동으로 LoadingScreen을 초기화하고, 필요에 따라 자동으로 로딩 화면을 표시합니다.

## 주요 특징

- ✅ **재사용 가능**: LoadingScreen 인스턴스를 파괴하지 않고 재사용
- ✅ **메모리 효율적**: 한 번 생성된 인스턴스를 계속 사용
- ✅ **빠른 전환**: 인스턴스가 이미 존재하면 즉시 활성화
- ✅ **상태 초기화**: 재사용 시 이전 상태를 초기화
- ✅ **강제 재생성**: 문제 발생 시 강제로 재생성 가능
- ✅ **자동 초기화**: UIManager가 게임 시작 시 자동으로 LoadingScreen 초기화
- ✅ **자동 표시**: 설정에 따라 게임 시작 시 자동으로 로딩 화면 표시

## UIManager 자동 초기화

UIManager는 게임 시작 시 자동으로 LoadingScreen을 초기화하고 관리합니다:

### 1. 자동 초기화 과정

```csharp
// UIManager.Awake()에서 자동으로 실행되는 과정:
// 1. Addressable Canvas들 초기화
// 2. LoadingScreen 초기화 (새로운 패턴 사용)
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
```

## 기본 사용법

### 1. 간단한 로딩 화면 표시

```csharp
// 기본 로딩 화면 표시
UIManager.Instance.ShowLoadingScreen("로딩 중...");

// 진행률 업데이트
UIManager.Instance.SetLoadingProgress(0.5f);

// 로딩 완료
UIManager.Instance.FinishLoading();
```

### 2. 로컬라이제이션 사용

```csharp
// 로컬라이제이션 키로 로딩 화면 표시
UIManager.Instance.ShowLoadingScreenByKey("loading_data", "데이터를 불러오는 중...");

// 메시지 변경
UIManager.Instance.SetLoadingMessageByKey("loading_almost_done", "거의 완료되었습니다...");

// 완료
UIManager.Instance.FinishLoading();
```

### 3. 편의 메서드 사용

```csharp
// 간단한 로딩 시작
UIManager.Instance.StartLoading("로딩 중...");

// 단계별 진행률 (25%, 50%, 75%, 100%)
UIManager.Instance.SetLoadingProgressStep(1); // 25%
UIManager.Instance.SetLoadingProgressStep(2); // 50%

// 퍼센트로 진행률 설정
UIManager.Instance.SetLoadingProgressPercent(30); // 30%

// 로딩 완료
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
// 비동기로 로딩 화면 표시
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

## 편의 메서드들

### 1. 게임 로딩 시나리오

```csharp
// 미리 정의된 게임 로딩 시나리오 실행
await UIManager.Instance.ExecuteGameLoadingScenario();
```

이 메서드는 다음 단계를 자동으로 실행합니다:
1. 게임 시작 로딩
2. 데이터 로딩 (20%)
3. 리소스 로딩 (40%)
4. 씬 로딩 (60%)
5. 초기화 (80%)
6. 완료 (100%)

### 2. 단계별 로딩 진행

```csharp
// 커스텀 단계별 로딩
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

### 4. 진행률 애니메이션

```csharp
// 부드러운 진행률 애니메이션 (1초 동안 0%에서 50%로)
await UIManager.Instance.AnimateLoadingProgress(0.5f, 1f);

// 더 세밀한 애니메이션 (120단계로 나누어 애니메이션)
await UIManager.Instance.AnimateLoadingProgress(0.8f, 2f, 120);
```

### 5. 로딩 완료 애니메이션

```csharp
// 로딩 완료 애니메이션 (0.5초 동안 100%까지 애니메이션 후 자동 숨김)
await UIManager.Instance.AnimateLoadingComplete("완료!", 0.5f);

// 로컬라이제이션 키로 완료 애니메이션
await UIManager.Instance.AnimateLoadingCompleteByKey("loading_complete", "완료!", 0.5f);
```

## 실제 사용 시나리오

### 1. 게임 시작 시 로딩

```csharp
public async void StartGame()
{
    // 로딩 시작
    UIManager.Instance.StartLoadingByKey("loading_game_start", "게임을 시작합니다...");
    
    // 게임 데이터 로드
    await LoadGameData();
    UIManager.Instance.SetLoadingProgressPercent(30);
    UIManager.Instance.SetLoadingMessageByKey("loading_data", "데이터를 불러오는 중...");
    
    // 리소스 로드
    await LoadResources();
    UIManager.Instance.SetLoadingProgressPercent(60);
    UIManager.Instance.SetLoadingMessageByKey("loading_resources", "리소스를 불러오는 중...");
    
    // 씬 전환
    await LoadScene();
    UIManager.Instance.SetLoadingProgressPercent(90);
    UIManager.Instance.SetLoadingMessageByKey("loading_scene", "씬을 불러오는 중...");
    
    // 완료
    await UIManager.Instance.AnimateLoadingCompleteByKey("loading_complete", "완료!");
}
```

### 2. 데이터 다운로드 시 로딩

```csharp
public async void DownloadData()
{
    // 로딩 시작
    UIManager.Instance.StartLoading("데이터 다운로드 중...");
    
    // 다운로드 진행률 모니터링
    var downloadTask = DownloadDataAsync();
    
    while (!downloadTask.IsCompleted)
    {
        float progress = GetDownloadProgress();
        UIManager.Instance.SetLoadingProgress(progress);
        UIManager.Instance.SetLoadingMessage($"다운로드 중... {progress * 100:F1}%");
        await System.Threading.Tasks.Task.Delay(100);
    }
    
    // 완료
    await UIManager.Instance.AnimateLoadingComplete("다운로드 완료!");
}
```

### 3. 복잡한 로딩 시나리오

```csharp
public async void ComplexLoadingScenario()
{
    // 1단계: 초기화
    await UIManager.Instance.ShowLoadingScreenByKeyAsync("loading_init", "초기화 중...");
    await InitializeSystem();
    await UIManager.Instance.AnimateLoadingProgress(0.2f, 0.5f);
    
    // 2단계: 데이터 로드
    UIManager.Instance.SetLoadingMessageByKey("loading_data", "데이터를 불러오는 중...");
    await LoadUserData();
    await UIManager.Instance.AnimateLoadingProgress(0.4f, 0.5f);
    
    // 3단계: 리소스 로드
    UIManager.Instance.SetLoadingMessageByKey("loading_resources", "리소스를 불러오는 중...");
    await LoadGameResources();
    await UIManager.Instance.AnimateLoadingProgress(0.6f, 0.5f);
    
    // 4단계: 설정 로드
    UIManager.Instance.SetLoadingMessageByKey("loading_settings", "설정을 불러오는 중...");
    await LoadSettings();
    await UIManager.Instance.AnimateLoadingProgress(0.8f, 0.5f);
    
    // 5단계: 완료
    UIManager.Instance.SetLoadingMessageByKey("loading_complete", "완료!");
    await UIManager.Instance.AnimateLoadingProgress(1f, 0.5f);
    
    // 로딩 화면 숨기기
    UIManager.Instance.HideLoadingScreen();
}
```

## API 참조

### 기본 메서드

- `ShowLoadingScreen(string message)` - 로딩 화면 표시
- `ShowLoadingScreenByKey(string key, string fallback)` - 로컬라이제이션 키로 표시
- `ShowLoadingScreenAsync(string message)` - 비동기 로딩 화면 표시
- `ShowLoadingScreenByKeyAsync(string key, string fallback)` - 로컬라이제이션 키로 비동기 표시
- `HideLoadingScreen()` - 로딩 화면 숨기기

### 편의 메서드

- `StartLoading(string message)` - 로딩 시작 (0%로 초기화)
- `StartLoadingByKey(string key, string fallback)` - 로컬라이제이션 키로 로딩 시작
- `FinishLoading()` - 로딩 완료 (100%로 설정 후 숨김)

### 진행률 설정 메서드

- `SetLoadingProgress(float progress)` - 진행률 설정 (0.0 ~ 1.0)
- `SetLoadingProgressPercent(int percent)` - 퍼센트로 진행률 설정 (0-100)
- `SetLoadingProgressStep(int step)` - 단계별 진행률 (1=25%, 2=50%, 3=75%, 4=100%)

### 메시지 설정 메서드

- `SetLoadingMessage(string message)` - 메시지 설정
- `SetLoadingMessageByKey(string key, string fallback)` - 로컬라이제이션 키로 메시지 설정

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
- `AnimateLoadingProgress(float targetProgress, float duration, int steps)` - 진행률 애니메이션
- `AnimateLoadingComplete(string message, float duration)` - 로딩 완료 애니메이션
- `AnimateLoadingCompleteByKey(string key, string fallback, float duration)` - 로컬라이제이션 키로 완료 애니메이션

## 주의사항

1. **UIManager 초기화**: UIManager가 초기화된 후에 사용해야 합니다.
2. **로컬라이제이션**: LocalizationManager가 초기화되어야 로컬라이제이션 기능이 작동합니다.
3. **비동기 처리**: 비동기 메서드는 `await` 키워드와 함께 사용해야 합니다.
4. **메모리 관리**: 로딩 화면은 자동으로 관리되므로 수동으로 해제할 필요가 없습니다.
5. **재사용**: LoadingScreen은 파괴되지 않고 재사용되므로, 상태 초기화가 필요할 때는 `ResetLoadingScreen()`을 사용하세요.
6. **강제 재생성**: 문제가 발생할 경우 `RecreateLoadingScreen()`으로 강제 재생성할 수 있습니다.
7. **자동 초기화**: UIManager는 게임 시작 시 자동으로 LoadingScreen을 초기화합니다.
8. **자동 표시 설정**: UIManager의 `showLoadingScreenOnStart` 필드를 통해 게임 시작 시 자동 로딩 화면 표시를 제어할 수 있습니다.

## 예시 코드

더 자세한 사용 예시는 각 메서드의 XML 문서 주석을 참조하세요. 모든 메서드에는 상세한 설명과 사용 예시가 포함되어 있습니다.
