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

        Dictionary<string, BuildingData> buildingDatas = new();
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


            // 튜토리얼 씬의 경우, 건물데이터 하나의 슬롯만 생성
            if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
            {
                Addressables.LoadAssetAsync<BuildingData>(TutorialManager.Instance.tutoHarvestBuildingID).Completed += task =>
                {
                    BuildingData bd = task.Result;
                    PropertyContent content = Instantiate(contentPrefab, contentParent).GetComponent<PropertyContent>();
                    contentInstances.Add(bd.ID, content);

                    // PropertyContent 초기화
                    content.Initialize();

                    // 업그레이드 정보가 있는 건물이라면 해당 정보도 같이 업데이트
                    Dictionary<string, UpgradeData> upgradeDic = Manager.buildings.upgradeDataDic;
                    if (upgradeDic.ContainsKey(bd.ID)) // 현재 건물에 업그레이드 정보가 있다면
                    {
                        content.SetBuildingData(bd, upgradeDic[bd.ID]);
                    }
                    else
                    {
                        content.SetBuildingData(bd);
                    }
                };

                return;
            }

            // 일반 스테이지의 경우
            else
            {
                // 건물 데이터 불러오기
                Addressables.LoadAssetsAsync<BuildingData>("Data", null, true).Completed += task =>
                {
                    foreach (BuildingData bd in task.Result)
                    {
                        if (!buildingDatas.ContainsKey(bd.ID))
                        {
                            buildingDatas.Add(bd.ID, bd); // 건물 데이터 추가
                        }

                        // 건물 정보 슬롯 생성 (중복 생성 방지)
                        if (!contentInstances.ContainsKey(bd.ID))
                        {
                            PropertyContent content = Instantiate(contentPrefab, contentParent).GetComponent<PropertyContent>();
                            contentInstances.Add(bd.ID, content);

                            // PropertyContent 초기화
                            content.Initialize();

                            // 업그레이드 정보가 있는 건물이라면 해당 정보도 같이 업데이트
                            Dictionary<string, UpgradeData> upgradeDic = Manager.buildings.upgradeDataDic;
                            if (upgradeDic.ContainsKey(bd.ID)) // 현재 건물에 업그레이드 정보가 있다면
                            {
                                content.SetBuildingData(bd, upgradeDic[bd.ID]);
                            }
                            else
                            {
                                content.SetBuildingData(bd);
                            }
                        }
                    }
                };
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