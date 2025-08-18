# DoTween 설치 및 설정 가이드

## DoTween 설치 후 설정 방법

### 1. DoTween 설치
- Unity Asset Store에서 "DOTween (HOTween v2)" 검색 후 다운로드
- 또는 GitHub에서 직접 다운로드: https://github.com/Demigiant/dotween

### 2. 스크립트 정의 심볼 추가
DoTween을 설치한 후 Unity에서 다음 설정을 해주세요:

1. Unity 에디터에서 `Edit > Project Settings` 메뉴 선택
2. `Player` 탭 선택
3. `Other Settings` 섹션에서 `Scripting Define Symbols` 찾기
4. 기존 심볼에 `DOTWEEN_AVAILABLE` 추가 (쉼표로 구분)

예시:
```
UNITY_POST_PROCESSING_STACK_V2;DOTWEEN_AVAILABLE
```

### 3. DoTween 초기화
프로젝트의 초기화 스크립트에서 DoTween을 초기화해야 합니다:

```csharp
using DG.Tweening;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        // DoTween 초기화
        DOTween.SetTweensCapacity(500, 50);
    }
}
```

### 4. 현재 상태
- DoTween이 설치되지 않은 상태에서는 애니메이션이 비활성화되고 즉시 UI가 표시/숨김됩니다.
- 콘솔에 경고 메시지가 출력됩니다: "[BaseUI] DoTween이 설치되지 않아 애니메이션이 비활성화되었습니다."

### 5. DoTween 설치 후
- `DOTWEEN_AVAILABLE` 심볼을 추가하면 자동으로 애니메이션이 활성화됩니다.
- UI 애니메이션이 정상적으로 작동합니다.

## 주의사항
- DoTween을 설치하지 않은 상태에서도 코드가 정상적으로 컴파일됩니다.
- 애니메이션 기능만 비활성화되고 다른 UI 기능은 정상 작동합니다.
