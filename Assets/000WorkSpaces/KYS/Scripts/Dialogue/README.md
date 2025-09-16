# Dialogue 시스템 종합 가이드

## 📋 개요

KYS Dialogue 시스템은 **CSV 기반 대화 데이터**와 **Firebase 연동**을 통해 복잡한 대화 시스템을 구현합니다. 다국어 지원, 선택지 시스템, 조건부 대화, 이미지 관리 등 다양한 기능을 제공합니다.

## 🎯 주요 특징

- ✅ **CSV 기반 데이터**: Google Sheets 연동으로 대화 데이터 관리
- ✅ **Firebase 연동**: 실시간 데이터 동기화 및 사용자 진행도 저장
- ✅ **다국어 지원**: 한국어, 영어 등 동적 언어 변경
- ✅ **선택지 시스템**: 4개까지 선택지 지원
- ✅ **조건부 대화**: 퀘스트, 아이템, 플래그 기반 조건
- ✅ **이미지 관리**: Addressable 기반 캐릭터/배경 이미지
- ✅ **타이핑 효과**: 부드러운 텍스트 표시
- ✅ **자동 진행**: 설정 가능한 자동 대화 진행

## 🏗️ 시스템 구조

### 1. 핵심 컴포넌트

```
Dialogue/
├── DialogueManager.cs          # 대화 시스템 관리자 (Singleton)
├── DialogueData.cs             # 대화 데이터 클래스 (Firebase 연동)
├── DataManager_Dialogue.cs     # CSV 데이터 파싱 및 이미지 관리
├── DialogueGraph.cs            # 노드 기반 대화 그래프 (선택사항)
├── DialogueNode.cs             # 대화 노드 정의
├── DialogueSystemTester.cs     # 테스트 및 디버깅 도구
└── UserData_Dialogue.cs        # 사용자 대화 진행도 저장
```

### 2. 데이터 흐름

```
Google Sheets → CSV → DataManager → DialogueManager → StoryPanel → UI
     ↓
Firebase ← UserData (진행도 저장)
```

## ⚙️ Addressable 설정

### 1. Addressable 패키지 설치

```bash
# Package Manager에서 설치
com.unity.addressables@1.21.19
```

### 2. Addressable 그룹 생성

#### Unity Editor에서 설정
1. **Window → Asset Management → Addressables → Groups** 열기
2. **Create → New Group** 으로 그룹 생성:
   - `Characters` - 캐릭터 이미지용
   - `Backgrounds` - 배경 이미지용  
   - `Constellations` - 별자리 이미지용
   - `Events` - 이벤트 이미지용

#### 그룹 설정
```csharp
// Addressable 그룹 설정 예시
[CreateAssetMenu(fileName = "DialogueAddressableSettings", menuName = "KYS/Dialogue Addressable Settings")]
public class DialogueAddressableSettings : ScriptableObject
{
    [Header("Addressable 그룹 설정")]
    public string characterGroupName = "Characters";
    public string backgroundGroupName = "Backgrounds";
    public string constellationGroupName = "Constellations";
    public string eventGroupName = "Events";
    
    [Header("이미지 폴더 경로")]
    public string characterFolderPath = "Assets/Images/Characters/";
    public string backgroundFolderPath = "Assets/Images/Backgrounds/";
    public string constellationFolderPath = "Assets/Images/Constellations/";
    public string eventFolderPath = "Assets/Images/Events/";
}
```

### 3. 이미지 등록 방법

#### 자동 등록 스크립트
```csharp
// Addressable 자동 등록 도구
[MenuItem("KYS/Setup Dialogue Addressables")]
public static void SetupDialogueAddressables()
{
    // Characters 폴더의 모든 이미지를 Characters 그룹에 등록
    SetupImageGroup("Assets/Images/Characters/", "Characters");
    
    // Backgrounds 폴더의 모든 이미지를 Backgrounds 그룹에 등록
    SetupImageGroup("Assets/Images/Backgrounds/", "Backgrounds");
    
    // Constellations 폴더의 모든 이미지를 Constellations 그룹에 등록
    SetupImageGroup("Assets/Images/Constellations/", "Constellations");
    
    // Events 폴더의 모든 이미지를 Events 그룹에 등록
    SetupImageGroup("Assets/Images/Events/", "Events");
}

private static void SetupImageGroup(string folderPath, string groupName)
{
    var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
    if (group == null)
    {
        Debug.LogError($"Addressable 그룹을 찾을 수 없습니다: {groupName}");
        return;
    }
    
    var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
    foreach (var guid in guids)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var fileName = Path.GetFileNameWithoutExtension(path);
        
        var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group);
        entry.address = fileName; // 파일명을 그대로 키로 사용
        
        Debug.Log($"Addressable 등록: {fileName} -> {path}");
    }
}
```

