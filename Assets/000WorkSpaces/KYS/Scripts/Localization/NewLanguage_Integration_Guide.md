# 새로운 언어 추가 가이드 (확장 가능한 구조)

## 🎯 **새로운 언어 추가 시 필요한 작업**

### 1단계: 데이터 구조에 필드 추가
```csharp
// BuildingData.cs에 추가
public string Name_JP;    // 일본어 이름
public string Description_JP;  // 일본어 설명

// IngrediantData.cs에 추가  
public string Name_JP;    // 일본어 이름
```

### 2단계: CSV 데이터 업데이트
```csv
ID,Name_KR,Name_EN,Name_JP,Description_KR,Description_EN,Description_JP
BuildingA,건물A,Building A,建物A,건물A 설명,Building A Description,建物Aの説明
IngrediantA,재료A,Material A,材料A
```

### 3단계: ExtensibleLocalizationHelper에 언어 코드 추가
```csharp
// ExtensibleLocalizationHelper.cs의 LanguageCodeMap에 추가
{ SystemLanguage.Japanese, "JP" },
```

### 4단계: LocalizationManager 설정 업데이트 (선택사항)
```csharp
// LocalizationManager.cs의 allSupportedLanguages에 추가
SystemLanguage.Japanese,
```

## ✅ **완료!**

**이제 일본어가 자동으로 지원됩니다!**

## 🚀 **장점들:**

### 1. **코드 수정 최소화**
- 기존 헬퍼 클래스 메서드 시그니처 변경 없음
- switch문이나 if문 추가 불필요
- 리플렉션으로 동적 필드 접근

### 2. **자동 폴백 지원**
- 일본어 번역이 없으면 영어로 폴백
- 영어도 없으면 한국어로 폴백
- 안정적인 동작 보장

### 3. **기존 코드 호환성**
```csharp
// 기존 코드 그대로 사용 가능
string name = BuildingLocalizationHelper.GetBuildingName("BuildingA");
// 자동으로 현재 언어에 맞는 번역 반환
```

### 4. **확장성**
```csharp
// 특정 언어로 직접 가져오기
string japaneseName = ExtensibleLocalizationHelper.GetBuildingName("BuildingA", SystemLanguage.Japanese);

// 모든 언어로 가져오기
var allNames = ExtensibleLocalizationHelper.GetAllBuildingNames("BuildingA");
foreach (var kvp in allNames)
{
    Debug.Log($"{kvp.Key}: {kvp.Value}");
}
```

## 📋 **새로운 언어 추가 체크리스트:**

- [ ] BuildingData.cs에 Name_{언어코드}, Description_{언어코드} 필드 추가
- [ ] IngrediantData.cs에 Name_{언어코드} 필드 추가
- [ ] CSV 데이터에 새로운 언어 컬럼 추가
- [ ] ExtensibleLocalizationHelper.cs의 LanguageCodeMap에 언어 코드 추가
- [ ] LocalizationManager.cs의 allSupportedLanguages에 언어 추가 (선택사항)
- [ ] 테스트: 기존 코드가 정상 작동하는지 확인

## 🔧 **런타임에서 새로운 언어 추가**

```csharp
// 런타임에서 새로운 언어 지원 추가
ExtensibleLocalizationHelper.AddLanguageSupport(SystemLanguage.Thai, "TH");

// 즉시 사용 가능
string thaiName = ExtensibleLocalizationHelper.GetBuildingName("BuildingA", SystemLanguage.Thai);
```

## 🎉 **결과**

이제 새로운 언어를 추가할 때:
- **코드 수정**: 최소한 (데이터 필드 + 언어 코드 매핑)
- **기존 코드**: 변경 없음
- **자동 지원**: 폴백, 리플렉션, 동적 접근
- **확장성**: 무제한 언어 지원 가능

**훨씬 더 편리하고 확장 가능한 구조가 되었습니다!**
