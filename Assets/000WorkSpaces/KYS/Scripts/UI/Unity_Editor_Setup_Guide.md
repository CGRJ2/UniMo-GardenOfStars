# Unity 에디터 설정 가이드

## 📋 개요

이 가이드는 Unity 에디터에서 KYS UI 시스템을 설정하고 구성하는 방법을 단계별로 설명합니다. Addressables, SafeArea, UI 프리팹 설정 등을 포함합니다.

## 🎯 설정 목표

- **Addressables 설정**: UI 프리팹을 Addressable로 구성
- **UIManager 설정**: Canvas Reference 및 SafeArea 설정
- **프리팹 구성**: UI 프리팹 구조 및 컴포넌트 설정
- **테스트 환경**: 에디터에서 UI 시스템 테스트

## ⚙️ 1단계: Addressables 패키지 설치

### 1.1 패키지 설치

1. **Window > Package Manager** 열기
2. **Unity Registry** 선택
3. 검색창에 **"Addressables"** 입력
4. **Addressables** 패키지 선택 후 **Install** 클릭

### 1.2 초기 설정

1. **Window > Asset Management > Addressables > Groups** 열기
2. **Create Addressables Settings** 클릭 (처음 사용 시)
3. 기본 그룹이 생성됩니다:
   - `Default Local Group`
   - `Built In Data`

## 🏗️ 2단계: Addressable Groups 구성

### 2.1 UI 전용 그룹 생성

**권장 그룹 구조:**
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

### 2.2 그룹별 설정

**Canvas 그룹:**
- **Include In Build**: ✅ 체크
- **Bundle Mode**: `Pack Together`
- **Bundle Naming**: `Project Name + Group Name`

**UI 요소 그룹:**
- **Include In Build**: ❌ 체크 해제 (동적 로딩)
- **Bundle Mode**: `Pack Together`
- **Bundle Naming**: `Project Name + Group Name`

## 📁 3단계: UI 프리팹 설정

### 3.1 프리팹을 Addressable로 설정

1. **프리팹 선택** → Inspector
2. **Addressable** 체크박스 활성화
3. **Address** 설정 (예: "UI/Canvas/HUDCanvas")
4. **Group** 설정 (예: "UI/Canvas")

### 3.2 권장 Address 키 구조

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

### 3.3 프리팹 구조 설정

**Canvas 프리팹 구조:**
```
Canvas (Canvas)
├── SafeAreaPanel (RectTransform)
│   └── Content (RectTransform)
│       └── UI Elements...
└── Background (RectTransform) - Optional
```

**SafeAreaPanel 설정:**
- **Anchor**: Stretch-Stretch (전체 화면)
- **Pivot**: (0.5, 0.5)
- **Size**: Canvas 크기에 맞춤
- **SafeAreaPanel** 컴포넌트 추가

## 🔧 4단계: UIManager 설정

### 4.1 UIManager 오브젝트 생성

1. **빈 GameObject 생성** (이름: "UIManager")
2. **UIManager** 컴포넌트 추가
3. **DontDestroyOnLoad** 설정

### 4.2 Canvas Reference 설정

**Inspector에서 설정:**
1. UIManager 오브젝트 선택
2. **Addressable Canvas References** 섹션에서:
   - **HUD Canvas Reference**: HUDCanvas 프리팹 드래그 앤 드롭
   - **Panel Canvas Reference**: PanelCanvas 프리팹 드래그 앤 드롭
   - **Popup Canvas Reference**: PopupCanvas 프리팹 드래그 앤 드롭
   - **Loading Canvas Reference**: LoadingCanvas 프리팹 드래그 앤 드롭

**또는 Select 버튼으로 Addressable 키 선택:**
1. 각 Reference 필드 옆의 **Select** 버튼 클릭
2. Addressable 키 선택 (예: "UI/Canvas/HUDCanvas")

### 4.3 SafeArea 설정

**SafeArea Settings 섹션:**
- **Enable Safe Area**: ✅ 체크
- **Debug Color**: 빨간색 (0.3f 알파)
- **Show Debug Area**: 개발 중에만 체크

## 🎮 5단계: SafeAreaManager 설정

### 5.1 SafeAreaManager 오브젝트 생성

1. **빈 GameObject 생성** (이름: "SafeAreaManager")
2. **SafeAreaManager** 컴포넌트 추가
3. **DontDestroyOnLoad** 설정

### 5.2 SafeArea 설정

**Inspector에서 설정:**
- **Enable Safe Area**: ✅ 체크
- **Debug Color**: 빨간색 (0.3f 알파)
- **Show Debug Area**: 개발 중에만 체크

## 🧪 6단계: 테스트 환경 설정

### 6.1 Game View 설정

**해상도 설정:**
1. **Game View**에서 **Resolution** 드롭다운 선택
2. **iPhone X** 또는 **Android 노치** 해상도 선택
3. **SafeArea** 확인

**추가 해상도:**
- iPhone X: 375 x 812
- iPhone 12 Pro: 390 x 844
- Samsung Galaxy S21: 360 x 800
- Google Pixel 5: 393 x 851

### 6.2 테스트 씬 구성

**기본 씬 구조:**
```
Scene
├── UIManager (DontDestroyOnLoad)
├── SafeAreaManager (DontDestroyOnLoad)
├── Main Camera
└── Directional Light
```

### 6.3 테스트 스크립트 추가