### 4. Addressable 빌드 설정

#### 빌드 스크립트
```csharp
// Addressable 빌드 자동화
[MenuItem("KYS/Build Addressables")]
public static async void BuildAddressables()
{
    try
    {
        Debug.Log("Addressable 빌드 시작...");
        
        // Addressable 콘텐츠 빌드
        var result = await AddressableAssetSettings.BuildPlayerContent();
        
        if (result.Length > 0)
        {
            Debug.Log($"Addressable 빌드 완료: {result.Length}개 그룹");
        }
        else
        {
            Debug.LogWarning("Addressable 빌드 결과가 비어있습니다.");
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Addressable 빌드 실패: {e.Message}");
    }
}
```

## 🚀 기본 사용법

### 1. 대화 시작

```csharp
// 기본 대화 시작
bool success = DialogueManager.Instance.StartDialogue("npc001", "stage_01");

// 특정 노드부터 시작
bool success = DialogueManager.Instance.StartDialogue("npc001", "stage_01", "npc001_start");

// StoryPanel과 함께 시작 (UI 자동 표시)
bool success = DialogueManager.Instance.StartDialogueWithPanel("npc001", "stage_01");
```

### 2. 대화 진행

```csharp
// 다음 노드로 이동
bool success = DialogueManager.Instance.MoveToNextNode();

// 특정 노드로 이동
bool success = DialogueManager.Instance.MoveToNode("npc001_choice");

// 선택지 선택 (0-3 인덱스)
bool success = DialogueManager.Instance.SelectChoice(0);
```

### 3. 대화 종료

```csharp
// 대화 종료
DialogueManager.Instance.EndDialogue();
```

## 📊 CSV 데이터 구조

### 1. 필수 컬럼

| 컬럼명 | 설명 | 예시 |
|--------|------|------|
| Id | 노드 고유 ID | npc001_start |
| NpcId | NPC ID | npc001 |
| StageId | 스테이지 ID | stage_01 |
| NodeType | 노드 타입 | dialogue, choice, story, end |
| Speaker | 스피커 이름 | Player, NPC |
| DialogueText | 대화 텍스트 | 안녕하세요! |

### 2. 선택지 관련 컬럼

| 컬럼명 | 설명 | 예시 |
|--------|------|------|
| ChoiceText1 | 선택지 1 텍스트 | 네, 좋습니다 |
| ChoiceNext1 | 선택지 1 다음 노드 | npc001_yes |
| ChoiceText2 | 선택지 2 텍스트 | 아니요, 싫습니다 |
| ChoiceNext2 | 선택지 2 다음 노드 | npc001_no |

### 3. 이미지 관련 컬럼 (Addressable)

| 컬럼명 | 설명 | 예시 | Addressable 그룹 |
|--------|------|------|------------------|
| CharacterImage | 캐릭터 이미지 키 | npc001 | Characters/ |
| CharacterImagePosition | 이미지 위치 | left, right, center | - |
| BackgroundImage | 배경 이미지 키 | forest | Backgrounds/ |
| ConstellationImage | 별자리 이미지 키 | leo | Constellations/ |
| CenterImage | 가운데 이미지 키 | event_001 | Events/ |
| CenterImageDuration | 가운데 이미지 표시 시간 | 3.0, infinite | - |
| CenterImageFadeInTime | 페이드인 시간 | 0.5 | - |
| CenterImageFadeOutTime | 페이드아웃 시간 | 0.5 | - |
| HideCharacterImages | 캐릭터 이미지 숨김 | true, false | - |

