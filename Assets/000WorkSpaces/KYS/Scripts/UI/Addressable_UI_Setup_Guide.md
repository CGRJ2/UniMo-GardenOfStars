# Addressable 기반 UI 관리 시스템 설정 가이드

## 1. 개요

현재 UIManager는 HUD, Panel, Popup, Loading으로 캔버스를 분리하는 구조이며, Addressable을 통해 UI 프리팹을 관리할 수 있도록 설계되어 있습니다.

## 2. 현재 구조 분석

### 2.1 캔버스 분리 구조
```csharp
[Header("UI Layer Settings")]
[SerializeField] private Canvas hudCanvas;      // HUD 레이어 (가장 뒤)
[SerializeField] private Canvas panelCanvas;    // 패널 레이어
[SerializeField] private Canvas popupCanvas;    // 팝업 레이어
[SerializeField] private Canvas loadingCanvas;  // 로딩 레이어 (가장 앞)
```

### 2.2 UI 레이어 타입
```csharp
public enum UILayerType
{
    HUD = 0,     // 재화 표시, 기본 UI 패널 및 버튼들
    Panel = 1,   // 열고 닫기 가능한 UI 패널
    Popup = 2,   // 상호작용 영역 진입 시 정보 표기
    Loading = 3  // 모든 화면을 덮는 최상위 레이어
}
```

## 3. Addressable 설정

### 3.1 Addressable 패키지 설치
- Unity Package Manager에서 "Addressables" 패키지 설치
- 현재 프로젝트에는 이미 설치되어 있음 (버전: 1.22.3)

### 3.2 Addressable Groups 설정
1. **Window > Asset Management > Addressables > Groups** 메뉴 열기
2. **Create Addressables Settings** 클릭 (처음 사용 시)
3. **Create > Group** 클릭하여 그룹 생성

### 3.3 UI 프리팹 그룹 구성
```
AddressableGroups/
├── UI_Canvases/          # Canvas 프리팹들
│   ├── HUDCanvas
│   ├── PanelCanvas
│   ├── PopupCanvas
│   └── LoadingCanvas
├── UI_HUD/              # HUD UI 프리팹들
│   ├── StatusPanel
│   ├── HealthBar
│   └── Minimap
├── UI_Panel/            # 패널 UI 프리팹들
│   ├── MainMenu
│   ├── Settings
│   ├── Inventory
│   └── Shop
├── UI_Popup/            # 팝업 UI 프리팹들
│   ├── ConfirmDialog
│   ├── AlertDialog
│   ├── LoadingPopup
│   └── ErrorPopup
└── UI_Loading/          # 로딩 UI 프리팹들
    ├── LoadingScreen
    └── SplashScreen
```

### 3.4 라벨 설정
각 그룹에 적절한 라벨을 설정:
- `UI_HUD`: HUD 관련 UI들
- `UI_Panel`: 패널 관련 UI들
- `UI_Popup`: 팝업 관련 UI들
- `UI_Loading`: 로딩 관련 UI들

## 4. UIManager 설정

### 4.1 Addressable 참조 설정
```csharp
[Header("Addressable Canvas References")]
[SerializeField] private AssetReferenceGameObject hudCanvasReference;
[SerializeField] private AssetReferenceGameObject panelCanvasReference;
[SerializeField] private AssetReferenceGameObject popupCanvasReference;
[SerializeField] private AssetReferenceGameObject loadingCanvasReference;
```

### 4.2 라벨 설정
```csharp
[Header("Addressable UI Settings")]
[SerializeField] private string uiPrefabLabel = "UI";
[SerializeField] private string hudPrefabLabel = "UI_HUD";
[SerializeField] private string panelPrefabLabel = "UI_Panel";
[SerializeField] private string popupPrefabLabel = "UI_Popup";
```

## 5. 사용 방법

### 5.1 UI 로드
```csharp
// 개별 UI 로드
BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");

// 라벨로 일괄 로드
List<BaseUI> hudUIs = await UIManager.Instance.LoadUIsByLabelAsync<BaseUI>("UI_HUD");
```

