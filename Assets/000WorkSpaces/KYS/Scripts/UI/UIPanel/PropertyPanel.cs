using KYS;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KYS
{

    public class PropertyPanel : BaseUI
    {
        [SerializeField] private string propertyTextName = "PropertyText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private Transform PropertyContentGroup;

        private TextMeshProUGUI propertyText => GetUI<TextMeshProUGUI>(propertyTextName);


        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Panel;
            }

           Initialize();

        }
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                // 여기에 자동 현지화 키 추가
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
            //Debug.Log("[TitlePanel] SetupButtons() 시작");

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var confirmEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (confirmEventHandler != null)
            {
                confirmEventHandler.Click += OnCloseButton;
    
            }
            else
            {
                Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
            }

            var closeEventHandler = GetBackEvent(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                //closeEventHandler.Click += OnCancelClicked;
           
            }
            else
            {
                Debug.LogError($"[TitlePanel] 닫기 버튼 이벤트 설정 실패: {closeButtonName}");
            }


        }


        private void OnCloseButton(PointerEventData data)
        {
            // 확인 버튼 클릭 시 동작
            Debug.Log("확인 버튼 클릭됨");
            Manager.ui.ClosePanel();

        }






    }




}