### 4. 다국어 지원 컬럼

| 컬럼명 | 설명 | 예시 |
|--------|------|------|
| Speaker_Korea | 한국어 스피커 이름 | 플레이어 |
| Speaker_English | 영어 스피커 이름 | Player |
| DialogueText_Korea | 한국어 대화 텍스트 | 안녕하세요! |
| DialogueText_English | 영어 대화 텍스트 | Hello! |

### 5. 고급 기능 컬럼

| 컬럼명 | 설명 | 예시 |
|--------|------|------|
| UseTypingEffect | 타이핑 효과 사용 | true, false |
| AutoAdvanceDelay | 자동 진행 지연 시간 | 3.0 |
| ConditionType | 조건 타입 | QuestProgress, ItemOwned |
| ConditionValue | 조건 값 | quest_001, item_sword |
| EffectType | 효과 타입 | SetFlag, GiveItem |
| EffectValue | 효과 값 | flag_talked, item_potion |

## 🎨 노드 타입별 사용법

### 1. Dialogue 노드 (일반 대화)

```csv
Id,NpcId,StageId,NodeType,Speaker,DialogueText,NextNodeId,CharacterImage,CharacterImagePosition
npc001_start,npc001,stage_01,dialogue,NPC,안녕하세요! 처음 뵙네요.,npc001_choice,character_npc001,right
```

**특징:**
- 캐릭터 이미지와 함께 대화 표시
- CharacterImagePosition으로 이미지 위치 제어 (left/right/center)
- NextNodeId로 다음 노드 지정

### 2. Choice 노드 (선택지)

```csv
Id,NpcId,StageId,NodeType,Speaker,DialogueText,ChoiceText1,ChoiceNext1,ChoiceText2,ChoiceNext2
npc001_choice,npc001,stage_01,choice,NPC,어떻게 하시겠어요?,네, 좋습니다,npc001_yes,아니요, 싫습니다,npc001_no
```

**특징:**
- 최대 4개 선택지 지원
- 각 선택지마다 다른 다음 노드 지정
- StoryPanel에서 자동으로 선택지 UI 생성

### 3. Story 노드 (스토리)

```csv
Id,NpcId,StageId,NodeType,Speaker,DialogueText,NextNodeId,BackgroundImage
npc001_story,npc001,stage_01,story,Story,어느 날, 마법의 숲에서...,npc001_end,,
```

**특징:**
- 캐릭터 이미지 없이 스토리 텍스트만 표시
- 배경 이미지와 함께 사용 가능
- 내레이션 용도로 활용

### 4. End 노드 (종료)

```csv
Id,NpcId,StageId,NodeType,Speaker,DialogueText
npc001_end,npc001,stage_01,end,NPC,그럼 안녕히 가세요!
```

**특징:**
- 대화 종료를 알리는 노드
- NextNodeId 없음
- 자동으로 대화 종료

## 🌍 다국어 지원

### 1. 언어별 컬럼 사용

```csv
Id,Speaker,Speaker_Korea,Speaker_English,DialogueText,DialogueText_Korea,DialogueText_English
npc001_start,Player,플레이어,Player,Hello!,안녕하세요!,Hello!
```

### 2. 코드에서 언어 변경

```csharp
// 언어 변경
LocalizationManager.Instance.SetLanguage(SystemLanguage.English);

// 현재 언어에 맞는 텍스트 자동 표시
string localizedText = dialogueData.GetLocalizedDialogueText(SystemLanguage.English);
```

### 3. 동적 언어 지원

```csharp
// 현재 설정된 언어로 자동 번역
string speaker = dialogueData.GetLocalizedSpeaker();
string text = dialogueData.GetLocalizedDialogueText();
string[] choices = dialogueData.GetLocalizedChoiceTexts();
```

## 🖼️ 이미지 관리 (Addressable 시스템)

### 1. Addressable 설정

#### Addressable 그룹 구성
```
Addressable Groups/
├── Characters/           # 캐릭터 이미지
│   ├── npc001
│   ├── npc002
│   └── player
├── Backgrounds/          # 배경 이미지
│   ├── forest
│   ├── castle
│   └── village
├── Constellations/       # 별자리 이미지
│   ├── leo
│   ├── aries
│   └── gemini
└── Events/              # 이벤트 이미지
    ├── event_001
    ├── event_002
    └── event_003
```

