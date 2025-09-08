# 레이어별 HUD 시스템 사용 가이드

## 📋 개요

TouchInfoManager가 레이어별로 다른 HUD를 생성하고 데이터를 연결할 수 있도록 개선되었습니다. 각 레이어에 맞는 특화된 HUD 프리팹을 사용하여 더 풍부한 정보와 상호작용을 제공할 수 있습니다.

## 🏗️ 시스템 구성

### 1. HUD 타입 정의
```csharp
public enum HUDType
{
    Building,       // 건물 정보
    Worker,         // 작업자 정보
    Resource,       // 자원 정보
    Facility,       // 시설 정보
    NPC,           // NPC 정보
    Default         // 기본 정보
}
```

### 2. HUD 데이터 구조
```csharp
public class HUDData
{
    public string title;
    public string description;
    public Sprite icon;
    public HUDType hudType;
    public Dictionary<string, object> customData;
}
```

## 🎯 레이어별 HUD 프리팹

### BuildingInfoHUD (건물 정보)
- **특화 기능**: 건물 타입, 레벨, 효율성, 상태 표시
- **액션 버튼**: 업그레이드, 철거
- **데이터**: buildingType, level, efficiency

### WorkerInfoHUD (작업자 정보)
- **특화 기능**: 작업자 레벨, 기술, 만족도, 급여 표시
- **액션 버튼**: 고용, 해고, 훈련
- **데이터**: workerLevel, skill, happiness, salary

### ResourceInfoHUD (자원 정보)
- **특화 기능**: 자원 타입, 수량, 품질, 상태 표시
- **액션 버튼**: 채굴, 운송
- **데이터**: resourceType, quantity, quality

### FacilityInfoHUD (시설 정보)
- **특화 기능**: 시설 타입, 용량, 사용량, 상태 표시
- **액션 버튼**: 이용, 유지보수
- **데이터**: facilityType, capacity, currentUsage

### NPCInfoHUD (NPC 정보)
- **특화 기능**: NPC 타입, 평판, 퀘스트 수, 상태 표시
- **액션 버튼**: 대화, 거래, 퀘스트
- **데이터**: npcType, reputation, availableQuests

## ⚙️ 설정 방법

### 1. TouchInfoManager 설정
```csharp
// 인스펙터에서 레이어별 레이어마스크 설정
[SerializeField] private LayerMask buildingLayerMask = 1 << 8; // Building 레이어
[SerializeField] private LayerMask workerLayerMask = 1 << 9;   // Worker 레이어
[SerializeField] private LayerMask resourceLayerMask = 1 << 10; // Resource 레이어
[SerializeField] private LayerMask facilityLayerMask = 1 << 11; // Facility 레이어
[SerializeField] private LayerMask npcLayerMask = 1 << 12;      // NPC 레이어

// HUD 프리팹 키 설정
[SerializeField] private string buildingHUDKey = "BuildingInfoHUD";
[SerializeField] private string workerHUDKey = "WorkerInfoHUD";
[SerializeField] private string resourceHUDKey = "ResourceInfoHUD";
[SerializeField] private string facilityHUDKey = "FacilityInfoHUD";
[SerializeField] private string npcHUDKey = "NPCInfoHUD";
```

### 2. 레이어 설정
Unity에서 레이어를 다음과 같이 설정하세요:
- **Layer 8**: Building
- **Layer 9**: Worker
- **Layer 10**: Resource
- **Layer 11**: Facility
- **Layer 12**: NPC

### 3. Addressable 설정
각 HUD 프리팹을 Addressable에 등록하고 키를 설정하세요:
- `BuildingInfoHUD`
- `WorkerInfoHUD`
- `ResourceInfoHUD`
- `FacilityInfoHUD`
- `NPCInfoHUD`

## 💻 사용 예제

### 1. 기본 사용법
```csharp
// TouchInfoManager는 자동으로 레이어를 감지하여 적절한 HUD를 생성합니다
// 별도의 코드 작성 없이 오브젝트의 레이어만 설정하면 됩니다

// 예: 건물 오브젝트의 레이어를 Building으로 설정
buildingObject.layer = 8; // Building 레이어
```

### 2. 프로그래밍 방식으로 HUD 생성
```csharp
// 특정 레이어의 오브젝트에 대해 강제로 HUD 타입 지정
TouchInfoManager touchManager = FindObjectOfType<TouchInfoManager>();
touchManager.ShowHUDForLayerObject(targetObject, screenPosition, HUDType.Building);
```

