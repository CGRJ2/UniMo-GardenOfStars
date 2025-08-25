# Addressables 씬 설정 가이드

## 📋 개요

Addressables를 사용하면 Build Settings에 씬을 추가하지 않고도 동적으로 씬을 로드할 수 있습니다. 이를 통해 더 유연한 씬 관리가 가능합니다. 현재 프로젝트에서는 **InfoHUD 시스템**, **중복 생성 방지**, **로컬라이제이션** 기능과 함께 사용됩니다.

## 🎯 Build Settings 구성

### **1. 기본 씬 구성**

#### **Build Settings에 포함할 씬들:**
```
0. BootScene (게임 시작 씬)
1. LoadingScene (로딩 화면 씬)
2. MainMenuScene (메인 메뉴 씬)
```

#### **Addressables로 관리할 씬들:**
```
- GameScene1
- GameScene2
- Level1Scene
- Level2Scene
- BossScene
- EndingScene
- InfoHUDTestScene (InfoHUD 테스트용)
- LocalizationTestScene (로컬라이제이션 테스트용)
```

### **2. 씬 구성 전략**

#### **A. 하이브리드 방식 (권장)**
```
Build Settings:
├── BootScene (게임 초기화)
├── LoadingScene (로딩 화면)
└── MainMenuScene (메인 메뉴)

Addressables:
├── GameScenes/
│   ├── Level1Scene
│   ├── Level2Scene
│   └── BossScene
├── StoryScenes/
│   ├── IntroScene
│   ├── EndingScene
│   └── CreditsScene
├── TestScenes/
│   ├── InfoHUDTestScene
│   ├── LocalizationTestScene
│   └── DebugScene
└── SpecialScenes/
    ├── TutorialScene
    └── DebugScene
```

#### **B. 완전 Addressables 방식**
```
Build Settings:
└── BootScene (게임 시작 씬만)

Addressables:
├── LoadingScene
├── MainMenuScene
├── GameScenes/
├── StoryScenes/
└── TestScenes/
```

## 🔧 설정 방법

### **1. Addressables 설정**

#### **A. Addressables Groups 생성**
1. **Window > Asset Management > Addressables > Groups**
2. **Create Addressables Settings** 클릭
3. **Groups** 생성:
   - `Scenes` (기본 그룹)
   - `GameScenes` (게임 씬들)
   - `StoryScenes` (스토리 씬들)
   - `TestScenes` (테스트 씬들)

#### **B. 씬을 Addressables에 추가**
1. **Project 창에서 씬 파일 선택**
2. **Inspector에서 "Addressable" 체크**
3. **Address** 설정 (예: `Scenes/Level1Scene`)
4. **Group** 설정 (예: `GameScenes`)

### **2. 씬 파일 설정**

#### **씬 파일 Inspector 설정:**
```
Addressable: ✓ 체크
Address: Scenes/Level1Scene
Group: GameScenes
Include In Build: ✗ 체크 해제 (Addressables로 관리)
```

### **3. Build Settings 설정**

#### **Build Settings에 포함할 씬들:**
```
0. Assets/Scenes/BootScene.unity
1. Assets/Scenes/LoadingScene.unity
2. Assets/Scenes/MainMenuScene.unity
```

## 💻 사용 방법

### **1. 기본 사용**

```csharp
// AddressableSceneLoadingManager 사용
AddressableSceneLoadingManager.LoadAddressableScene("Scenes/Level1Scene", true);

// 또는 AssetReference 사용
[SerializeField] private AssetReference level1SceneRef;
AddressableSceneLoadingManager.LoadAddressableScene(level1SceneRef, true);
```

### **2. 씬 전환 예제**

```csharp
public class SceneTransitionManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private AssetReference mainMenuScene;
    [SerializeField] private AssetReference gameScene;
    [SerializeField] private AssetReference bossScene;
    [SerializeField] private AssetReference infoHUDTestScene;
    [SerializeField] private AssetReference localizationTestScene;
    
    public void LoadMainMenu()
    {
        AddressableSceneLoadingManager.LoadAddressableScene(mainMenuScene, true);
    }
    
    public void LoadGameScene()
    {
        AddressableSceneLoadingManager.LoadAddressableScene(gameScene, true);
    }
    
    public void LoadBossScene()
    {
        AddressableSceneLoadingManager.LoadAddressableScene(bossScene, true);
    }
    
    public void LoadInfoHUDTestScene()
    {
        AddressableSceneLoadingManager.LoadAddressableScene(infoHUDTestScene, true);
    }
    
    public void LoadLocalizationTestScene()
    {
        AddressableSceneLoadingManager.LoadAddressableScene(localizationTestScene, true);
    }
}
```

### **3. 동적 씬 로딩**