#### Addressable 키 사용법
- **자유로운 키 명명**: Addressable에 등록된 키를 그대로 사용
- **CSV에서 직접 지정**: CharacterImage, BackgroundImage 등 컬럼에 키 값 입력
- **예시**: `npc001`, `forest_bg`, `leo_constellation` 등 자유롭게 설정 가능

### 2. 이미지 로드 시스템

#### DataManager의 이미지 관리
```csharp
// DataManager_Dialogue.cs에서 자동으로 처리
public async Task<Sprite> LoadCharacterImageAsync(string imageKey)
{
    // 1. 캐시에서 먼저 확인
    if (imageCache.TryGetValue(imageKey, out Sprite cachedSprite))
    {
        return cachedSprite;
    }

    // 2. Addressable에서 비동기 로드
    var handle = Addressables.LoadAssetAsync<Sprite>(imageKey);
    var sprite = await handle.Task;
    
    // 3. 캐시에 저장
    if (sprite != null)
    {
        imageCache[imageKey] = sprite;
    }
    
    return sprite;
}
```

#### 캐시 시스템 활용
```csharp
// 동기적으로 캐시된 이미지 가져오기
Sprite cachedSprite = Manager.data.GetCachedCharacterImage("character_npc001");
if (cachedSprite != null)
{
    // 즉시 사용 가능
    storyPanel.SetCharacterImage(cachedSprite, true);
}
```

### 3. 이미지 타입별 사용법

#### 캐릭터 이미지
```csharp
// CSV에서 CharacterImage 컬럼 사용
// npc001, player 등 (Addressable에 등록된 키 그대로 사용)

// 자동 로드 및 설정
private async void LoadAndSetCharacterImage(string imageKey)
{
    Sprite characterSprite = await Manager.data.LoadCharacterImageAsync(imageKey);
    if (characterSprite != null)
    {
        // StoryPanel에서 자동으로 적절한 위치에 설정
        storyPanel.SetCharacterImage(characterSprite, isRightCharacter);
    }
}
```

#### 배경 이미지
```csharp
// CSV에서 BackgroundImage 컬럼 사용
// forest, castle 등 (Addressable에 등록된 키 그대로 사용)

// 배경 이미지 설정
if (!string.IsNullOrEmpty(dialogueData.BackgroundImage))
{
    Sprite bgSprite = await Manager.data.LoadCharacterImageAsync(dialogueData.BackgroundImage);
    storyPanel.SetBackgroundImage(bgSprite);
}
```

#### 별자리 이미지
```csharp
// CSV에서 ConstellationImage 컬럼 사용
// leo, aries 등 (Addressable에 등록된 키 그대로 사용)

// 별자리 이미지 설정
if (!string.IsNullOrEmpty(dialogueData.ConstellationImage))
{
    Sprite constellationSprite = await Manager.data.LoadCharacterImageAsync(dialogueData.ConstellationImage);
    storyPanel.SetConstellationImage(constellationSprite);
}
```

#### 가운데 이미지 (이벤트)
```csharp
// CSV에서 CenterImage 컬럼 사용
// event_001, event_002 등 (Addressable에 등록된 키 그대로 사용)

// 가운데 이미지 표시 (페이드 효과 포함)
if (!string.IsNullOrEmpty(dialogueData.CenterImage))
{
    storyPanel.ShowCenterImage(
        dialogueData.CenterImage,
        dialogueData.CenterImageDuration,
        dialogueData.CenterImageFadeInTime,
        dialogueData.CenterImageFadeOutTime
    );
}
```

### 4. 이미지 로드 최적화