### 3. 레이어별 HUD 키 동적 설정
```csharp
// 런타임에 HUD 키 변경
touchManager.SetLayerHUDKey(HUDType.Building, "CustomBuildingHUD");
```

### 4. 레이어별 레이어마스크 동적 설정
```csharp
// 런타임에 레이어마스크 변경
LayerMask customBuildingMask = 1 << 13; // 새로운 레이어
touchManager.SetLayerMask(HUDType.Building, customBuildingMask);
```

## 🔧 커스터마이징

### 1. 새로운 HUD 타입 추가
```csharp
// HUDType enum에 새로운 타입 추가
public enum HUDType
{
    Building,
    Worker,
    Resource,
    Facility,
    NPC,
    Custom,     // 새로운 타입
    Default
}

// TouchInfoManager에 새로운 레이어마스크 추가
[SerializeField] private LayerMask customLayerMask = 1 << 13;

// InitializeLayerMapping() 메서드에 매핑 추가
layerToHUDType.Add(customLayerMask, HUDType.Custom);
```

### 2. 새로운 HUD 프리팹 생성
```csharp
// TouchInfoHUD를 상속받아 새로운 HUD 클래스 생성
public class CustomInfoHUD : TouchInfoHUD
{
    [Header("Custom Specific UI Elements")]
    [SerializeField] private string customTextName = "CustomText";
    
    private TextMeshProUGUI customText => GetUI<TextMeshProUGUI>(customTextName);
    
    public override string[] GetAutoLocalizeKeys()
    {
        return new string[]
        {
            "custom_text"
        };
    }
    
    public void SetCustomInfo(string customData)
    {
        if (customText != null)
            customText.text = customData;
    }
}
```

### 3. 레이어별 데이터 생성 커스터마이징
```csharp
// TouchInfoManager의 CreateHUDDataForObject 메서드 수정
private HUDData CreateCustomHUDData(GameObject customObject)
{
    HUDData data = new HUDData(
        customObject.name,
        $"커스텀 정보\n위치: {customObject.transform.position}",
        null,
        HUDType.Custom
    );
    
    // 커스텀 데이터 추가
    data.customData["customProperty"] = "커스텀 값";
    
    return data;
}
```

## 🧪 테스트 방법

### 1. Context Menu 테스트
```csharp
// TouchInfoManager의 Context Menu 사용
[ContextMenu("테스트 - 레이어별 HUD")]
public void TestLayerSpecificHUD()
{
    // 각 레이어별 HUD 테스트
}
```

### 2. 런타임 테스트
```csharp
// 게임 실행 중 테스트
public void TestHUDs()
{
    // 건물 HUD 테스트
    var buildingData = new HUDData("테스트 건물", "건물 정보", null, HUDType.Building);
    CreateLayerSpecificHUD(buildingData, Input.mousePosition);
    
    // 작업자 HUD 테스트
    var workerData = new HUDData("테스트 작업자", "작업자 정보", null, HUDType.Worker);
    CreateLayerSpecificHUD(workerData, Input.mousePosition);
}
```

## 📝 주의사항

1. **레이어 설정**: 오브젝트의 레이어가 올바르게 설정되어야 합니다.
2. **Addressable 등록**: 모든 HUD 프리팹이 Addressable에 등록되어야 합니다.
3. **키 일치**: TouchInfoManager의 HUD 키와 Addressable 키가 일치해야 합니다.
4. **Fallback**: 특정 HUD를 찾을 수 없는 경우 기본 TouchInfoHUD가 사용됩니다.

## 🎮 실제 게임에서의 활용

### 건물 관리 시스템
```csharp
// 건물 클릭 시 상세 정보 표시
// - 건물 타입, 레벨, 효율성
// - 업그레이드, 철거 옵션
```

### 작업자 관리 시스템
```csharp
// 작업자 클릭 시 상세 정보 표시
// - 레벨, 기술, 만족도, 급여
// - 고용, 해고, 훈련 옵션
```

### 자원 관리 시스템
```csharp
// 자원 클릭 시 상세 정보 표시
// - 자원 타입, 수량, 품질
// - 채굴, 운송 옵션
```

이 시스템을 통해 각 오브젝트 타입에 맞는 특화된 정보와 상호작용을 제공할 수 있습니다.
