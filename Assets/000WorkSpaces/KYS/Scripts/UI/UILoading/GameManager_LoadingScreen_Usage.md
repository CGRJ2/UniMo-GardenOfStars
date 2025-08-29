# GameManager LoadingScreen 완전체 사용 가이드

## 개요

GameManager에서 LoadingScreen을 사용하는 완전체 구성이 구현되었습니다. 이 시스템은 다음과 같은 기능을 제공합니다:

- **로컬라이제이션 지원**: 다국어 메시지 키 사용
- **부드러운 애니메이션**: 진행률 애니메이션과 메시지 전환
- **자동 숨김**: 로딩 완료 시 자동으로 LoadingScreen 숨김
- **다양한 로딩 시나리오**: 완전체, 간단, 스테이지별 로딩 지원

## 주요 메서드

### 1. 완전체 로딩 시스템

```csharp
public IEnumerator LoadSceneWithCompleteLoading(string sceneName, string prepareKey, string loadingKey, string completeKey)
```

**특징:**
- 5단계 로딩 프로세스 (준비 → 로드 시작 → 진행률 모니터링 → 완료 대기 → 완료 메시지)
- 부드러운 진행률 애니메이션
- 자동 숨김 기능
- 로컬라이제이션 키 지원

**사용 예시:**
```csharp
// 기본 사용
yield return StartCoroutine(LoadSceneWithCompleteLoading("StageScene", 
    LoadingLocalizationKeys.LOADING_PREPARE, 
    LoadingLocalizationKeys.LOADING_PROGRESS, 
    LoadingLocalizationKeys.LOADING_COMPLETE));

// 스테이지 로딩
yield return StartCoroutine(LoadSceneWithCompleteLoading("StageScene", 
    LoadingLocalizationKeys.STAGE_PREPARE, 
    LoadingLocalizationKeys.STAGE_LOADING, 
    LoadingLocalizationKeys.STAGE_COMPLETE));
```

### 2. 간단한 로딩 시스템

```csharp
public IEnumerator LoadSceneSimple(string sceneName, string messageKey)
```

**특징:**
- 단순한 로딩 프로세스
- 실시간 진행률 업데이트
- 수동 숨김 필요

**사용 예시:**
```csharp
yield return StartCoroutine(LoadSceneSimple("StageScene", LoadingLocalizationKeys.LOADING_PROGRESS));
```

### 3. 스테이지별 로딩

```csharp
public IEnumerator LoadStage(string stageId)
```

**특징:**
- 스테이지 ID별 커스텀 메시지
- 완전체 로딩 시스템 기반
- 자동 스테이지 ID 설정

**사용 예시:**
```csharp
yield return StartCoroutine(LoadStage("Stage00"));
```

## 로컬라이제이션 키

### 기본 키들

```csharp
// 기본 로딩 메시지
LoadingLocalizationKeys.LOADING_PREPARE      // "준비 중..."
LoadingLocalizationKeys.LOADING_PROGRESS     // "로딩 중..."
LoadingLocalizationKeys.LOADING_COMPLETE     // "완료 중..."
LoadingLocalizationKeys.LOADING_SUCCESS      // "로딩이 완료되었습니다!"

// 스테이지 로딩 메시지
LoadingLocalizationKeys.STAGE_PREPARE        // "스테이지를 준비하는 중..."
LoadingLocalizationKeys.STAGE_LOADING        // "스테이지를 로딩하는 중..."
LoadingLocalizationKeys.STAGE_COMPLETE       // "스테이지 로딩 완료!"

// 빠른 로딩 메시지
LoadingLocalizationKeys.LOADING_FAST_PREPARE // "빠른 로딩 준비 중..."
LoadingLocalizationKeys.LOADING_FAST_PROGRESS // "빠른 로딩 중..."
LoadingLocalizationKeys.LOADING_FAST_COMPLETE // "빠른 로딩 완료!"
```

### 동적 키 생성

```csharp
// 스테이지별 메시지 키 생성
string prepareKey = LoadingLocalizationKeys.GetStagePrepareKey("Stage00");  // "stage_Stage00_prepare"
string loadingKey = LoadingLocalizationKeys.GetStageLoadingKey("Stage00");  // "stage_Stage00_loading"
string completeKey = LoadingLocalizationKeys.GetStageCompleteKey("Stage00"); // "stage_Stage00_complete"
```