**간단한 테스트 스크립트:**
```csharp
public class UITest : MonoBehaviour
{
    private void Start()
    {
        // UI 시스템 초기화 확인
        if (UIManager.Instance != null)
        {
            Debug.Log("UIManager 초기화 완료");
        }
        
        if (SafeAreaManager.Instance != null)
        {
            Debug.Log("SafeAreaManager 초기화 완료");
        }
    }
    
    private void Update()
    {
        // 테스트 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestLoadUI();
        }
    }
    
    private async void TestLoadUI()
    {
        // 테스트 UI 로드
        BaseUI testUI = await UIManager.Instance.LoadUIAsync<BaseUI>("UI/Panel/MainMenu");
        if (testUI != null)
        {
            UIManager.Instance.OpenPanel(testUI);
            Debug.Log("테스트 UI 로드 성공");
        }
    }
}
```

## 🔍 7단계: Addressables 빌드

### 7.1 첫 번째 빌드

1. **Window > Asset Management > Addressables > Groups**
2. **Build > New Build > Default Build Script**
3. 빌드 완료 대기
4. 생성된 파일들 확인:
   - `catalog.json`: 에셋 카탈로그
   - `*.bundle`: 에셋 번들 파일들

### 7.2 빌드 설정 확인

**Build Settings:**
- **Platform**: Android 또는 iOS
- **Scripting Backend**: IL2CPP
- **Target Architectures**: ARM64

**Player Settings:**
- **Other Settings > Scripting Define Symbols**: `DOTWEEN_AVAILABLE` 추가 (DoTween 사용 시)

## 🛠️ 8단계: 문제 해결

### 8.1 일반적인 문제들

**Addressable 키를 찾을 수 없음:**
```
[UIManager] UI 로드 실패: UI/Panel/MainMenu
```
- **해결**: Addressable 키 확인
- **해결**: 프리팹이 올바른 그룹에 있는지 확인
- **해결**: Addressables 빌드 재실행

**Canvas Reference 누락:**
```
[UIManager] UI 부모를 찾을 수 없음
```
- **해결**: UIManager의 Canvas Reference 설정 확인
- **해결**: 프리팹이 Addressable로 설정되었는지 확인

**SafeArea가 적용되지 않음:**
- **해결**: SafeAreaManager가 씬에 있는지 확인
- **해결**: Canvas에 SafeAreaPanel이 있는지 확인

### 8.2 디버깅 도구

**Addressables Profiler:**
1. **Window > Asset Management > Addressables > Profiler**
2. 로딩 상태 및 메모리 사용량 확인

**Console 로그 확인:**
- UIManager의 상세 로그 확인
- SafeAreaManager의 디버그 정보 확인
- Addressables 관련 에러 메시지 확인

## 📋 9단계: 검증 체크리스트

### 9.1 기본 설정 확인

- [ ] Addressables 패키지 설치 완료
- [ ] Addressable Groups 생성 완료
- [ ] UI 프리팹을 Addressable로 설정 완료
- [ ] UIManager 오브젝트 생성 및 설정 완료
- [ ] SafeAreaManager 오브젝트 생성 및 설정 완료

### 9.2 기능 테스트

- [ ] UIManager 초기화 확인
- [ ] SafeAreaManager 초기화 확인
- [ ] UI 로드 테스트 성공
- [ ] SafeArea 적용 확인
- [ ] 팝업 표시 테스트 성공

### 9.3 빌드 테스트

- [ ] Addressables 빌드 성공
- [ ] 플랫폼별 빌드 성공
- [ ] 실제 디바이스에서 테스트 성공

## 🎯 10단계: 최적화

### 10.1 성능 최적화

**번들 크기 최적화:**
- 관련 UI들을 같은 그룹에 배치
- 불필요한 에셋 제거
- 텍스처 압축 설정 최적화

**로딩 시간 최적화:**
- 자주 사용하는 UI 미리 로드
- 로딩 화면 표시
- 비동기 로딩 활용

### 10.2 메모리 관리

**메모리 누수 방지:**
- UI 사용 후 반드시 `ReleaseUI()` 호출
- 씬 전환 시 모든 UI 해제
- Addressables 핸들 관리

## 📚 추가 리소스

- [Unity Addressables 공식 문서](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [프로젝트 README.md](./README.md)
- [Addressable UI 설정 가이드](./Addressable_UI_Setup_Guide.md)
- [SafeArea 설정 가이드](./SafeArea_Setup_Guide.md)

## 🎯 모범 사례

### 1. 네이밍 컨벤션
- **Address 키**: `UI/Type/Name` 형식 사용
- **그룹명**: 기능별로 명확하게 구분
- **프리팹명**: PascalCase 사용

### 2. 파일 구조
- **UI 프리팹**: `Assets/000WorkSpaces/KYS/Prefabs/UI/` 하위에 구성
- **스크립트**: `Assets/000WorkSpaces/KYS/Scripts/UI/` 하위에 구성
- **문서**: 각 기능별로 별도 가이드 문서 작성

### 3. 버전 관리
- **Addressables 빌드**: 버전별로 관리
- **프리팹 변경**: Addressables 재빌드 필요
- **설정 변경**: 문서 업데이트

---

**버전**: 2.0  
**최종 업데이트**: 2024년  
**Unity 버전**: 2022.3 LTS 이상  
**지원 플랫폼**: iOS, Android, Windows, macOS
