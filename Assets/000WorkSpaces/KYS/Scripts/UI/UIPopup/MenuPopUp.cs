using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
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



        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup; // Popup으로 변경
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
            SetupBackdrop();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            if (startButton != null)
                GetEventWithSFX("StartButton", "SFX_ButtonClick").Click += OnStartClicked;
            
            if (settingsButton != null)
                GetEventWithSFX("SettingsButton", "SFX_ButtonClick").Click += OnSettingsClicked;
            
            if (exitButton != null)
                GetEventWithSFX("ExitButton", "SFX_ButtonClick").Click += OnExitClicked;
            
            if (closeButton != null)
                GetBackEvent("CloseButton", "SFX_ButtonClickBack").Click += OnCloseClicked;
        }

        private void OnStartClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUp] 시작 버튼 클릭");
            UIManager.Instance.ClosePopup();
        }

        private void OnSettingsClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUp] 설정 버튼 클릭");
            UIManager.Instance.ClosePopup();
        }

        private void OnExitClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUp] 종료 버튼 클릭");
            UIManager.Instance.ClosePopup();
        }

        private void OnCloseClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUp] 닫기 버튼 클릭");
            UIManager.Instance.ClosePopup();
        }

        private void SetupBackdrop()
        {
            // PopupCanvas에서 Backdrop 찾기
            Canvas popupCanvas = UIManager.Instance?.PopupCanvas;
            if (popupCanvas == null)
            {
                Debug.LogWarning("[MenuPopUp] PopupCanvas를 찾을 수 없습니다.");
                return;
            }

            BackdropUI backdrop = popupCanvas.GetComponentInChildren<BackdropUI>();
            if (backdrop != null)
            {
                // Backdrop 클릭 시 Popup 닫기
                backdrop.OnBackdropClicked += () =>
                {
                    Debug.Log("[MenuPopUp] Backdrop 클릭으로 Popup 닫기");
                    UIManager.Instance.ClosePopup();
                };
                
                Debug.Log("[MenuPopUp] Backdrop 설정 완료");
            }
            else
            {
                Debug.LogWarning("[MenuPopUp] Backdrop를 찾을 수 없습니다.");
            }
        }

        public static void ShowMenuPopUp()
        {
            UIManager.Instance.ShowPopUpAsync<MenuPopUp>();
        }
    }
}
