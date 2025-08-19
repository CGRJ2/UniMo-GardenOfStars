# Unity 에디터 UIManager 설정 가이드

## 1. UIManager Addressable 참조 설정

### 1.1 UIManager 오브젝트 선택
1. **Hierarchy 창**에서 `UIManager` 오브젝트 선택
2. **Inspector 창**에서 UIManager 컴포넌트 확인

### 1.2 Addressable Canvas References 설정
다음 필드들에 해당하는 Canvas 프리팹들을 드래그 앤 드롭으로 설정:

```
hudCanvasReference: HUDCanvas 프리팹
panelCanvasReference: PanelCanvas 프리팹  
popupCanvasReference: PopupCanvas 프리팹
loadingCanvasReference: LoadingCanvas 프리팹
```

**설정 방법:**
1. **Project 창**에서 `Assets/000WorkSpaces/KYS/Prefabs/UI/Canvas/` 폴더 열기
2. 각 Canvas 프리팹을 해당하는 Reference 필드에 드래그 앤 드롭

### 1.3 라벨 설정 확인
다음 라벨들이 올바르게 설정되어 있는지 확인:

```
uiPrefabLabel: UI
hudPrefabLabel: UI_HUD
panelPrefabLabel: UI_Panel
popupPrefabLabel: UI_Popup
```

## 2. Addressable Groups 설정

### 2.1 Addressable Groups 창 열기
1. **Window > Asset Management > Addressables > Groups** 메뉴 선택

### 2.2 Canvas 프리팹들을 Addressable로 설정
1. **Project 창**에서 Canvas 프리팹들 선택:
   - `HUDCanvas.prefab`
   - `PanelCanvas.prefab`
   - `PopupCanvas.prefab`
   - `LoadingCanvas.prefab`

2. **Inspector 창**에서 **Addressable** 체크박스 활성화

3. **Addressable Groups 창**에서 적절한 그룹으로 이동

### 2.3 UI 프리팹들을 Addressable로 설정
1. **Project 창**에서 UI 프리팹들 선택
2. **Inspector 창**에서 **Addressable** 체크박스 활성화
3. **Addressable Groups 창**에서 적절한 그룹으로 이동

## 3. Addressable 키 설정

### 3.1 Canvas 프리팹 키 설정
각 Canvas 프리팹의 Addressable 키를 다음과 같이 설정:

```
HUDCanvas: UI/Canvas/HUDCanvas
PanelCanvas: UI/Canvas/PanelCanvas
PopupCanvas: UI/Canvas/PopupCanvas
LoadingCanvas: UI/Canvas/LoadingCanvas
```

### 3.2 UI 프리팹 키 설정
UI 프리팹들의 Addressable 키를 다음과 같이 설정:

```
HUD UI: UI/HUD/{UIName}
Panel UI: UI/Panel/{UIName}
Popup UI: UI/Popup/{UIName}
Loading UI: UI/Loading/{UIName}
```

## 4. 라벨 설정

### 4.1 Canvas 라벨 설정
각 Canvas 프리팹에 적절한 라벨 추가:

```
HUDCanvas: UI_Canvas
PanelCanvas: UI_Canvas
PopupCanvas: UI_Canvas
LoadingCanvas: UI_Canvas
```

### 4.2 UI 프리팹 라벨 설정
각 UI 프리팹에 적절한 라벨 추가:

```
HUD UI: UI_HUD
Panel UI: UI_Panel
Popup UI: UI_Popup
Loading UI: UI_Loading
```

## 5. Build Settings 확인

### 5.1 Addressable Build 설정
1. **Addressable Groups 창**에서 **Build > New Build > Default Build Script** 선택
2. 빌드 완료 후 **Build > Update a Previous Build** 선택

### 5.2 플랫폼 설정
1. **File > Build Settings** 메뉴 선택
2. **Platform** 설정 확인
3. **Addressable Groups**에서 **Build > New Build > Default Build Script** 실행

## 6. 테스트 및 확인

### 6.1 UIManager 초기화 확인
1. **Play 모드** 진입
2. **Console 창**에서 다음 로그 확인:
   ```
   [UIManager] Addressable Canvas 초기화 시작
   [UIManager] Addressable Canvas 초기화 완료
   [UIManager] SafeArea 적용 완료
   ```

### 6.2 UI 로드 테스트
1. **AddressableUIExamples** 컴포넌트 사용
2. **Console 창**에서 UI 로드 로그 확인

## 7. 문제 해결

### 7.1 Addressable 참조가 비어있는 경우
1. **UIManager** 오브젝트 선택
2. **Inspector**에서 **Addressable Canvas References** 확인
3. 각 필드에 해당하는 프리팹을 드래그 앤 드롭

### 7.2 프리팹을 찾을 수 없는 경우
1. **Project 창**에서 프리팹 경로 확인
2. **Addressable Groups**에서 프리팹이 올바른 그룹에 있는지 확인
3. **Addressable 키**가 올바르게 설정되었는지 확인

### 7.3 빌드 오류가 발생하는 경우
1. **Addressable Groups**에서 **Build > Clean Build** 실행
2. **Build > New Build > Default Build Script** 실행
3. **Console 창**에서 오류 메시지 확인

## 8. 권장 사항

### 8.1 프리팹 구조
```
Assets/000WorkSpaces/KYS/Prefabs/UI/
├── Canvas/
│   ├── HUDCanvas.prefab
│   ├── PanelCanvas.prefab
│   ├── PopupCanvas.prefab
│   └── LoadingCanvas.prefab
├── HUD/
│   └── {HUD UI 프리팹들}
├── Panel/
│   └── {Panel UI 프리팹들}
├── Popup/
│   └── {Popup UI 프리팹들}
└── Loading/
    └── {Loading UI 프리팹들}
```

### 8.2 Addressable Groups 구조
```
AddressableGroups/
├── UI_Canvases/
│   ├── HUDCanvas
│   ├── PanelCanvas
│   ├── PopupCanvas
│   └── LoadingCanvas
├── UI_HUD/
│   └── {HUD UI 프리팹들}
├── UI_Panel/
│   └── {Panel UI 프리팹들}
├── UI_Popup/
│   └── {Popup UI 프리팹들}
└── UI_Loading/
    └── {Loading UI 프리팹들}
```

이 가이드를 따라 설정하면 UIManager가 올바르게 작동할 것입니다.
