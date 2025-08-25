using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
    /// <summary>
    /// MenuPopUp - UI 표시와 사용자 입력만 담당 (View만 사용)
    /// </summary>
    public class MenuPopUp : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string startButtonName = "StartButton";
        [SerializeField] private string settingsButtonName = "SettingsButton";
        [SerializeField] private string exitButtonName = "ExitButton";
        [SerializeField] private string closeButtonName = "CloseButton";

        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private Button startButton => GetUI<Button>(startButtonName);
        private Button settingsButton => GetUI<Button>(settingsButtonName);
        private Button exitButton => GetUI<Button>(exitButtonName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        #endregion
        
        protected override void Awake()
        {
            base.Awake();
            
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Popup;
            }
        }

        protected override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "menu_start",
                "menu_settings", 
                "menu_exit",
                "menu_close"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            SetupAutoLocalization();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var startEventHandler = GetEventWithSFX(startButtonName, "SFX_ButtonClick");
            if (startEventHandler != null)
            {
                startEventHandler.Click += (data) => OnStartButtonClicked();
            }
            
            var settingsEventHandler = GetEventWithSFX(settingsButtonName, "SFX_ButtonClick");
            if (settingsEventHandler != null)
            {
                settingsEventHandler.Click += (data) => OnSettingsButtonClicked();
            }
            
            var exitEventHandler = GetEventWithSFX(exitButtonName, "SFX_ButtonClick");
            if (exitEventHandler != null)
            {
                exitEventHandler.Click += (data) => OnExitButtonClicked();
            }
            
            var closeEventHandler = GetBackEvent(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }
        }

        #region Event Handlers

        private void OnStartButtonClicked()
        {
            Debug.Log("[MenuPopUp] 시작 버튼 클릭");
            // 게임 시작 로직
            Manager.ui.ClosePopup();
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MenuPopUp] 설정 버튼 클릭");
            // 언어 설정 패널 열기
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPopUpAsync<LanguageSettingsPanel>();
            }
        }

        private void OnExitButtonClicked()
        {
            Debug.Log("[MenuPopUp] 종료 버튼 클릭");
            // 게임 종료 확인 팝업
            CheckPopUp.ShowCheckPopUp(
                "게임을 종료하시겠습니까?",
                "종료",
                "취소",
                () => {
                    Debug.Log("[MenuPopUp] 게임 종료");
                    #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                    #else
                        Application.Quit();
                    #endif
                },
                null
            );
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[MenuPopUp] 닫기 버튼 클릭");
            Manager.ui.ClosePopup();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 시작 버튼 상호작용 가능 여부 설정
        /// </summary>
        public void SetStartButtonInteractable(bool interactable)
        {
            if (startButton != null)
            {
                startButton.interactable = interactable;
            }
        }

        /// <summary>
        /// 설정 버튼 상호작용 가능 여부 설정
        /// </summary>
        public void SetSettingsButtonInteractable(bool interactable)
        {
            if (settingsButton != null)
            {
                settingsButton.interactable = interactable;
            }
        }

        /// <summary>
        /// 종료 버튼 상호작용 가능 여부 설정
        /// </summary>
        public void SetExitButtonInteractable(bool interactable)
        {
            if (exitButton != null)
            {
                exitButton.interactable = interactable;
            }
        }

        /// <summary>
        /// 버튼 텍스트 업데이트
        /// </summary>
        public void UpdateButtonTexts(string startText = null, string settingsText = null, string exitText = null)
        {
            if (startText != null && startButton != null)
            {
                var textComponent = startButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = startText;
                }
            }
            
            if (settingsText != null && settingsButton != null)
            {
                var textComponent = settingsButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = settingsText;
                }
            }
            
            if (exitText != null && exitButton != null)
            {
                var textComponent = exitButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = exitText;
                }
            }
        }

        /// <summary>
        /// UI 상태 업데이트
        /// </summary>
        public void UpdateUIState(bool canStart, bool canSettings, bool canExit)
        {
            SetStartButtonInteractable(canStart);
            SetSettingsButtonInteractable(canSettings);
            SetExitButtonInteractable(canExit);
        }

        #endregion

        #region Debug Methods

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[MenuPopUp] UI 요소 정보:");
            Debug.Log($"  - startButton: {startButtonName} -> {(startButton != null ? "찾음" : "없음")}");
            Debug.Log($"  - settingsButton: {settingsButtonName} -> {(settingsButton != null ? "찾음" : "없음")}");
            Debug.Log($"  - exitButton: {exitButtonName} -> {(exitButton != null ? "찾음" : "없음")}");
            Debug.Log($"  - closeButton: {closeButtonName} -> {(closeButton != null ? "찾음" : "없음")}");
        }

        #endregion

        public static void ShowMenuPopUp()
        {
            UIManager.Instance.ShowPopUpAsync<MenuPopUp>();
        }
    }
}
