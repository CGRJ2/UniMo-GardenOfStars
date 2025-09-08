# ChoicePanel 사용 가이드

## 📋 개요

`ChoicePanel`은 대화 시스템에서 선택지를 표시하는 UI 컴포넌트입니다. **풀링 시스템**을 사용하여 성능을 최적화하고, `Destroy` 대신 `SetActive(false)`를 사용하여 선택지를 효율적으로 관리합니다.

## 🚀 주요 기능

### 1. **풀링 시스템 (Pooling System)**
- 선택지 버튼을 미리 생성하여 재사용
- 메모리 할당/해제 최소화
- 빠른 선택지 표시/숨김

### 2. **SetActive(false) 기반 관리**
- `Destroy` 대신 `SetActive(false)` 사용
- 선택지 재사용 가능
- 메모리 누수 방지

### 3. **고급 선택지 제어**
- 개별 선택지 활성화/비활성화
- 선택지 상호작용 제어
- 동적 선택지 변경

## ⚙️ 설정

### Inspector 설정
```csharp
[Header("Choice Settings")]
[SerializeField] private GameObject choiceButtonPrefab;    // 선택지 버튼 프리팹
[SerializeField] private int maxChoices = 4;              // 최대 선택지 개수
[SerializeField] private int initialPoolSize = 8;         // 초기 풀 크기
```

### UI Element Names
```csharp
[Header("UI Element Names")]
[SerializeField] private string choiceContentName = "ChoiceContent";        // 선택지 컨테이너
[SerializeField] private string choiceButtonPrefabName = "ChoiceButton";    // 선택지 버튼 프리팹
```

## 📖 기본 사용법

### 1. 선택지 설정
```csharp
// 기본 사용법
choicePanel.SetupChoices(choices, OnChoiceSelected);

// 예시
string[] choices = { "예", "아니오", "취소" };
choicePanel.SetupChoices(choices, (index) => {
    Debug.Log($"선택됨: {index}");
});
```

### 2. 선택지 숨기기
```csharp
// SetActive(false) 사용 (권장)
choicePanel.ClearChoices();

// 완전 제거 (메모리 정리 시에만)
choicePanel.DestroyAllChoices();
```

### 3. 선택지 상태 확인
```csharp
int choiceCount = choicePanel.GetActiveChoiceCount();
bool hasChoices = choicePanel.HasActiveChoices();
```

## 🔧 고급 기능

### 1. 개별 선택지 제어
```csharp
// 특정 선택지 비활성화
choicePanel.SetChoiceButtonActive(1, false);

// 모든 선택지 상호작용 비활성화
choicePanel.SetChoiceButtonsInteractable(false);
```

### 2. 동적 선택지 변경
```csharp
// 첫 번째 선택지 세트
choicePanel.SetupChoices(firstChoices, OnChoiceSelected);

// 다른 선택지로 변경
choicePanel.SetupChoices(secondChoices, OnChoiceSelected);
```

### 3. 풀 상태 모니터링
```csharp
// 풀 상태 확인
choicePanel.CheckPoolStatus();

// UI 요소 정보 출력
choicePanel.PrintUIElementInfo();
```

## 🎮 StoryPanel과의 통합

### 1. StoryPanel을 통한 선택지 관리
```csharp
// 선택지 설정
storyPanel.SetupChoices(choices, OnChoiceSelected);

// 선택지 숨기기
storyPanel.HideChoices();

// 선택지 표시
storyPanel.ShowChoices();

// 선택지 개수 확인
int count = storyPanel.GetChoiceCount();
bool hasChoices = storyPanel.HasChoices();
```

### 2. 모드별 선택지 관리
```csharp
// 선택지 모드로 전환
storyPanel.SwitchToChoiceMode();

// 선택지 질문 설정
storyPanel.SetChoiceQuestion("어떤 것을 원하시나요?");

// 선택지 설정
storyPanel.SetupChoices(choices, OnChoiceSelected);
```

