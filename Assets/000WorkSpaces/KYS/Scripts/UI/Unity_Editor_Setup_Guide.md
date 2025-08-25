# Unity 에디터 설정 가이드

## 📋 개요

이 가이드는 Unity 에디터에서 KYS UI 시스템을 설정하고 구성하는 방법을 단계별로 설명합니다. Addressables, SafeArea, UI 프리팹 설정 등을 포함합니다. 현재 프로젝트에서는 **InfoHUD 시스템**, **중복 생성 방지**, **로컬라이제이션** 기능과 함께 사용됩니다.

## 🎯 설정 목표

- **Addressables 설정**: UI 프리팹을 Addressable로 구성
- **UIManager 설정**: Canvas Reference 및 SafeArea 설정
- **프리팹 구성**: UI 프리팹 구조 및 컴포넌트 설정
- **InfoHUD 시스템 설정**: TouchInfoHUD 및 HUDBackdropUI 설정
- **로컬라이제이션 설정**: 다국어 지원 시스템 설정
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
├── HUD/            # HUD UI 요소 (TouchInfoHUD, HUDBackdropUI 포함)
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

# HUD UI (새로 추가)
UI/HUD/TouchInfoHUD
UI/HUD/HUDBackdropUI
UI/HUD/HUDAllPanel
UI/HUD/StatusPanel
UI/HUD/HealthBar
UI/HUD/ScoreDisplay

# 패널 UI
UI/Panel/MainMenu
UI/Panel/Settings
UI/Panel/Inventory
UI/Panel/Shop
UI/Panel/TitlePanel
UI/Panel/LanguageSettingsPanel

# 팝업 UI
UI/Popup/MessagePopup
UI/Popup/CheckPopUp
UI/Popup/ItemDetailPopup
UI/Popup/LanguageSettingPopup

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

### 3.4 InfoHUD 프리팹 설정

**TouchInfoHUD 프리팹 구조:**
```
TouchInfoHUD (TouchInfoHUD)
├── Background (Image)
├── TitleText (TextMeshProUGUI)
├── DescriptionText (TextMeshProUGUI)
├── IconImage (Image)
└── CloseButton (Button)
```

**HUDBackdropUI 프리팹 구조:**
```
HUDBackdropUI (HUDBackdropUI)
└── Backdrop (Image)
    └── PointerHandler (PointerHandler)
```

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

### 4.4 중복 생성 방지 설정

**Duplicate Prevention Settings 섹션:**
- **Enable Duplicate Prevention**: ✅ 체크
- **Debug Duplicate Prevention**: 개발 중에만 체크

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

## 🌐 6단계: 로컬라이제이션 설정

### 6.1 LocalizationManager 설정

1. **빈 GameObject 생성** (이름: "LocalizationManager")
2. **LocalizationManager** 컴포넌트 추가
3. **DontDestroyOnLoad** 설정

### 6.2 CSV 파일 설정

**LanguageData.csv 파일 구성:**
```csv
Key,Korean,English
title,제목,Title
description,설명,Description
confirm,확인,Confirm
cancel,취소,Cancel
```

### 6.3 언어 설정

**Inspector에서 설정:**
- **Default Language**: Korean
- **Current Language**: Korean
- **CSV File**: LanguageData.csv 파일 할당

## 🎯 7단계: InfoHUD 시스템 설정

### 7.1 TouchInfoManager 설정

1. **빈 GameObject 생성** (이름: "TouchInfoManager")
2. **TouchInfoManager** 컴포넌트 추가
3. **DontDestroyOnLoad** 설정

### 7.2 TouchInfoHUD AssetReference 설정

**Inspector에서 설정:**
- **TouchInfoHUD Reference**: TouchInfoHUD 프리팹 할당
- **HUDBackdropUI Reference**: HUDBackdropUI 프리팹 할당

### 7.3 터치 감지 설정

**Touch Detection Settings:**
- **Enable Touch Detection**: ✅ 체크
- **Touch Layer**: UI가 아닌 오브젝트 레이어 설정
- **Debug Touch Detection**: 개발 중에만 체크

## 🧪 8단계: 테스트 환경 설정

### 8.1 Game View 설정

**해상도 설정:**
1. **Game View**에서 **Resolution** 드롭다운 선택
2. **iPhone X** 또는 **Android 노치** 해상도 선택
3. **SafeArea** 확인

**추가 해상도:**
- iPhone X: 375 x 812
- iPhone 12 Pro: 390 x 844
- Samsung Galaxy S21: 360 x 800
- Google Pixel 5: 393 x 851

### 8.2 테스트 씬 구성

**기본 씬 구조:**
```
Scene
├── UIManager (DontDestroyOnLoad)
├── SafeAreaManager (DontDestroyOnLoad)
├── LocalizationManager (DontDestroyOnLoad)
├── TouchInfoManager (DontDestroyOnLoad)
├── Main Camera
└── Directional Light
```

