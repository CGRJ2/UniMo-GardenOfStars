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
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";

        private TextMeshProUGUI propertyText => GetUI<TextMeshProUGUI>(propertyTextName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);


        // 추가 변수 선언
        private int currentMoney = 0;


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


            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);

            // ObservableProperty 구독 - 실시간 돈 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);

        }
        public override void Cleanup()
        {


            // ObservableProperty 구독 해제
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);


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


        public void UpdateMoney(int amount)
        {

            currentMoney = amount; // 현재 값 저장
            if (moneyText != null)
            {


                moneyText.text = $"{amount:N0}";
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
        }


        private void OnCloseButton(PointerEventData data)
        {
            // 확인 버튼 클릭 시 동작
            Debug.Log("확인 버튼 클릭됨");
            Manager.ui.ClosePanel();

        }






    }




}