# Addressable UI 설정 가이드

## 📋 개요

이 가이드는 Unity Addressables를 사용한 UI 시스템 설정 방법을 설명합니다. Addressables를 통해 UI 프리팹을 동적으로 로드하고 메모리를 효율적으로 관리할 수 있습니다.

## 🎯 주요 장점

- **동적 로딩**: 필요할 때만 UI 로드
- **메모리 효율성**: 사용하지 않는 UI 자동 해제
- **번들 관리**: UI별로 독립적인 에셋 번들
- **플랫폼 최적화**: 플랫폼별 최적화된 번들 생성

## ⚙️ Unity 에디터 설정

### 1. Addressables 패키지 설치

1. **Window > Package Manager** 열기
2. **Unity Registry** 선택
3. **Addressables** 검색 후 설치

### 2. Addressables 초기 설정

1. **Window > Asset Management > Addressables > Groups** 열기
2. **Create Addressables Settings** 클릭 (처음 사용 시)
3. 기본 그룹이 생성됩니다:
   - `Default Local Group`: 로컬 에셋용
   - `Built In Data`: 빌드 데이터용

### 3. UI 전용 그룹 생성

권장 그룹 구조:
```
UI/
├── Canvas/          # 캔버스 프리팹
├── HUD/            # HUD UI 요소
├── Panel/          # 패널 UI
├── Popup/          # 팝업 UI
└── Loading/        # 로딩 UI
```

**그룹 생성 방법:**
1. Addressables Groups 창에서 **Create > Group**
2. 그룹 이름 입력 (예: "UI/Canvas")
3. **Schema** 설정:
   - **Content Packing & Loading**: `Packed Mode`
   - **Content Update Restriction**: `Can Change Post Release`

## 📁 UI 프리팹 설정

### 1. 프리팹을 Addressable로 설정

1. **프리팹 선택** → Inspector
2. **Addressable** 체크박스 활성화
3. **Address** 설정 (예: "UI/Canvas/HUDCanvas")
4. **Group** 설정 (예: "UI/Canvas")

### 2. 권장 Address 키 구조

```
# 캔버스
UI/Canvas/HUDCanvas
UI/Canvas/PanelCanvas
UI/Canvas/PopupCanvas
UI/Canvas/LoadingCanvas

# HUD UI
UI/HUD/StatusPanel
UI/HUD/HealthBar
UI/HUD/ScoreDisplay

# 패널 UI
UI/Panel/MainMenu
UI/Panel/Settings
UI/Panel/Inventory
UI/Panel/Shop

# 팝업 UI
UI/Popup/MessagePopup
UI/Popup/CheckPopUp
UI/Popup/ItemDetailPopup

# 로딩 UI
UI/Loading/LoadingScreen
UI/Loading/ProgressBar
```

### 3. 프리팹 설정 최적화

**Inspector에서 설정할 항목:**
- **Addressable**: ✅ 체크
- **Address**: 명확하고 일관된 키 사용
- **Group**: 적절한 그룹 선택
- **Include In Build**: ✅ 체크 (필요한 경우)
- **Labels**: 검색 및 필터링용 태그 추가

## 🔧 UIManager 설정

### 1. Canvas Reference 설정

```csharp
[Header("Addressable Canvas References")]
[SerializeField] private AssetReferenceGameObject hudCanvasReference;
[SerializeField] private AssetReferenceGameObject panelCanvasReference;
[SerializeField] private AssetReferenceGameObject popupCanvasReference;
[SerializeField] private AssetReferenceGameObject loadingCanvasReference;
```

**Inspector에서 설정:**
1. UIManager 오브젝트 선택
2. 각 Reference 필드에 해당 캔버스 프리팹 드래그 앤 드롭
3. 또는 **Select** 버튼으로 Addressable 키 선택

### 2. SafeArea 설정

```csharp
[Header("SafeArea Settings")]
[SerializeField] private bool enableSafeArea = true;
[SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
[SerializeField] private bool showDebugArea = false;
```

## 📖 사용법

### 1. 기본 UI 로드

```csharp
// Addressable 키로 로드
BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");
UIManager.Instance.OpenPanel(mainMenu);

// AssetReference로 로드
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

### 3. UI 해제

```csharp
// 개별 UI 해제
UIManager.Instance.ReleaseUI("UI/Panel/MainMenu");