#### 사전 로드 시스템
```csharp
// DataManager에서 대화 시작 시 모든 이미지 사전 로드
private async Task LoadAllCharacterImages()
{
    var uniqueImageKeys = new HashSet<string>();
    
    // CSV에서 사용되는 모든 이미지 키 수집
    foreach (var dialogue in Dialogue.Values.Values)
    {
        if (!string.IsNullOrEmpty(dialogue.CharacterImage))
            uniqueImageKeys.Add(dialogue.CharacterImage);
        if (!string.IsNullOrEmpty(dialogue.BackgroundImage))
            uniqueImageKeys.Add(dialogue.BackgroundImage);
        if (!string.IsNullOrEmpty(dialogue.ConstellationImage))
            uniqueImageKeys.Add(dialogue.ConstellationImage);
        if (!string.IsNullOrEmpty(dialogue.CenterImage))
            uniqueImageKeys.Add(dialogue.CenterImage);
    }
    
    // 병렬로 모든 이미지 로드
    var loadTasks = uniqueImageKeys.Select(key => LoadCharacterImageAsync(key));
    await Task.WhenAll(loadTasks);
}
```

#### 메모리 관리
```csharp
// 이미지 언로드 (필요시)
public void UnloadImage(string imageKey)
{
    if (imageCache.TryGetValue(imageKey, out Sprite sprite))
    {
        // Addressable 핸들 해제
        if (loadingHandles.TryGetValue(imageKey, out var handle))
        {
            Addressables.Release(handle);
            loadingHandles.Remove(imageKey);
        }
        
        // 캐시에서 제거
        imageCache.Remove(imageKey);
    }
}
```

### 5. Addressable 설정 가이드

#### Unity Editor에서 설정
1. **Window → Asset Management → Addressables → Groups** 열기
2. **Create → New Group** 으로 그룹 생성
3. 이미지들을 해당 그룹으로 드래그
4. **Address** 필드에 키 이름 설정 (예: character_npc001)

#### 스크립트에서 Addressable 확인
```csharp
// Addressable 키 존재 여부 확인
public bool IsAddressableKeyValid(string key)
{
    return Addressables.ResourceLocators.Any(locator => 
        locator.Locate(key, typeof(Sprite), out var locations));
}
```

### 6. 에러 처리 및 폴백

#### 이미지 로드 실패 시 처리
```csharp
public async Task<Sprite> LoadCharacterImageAsync(string imageKey)
{
    try
    {
        // Addressable에서 로드 시도
        if (IsAddressableKeyValid(imageKey))
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(imageKey);
            var sprite = await handle.Task;
            
            if (sprite != null)
            {
                imageCache[imageKey] = sprite;
                return sprite;
            }
        }
        
        // Addressable 실패 시 Resources 폴더에서 시도
        return Resources.Load<Sprite>($"Characters/{imageKey}") ??
               Resources.Load<Sprite>($"UI/Characters/{imageKey}");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[DataManager] 이미지 로드 실패: {imageKey} - {e.Message}");
        return null;
    }
}
```

### 7. 성능 최적화 팁

#### 이미지 압축 설정
- **캐릭터 이미지**: RGBA32, 압축 없음 (품질 우선)
- **배경 이미지**: DXT5, 압축 사용 (용량 우선)
- **UI 이미지**: RGBA32, 압축 없음 (선명도 우선)

#### 로드 우선순위
```csharp
// 중요한 이미지 우선 로드
private async Task LoadCriticalImages()
{
    // 현재 대화에 필요한 이미지들 먼저 로드
    var currentDialogue = DialogueManager.Instance.CurrentDialogueData;
    if (currentDialogue != null)
    {
        await LoadCharacterImageAsync(currentDialogue.CharacterImage);
        await LoadCharacterImageAsync(currentDialogue.BackgroundImage);
    }
}
```

## ⚙️ 조건부 대화

### 1. 퀘스트 진행도 조건

```csv
Id,NodeType,ConditionType,ConditionValue,DialogueText
npc001_quest,dialogue,QuestProgress,quest_001_50,퀘스트를 50% 완료했군요!
```

### 2. 아이템 보유 조건

```csv
Id,NodeType,ConditionType,ConditionValue,DialogueText
npc001_item,dialogue,ItemOwned,item_sword,검을 가지고 있군요!
```

### 3. 플래그 설정 조건

```csv
Id,NodeType,ConditionType,ConditionValue,DialogueText
npc001_flag,dialogue,FlagSet,flag_talked,이미 말씀드린 내용이에요.
```