## 📊 성능 최적화

### 1. 풀링 시스템 활용
- `initialPoolSize`를 적절히 설정 (기본값: 8)
- 자주 사용하는 선택지 개수에 맞춰 조정
- 풀이 부족하면 자동으로 확장

### 2. 메모리 관리
- 일반적인 사용: `ClearChoices()` 사용
- 장시간 사용하지 않을 때: `DestroyAllChoices()` 사용
- 씬 전환 시 자동 정리

### 3. 선택지 재사용
```csharp
// 선택지 숨기기 (재사용 가능)
choicePanel.ClearChoices();

// 같은 선택지 다시 표시
choicePanel.SetupChoices(choices, OnChoiceSelected);
```

## 🧪 테스트 및 디버깅

### 1. ContextMenu 테스트
```csharp
[ContextMenu("테스트 - 샘플 선택지 생성")]
public void CreateSampleChoices()

[ContextMenu("테스트 - 선택지 숨기기")]
public void TestHideChoices()

[ContextMenu("테스트 - 선택지 다시 표시")]
public void TestShowChoices()

[ContextMenu("풀 상태 확인")]
public void CheckPoolStatus()
```

### 2. 디버그 정보 출력
```csharp
// UI 요소 정보 출력
choicePanel.PrintUIElementInfo();

// 풀 상태 확인
choicePanel.CheckPoolStatus();
```

## 🔍 문제 해결

### 1. 선택지가 표시되지 않음
- `choiceButtonPrefab` 설정 확인
- `choiceContent` Transform 확인
- 풀 초기화 상태 확인

### 2. 메모리 사용량 증가
- `ClearChoices()` 사용 확인
- 불필요한 `DestroyAllChoices()` 호출 확인
- 풀 크기 적절성 확인

### 3. 성능 문제
- 풀 크기 조정
- 불필요한 선택지 생성/제거 최소화
- `SetActive(false)` 사용 확인

## 📚 사용 예제

### 1. 기본 대화 시스템
```csharp
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private ChoicePanel choicePanel;
    
    public void ShowChoices(string[] choices, System.Action<int> onSelected)
    {
        choicePanel.SetupChoices(choices, onSelected);
    }
    
    public void HideChoices()
    {
        choicePanel.ClearChoices();
    }
}
```

### 2. 동적 선택지 변경
```csharp
public void UpdateChoicesBasedOnContext()
{
    string[] newChoices = GetContextualChoices();
    choicePanel.SetupChoices(newChoices, OnChoiceSelected);
}
```

### 3. 선택지 상태 관리
```csharp
public void DisableSpecificChoice(int index)
{
    choicePanel.SetChoiceButtonActive(index, false);
}

public void DisableAllChoices()
{
    choicePanel.SetChoiceButtonsInteractable(false);
}
```

## 🎯 모범 사례

### 1. 선택지 관리
- **표시**: `SetupChoices()` 사용
- **숨김**: `ClearChoices()` 사용 (일반적)
- **제거**: `DestroyAllChoices()` 사용 (메모리 정리 시)

### 2. 성능 최적화
- 적절한 풀 크기 설정
- 선택지 재사용 최대화
- 불필요한 생성/제거 최소화

### 3. 메모리 관리
- `SetActive(false)` 우선 사용
- 장시간 미사용 시 완전 제거
- 씬 전환 시 자동 정리

## 📝 변경 사항

### v2.0 (현재 버전)
- ✅ 풀링 시스템 도입
- ✅ `SetActive(false)` 기반 관리
- ✅ 고급 선택지 제어 기능
- ✅ StoryPanel 통합 기능
- ✅ 성능 최적화

### v1.0 (이전 버전)
- ❌ `Destroy` 기반 관리
- ❌ 매번 새로운 버튼 생성
- ❌ 제한된 제어 기능

---

**버전**: 2.0  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**주요 기능**: 풀링 시스템, SetActive(false) 관리, 고급 제어 기능
