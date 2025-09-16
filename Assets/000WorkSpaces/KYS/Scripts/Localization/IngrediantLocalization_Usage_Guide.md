# 재료 번역 시스템 사용 가이드

## 개요
재료 번역 시스템은 BuildingLocalization과 동일한 방식으로 구성되어 있으며, 재료 ID를 기반으로 다국어 지원을 제공합니다.

## 구성 요소

### 1. DataManager_IngrediantLocalization.cs
- 재료 번역 데이터를 CSV에서 로드하고 관리
- `GetIngrediantLocalizedName()` 메서드로 번역된 이름 조회

### 2. IngrediantLocalizationHelper.cs
- 재료 번역을 위한 헬퍼 클래스
- LocalizationManager와 DataManager를 활용한 번역 처리
- 언어 변경 이벤트 관리

### 3. IngrediantLocalizedText.cs
- UI에서 재료 이름을 자동으로 번역하여 표시하는 컴포넌트
- 재료 ID 변경 시 즉시 번역 적용

## CSV 데이터 형식

재료 번역용 CSV 파일은 다음 형식으로 구성해야 합니다:

```csv
ID,Name_Korean,Name_English
IngrediantA,재료A,Material A
IngrediantB,재료B,Material B
IngrediantC,재료C,Material C
```

### 필드 설명
- **ID**: 재료의 고유 식별자
- **Name_Korean**: 한국어 이름
- **Name_English**: 영어 이름

## 사용 방법

### 1. 기본 사용법

```csharp
// 재료 이름 가져오기
string ingrediantName = IngrediantLocalizationHelper.GetIngrediantName("IngrediantA");
Debug.Log(ingrediantName); // 현재 언어에 따라 "재료A" 또는 "Material A" 출력
```

### 2. UI 컴포넌트 사용

1. **IngrediantLocalizedText 컴포넌트 추가**:
   ```csharp
   // 재료 이름을 표시할 TextMeshProUGUI에 IngrediantLocalizedText 컴포넌트 추가
   IngrediantLocalizedText ingrediantText = textComponent.gameObject.AddComponent<IngrediantLocalizedText>();
   ```

2. **인스펙터에서 설정**:
   - Ingrediant ID: "IngrediantA"
   - Target Text: 번역된 텍스트를 표시할 TextMeshProUGUI

3. **런타임에서 재료 ID 변경**:
   ```csharp
   ingrediantText.SetIngrediantID("IngrediantB");
   ```

### 3. 프로그래밍 방식 사용

```csharp
public class IngrediantDisplay : MonoBehaviour
{
    [SerializeField] private string ingrediantID;
    [SerializeField] private TextMeshProUGUI ingrediantNameText;

    private void Start()
    {
        // 언어 변경 이벤트 구독
        IngrediantLocalizationHelper.SubscribeToLanguageChanged(UpdateIngrediantName);
        
        // 초기 번역 적용
        UpdateIngrediantName();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        IngrediantLocalizationHelper.UnsubscribeFromLanguageChanged(UpdateIngrediantName);
    }

    private void UpdateIngrediantName()
    {
        if (ingrediantNameText != null)
        {
            string localizedName = IngrediantLocalizationHelper.GetIngrediantName(ingrediantID);
            ingrediantNameText.text = localizedName;
        }
    }

    public void SetIngrediant(string newIngrediantID)
    {
        ingrediantID = newIngrediantID;
        UpdateIngrediantName();
    }
}
```

## 설정 방법

### 1. CSV 파일 준비
1. 위의 형식에 맞춰 재료 번역 CSV 파일 생성
2. Addressable Asset으로 등록 (선택사항)

### 2. DataManager 설정
- `_isIngrediantLocalizationAddressable`: Addressable 사용 여부 설정
- `_ingrediantLocalizationAddress`: Addressable 에셋 주소 설정
- `_ingrediantLocalizationDataTableURL`: 구글 스프레드시트 URL 설정

### 3. UI 설정
- 재료 이름을 표시할 UI 요소에 `IngrediantLocalizedText` 컴포넌트 추가
- Ingrediant ID 설정

## 주의사항

1. **CSV 파일 형식**: 반드시 지정된 컬럼명을 사용해야 합니다
2. **재료 ID 일치**: CSV의 ID와 실제 사용하는 재료 ID가 정확히 일치해야 합니다
3. **언어 변경**: 언어가 변경되면 자동으로 UI가 업데이트됩니다
4. **폴백 처리**: 번역을 찾을 수 없는 경우 재료 ID가 그대로 표시됩니다

## 디버깅

번역이 제대로 작동하지 않는 경우:
1. CSV 파일이 올바르게 로드되었는지 확인
2. 재료 ID가 정확한지 확인
3. LocalizationManager가 초기화되었는지 확인
4. Debug.Log를 통해 각 단계별 결과 확인

## 예제 시나리오

### 재료 상점 UI
```csharp
public class IngrediantShopItem : MonoBehaviour
{
    [SerializeField] private string ingrediantID;
    [SerializeField] private TextMeshProUGUI ingrediantNameText;
    [SerializeField] private TextMeshProUGUI priceText;

    private void Start()
    {
        // 재료 이름 번역
        IngrediantLocalizationHelper.SubscribeToLanguageChanged(UpdateDisplay);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // 재료 이름 번역 적용
        string localizedName = IngrediantLocalizationHelper.GetIngrediantName(ingrediantID);
        ingrediantNameText.text = localizedName;
        
        // 가격은 별도 처리
        priceText.text = $"${GetIngrediantPrice(ingrediantID)}";
    }
}
```

이 시스템을 사용하면 재료 관련 UI에서 일관된 다국어 지원을 제공할 수 있습니다.