## 테스트 메서드

Unity Inspector에서 ContextMenu를 통해 테스트할 수 있습니다:

### 1. 완전체 로딩 테스트
```csharp
[ContextMenu("완전체 로딩 테스트")]
public void TestCompleteLoading()
```

### 2. 간단 로딩 테스트
```csharp
[ContextMenu("간단 로딩 테스트")]
public void TestSimpleLoading()
```

### 3. 스테이지 로딩 테스트
```csharp
[ContextMenu("스테이지 로딩 테스트")]
public void TestStageLoading()
```

### 4. 빠른 애니메이션 로딩 테스트
```csharp
[ContextMenu("빠른 애니메이션 로딩 테스트")]
public void TestFastAnimationLoading()
```

## 실제 사용 예시

### 타이틀에서 인게임으로 이동

```csharp
public IEnumerator Temp_InGameLoad()
{
    Manager.game.curStageId = "Stage00";
    
    // 완전체 로딩 시스템 사용 (자동 숨김 포함)
    yield return StartCoroutine(LoadSceneWithCompleteLoading("StageScene", 
        LoadingLocalizationKeys.STAGE_PREPARE, 
        LoadingLocalizationKeys.STAGE_LOADING, 
        LoadingLocalizationKeys.STAGE_COMPLETE));
}
```

### 스테이지 간 이동

```csharp
public void LoadNextStage(string currentStageId, string nextStageId)
{
    // 현재 스테이지 완료 처리
    Manager.game.StageUnlock(currentStageId);
    
    // 다음 스테이지 로드
    StartCoroutine(LoadStage(nextStageId));
}
```

### 커스텀 로딩 시나리오

```csharp
public IEnumerator LoadCustomScene(string sceneName, string[] customMessages)
{
    // 커스텀 메시지로 완전체 로딩
    yield return StartCoroutine(LoadSceneWithCompleteLoading(sceneName, 
        customMessages[0],  // 준비 메시지
        customMessages[1],  // 로딩 메시지
        customMessages[2]   // 완료 메시지
    ));
}
```

## 로딩 프로세스 단계

### 완전체 로딩 시스템 (5단계)

1. **준비 단계** (0.2초)
   - 준비 메시지 표시
   - 부드러운 애니메이션 (0.3초)

2. **씬 로드 시작**
   - Addressables.LoadSceneAsync 호출

3. **로딩 진행률 모니터링** (0.8초)
   - 로딩 메시지 표시
   - 부드러운 애니메이션 (0.4초)

4. **씬 로드 완료 대기**
   - 실시간 진행률 업데이트
   - `loadSceneHandle.PercentComplete` 사용

5. **완료 메시지 표시 및 자동 숨김** (0.5초)
   - 완료 메시지 표시
   - 자동으로 LoadingScreen 숨김
   - 성공 메시지 표시

## 주의사항

1. **로컬라이제이션 키 등록**: 사용하는 모든 키가 LocalizationManager에 등록되어 있어야 합니다.

2. **폴백 메시지**: 로컬라이제이션 실패 시 `LoadingLocalizationKeys.FallbackMessages`의 메시지가 사용됩니다.

3. **애니메이션 시간**: 각 단계의 애니메이션 시간을 조정하여 사용자 경험을 최적화할 수 있습니다.

4. **자동 숨김**: `autoHide: true`로 설정하면 로딩 완료 시 자동으로 LoadingScreen이 숨겨집니다.

5. **진행률 업데이트**: `SetLoadingProgressAnimated`를 사용하여 부드러운 진행률 애니메이션을 제공합니다.

## 확장 가능성

이 시스템은 다음과 같이 확장할 수 있습니다:

- **에러 처리**: 로딩 실패 시 에러 메시지 표시
- **타임아웃**: 로딩 시간 초과 시 재시도 옵션 제공
- **다양한 애니메이션**: 씬별로 다른 애니메이션 효과 적용
- **사운드 효과**: 로딩 단계별 사운드 재생
- **진행률 표시 방식**: 퍼센트, 바, 원형 등 다양한 UI 요소 지원
