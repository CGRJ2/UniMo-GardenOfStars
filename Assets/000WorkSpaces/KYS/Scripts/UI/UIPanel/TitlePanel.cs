

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



        [Header("Auto Localization Keys")]
        [SerializeField] private string[] customLocalizeKeys = {
            "title_panel_title",
            "ui_confirm",
            "ui_cancel"
        };

        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Panel;
            }
            if (panelGroup == UIPanelGroup.Other) // BaseUI의 기본값
            {
                panelGroup = UIPanelGroup.Other;
            }
            // hidePreviousUI와 disablePreviousUI는 인스펙터에서 설정 가능하도록 유지
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
            Manager.ui.ShowAllHUDElements();
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