```csharp
public void LoadSceneByName(string sceneName)
{
    // Addressables에서 씬 이름으로 로딩
    AddressableSceneLoadingManager.LoadAddressableScene(sceneName, true);
}

public void LoadSceneByIndex(int sceneIndex)
{
    // 미리 설정된 씬 인덱스로 로딩
    AddressableSceneLoadingManager.LoadAddressableScene(sceneIndex, true);
}
```

### **4. InfoHUD 시스템과 연동**

```csharp
public class InfoHUDSceneManager : MonoBehaviour
{
    [Header("InfoHUD Test Scenes")]
    [SerializeField] private AssetReference infoHUDTestScene;
    [SerializeField] private AssetReference touchTestScene;
    
    public void LoadInfoHUDTestScene()
    {
        // InfoHUD 테스트 씬 로드
        AddressableSceneLoadingManager.LoadAddressableScene(infoHUDTestScene, true);
    }
    
    public void LoadTouchTestScene()
    {
        // 터치 테스트 씬 로드
        AddressableSceneLoadingManager.LoadAddressableScene(touchTestScene, true);
    }
}
```

### **5. 로컬라이제이션과 연동**

```csharp
public class LocalizationSceneManager : MonoBehaviour
{
    [Header("Localization Test Scenes")]
    [SerializeField] private AssetReference koreanTestScene;
    [SerializeField] private AssetReference englishTestScene;
    
    public void LoadLocalizationTestScene()
    {
        // 현재 언어에 따른 테스트 씬 로드
        SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        
        AssetReference targetScene = currentLanguage == SystemLanguage.Korean 
            ? koreanTestScene 
            : englishTestScene;
            
        AddressableSceneLoadingManager.LoadAddressableScene(targetScene, true);
    }
}
```

## 📦 빌드 설정

### **1. Addressables 빌드**

#### **A. 콘텐츠 빌드**
1. **Window > Asset Management > Addressables > Build**
2. **New Build > Default Build Script** 선택
3. **Build** 클릭

#### **B. 카탈로그 생성**
- 빌드 후 `Assets/AddressableAssetsData/` 폴더에 카탈로그 파일들이 생성됩니다.

### **2. 플랫폼별 설정**

#### **A. Android 설정**
```
Player Settings > Other Settings:
- Scripting Backend: IL2CPP
- Target Architectures: ARM64

Addressables Groups:
- Platform: Android
- Build Path: [BuildPath]/[BuildTarget]/
```

#### **B. iOS 설정**
```
Player Settings > Other Settings:
- Scripting Backend: IL2CPP
- Target Architectures: ARM64

Addressables Groups:
- Platform: iOS
- Build Path: [BuildPath]/[BuildTarget]/
```

## 🔍 장점과 단점

### **장점:**
- ✅ **동적 씬 로딩**: 런타임에 씬을 다운로드하고 로드
- ✅ **번들 크기 감소**: 필요한 씬만 다운로드
- ✅ **유연한 씬 관리**: 씬을 그룹별로 관리
- ✅ **업데이트 용이**: 씬만 별도 업데이트 가능
- ✅ **InfoHUD 테스트**: InfoHUD 시스템 전용 테스트 씬 관리
- ✅ **로컬라이제이션 테스트**: 언어별 테스트 씬 관리

### **단점:**
- ❌ **초기 로딩 시간**: 첫 번째 씬 로딩 시 다운로드 필요
- ❌ **네트워크 의존성**: 온라인 상태에서만 씬 다운로드 가능
- ❌ **복잡성 증가**: 씬 관리 시스템이 복잡해짐

## 🎮 InfoHUD 시스템과의 통합

### **1. InfoHUD 테스트 씬 구성**

```csharp
// InfoHUD 테스트 씬에서 사용할 매니저
public class InfoHUDTestManager : MonoBehaviour
{
    [Header("Test Objects")]
    [SerializeField] private GameObject[] testObjects;
    
    private void Start()
    {
        // InfoHUD 시스템 초기화 확인
        if (TouchInfoManager.Instance != null)
        {
            Debug.Log("TouchInfoManager 초기화 완료");
        }
        
        // 테스트 오브젝트에 InfoHUD 데이터 설정
        SetupTestObjects();
    }
    
    private void SetupTestObjects()
    {
        foreach (GameObject obj in testObjects)
        {
            // 각 오브젝트에 InfoHUD 데이터 추가
            InfoHUDData data = obj.GetComponent<InfoHUDData>();
            if (data == null)
            {
                data = obj.AddComponent<InfoHUDData>();
            }
            
            data.title = $"{obj.name} 정보";
            data.description = $"{obj.name}에 대한 상세한 설명입니다.";
        }
    }
}
```

### **2. 터치 테스트 씬 구성**