### 8.3 테스트 스크립트 추가

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
        
        if (LocalizationManager.Instance != null)
        {
            Debug.Log("LocalizationManager 초기화 완료");
        }
        
        if (TouchInfoManager.Instance != null)
        {
            Debug.Log("TouchInfoManager 초기화 완료");
        }
    }
    
    // InfoHUD 테스트
    public async void TestInfoHUD()
    {
        await TouchInfoHUD.ShowInfoHUD(
            screenPosition: Input.mousePosition,
            title: "테스트 제목",
            description: "테스트 설명입니다.",
            icon: null
        );
    }
    
    // 로컬라이제이션 테스트
    public void TestLocalization()
    {
        string title = LocalizationManager.Instance.GetLocalizedText("title");
        Debug.Log($"현재 언어: {LocalizationManager.Instance.CurrentLanguage}");
        Debug.Log($"제목: {title}");
    }
}
```

## 🔍 9단계: 디버깅 및 검증

### 9.1 콘솔 로그 확인

**정상 초기화 시 나타나는 로그:**
```
[UIManager] UIManager 초기화 완료
[SafeAreaManager] SafeAreaManager 초기화 완료
[LocalizationManager] LocalizationManager 초기화 완료
[TouchInfoManager] TouchInfoManager 초기화 완료
```

### 9.2 SafeArea 디버그

**SafeArea 시각화:**
1. SafeAreaManager의 **Show Debug Area** 체크
2. Game View에서 SafeArea 영역 확인
3. 빨간색 영역이 안전 영역을 나타냄

### 9.3 InfoHUD 테스트

**InfoHUD 기능 테스트:**
1. UI가 아닌 오브젝트 클릭
2. InfoHUD가 나타나는지 확인
3. 다른 곳 클릭 시 InfoHUD가 사라지는지 확인
4. InfoHUD 자체 클릭 시 닫히지 않는지 확인

### 9.4 로컬라이제이션 테스트

**언어 변경 테스트:**
1. LanguageSettingsPanel 열기
2. 언어 변경
3. UI 텍스트가 변경되는지 확인
4. 드롭다운 언어명이 올바르게 표시되는지 확인

## 🛠️ 10단계: 문제 해결

### 10.1 일반적인 문제들

**UIManager 초기화 실패:**
```
[UIManager] UIManager 초기화 실패
```
- **해결**: Canvas Reference 설정 확인
- **해결**: Addressable 키가 올바른지 확인

**SafeArea 적용 안됨:**
```
[SafeAreaManager] SafeArea 적용 실패
```
- **해결**: SafeAreaManager가 씬에 있는지 확인
- **해결**: Canvas에 SafeAreaPanel이 있는지 확인

**InfoHUD 생성 안됨:**
```
[TouchInfoHUD] InfoHUD 생성 실패
```
- **해결**: TouchInfoHUD AssetReference 설정 확인
- **해결**: TouchInfoManager가 초기화되었는지 확인

**로컬라이제이션 작동 안됨:**
```
[LocalizationManager] 로컬라이제이션 초기화 실패
```
- **해결**: CSV 파일 경로 확인
- **해결**: CSV 파일 형식 확인

### 10.2 성능 최적화

**번들 크기 최적화:**
- 관련 UI들을 같은 그룹에 배치
- 불필요한 에셋 제거
- 텍스처 압축 설정 최적화

**로딩 시간 최적화:**
- 자주 사용하는 UI 미리 로드
- 로딩 화면 표시
- 비동기 로딩 활용

## 📚 추가 리소스

- [프로젝트 README.md](./README.md)
- [Addressable UI 설정 가이드](./Addressable_UI_Setup_Guide.md)
- [SafeArea 설정 가이드](./SafeArea_Setup_Guide.md)
- [현재 사용 패턴 가이드](./현재_사용_패턴_가이드.md)

## 🎯 모범 사례

### 1. 설정 순서
- **Addressables** → **UIManager** → **SafeAreaManager** → **LocalizationManager** → **TouchInfoManager**

### 2. 테스트 전략
- **단계별 테스트**: 각 단계별로 기능 확인
- **통합 테스트**: 모든 시스템이 함께 작동하는지 확인
- **성능 테스트**: 실제 디바이스에서 성능 확인

### 3. 유지보수
- **정기적인 업데이트**: Addressables 카탈로그 업데이트
- **로그 모니터링**: 콘솔 로그를 통한 문제 감지
- **문서 업데이트**: 설정 변경 시 문서 업데이트

---

**버전**: 2.1  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: InfoHUD 시스템, 중복 생성 방지, 로컬라이제이션, 통합 설정 가이드
