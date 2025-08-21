

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KYS
{

    public class TitlePanel : BaseUI
    {


        [Header("UI Elements")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        [Header("Localized Text Components")]
        [SerializeField] private LocalizedText titleText;
        [SerializeField] private LocalizedText confirmButtonText;
        [SerializeField] private LocalizedText closeButtonText;

        [Header("Auto Localization Keys")]
        [SerializeField] private string[] customLocalizeKeys = {
            "title_panel_title",
            "ui_confirm",
            "ui_cancel"
        };

        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;  // Panel 레이어 사용
            panelGroup = UIPanelGroup.Other; // 패널 그룹 설정
            hidePreviousUI = false;  // 이전 UI 숨기지 않음 (덮어쓰기 모드)
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음 (CanvasGroup.interactable = false)
        }


        public override void Initialize()
        {
            Debug.Log("[TitlePanel] Initialize() 메서드 시작");
            base.Initialize();
            
            // 자동 로컬라이제이션 설정
            SetupAutoLocalization();
            
            // 버튼 설정
            SetupButtons();
            
            Debug.Log("[TitlePanel] Initialize() 완료");
        }

        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }

        private bool buttonsSetup = false;

        private void SetupButtons()
        {
            GetEventWithSFX("ConfirmButton").Click += OnConfirmClicked;
            GetBackEvent("CancelButton").Click += OnCancelClicked;
        }

        private void OnConfirmClicked(PointerEventData data)
        {
            Debug.Log("[TitlePanel] 확인 버튼 클릭");
            UIManager.Instance.ShowPopUpAsync<MenuPopUp>();
           
        }

        private void OnCancelClicked(PointerEventData data)
        {
            Debug.Log("[TitlePanel] Cancel 버튼 클릭");
            UIManager.Instance.ClosePanel();
        }

        private void OnCloseClicked(PointerEventData data)
        {
            Debug.Log("[TitlePanel] Close 버튼 클릭");
            UIManager.Instance.ClosePanel();
        }
        
        /// <summary>
        /// TitlePanel 표시 (정적 메서드)
        /// </summary>
        public static void ShowTitlePanel()
        {
            UIManager.Instance.ShowPopUpAsync<TitlePanel>();
        }
    }

}