### 5.2 UI 표시
```csharp
// 패널 열기 (Stack 관리)
UIManager.Instance.OpenPanel(mainMenu);

// 팝업 열기 (Stack 관리)
UIManager.Instance.OpenPopup(confirmDialog);

// 직접 표시
hudUI.Show();
```

### 5.3 UI 해제
```csharp
// 개별 UI 해제
UIManager.Instance.ReleaseUI("UI/Panel/MainMenu");

// 모든 UI 해제
UIManager.Instance.ReleaseAllAddressables();
```

## 6. Addressable 키 명명 규칙

### 6.1 권장 키 구조
```
UI/{LayerType}/{UIName}
```

### 6.2 예시
- `UI/HUD/StatusPanel`
- `UI/Panel/MainMenu`
- `UI/Panel/Settings`
- `UI/Popup/ConfirmDialog`
- `UI/Loading/LoadingScreen`

## 7. 성능 최적화

### 7.1 미리 로드
```csharp
// 자주 사용하는 UI 미리 로드
await UIManager.Instance.PreloadUIAsync<BaseUI>("UI/Panel/MainMenu");
```

### 7.2 라벨 기반 일괄 로드
```csharp
// 특정 카테고리의 모든 UI 로드
List<BaseUI> panelUIs = await UIManager.Instance.LoadUIsByLabelAsync<BaseUI>("UI_Panel");
```

### 7.3 메모리 관리
```csharp
// 사용하지 않는 UI 해제
UIManager.Instance.ReleaseUI("UI/Panel/OldPanel");

// 씬 전환 시 모든 UI 해제
UIManager.Instance.ReleaseAllAddressables();
```

## 8. 디버깅 및 모니터링

### 8.1 Canvas 정보 확인
```csharp
// 특정 레이어의 Canvas 정보
Canvas hudCanvas = UIManager.Instance.GetCanvasByLayer(UILayerType.HUD);

// 모든 Canvas 정보 출력
PrintAllCanvasInfo();
```

### 8.2 레이어별 UI 확인
```csharp
// 특정 레이어의 모든 UI
List<BaseUI> hudUIs = UIManager.Instance.GetUIsByLayer(UILayerType.HUD);

// 모든 레이어의 UI 정보 출력
PrintAllLayerUIs();
```

## 9. 마이그레이션 가이드

### 9.1 기존 UIManagerOld에서 UIManager로 전환

1. **기존 프리팹을 Addressable로 변환**
   - 프리팹을 Addressable Groups에 추가
   - 적절한 키와 라벨 설정

2. **코드 수정**
   ```csharp
   // 기존
   UIManagerOld.Instance.ShowPopUp<MainMenu>();
   
   // 새로운 방식
   BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");
   UIManager.Instance.OpenPanel(mainMenu);
   ```

3. **점진적 전환**
   - 기존 UIManagerOld와 UIManager를 병행 사용
   - UI별로 하나씩 전환

## 10. 주의사항

### 10.1 메모리 관리
- Addressable 리소스는 반드시 해제해야 함
- 씬 전환 시 모든 핸들 해제
- 사용하지 않는 UI는 즉시 해제

### 10.2 비동기 처리
- 모든 Addressable 작업은 비동기
- await 키워드 사용 필수
- 예외 처리 필요

### 10.3 키 관리
- Addressable 키는 중복되지 않도록 주의
- 명명 규칙 준수
- 키 변경 시 코드 수정 필요

## 11. 결론

Addressable 기반 UI 관리 시스템은 다음과 같은 장점을 제공합니다:

1. **메모리 효율성**: 필요할 때만 로드, 사용 후 해제
2. **유연성**: 런타임에 동적으로 UI 로드/해제
3. **확장성**: 새로운 UI 추가가 용이
4. **성능**: 미리 로드, 라벨 기반 일괄 처리
5. **관리 편의성**: Addressable Groups를 통한 체계적 관리

이 시스템을 통해 효율적이고 확장 가능한 UI 관리가 가능합니다.