## 🎭 이벤트 및 효과

### 1. 플래그 설정

```csv
Id,NodeType,EffectType,EffectValue,DialogueText
npc001_flag_set,dialogue,SetFlag,flag_talked_npc001,이제 이 플래그가 설정됩니다.
```

### 2. 아이템 지급

```csv
Id,NodeType,EffectType,EffectValue,DialogueText
npc001_give_item,dialogue,GiveItem,item_potion,포션을 드릴게요!
```

### 3. 퀘스트 시작

```csv
Id,NodeType,EffectType,EffectValue,DialogueText
npc001_start_quest,dialogue,StartQuest,quest_001,새로운 퀘스트를 시작합니다!
```

## 🔧 고급 설정

### 1. 타이핑 효과 제어

```csv
Id,NodeType,UseTypingEffect,DialogueText
npc001_typing,dialogue,true,타이핑 효과가 있는 텍스트입니다.
npc001_instant,dialogue,false,즉시 표시되는 텍스트입니다.
```

### 2. 자동 진행 설정

```csv
Id,NodeType,AutoAdvanceDelay,DialogueText
npc001_auto,dialogue,3.0,3초 후 자동으로 다음으로 진행됩니다.
```

### 3. 가운데 이미지 설정

```csv
Id,NodeType,CenterImage,CenterImageDuration,CenterImageFadeInTime,CenterImageFadeOutTime
npc001_center,dialogue,event_image,5.0,1.0,1.0,이벤트 이미지가 5초간 표시됩니다.
```

## 🧪 테스트 및 디버깅

### 1. DialogueSystemTester 사용

```csharp
// Inspector에서 Context Menu 사용
[ContextMenu("테스트 - 기본 대화 시작")]
public void TestStartBasicDialogue()
{
    DialogueManager.Instance.StartDialogueWithPanel("npc001", "stage_01");
}
```

### 2. 디버그 정보 출력

```csharp
// DialogueManager 디버그 정보
DialogueManager.Instance.PrintDebugInfo();

// 현재 대화 상태 확인
Debug.Log($"대화 활성화: {DialogueManager.Instance.IsDialogueActive}");
Debug.Log($"현재 노드: {DialogueManager.Instance.CurrentNodeId}");
```

### 3. 데이터 검증

```csharp
// CSV 데이터 로드 상태 확인
if (Manager.data.Dialogue != null)
{
    Debug.Log($"로드된 대화 데이터: {Manager.data.Dialogue.Values.Count}개");
}
```

## 📱 StoryPanel 연동

### 1. 자동 UI 업데이트

```csharp
// DialogueManager 이벤트 구독
DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
DialogueManager.Instance.OnDialogueNodeChanged += OnDialogueNodeChanged;
DialogueManager.Instance.OnChoiceSelected += OnChoiceSelected;
```

### 2. 수동 UI 제어

```csharp
// StoryPanel 직접 제어
StoryPanel storyPanel = FindObjectOfType<StoryPanel>();

// 대화 시작
storyPanel.StartCSVDialogue("npc001", "stage_01");

// 특정 노드로 이동
DialogueManager.Instance.MoveToNode("npc001_choice");
```

## 🚨 주의사항

### 1. 데이터 로드 순서

```csharp
// DataManager 초기화 후 사용
if (Manager.data.Dialogue != null)
{
    // 대화 시작
    DialogueManager.Instance.StartDialogue("npc001", "stage_01");
}
```

### 2. 메모리 관리

```csharp
// 이벤트 구독 해제
private void OnDestroy()
{
    if (DialogueManager.Instance != null)
    {
        DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
        DialogueManager.Instance.OnDialogueNodeChanged -= OnDialogueNodeChanged;
    }
}
```

### 3. Firebase 연동

```csharp
// Firebase 초기화 확인
if (Manager.firebase != null && Manager.firebase.IsInitialized)
{
    // Firebase 연동 기능 사용
    DialogueManager.Instance.StartDialogue("npc001", "stage_01");
}
```

### 4. Addressable 관련 주의사항

