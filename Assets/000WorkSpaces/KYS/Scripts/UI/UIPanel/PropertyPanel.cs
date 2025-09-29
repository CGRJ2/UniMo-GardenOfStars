using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace KYS
{

    public class PropertyPanel : BaseUI
    {
        [SerializeField] private string propertyTextName = "PropertyText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";
        [SerializeField] Transform contentParent;
        [SerializeField] GameObject contentPrefab;

        private TextMeshProUGUI propertyText => GetUI<TextMeshProUGUI>(propertyTextName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);


        // 추가 변수 선언
        private int currentMoney = 0;

        //Dictionary<string, BuildingData> buildingDatas = new();
        Dictionary<string, PropertyContent> contentInstances = new();

        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Panel;
            }

            Initialize();

            Manager.Audio.SfxPlay("DoorBell");
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

            // 현재 스테이지에 판매 중인 건물들만 불러와서 생성
            string curStageID = Manager.firebase.UserData.CurStage.Value;
            string[] buildingIDs = Manager.data.Stage.Values[curStageID].GetBuildingIdList();

            foreach (string id in buildingIDs)
            {
                // 건물 정보 슬롯 생성 (중복 생성 방지)
                if (!contentInstances.ContainsKey(id))
                {
                    BuildingData bd = Manager.data.Building[id];
                    UpgradeData upgradeData = Manager.firebase.UserData.BuildingUpgradeList.Get(id);

                    PropertyContent content = Instantiate(contentPrefab, contentParent).GetComponent<PropertyContent>();
                    contentInstances.Add(id, content);

                    // PropertyContent 초기화
                    content.Initialize();
                    content.SetBuildingData(bd, upgradeData);
                }
            }
        }
        public override void Cleanup()
        {
            // 생성된 PropertyContent 인스턴스들 정리
            foreach (var content in contentInstances.Values)
            {
                if (content != null)
                {
                    Destroy(content.gameObject);
                }
            }
            contentInstances.Clear();

            // ObservableProperty 구독 해제
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);

            base.Cleanup();
        }


        private void SetupButtons()
        {
            Debug.Log($"[PropertyPanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[PropertyPanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                Debug.Log($"[PropertyPanel] 새 이벤트 구독 추가 - Time: {Time.time}");
                closeEventHandler.Click += OnCloseButton;
                isButtonsSetup = true; // 설정 완료 플래그
            }
            else
            {
                Debug.LogError($"[PropertyPanel] 닫기 버튼 이벤트 설정 실패: {closeButtonName}");
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
            Debug.Log($"[PropertyPanel] OnCloseButton 호출됨 - Time: {Time.time}");
            Manager.ui.ClosePanel();
        }






    }




}