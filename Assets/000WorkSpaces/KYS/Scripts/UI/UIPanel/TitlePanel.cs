

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


        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string confirmButtonName = "ConfirmButton";
        [SerializeField] private string closeButtonName = "CancelButton";
        




        [Header("Auto Localization Keys")]
        [SerializeField] private string[] customLocalizeKeys = {
            "title_panel_title",
            "ui_confirm",
            "ui_cancel"
        };

        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private Button confirmButton => GetUI<Button>(confirmButtonName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        #endregion
        
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

        private void SetupButtons()
        {
            Debug.Log("[TitlePanel] SetupButtons() 시작");
            
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var confirmEventHandler = GetEventWithSFX(confirmButtonName, "SFX_ButtonClick");
            if (confirmEventHandler != null)
            {
                confirmEventHandler.Click += OnConfirmClicked;
                Debug.Log($"[TitlePanel] 확인 버튼 이벤트 설정 완료: {confirmButtonName}");
            }
            else
            {
                Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {confirmButtonName}");
            }
            
            var closeEventHandler = GetBackEvent(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += OnCancelClicked;
                Debug.Log($"[TitlePanel] 닫기 버튼 이벤트 설정 완료: {closeButtonName}");
            }
            else
            {
                Debug.LogError($"[TitlePanel] 닫기 버튼 이벤트 설정 실패: {closeButtonName}");
            }
            
            Debug.Log("[TitlePanel] SetupButtons() 완료");
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

        
        /// <summary>
        /// TitlePanel 표시 (정적 메서드)
        /// </summary>
        public static void ShowTitlePanel()
        {
            Debug.Log("[TitlePanel] ShowTitlePanel() 정적 메서드 호출됨");
            
            if (UIManager.Instance == null)
            {
                Debug.LogError("[TitlePanel] UIManager.Instance가 null입니다!");
                return;
            }
            
            // 이미 TitlePanel이 열려있는지 확인
            var existingPanels = UIManager.Instance.GetUIsByLayer(UILayerType.Panel);
            foreach (var panel in existingPanels)
            {
                if (panel is TitlePanel)
                {
                    Debug.Log("[TitlePanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                    return;
                }
            }
            
            // TitlePanel은 Panel 타입이므로 Panel 전용 메서드 사용
            UIManager.Instance.ShowPanelAsync<TitlePanel>((panel) => {
                if (panel != null)
                {
                    Debug.Log("[TitlePanel] ShowTitlePanel() 완료 - 패널 생성 성공");
                }
                else
                {
                    Debug.LogError("[TitlePanel] ShowTitlePanel() 실패 - 패널 생성 실패");
                }
            });
        }
    }

}