#### Addressable 키 사용 주의사항
```csharp
// Addressable에 등록된 키를 정확히 사용
string characterKey = "npc001";            // ✅ 올바름 (Addressable에 등록된 키)
string backgroundKey = "forest";           // ✅ 올바름 (Addressable에 등록된 키)
string eventKey = "event_001";             // ✅ 올바름 (Addressable에 등록된 키)

// 잘못된 키 사용
string wrongKey = "character_npc001";      // ❌ 잘못됨 (Addressable에 등록되지 않은 키)
string wrongKey2 = "npc_001";              // ❌ 잘못됨 (Addressable에 등록되지 않은 키)
```

#### 메모리 관리
```csharp
// 사용하지 않는 이미지는 언로드
public void CleanupUnusedImages()
{
    var currentDialogue = DialogueManager.Instance.CurrentDialogueData;
    var usedImages = new HashSet<string>();
    
    // 현재 사용 중인 이미지 수집
    if (currentDialogue != null)
    {
        if (!string.IsNullOrEmpty(currentDialogue.CharacterImage))
            usedImages.Add(currentDialogue.CharacterImage);
        if (!string.IsNullOrEmpty(currentDialogue.BackgroundImage))
            usedImages.Add(currentDialogue.BackgroundImage);
    }
    
    // 사용하지 않는 이미지 언로드
    foreach (var cachedImage in imageCache.Keys.ToList())
    {
        if (!usedImages.Contains(cachedImage))
        {
            Manager.data.UnloadImage(cachedImage);
        }
    }
}
```

#### Addressable 그룹 설정 확인
```csharp
// 빌드 전 Addressable 그룹 확인
[MenuItem("KYS/Check Addressable Groups")]
public static void CheckAddressableGroups()
{
    var settings = AddressableAssetSettingsDefaultObject.Settings;
    var requiredGroups = new[] { "Characters", "Backgrounds", "Constellations", "Events" };
    
    foreach (var groupName in requiredGroups)
    {
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            Debug.LogError($"필수 Addressable 그룹이 없습니다: {groupName}");
        }
        else
        {
            Debug.Log($"Addressable 그룹 확인: {groupName} ({group.entries.Count}개 항목)");
        }
    }
}
```

## 📚 API 참조

### DialogueManager 주요 메서드

- `StartDialogue(string npcId, string stageId, string startNodeId = null)` - 대화 시작
- `StartDialogueWithPanel(string npcId, string stageId, string startNodeId = null)` - UI와 함께 대화 시작
- `MoveToNextNode()` - 다음 노드로 이동
- `MoveToNode(string nodeId)` - 특정 노드로 이동
- `SelectChoice(int choiceIndex)` - 선택지 선택
- `EndDialogue()` - 대화 종료

### DialogueData 주요 프로퍼티

- `Id` - 노드 ID
- `NodeType` - 노드 타입
- `Speaker` - 스피커 이름
- `DialogueText` - 대화 텍스트
- `HasChoices` - 선택지 존재 여부
- `ChoiceCount` - 선택지 개수
- `IsAutoAdvance` - 자동 진행 여부

### DataManager 주요 메서드

- `LoadCharacterImageAsync(string imageKey)` - 이미지 비동기 로드 (Addressable)
- `GetCachedCharacterImage(string imageKey)` - 캐시된 이미지 가져오기
- `LoadAllCharacterImages()` - 모든 대화 이미지 사전 로드
- `UnloadImage(string imageKey)` - 이미지 언로드 (메모리 해제)
- `IsAddressableKeyValid(string key)` - Addressable 키 유효성 검사

## 🔄 업데이트 이력

- **v1.0** (2025년 9월 15일): 초기 버전 - 기본 대화 시스템
- **v1.1** (2025년 9월 15일): 다국어 지원 추가
- **v1.2** (2025년 9월 15일): 이미지 관리 시스템 추가
- **v1.3** (2025년 9월 15일): 조건부 대화 및 이벤트 시스템 추가

---

**버전**: 1.3  
**최종 업데이트**: 2025년 9월 15일  
**Unity 버전**: 2022.3 LTS 이상  
**주요 기능**: CSV 기반 대화, Firebase 연동, 다국어 지원, 이미지 관리, 조건부 대화
