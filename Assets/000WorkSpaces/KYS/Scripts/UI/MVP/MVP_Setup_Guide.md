# MVP 구조 설정 가이드

## 📋 개요

MVP (Model-View-Presenter) 패턴을 사용하여 UI의 비즈니스 로직과 데이터를 분리할 수 있습니다.

**⚠️ 현재 프로젝트에서는 선택적으로 MVP 패턴을 사용합니다:**
- **간단한 UI**: View만 사용 (현재 대부분의 UI)
- **복잡한 UI**: 필요시에만 MVP 전체 구조 적용

## 🎯 MVP 구조

### **1. 기본 구조**
```
View (BaseUI 상속)
├── UI 요소 표시
├── 사용자 입력 처리
└── 이벤트 발생

Presenter (BaseUIPresenter 상속)
├── View 이벤트 구독
├── 비즈니스 로직 처리
└── Model과 View 연결

Model (BaseUIModel 상속)
├── 데이터 관리
├── 상태 관리
└── 비즈니스 규칙
```

## 🔧 프리팹 설정 방법

### **1. 간단한 UI (View만 사용)**

#### **프리팹 구성:**
```
MenuPopUp (GameObject)
└── MenuPopUp.cs (BaseUI 상속)
```

#### **설정 단계:**
1. **프리팹 생성**: 빈 GameObject 생성
2. **MenuPopUp 스크립트 추가**: `MenuPopUp.cs` 컴포넌트 추가
3. **UI 요소 연결**: Inspector에서 버튼, 텍스트 등 연결
4. **완료**: 자동으로 MVP 컴포넌트들이 추가됨

### **2. 복잡한 UI (MVP 전체 사용)**

#### **프리팹 구성:**
```
MenuPopUp (GameObject)
├── MenuPopUp.cs (View)
├── MenuPopUpPresenter.cs (Presenter)
└── MenuPopUpModel.cs (Model)
```

#### **설정 단계:**
1. **프리팹 생성**: 빈 GameObject 생성
2. **View 스크립트 추가**: `MenuPopUp.cs` 컴포넌트 추가
3. **Presenter 스크립트 추가**: `MenuPopUpPresenter.cs` 컴포넌트 추가
4. **Model 스크립트 추가**: `MenuPopUpModel.cs` 컴포넌트 추가
5. **UI 요소 연결**: Inspector에서 버튼, 텍스트 등 연결

## 💻 코드 예시

### **1. View (MenuPopUp.cs)**
```csharp
public class MenuPopUp : BaseUI
{
    // 이벤트 - Presenter가 구독
    public System.Action<PointerEventData> OnStartButtonClicked;
    
    private void SetupButtons()
    {
        // 버튼 이벤트를 Presenter로 전달
        GetEventWithSFX("StartButton").Click += (data) => OnStartButtonClicked?.Invoke(data);
    }
    
    // Presenter가 호출하는 메서드
    public void SetStartButtonInteractable(bool interactable)
    {
        if (startButton != null)
        {
            startButton.interactable = interactable;
        }
    }
}
```

### **2. Presenter (MenuPopUpPresenter.cs)**
```csharp
public class MenuPopUpPresenter : BaseUIPresenter
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        // View의 이벤트 구독
        if (view is MenuPopUp menuView)
        {
            menuView.OnStartButtonClicked += HandleStartButtonClicked;
        }
    }
    
    private void HandleStartButtonClicked(PointerEventData data)
    {
        // 비즈니스 로직 처리
        Debug.Log("시작 버튼 클릭 처리");
    }
}
```

### **3. Model (MenuPopUpModel.cs)**
```csharp
public class MenuPopUpModel : BaseUIModel
{
    public bool CanStartGame()
    {
        return isGameReady && currentGameState == GameState.MainMenu;
    }
    
    public void SetGameState(GameState newState)
    {
        currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
```

## 🚀 사용 방법

### **1. UI 표시**
```csharp
// 자동으로 MVP 구조 사용
MenuPopUp.ShowMenuPopUp();
```

### **2. 이벤트 처리**
```csharp
// View에서 이벤트 발생
OnStartButtonClicked?.Invoke(data);

// Presenter에서 이벤트 처리
private void HandleStartButtonClicked(PointerEventData data)
{
    // 비즈니스 로직
}
```

### **3. 데이터 접근**
```csharp
// Presenter에서 Model 데이터 사용
bool canStart = menuModel.CanStartGame();
menuView.SetStartButtonInteractable(canStart);
```

## 📊 적용 기준

### **1. 간단한 UI (View만)**
- ✅ 버튼 클릭만
- ✅ 데이터 저장/로드 없음
- ✅ 복잡한 비즈니스 로직 없음
- **예시**: `CheckPopUp`, `LoadingScreen`

### **2. 복잡한 UI (MVP 전체)**
- ✅ 여러 버튼과 상태
- ✅ 데이터 저장/로드
- ✅ 복잡한 비즈니스 로직
- **예시**: `MenuPopUp`, `InventoryPanel`

## 🔍 디버깅

### **1. MVP 컴포넌트 확인**
```csharp
// View에서 MVP 컴포넌트 확인
Debug.Log($"Presenter: {presenter != null}");
Debug.Log($"Model: {model != null}");
```

### **2. 이벤트 연결 확인**
```csharp
// Presenter에서 이벤트 구독 확인
if (view is MenuPopUp menuView)
{
    Debug.Log("MenuPopUp View 연결됨");
}
```

## ⚠️ 주의사항

1. **프리팹 설정**: View 스크립트만 추가하면 자동으로 MVP 컴포넌트들이 추가됩니다.
2. **이벤트 구독**: Presenter에서 View의 이벤트를 구독해야 합니다.
3. **데이터 접근**: Presenter를 통해서만 Model의 데이터에 접근해야 합니다.
4. **UI 업데이트**: Model의 데이터 변경 시 Presenter가 View를 업데이트해야 합니다.

이 가이드를 따라 설정하면 MVP 구조를 효과적으로 사용할 수 있습니다!