// 모든 Addressable 해제
UIManager.Instance.ReleaseAllAddressables();
```

### 4. 미리 로드

```csharp
// 자주 사용하는 UI 미리 로드
await UIManager.Instance.PreloadUIAsync<BaseUI>("UI/Panel/MainMenu");
```

## 🏗️ 프로젝트 구조 예시

```
Assets/000WorkSpaces/KYS/Prefabs/UI/
├── Canvas/
│   ├── HUDCanvas.prefab
│   ├── PanelCanvas.prefab
│   ├── PopupCanvas.prefab
│   └── LoadingCanvas.prefab
├── HUD/
│   ├── StatusPanel.prefab
│   ├── HealthBar.prefab
│   └── ScoreDisplay.prefab
├── Panel/
│   ├── MainMenu.prefab
│   ├── Settings.prefab
│   ├── Inventory.prefab
│   └── Shop.prefab
├── Popup/
│   ├── MessagePopup.prefab
│   ├── CheckPopUp.prefab
│   └── ItemDetailPopup.prefab
└── Loading/
    ├── LoadingScreen.prefab
    └── ProgressBar.prefab
```

## 🔍 Addressables Groups 설정

### 1. 그룹별 설정

**Canvas 그룹:**
- **Content Packing & Loading**: `Packed Mode`
- **Content Update Restriction**: `Can Change Post Release`
- **Include In Build**: ✅ 체크

**UI 요소 그룹:**
- **Content Packing & Loading**: `Packed Mode`
- **Content Update Restriction**: `Can Change Post Release`
- **Include In Build**: ❌ 체크 해제 (동적 로딩)

### 2. 번들 설정

**Bundle Mode**: `Pack Together`
**Bundle Naming**: `Project Name + Group Name`

## 🚀 빌드 및 배포

### 1. Addressables 빌드

1. **Window > Asset Management > Addressables > Groups**
2. **Build > New Build > Default Build Script**
3. 빌드 완료 후 생성된 파일들:
   - `catalog.json`: 에셋 카탈로그
   - `*.bundle`: 에셋 번들 파일들

### 2. 플랫폼별 설정

**Android:**
- **Bundle Naming**: `Project Name + Group Name`
- **Compression**: `LZ4`

**iOS:**
- **Bundle Naming**: `Project Name + Group Name`
- **Compression**: `LZMA`

### 3. 콘텐츠 업데이트

1. **Build > Update a Previous Build**
2. 변경된 에셋만 업데이트
3. 새로운 카탈로그 생성

## 🛠️ 문제 해결

### 1. 일반적인 문제들

**UI 로드 실패:**
```
[UIManager] UI 로드 실패: UI/Panel/MainMenu
```
- **해결**: Addressable 키 확인, 프리팹이 올바른 그룹에 있는지 확인

**Canvas 참조 누락:**
```
[UIManager] UI 부모를 찾을 수 없음
```
- **해결**: UIManager의 Canvas Reference 설정 확인

**메모리 누수:**
- **해결**: UI 사용 후 반드시 `ReleaseUI()` 호출

### 2. 성능 최적화

**번들 크기 최적화:**
- 관련 UI들을 같은 그룹에 배치
- 불필요한 에셋 제거
- 텍스처 압축 설정 최적화

**로딩 시간 최적화:**
- 자주 사용하는 UI 미리 로드
- 로딩 화면 표시
- 비동기 로딩 활용

### 3. 디버깅

**Addressables Profiler:**
1. **Window > Asset Management > Addressables > Profiler**
2. 로딩 상태 및 메모리 사용량 확인

**Console 로그:**
- UIManager의 상세 로그 확인
- Addressables 관련 에러 메시지 확인

## 📚 추가 리소스

- [Unity Addressables 공식 문서](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [Addressables Best Practices](https://docs.unity3d.com/Packages/com.unity.addressables@latest/manual/AddressableAssetsBestPractices.html)
- [프로젝트 README.md](./README.md)

## 🎯 모범 사례

### 1. 네이밍 컨벤션
- **Address 키**: `UI/Type/Name` 형식 사용
- **그룹명**: 기능별로 명확하게 구분
- **프리팹명**: PascalCase 사용

### 2. 그룹 구성
- **Canvas**: 별도 그룹으로 분리
- **UI 타입별**: HUD, Panel, Popup, Loading 분리
- **업데이트 빈도**: 자주 변경되는 UI 별도 그룹

### 3. 성능 고려사항
- **번들 크기**: 10MB 이하 권장
- **로딩 순서**: 중요도에 따른 우선순위 설정
- **메모리 관리**: 사용 후 즉시 해제

---

**버전**: 2.0  
**최종 업데이트**: 2024년  
**Unity 버전**: 2022.3 LTS 이상
