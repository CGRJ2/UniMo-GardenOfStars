using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace KYS
{
    /// <summary>
    /// BaseUI의 string 기반 UI 요소 검색 방식을 사용하는 예시
    /// </summary>
    public class BaseUIUsageExamples : BaseUI
    {
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.Other;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // String 기반으로 UI 요소 가져오기
            SetupUIElements();
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        private void SetupUIElements()
        {
            // 1. 기본적인 UI 요소 가져오기
            GameObject titleObject = GetUI("TitleText");
            if (titleObject != null)
            {
                TextMeshProUGUI titleText = titleObject.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = "BaseUI String 방식 예시";
                }
            }
            
            // 2. 제네릭 메서드로 컴포넌트 직접 가져오기
            TextMeshProUGUI contentText = GetUI<TextMeshProUGUI>("ContentText");
            if (contentText != null)
            {
                contentText.text = "이 방식은 에디터에서 이름 변경해도 코드 수정이 필요 없습니다.";
            }
            
            // 3. 버튼 이벤트 설정
            Button confirmButton = GetUI<Button>("ConfirmButton");
            if (confirmButton != null)
            {
                GetEventWithSFX("ConfirmButton").Click += (data) => OnConfirmClicked();
            }
            
            Button cancelButton = GetUI<Button>("CancelButton");
            if (cancelButton != null)
            {
                GetBackEvent("CancelButton").Click += (data) => OnCancelClicked();
            }
            
            // 4. 이미지 컴포넌트 가져오기
            Image backgroundImage = GetUI<Image>("BackgroundImage");
            if (backgroundImage != null)
            {
                // 배경 이미지 설정
                backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            }
            
            // 5. 토글 컴포넌트 가져오기
            Toggle optionToggle = GetUI<Toggle>("OptionToggle");
            if (optionToggle != null)
            {
                optionToggle.onValueChanged.AddListener(OnToggleChanged);
            }
            
            // 6. 슬라이더 컴포넌트 가져오기
            Slider volumeSlider = GetUI<Slider>("VolumeSlider");
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }
        
        private void OnConfirmClicked()
        {
            Debug.Log("[BaseUIUsageExamples] 확인 버튼 클릭");
            
            // 동적으로 UI 요소 업데이트
            TextMeshProUGUI statusText = GetUI<TextMeshProUGUI>("StatusText");
            if (statusText != null)
            {
                statusText.text = "확인되었습니다!";
                statusText.color = Color.green;
            }
            
            Hide();
        }
        
        private void OnCancelClicked()
        {
            Debug.Log("[BaseUIUsageExamples] 취소 버튼 클릭");
            
            // 동적으로 UI 요소 업데이트
            TextMeshProUGUI statusText = GetUI<TextMeshProUGUI>("StatusText");
            if (statusText != null)
            {
                statusText.text = "취소되었습니다.";
                statusText.color = Color.red;
            }
            
            Hide();
        }
        
        private void OnToggleChanged(bool isOn)
        {
            Debug.Log($"[BaseUIUsageExamples] 토글 상태 변경: {isOn}");
            
            // 토글 상태에 따라 다른 UI 요소 활성화/비활성화
            GameObject advancedPanel = GetUI("AdvancedPanel");
            if (advancedPanel != null)
            {
                advancedPanel.SetActive(isOn);
            }
        }
        
        private void OnVolumeChanged(float value)
        {
            Debug.Log($"[BaseUIUsageExamples] 볼륨 변경: {value}");
            
            // 볼륨 값 표시
            TextMeshProUGUI volumeText = GetUI<TextMeshProUGUI>("VolumeValueText");
            if (volumeText != null)
            {
                volumeText.text = $"볼륨: {value:F1}";
            }
        }
        
        /// <summary>
        /// UI 요소 존재 여부 확인 예시
        /// </summary>
        public void CheckUIElements()
        {
            string[] uiNames = GetAllUINames();
            Debug.Log($"[BaseUIUsageExamples] 등록된 UI 요소 수: {uiNames.Length}");
            
            foreach (string name in uiNames)
            {
                bool exists = HasUI(name);
                Debug.Log($"[BaseUIUsageExamples] {name}: {exists}");
            }
            
            // 특정 컴포넌트 존재 여부 확인
            bool hasButton = HasUI<Button>("ConfirmButton");
            bool hasText = HasUI<TextMeshProUGUI>("TitleText");
            
            Debug.Log($"[BaseUIUsageExamples] ConfirmButton 존재: {hasButton}");
            Debug.Log($"[BaseUIUsageExamples] TitleText 존재: {hasText}");
        }
        
        /// <summary>
        /// 캐시 새로고침 예시 (UI 구조 변경 시)
        /// </summary>
        public void RefreshUIElements()
        {
            Debug.Log("[BaseUIUsageExamples] UI 캐시 새로고침 시작");
            RefreshCache();
            
            // 새로고침 후 UI 요소 다시 설정
            SetupUIElements();
            Debug.Log("[BaseUIUsageExamples] UI 캐시 새로고침 완료");
        }

        // 새로운 사운드 시스템 사용 예제
        private void SetupSoundExamples()
        {
            // 기본 클릭 사운드 (BaseUI의 defaultClickSound 사용)
            GetEventWithSFX("BasicButton").Click += (data) => Debug.Log("기본 클릭 사운드");
            
            // 커스텀 클릭 사운드
            GetEventWithSFX("CustomButton", "SFX_CustomClick").Click += (data) => Debug.Log("커스텀 클릭 사운드");
            
            // 기본 뒤로가기 사운드 (BaseUI의 defaultBackSound 사용)
            GetBackEvent("BackButton").Click += (data) => Debug.Log("기본 뒤로가기 사운드");
            
            // 커스텀 뒤로가기 사운드
            GetBackEvent("CustomBackButton", "SFX_CustomBack").Click += (data) => Debug.Log("커스텀 뒤로가기 사운드");
            
            // 호버 사운드
            GetHoverEvent("HoverButton").Enter += (data) => Debug.Log("호버 사운드");
            
            // 에러 사운드
            GetErrorEvent("ErrorButton").Click += (data) => Debug.Log("에러 사운드");
            
            // 성공 사운드
            GetSuccessEvent("SuccessButton").Click += (data) => Debug.Log("성공 사운드");
        }
    }
    
    /// <summary>
    /// 동적으로 UI 요소를 추가하는 예시
    /// </summary>
    public class DynamicUIExample : BaseUI
    {
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // 동적으로 UI 요소 생성 및 등록
            CreateDynamicUIElements();
            
            UIManager.Instance.RegisterUI(this);
        }
        
        private void CreateDynamicUIElements()
        {
            // 동적으로 버튼 생성
            GameObject buttonObject = new GameObject("DynamicButton");
            buttonObject.transform.SetParent(transform);
            
            Button button = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>();
            
            // 버튼 텍스트 추가
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform);
            
            TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
            buttonText.text = "동적 버튼";
            buttonText.color = Color.white;
            
            // 딕셔너리에 추가
            AddUIToDictionary(buttonObject);
            
            // 이벤트 설정
            GetEventWithSFX("DynamicButton").Click += (data) => OnDynamicButtonClicked();
        }
        
        private void OnDynamicButtonClicked()
        {
            Debug.Log("[DynamicUIExample] 동적 버튼 클릭!");
            
            // 동적으로 생성된 UI 요소 제거
            GameObject buttonObject = GetUI("DynamicButton");
            if (buttonObject != null)
            {
                DeleteFromDictionary("DynamicButton");
                Destroy(buttonObject);
            }
        }
    }
}
