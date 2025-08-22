# Addressables 씬 설정 가이드

## 📋 개요

Addressables를 사용하면 Build Settings에 씬을 추가하지 않고도 동적으로 씬을 로드할 수 있습니다. 이를 통해 더 유연한 씬 관리가 가능합니다.

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
└── StoryScenes/
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

### **단점:**
- ❌ **복잡한 설정**: 초기 설정이 복잡
- ❌ **네트워크 의존**: 온라인 씬 로딩 시 네트워크 필요
- ❌ **로딩 시간**: 첫 로딩 시 다운로드 시간 필요

## 🚀 최적화 팁

### **1. 씬 번들 최적화**
```
- 씬 크기 최소화
- 공통 에셋 분리
- LOD 시스템 활용
```

### **2. 로딩 전략**
```
- 프리로딩: 자주 사용되는 씬 미리 로드
- 백그라운드 로딩: 다음 씬 미리 로드
- 캐싱: 로드된 씬 메모리에 유지
```

### **3. 에러 처리**
```csharp
try
{
    AddressableSceneLoadingManager.LoadAddressableScene(sceneName, true);
}
catch (System.Exception e)
{
    Debug.LogError($"씬 로딩 실패: {e.Message}");
    // 폴백 씬 로드
    SceneManager.LoadScene("ErrorScene");
}
```

## 📁 파일 구조 예시

```
Assets/
├── Scenes/
│   ├── BootScene.unity (Build Settings 포함)
│   ├── LoadingScene.unity (Build Settings 포함)
│   ├── MainMenuScene.unity (Build Settings 포함)
│   ├── Level1Scene.unity (Addressables)
│   ├── Level2Scene.unity (Addressables)
│   └── BossScene.unity (Addressables)
├── AddressableAssetsData/
│   ├── AssetGroups/
│   ├── DataBuilders/
│   └── Settings/
└── Scripts/
    └── UI/
        ├── AddressableSceneLoadingManager.cs
        └── LoadingScreen.cs
```

이 가이드를 따라 설정하면 Addressables를 사용한 효율적인 씬 관리가 가능합니다!
