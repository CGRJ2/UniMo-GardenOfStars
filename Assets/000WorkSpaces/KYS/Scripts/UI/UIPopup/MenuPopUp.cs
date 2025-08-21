using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
    /// <summary>
    /// MenuPopUp의 View 클래스 - UI 표시와 사용자 입력만 담당
    /// </summary>
    public class MenuPopUp : BaseUI
    {
        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button closeButton;

        [Header("Localized Text Components")]
        [SerializeField] private LocalizedText startButtonText;
        [SerializeField] private LocalizedText settingsButtonText;
        [SerializeField] private LocalizedText exitButtonText;
        [SerializeField] private LocalizedText closeButtonText;

        // 이벤트 - Presenter가 구독
        public System.Action<PointerEventData> OnStartButtonClicked;
        public System.Action<PointerEventData> OnSettingsButtonClicked;
        public System.Action<PointerEventData> OnExitButtonClicked;
        public System.Action<PointerEventData> OnCloseButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
        }

        protected override void SetupMVP()
        {
            base.SetupMVP();
            
            // MVP 컴포넌트들을 자동으로 찾아서 설정
            // Presenter 찾기
            MenuPopUpPresenter presenter = GetComponent<MenuPopUpPresenter>();
            if (presenter == null)
            {
                presenter = gameObject.AddComponent<MenuPopUpPresenter>();
                Debug.Log("[MenuPopUp] MenuPopUpPresenter 컴포넌트를 자동으로 추가했습니다.");
            }
            SetPresenter(presenter);
            
            // Model 찾기
            MenuPopUpModel model = GetComponent<MenuPopUpModel>();
            if (model == null)
            {
                model = gameObject.AddComponent<MenuPopUpModel>();
                Debug.Log("[MenuPopUp] MenuPopUpModel 컴포넌트를 자동으로 추가했습니다.");
            }
            SetModel(model);
            
            Debug.Log("[MenuPopUp] MVP 컴포넌트 설정 완료");
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
            // 버튼 이벤트를 Presenter로 전달
            GetEventWithSFX("StartButton").Click += (data) => OnStartButtonClicked?.Invoke(data);
            GetEventWithSFX("SettingsButton").Click += (data) => OnSettingsButtonClicked?.Invoke(data);
            GetEventWithSFX("ExitButton").Click += (data) => OnExitButtonClicked?.Invoke(data);
            GetBackEvent("CloseButton").Click += (data) => OnCloseButtonClicked?.Invoke(data);
        }

        #region Public Methods (Presenter가 호출)

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
            if (startText != null && startButtonText != null)
            {
                startButtonText.SetText(startText);
            }
            
            if (settingsText != null && settingsButtonText != null)
            {
                settingsButtonText.SetText(settingsText);
            }
            
            if (exitText != null && exitButtonText != null)
            {
                exitButtonText.SetText(exitText);
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

        public static void ShowMenuPopUp()
        {
            UIManager.Instance.ShowPopUpAsync<MenuPopUp>();
        }
    }
}