```csharp
// 터치 테스트 씬에서 사용할 매니저
public class TouchTestManager : MonoBehaviour
{
    [Header("Touch Test Settings")]
    [SerializeField] private bool enableTouchDebug = true;
    [SerializeField] private Color touchDebugColor = Color.red;
    
    private void Update()
    {
        if (enableTouchDebug && Input.GetMouseButtonDown(0))
        {
            // 터치 위치 시각화
            Vector2 touchPosition = Input.mousePosition;
            Debug.Log($"터치 위치: {touchPosition}");
            
            // 터치된 오브젝트 확인
            GameObject hitObject = GetObjectAtPosition(touchPosition);
            if (hitObject != null)
            {
                Debug.Log($"터치된 오브젝트: {hitObject.name}");
            }
        }
    }
    
    private GameObject GetObjectAtPosition(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject;
        }
        
        return null;
    }
}
```

## 🌐 로컬라이제이션과의 통합

### **1. 언어별 테스트 씬**

```csharp
// 언어별 테스트 씬 매니저
public class LocalizationTestManager : MonoBehaviour
{
    [Header("Localization Test")]
    [SerializeField] private TextMeshProUGUI[] testTexts;
    
    private void Start()
    {
        // 현재 언어 확인
        SystemLanguage currentLanguage = LocalizationManager.Instance.CurrentLanguage;
        Debug.Log($"현재 언어: {currentLanguage}");
        
        // 테스트 텍스트 업데이트
        UpdateTestTexts();
    }
    
    private void UpdateTestTexts()
    {
        foreach (TextMeshProUGUI text in testTexts)
        {
            if (text != null)
            {
                // 로컬라이제이션 키 추출 (예: "TitleText" -> "title")
                string key = ExtractLocalizationKey(text.name);
                string localizedText = LocalizationManager.Instance.GetLocalizedText(key);
                
                if (!string.IsNullOrEmpty(localizedText))
                {
                    text.text = localizedText;
                }
            }
        }
    }
    
    private string ExtractLocalizationKey(string textName)
    {
        // "TitleText" -> "title", "DescriptionText" -> "description"
        if (textName.EndsWith("Text"))
        {
            string key = textName.Substring(0, textName.Length - 4);
            return key.ToLower();
        }
        return textName.ToLower();
    }
}
```

## 🛠️ 문제 해결

### **1. 일반적인 문제들**

**씬 로드 실패:**
```
[AddressableSceneLoadingManager] 씬 로드 실패: Scenes/Level1Scene
```
- **해결**: Addressable 키 확인
- **해결**: 씬이 올바른 그룹에 있는지 확인
- **해결**: Addressables 빌드 재실행

**씬 전환 시 InfoHUD 문제:**
```
[TouchInfoManager] 씬 전환 후 InfoHUD 초기화 실패
```
- **해결**: 씬 전환 시 InfoHUD 정리 확인
- **해결**: TouchInfoManager 재초기화 확인

**로컬라이제이션 씬 문제:**
```
[LocalizationManager] 언어별 씬 로드 실패
```
- **해결**: 언어별 씬 Addressable 키 확인
- **해결**: LocalizationManager 초기화 확인

### **2. 성능 최적화**

**씬 로딩 최적화:**
```csharp
// 씬 미리 로드
public async void PreloadScene(string sceneName)
{
    var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, false);
    await handle.Task;
    
    // 씬은 로드되었지만 활성화되지 않음
    // 필요할 때 활성화
    handle.Result.ActivateAsync();
}
```

**메모리 관리:**
```csharp
// 씬 전환 시 메모리 정리
public async void LoadSceneWithCleanup(string sceneName)
{
    // InfoHUD 정리
    UIManager.Instance.DestroyAllInfoHUDs();
    
    // UI 정리
    UIManager.Instance.ReleaseAllAddressables();
    
    // 씬 로드
    await AddressableSceneLoadingManager.LoadAddressableScene(sceneName, true);
}
```

## 📚 추가 리소스

- [Unity Addressables 공식 문서](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [프로젝트 README.md](./README.md)
- [Addressable UI 설정 가이드](./Addressable_UI_Setup_Guide.md)
- [현재 사용 패턴 가이드](./현재_사용_패턴_가이드.md)

## 🎯 모범 사례

### **1. 씬 구성 원칙**
- **핵심 씬**: Build Settings에 포함 (빠른 로딩)
- **게임 씬**: Addressables로 관리 (동적 로딩)
- **테스트 씬**: 별도 그룹으로 관리 (개발용)

### **2. InfoHUD 테스트 원칙**
- **전용 테스트 씬**: InfoHUD 기능 전용 테스트
- **다양한 시나리오**: 다양한 터치 상황 테스트
- **성능 테스트**: 실제 디바이스에서 성능 확인

### **3. 로컬라이제이션 테스트 원칙**
- **언어별 테스트**: 각 언어별 UI 확인
- **동적 언어 변경**: 런타임 언어 변경 테스트
- **텍스트 길이 테스트**: 긴 텍스트 UI 레이아웃 확인

---

**버전**: 2.1  
**최종 업데이트**: 2025년 8월  
**Unity 버전**: 2022.3 LTS 이상  
**주요 업데이트**: InfoHUD 시스템, 중복 생성 방지, 로컬라이제이션, 테스트 씬 관리
